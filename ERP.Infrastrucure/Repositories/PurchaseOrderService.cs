using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Presentation;
using ERP.Application.DTOs;
using ERP.Application.DTOs.Purchase;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Domain.Entities.PurchaseOrder;
using ERP.Domain.Entities.SalesReturn;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static ERP.Infrastructure.Repositories.SalesReturnService;

namespace ERP.Infrastructure.Repositories
{
    public  class PurchaseOrderService:IPurchaseOrderSerice
    {
        private readonly AppDbContext _context;
        private readonly ICodeGeneratorService _codeGeneratorService;

        public PurchaseOrderService(AppDbContext appDbContext, ICodeGeneratorService codeGeneratorService)
        {
            _context = appDbContext;
            _codeGeneratorService = codeGeneratorService;
        }




        public async Task<List<DropdownDto>> GetPoIds()
        {
            try
            {
                var result = await _context.purchaseorderheaders.Select(x => new DropdownDto
                {
                    Id = (int)x.po_id,//pk as id here
                    Name = x.po_no
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<GridDataResponse<PurchaseOrderGridDto>> GetPurchaseOrderGrid( PurchaseGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query =
                    from h in _context.purchaseorderheaders

                    join p in _context.customers
                        on h.vendor_id equals p.cust_pk

                    join s in _context.ddlookups
                        on h.status equals s.id

                    join d in _context.purchaseOrderDetails
                        on h.po_id equals d.po_id into details

                    select new
                    {
                        Header = h,
                        Vendor = p,
                        Status = s,

                        TotalItems = details.Count(),

                        TotalQty =
                            details.Sum(x => (decimal?)x.ordered_qty) ?? 0,

                        ReceivedQty =
                            details.Sum(x => (decimal?)x.received_qty) ?? 0,

                        BalanceQty =
                            details.Sum(x => (decimal?)x.balance_qty) ?? 0
                    };

                #region Filters

                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var search = filters.Search.Trim();

                    query = query.Where(x =>
                        EF.Functions.ILike(x.Header.po_no, $"%{search}%") ||
                        EF.Functions.ILike(x.Vendor.company_name, $"%{search}%"));
                }

                if (filters.invoiceno.HasValue)
                {
                    query = query.Where(x =>
                        x.Header.po_id == filters.invoiceno.Value);
                }

                if (filters.vendor != null && filters.vendor.Any())
                {
                    query = query.Where(x =>
                        filters.vendor.Contains(x.Header.vendor_id));
                }

                if (filters.status.HasValue)
                {
                    query = query.Where(x =>
                        x.Header.status == filters.status.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters.datefrom))
                {
                    var dateFrom =
                        DateTimeOffset.Parse(filters.datefrom).UtcDateTime.Date;

                    var dateTo =
                        !string.IsNullOrWhiteSpace(filters.dateto)
                        ? DateTimeOffset.Parse(filters.dateto).UtcDateTime.Date
                        : dateFrom;

                    query = query.Where(x =>
                        x.Header.po_date.Date >= dateFrom &&
                        x.Header.po_date.Date <= dateTo);
                }

                #endregion

                var total = await query.CountAsync();

                var data = await query
                    .OrderByDescending(x => x.Header.po_date)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

                var result = data.Select((x, index) =>
                    new PurchaseOrderGridDto
                    {
                        sno = offset + index + 1, // add if required

                        poid = x.Header.po_id,

                        pono = x.Header.po_no,

                        podate = x.Header.po_date,

                        vendor = x.Vendor.company_name,

                        items = x.TotalItems,

                        qty = x.TotalQty,

                        received = x.ReceivedQty,

                        balance = x.BalanceQty,

                        amount = x.Header.total_amount,

                        expecteddeliverydate =
                            x.Header.expected_delivery_date,

                        status = x.Status.value,
                        action =""
                    })
                    .ToList();

                return new GridDataResponse<PurchaseOrderGridDto>
                {
                    Data = result,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetPurchaseOrderGrid Error : " + ex.Message);
                throw;
            }
        }

       public async Task<PurchaseOrderDto> Createupdatepo(PurchaseOrderDto dto)
        {
            using var transaction =
               await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.LineItems == null ||
                    !dto.LineItems.Any())
                {
                    throw new Exception(
                        "Sales return must contain items.");
                }



                foreach (var item in dto.LineItems)
                {
                    if (item.orderedQty <= 0)
                        throw new Exception(
                            "Ordered quantity must be greater than zero.");

                    if (item.receivedQty > item.orderedQty)
                        throw new Exception(
                            $"Received quantity exceeds balance quantity for {item.partNo}");
                }


                PurchaseOrderHeader header;

                if (dto.poId == 0)
                {
                    await CreatePurchaseOrder(dto);
                }
                else
                {
                    await UpdatePurchaseOrder(dto);
                }

                await transaction.CommitAsync();

                return dto;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task CreatePurchaseOrder(PurchaseOrderDto dto)
        {
            string PoNo = await _codeGeneratorService.GenerateAsync("PurchaseOrder");

            var header =
                new PurchaseOrderHeader
                {
                    po_no = PoNo,
                    po_date = DateTime.SpecifyKind(dto.PoDate, DateTimeKind.Utc),

                    vendor_id = dto.vendorId,

                    requester_id = (long)dto.buyerId,

                    warehouse_id = (long)dto.warehouseId,

                    payment_terms = " ",

                    reference_no = " ",

                    status = dto.status,

                    remarks = dto.remarks,

                    subtotal = dto.LineItems.Sum(x=>x.orderedQty * x.unitPrice),  //dto.subTotal,

                    tax_amount = dto.LineItems.Sum(x=>(x.orderedQty * x.unitPrice) * (x.taxPct/100)),

                    total_amount = dto.LineItems.Sum(x=>x.totalPrice),

                    created_date = DateTime.UtcNow,

                    created_by = 1// (int)_currentuser.UserId
                };

            _context.purchaseorderheaders.Add(header);

            await _context.SaveChangesAsync();

            var details =
                dto.LineItems.Select(x =>
                    new purchaseOrderDetail
                    {
                        po_id = header.po_id,
                        item_id = x.itemId,

                        description= x.partName + " " + x.partNo,
                        ordered_qty = x.orderedQty,
                        received_qty = x.receivedQty,
                        balance_qty = x.balanceQty,
                        unit_price = x.unitPrice,
                        tax_id = (long)x.taxPct,
                        tax_percent = x.taxPct,
                        tax_amount =( x.unitPrice * x.orderedQty ) * (x.taxPct/100),
                        line_total = x.totalPrice,
                    });

            _context.purchaseOrderDetails.AddRange(details);

            await _context.SaveChangesAsync();
            
        }

        public static class StatusIds
        {
            public const int Draft = 33;

            public const int Approved = 34;

            public const int PartiallyReceived = 35;

            public const int Completed = 36;

            public const int Cancelled = 37;
        }



        private async Task UpdatePurchaseOrder(PurchaseOrderDto dto)
        {
            
            var header =
                await _context.purchaseorderheaders
                    .FirstOrDefaultAsync(
                        x => x.po_id == dto.poId);

            if (header == null)
                throw new Exception(
                    "Purchase Order not found");
            header.vendor_id = dto.vendorId;
            header.requester_id = (long)dto.buyerId;
            header.payment_terms = " ";
            header.reference_no = " ";
            header.status = dto.status;
            header.remarks = dto.remarks;
            header.discount_amount = dto.discountAmount;
            header.subtotal = dto.LineItems.Sum(x => x.orderedQty * x.unitPrice);  
            header.tax_amount = dto.LineItems.Sum(x => (x.orderedQty * x.unitPrice) * (x.taxPct / 100));
            header.total_amount = dto.LineItems.Sum(x => x.totalPrice);
            
            if(dto.status == StatusIds.Approved)
            {
                header.approved_by = 1;
                header.approved_date = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var existingLines =
                await _context.sales_Return_Details
                    .Where(x =>
                        x.sales_return_id == dto.poId)
                    .ToListAsync();

            _context.sales_Return_Details
                .RemoveRange(existingLines);

            var newLines =
                dto.LineItems.Select(x =>
                    new purchaseOrderDetail
                    {
                        po_id = header.po_id,
                        item_id = x.itemId,

                        description = x.partName + " " + x.partNo,
                        ordered_qty = x.orderedQty,
                        received_qty = x.receivedQty,
                        balance_qty = x.balanceQty,
                        unit_price = x.unitPrice,
                        tax_id = (long)x.taxPct,
                        tax_percent = x.taxPct,
                        tax_amount = (x.unitPrice * x.orderedQty) * (x.taxPct / 100),
                        line_total = x.totalPrice,
                    });

            _context.purchaseOrderDetails.AddRange(newLines);

            await _context.SaveChangesAsync();

           
        }

        public async Task<PurchaseOrderDto?> GetPurchaseOrderById(long poId)
        {
            try
            {
                var header = await _context.purchaseorderheaders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.po_id == poId);

                if (header == null)
                    return null;

                var details = await (
                    from d in _context.purchaseOrderDetails

                    join i in _context.itemmasters
                        on d.item_id equals i.id
                    join att in _context.itemattributes
                       on i.id equals att.item_id  

                    where d.po_id == poId
                    && att.attribute_name == "part_number"

                    select new PurchaseOrderLineDto
                    {
                        DbId = d.po_detail_id,

                        itemId = d.item_id,

                        partNo = att.attribute_value,

                        sku = i.sku,

                        partName = i.name,

                        orderedQty = d.ordered_qty,

                        receivedQty = d.received_qty,

                        balanceQty = d.balance_qty,

                        unitPrice = d.unit_price,

                        taxPct = d.tax_percent,

                        taxAmount = d.tax_amount,

                        totalPrice = d.line_total
                    })
                    .ToListAsync();

                return new PurchaseOrderDto
                {
                    poId = header.po_id,

                    poNo = header.po_no,

                    PoDate = header.po_date.Date,

                    vendorId = header.vendor_id,

                    buyerId = header.requester_id,

                    warehouseId = header.warehouse_id,

                    expectedDeliveryDate = header.expected_delivery_date,

                    status = header.status,

                    subTotal = header.subtotal,

                    taxTotal = header.tax_amount,

                    discountAmount = header.discount_amount,

                    grandTotal = header.total_amount,

                    remarks = header.remarks,

                    LineItems = details
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}

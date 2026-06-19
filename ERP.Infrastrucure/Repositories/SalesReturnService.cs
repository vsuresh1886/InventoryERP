using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities.SalesReturn;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class SalesReturnService : ISalesReturnService
    {

        private readonly AppDbContext _context;
        private readonly ICodeGeneratorService _codeGeneratorService;
        private readonly IInventoryService _inventoryService;
        public readonly ICurrentUser _currentuser;

        public SalesReturnService(AppDbContext context, ICodeGeneratorService codeGeneratorService ,IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
            _codeGeneratorService = codeGeneratorService;
        }

        public async Task<List<DropdownDto>> FetchInvoiceCust(long id)
        {
            try
            {
                var result = await _context.invoiceHeaders.Where(x=> x.party_id == id).Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.invoice_no
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }




        public async Task<GridDataResponse<SRGridDto>> FetchReturndtls(SRGridRequestsDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query = from sr in _context.sales_Return_Headers
                            join p in _context.customers on sr.customer_id equals p.cust_pk
                            join l in _context.ddlookups on sr.status equals l.id
                            join si in _context.invoiceHeaders on sr.invoice_id equals si.id
                            //join e in _context.employees on q.salesperson equals e.employee_pk
                            //  join il in _context.quotationsLines on q.id equals il.quotation_id
                            select new { sr, p, l,  si };



                if (!string.IsNullOrEmpty(filters.Search))
                {
                    var search = filters.Search.Trim();

                    query = query.Where(x =>
                        EF.Functions.ILike(x.sr.return_no, $"%{search}%") ||
                        EF.Functions.ILike(x.p.customer_name, $"%{search}%") ||
                        EF.Functions.ILike(x.si.invoice_no, $"%{search}%")
                    );
                }

                if (filters.invoiceno != null)
                {
                    query = query.Where(x => x.sr.id == filters.invoiceno);
                }


                if (filters.customer != null && filters.customer.Any())
                {
                    query = query.Where(x => filters.customer.Contains(x.sr.customer_id));
                }

                //if (filters.salesperson != null && filters.salesperson.Any())
                //{
                //    query = query.Where(x => filters.salesperson.Contains(x.sr.salesman));
                //}


                if (!String.IsNullOrWhiteSpace(filters.datefrom))
                {
                    var datefrm = DateTimeOffset.Parse(filters.datefrom).UtcDateTime;

                    DateTime dateto;

                    if (!string.IsNullOrWhiteSpace(filters.dateto))
                    {
                        dateto = DateTimeOffset.Parse(filters.dateto).UtcDateTime;
                    }
                    else
                    {
                        dateto = datefrm;
                    }
                    query = query.Where(x => x.sr.return_date >= datefrm && x.sr.return_date <= dateto);
                }


                var total = await query.CountAsync();

                var data = await query
                            .OrderBy(x => x.sr.id) // IMPORTANT: Always order before Skip
                            .Skip(offset)
                            .Take(limit).ToListAsync();
                var res = data.Select((x, index) => new SRGridDto
                {
                    sno = offset + index + 1,
                    id = x.sr.id,
                    returnno = x.sr.return_no,
                    returndate = DateOnly.FromDateTime( x.sr.return_date),
                    invoiceid = x.sr.invoice_id,
                    invoiceno = x.si.invoice_no,
                    customerid = x.p.company_name,
                    status = x.l.value,
                    returnedqty = x.sr.total_qty,
                    amount = x.sr.net_amount,
                    actions = "",

                }).ToList();

                return new GridDataResponse<SRGridDto>
                {
                    Data = res,
                    Total = total
                };

            }
            catch (Exception ex)
            {
                Console.WriteLine("FetchGridData error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<SalesReturnInvoiceDto> getInvoiceById(long id)
        {
            try
            {
                var result = await _context.invoiceHeaders
                     .AsNoTracking()
                     .Where(x => x.id == id)
                     .Select(y => new
                     {
                         InvoiceId = y.id,
                         CustomerId = y.party_id,
                         SalesPersonId = y.salesperson,

                         SubTotal = y.sub_total,
                         TaxAmount = y.tax_amount,
                         DiscountAmount = y.discount_amount,
                         TotalAmount = y.total_amount,

                         LineItems = _context.invoicelines
                             .Where(z => z.invoice_id == y.id)
                             .Select(litem => new
                             {
                                 InvoiceDetailId = litem.id,
                                 ItemId = litem.item_id,

                                 PartNo = litem.partno,
                                 PartName = litem.description ?? "",

                                 InvoiceQty = litem.quantity,

                                 UnitPrice = litem.unit_price,

                                 Amount = litem.line_total - (litem.unit_price * (litem.tax / 100)),
                                 TaxAmount = litem.unit_price * (litem.tax/100),

                                 TaxPercentage = litem.tax
                             })
                             .ToList()
                     })
                     .FirstOrDefaultAsync();

                                if (result == null)
                                    return null;

                                // Invoice Gross Before Discount
                                var invoiceGross =
                                    result.SubTotal + result.TaxAmount;

                                // Discount Allocation Factor
                                var factor =
                                    invoiceGross == 0
                                        ? 1m
                                        : result.TotalAmount / invoiceGross;

                                var returnedTotal =  await _context.sales_Return_Details
                                                .Where(x => x.sales_invoice_detail_id == id)
                                                .SumAsync(x => (decimal?)x.amount) ?? 0;
                                var returnedQty = await _context.sales_Return_Details
                                                    .Where(x => x.sales_invoice_detail_id == id)
                                                    .SumAsync(x => (decimal?)x.qty) ?? 0;

                                return new SalesReturnInvoiceDto
                                {
                                    InvoiceId = result.InvoiceId,
                                    CustomerId = result.CustomerId,
                                    SalesPersonId = result.SalesPersonId,
                                    InvoiceTotal = result.TotalAmount,
                                    ReturnedTotal = returnedTotal,
                                    BalanceTotal = result.TotalAmount - returnedTotal,
                                    InvoiceQty = result.LineItems.Sum(x => x.InvoiceQty),
                                    ReturnedQty = returnedQty,
                                    BalanceQty = result.LineItems.Sum(x => x.InvoiceQty) - returnedQty,

                                    LineItems = result.LineItems
                                        .Select(litem =>
                                        {
                                            // Line Total Before Discount
                                            var lineGross =
                                                litem.Amount +
                                                litem.TaxAmount;

                                            // Allocate Header Discount
                                            var netLineAmount =
                                                lineGross * factor;

                                            return new SalesReturnInvoiceDetailDto
                                            {
                                                InvoiceDetailId = litem.InvoiceDetailId,

                                                ItemId = litem.ItemId,

                                                PartNo = litem.PartNo,

                                                PartName = litem.PartName,

                                                InvoiceQty = litem.InvoiceQty,

                                                ReturnedQty = 0,

                                                BalanceQty = litem.InvoiceQty,

                                                UnitPrice = litem.UnitPrice,

                                                TaxPercentage = litem.TaxPercentage,

                                                // Rate shown in Return Screen
                                                Rate = Math.Round(
                                                    netLineAmount / litem.InvoiceQty,
                                                    2,
                                                    MidpointRounding.AwayFromZero),

                                                // Full line amount after discount allocation
                                                LineAmount = Math.Round(
                                                    netLineAmount,
                                                    2,
                                                    MidpointRounding.AwayFromZero)
                                            };
                                        })
                                        .ToList()
                                };

                

            }
            catch(Exception ex)
            {
                Console.WriteLine("Fetch invoice detail (return ) error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<SalesReturnDto> getReturnsById(long salesReturnId)
        {

            try
            {

            var header = await _context.sales_Return_Headers.AsNoTracking().FirstOrDefaultAsync(x => x.id == salesReturnId);

            if (header == null)
                return null;

            var details = await (
                from sr in _context.sales_Return_Details

                join invd in _context.invoicelines
                    on sr.sales_invoice_detail_id equals invd.id
                join srh in _context.sales_Return_Headers 
                    on sr.sales_return_id equals srh.id

                where sr.sales_return_id == salesReturnId

                select new SalesReturnDetailDto
                {
                    DbId = sr.id,

                    InvoiceDetailId = sr.sales_invoice_detail_id,

                    ItemId = sr.item_id,

                    partNo = invd.partno,

                    partName = invd.description,

                    InvoiceQty = invd.quantity,

                    ReturnedQty = 0, // calculate below

                    BalanceQty = 0, // calculate below

                    ReturnQty = sr.qty,

                    Rate = sr.rate,

                    Amount = sr.amount
                })
                .ToListAsync();
                foreach (var line in details)
                {
                    var totalReturned =
                                        await (
                                            from d in _context.sales_Return_Details
                                            join h in _context.sales_Return_Headers
                                                on d.sales_return_id equals h.id
                                            where d.sales_invoice_detail_id ==
                                                      line.InvoiceDetailId
                                                  &&
                                                  d.sales_return_id != salesReturnId
                                                  &&
                                                  h.status == StatusIds.Posted
                                            select d.qty
                                        )
                                        .SumAsync(x => (decimal?)x) ?? 0;

                    line.ReturnedQty = totalReturned;

                                line.BalanceQty =
                                    line.InvoiceQty -
                                    totalReturned;
                }

                return new SalesReturnDto
            {
                id = header.id,

                returnNo = header.return_no,

                returnDate = header.return_date,

                customerId = header.customer_id,

                invoiceId = header.invoice_id,

                salesperson = _context.invoiceHeaders.Where(x=>x.id == header.invoice_id).Select(x=>x.salesperson).FirstOrDefault(),

                invoiceTotal = _context.invoiceHeaders.Where(x => x.id == header.invoice_id).Select(x => x.total_amount).FirstOrDefault(),

                returnedTotal = header.net_amount,

                balanceTotal = 0,

                invoiceQty = details.Sum(x => x.InvoiceQty),

                returnedQty = details.Sum(x => x.ReturnedQty),

                balanceQty = details.Sum(x => x.BalanceQty),

                status = header.status,

                returnreason = header.return_reason_id,

                remarks = header.remarks,

                LineItems = details
            };


            }catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<SalesReturnDto> CreateUpdateSaleReturn( SalesReturnDto dto)
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
                    if (item.ReturnQty <= 0)
                        throw new Exception(
                            "Return quantity must be greater than zero.");

                    if (item.ReturnQty > item.BalanceQty)
                        throw new Exception(
                            $"Return quantity exceeds balance quantity for {item.partNo}");
                }


                await ValidateReturnQuantities(
                    dto.id,
                    dto.LineItems);


                dto.returnedQty =
                    dto.LineItems.Sum(x => x.ReturnQty);

                dto.returnedTotal =
                    dto.LineItems.Sum(x => x.Amount);

                SalesReturnHeader header;

                if (dto.id == 0)
                {
                    await CreateSalesReturn(dto);
                }
                else
                {
                    await UpdateSalesReturn(dto);
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

        private async Task CreateSalesReturn( SalesReturnDto dto)
        {
            string returnNo = await _codeGeneratorService.GenerateAsync("SalesReturn");

            var header =
                new SalesReturnHeader
                {
                    return_no = returnNo,
                    return_date = DateTime.SpecifyKind( dto.returnDate,  DateTimeKind.Utc),

                    customer_id = dto.customerId,

                    invoice_id = dto.invoiceId,

                    status = dto.status,

                    return_reason_id = dto.returnreason,

                    total_qty = dto.LineItems.Sum(x=>x.ReturnQty),  

                    remarks = dto.remarks,

                    net_amount = dto.returnedTotal,

                    

                    created_on = DateTime.UtcNow,
                    created_by =  1// (int)_currentuser.UserId
                };

            _context.sales_Return_Headers.Add(header);

            await _context.SaveChangesAsync();

            var details =
                dto.LineItems.Select(x =>
                    new salesreturndetail
                    {
                        sales_return_id = header.id,

                        sales_invoice_detail_id =
                            x.InvoiceDetailId,

                        item_id = x.ItemId,

                        qty =
                            x.ReturnQty,

                        rate = x.Rate,

                        amount = x.Amount,
                        partno = x.partNo,
                        partname = x.partName,
                    });

            _context.sales_Return_Details.AddRange(details);

            await _context.SaveChangesAsync();

            dto.id = header.id;

            if (dto.status == StatusIds.Posted)
            {

                await ValidateReturnQuantities(
                   dto.id,
                   dto.LineItems);



                await PostSalesReturnInventory(
                    header.id,
                    returnNo,
                    dto.LineItems);
            }
        }

        private async Task UpdateSalesReturn(SalesReturnDto dto)
        {
            var header =
                await _context.sales_Return_Headers
                    .FirstOrDefaultAsync(
                        x => x.id == dto.id);

            if (header == null)
                throw new Exception(
                    "Sales Return not found");

            int oldStatus = header.status;
            int newStatus = dto.status;

            if (oldStatus == StatusIds.Posted &&
                newStatus == StatusIds.Posted)
            {
                throw new Exception(
                    "Confirmed sales return cannot be modified.");
            }

            header.status = dto.status;
            header.return_reason_id = dto.returnreason;
            header.remarks = dto.remarks;
            header.net_amount = dto.returnedTotal;

            await _context.SaveChangesAsync();

            var existingLines =
                await _context.sales_Return_Details
                    .Where(x =>
                        x.sales_return_id == dto.id)
                    .ToListAsync();

            _context.sales_Return_Details
                .RemoveRange(existingLines);

            var newLines =
                dto.LineItems.Select(x =>
                    new salesreturndetail
                    {
                        sales_return_id = header.id,

                        sales_invoice_detail_id =
                            x.InvoiceDetailId,

                        item_id = x.ItemId,

                        qty =
                            x.ReturnQty,

                        rate = x.Rate,

                        amount = x.Amount,
                       
                        partno = x.partNo,
                        partname = x.partName,
                    });

            _context.sales_Return_Details.AddRange(newLines);

            await _context.SaveChangesAsync();

            if (oldStatus != StatusIds.Posted &&
                newStatus == StatusIds.Posted)
            {

                await ValidateReturnQuantities(
                   dto.id,
                   dto.LineItems);


                await PostSalesReturnInventory(
                    header.id,
                    header.return_no,
                    dto.LineItems);
            }

            if (oldStatus == StatusIds.Posted &&
                newStatus == StatusIds.Cancelled)
            {
                await ReverseSalesReturnInventory(
                    header.id,
                    header.return_no,
                    dto.LineItems);
            }
        }

        private async Task PostSalesReturnInventory(  long returnId,  string returnNo,  List<SalesReturnDetailDto> lines)
        {
            await _inventoryService
                .PostTransactionAsync(
                    transactionCode: "SALES_RETURN",
                    referenceId: returnId,
                    referenceNo: returnNo,

                    lines: lines.Select(x =>
                        new InventoryPostingLine
                        {
                            ItemId = x.ItemId,
                            Quantity = x.ReturnQty
                        }).ToList(),

                    isStockIn: true
                );
        }

        private async Task ReverseSalesReturnInventory( long returnId, string returnNo, List<SalesReturnDetailDto> lines)
        {
            await _inventoryService
                .PostTransactionAsync(
                    transactionCode:
                        "SALES_RETURN_CANCEL",

                    referenceId: returnId,
                    referenceNo: returnNo,

                    lines: lines.Select(x =>
                        new InventoryPostingLine
                        {
                            ItemId = x.ItemId,
                            Quantity = x.ReturnQty
                        }).ToList(),

                    isStockIn: false
                );
        }

        public static class StatusIds
        {
            public const int Draft = 16;

            public const int Posted = 17;

            public const int Cancelled = 18;
        }


        private async Task ValidateReturnQuantities( long currentSalesReturnId,  List<SalesReturnDetailDto> items)
        {
            foreach (var item in items)
            {
                var invoiceLine =
                    await _context.invoicelines
                        .Where(x =>
                            x.id == item.InvoiceDetailId)
                        .FirstOrDefaultAsync();

                if (invoiceLine == null)
                    throw new Exception(
                        $"Invoice line not found.");

                var confirmedReturnedQty =
                    await (
                        from d in _context.sales_Return_Details
                        join h in _context.sales_Return_Headers
                            on d.sales_return_id equals h.id
                        where d.sales_invoice_detail_id
                                == item.InvoiceDetailId
                            &&
                            d.sales_return_id
                                != currentSalesReturnId
                            &&
                            h.status == StatusIds.Posted
                        select d.qty
                    )
                    .SumAsync(x => (decimal?)x) ?? 0;

                var availableQty =
                    invoiceLine.quantity -
                    confirmedReturnedQty;

                if (item.ReturnQty > availableQty)
                {
                    throw new Exception(
                        $"{item.partNo} exceeds available quantity. Available: {availableQty}");
                }
            }
        }

    }
}

using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using ERP.Application.DTOs;
using ERP.Application.DTOs.GoodsReceiptNote;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Purchase;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Domain.Entities.GoodsReceiptNote;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.PurchaseOrder;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories.CodeGeneratorservice;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class GRNService:IGRNService
    {
        private readonly AppDbContext _context;
        private readonly ICodeGeneratorService _codeGeneratorService;
        private readonly IInventoryService _inventoryService;

        public GRNService(AppDbContext context,ICodeGeneratorService code, IInventoryService inventoryService   )
        {
            _context = context;
            _codeGeneratorService = code;
            _inventoryService = inventoryService;
        }
        public async  Task<List<DropdownDto>> getGRNids()
        {
            try
            {
                var result = await _context.goodsreceiptheaders.Select(x => new DropdownDto
                {
                    Id = (int)x.id,
                    Name = x.grn_no
                }).ToListAsync();

                return result;
            }
            catch(Exception ex)
            {
                throw;
            }
        }


        public async Task<GridDataResponse<GoodsReceiptListDto>> GetGRNGrid(GRNGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                // Base Query joining core tables, extensions, and grouping details
                var query =
                    from grn in _context.goodsreceiptheaders

                    join po in _context.purchaseorderheaders
                        on grn.po_id equals po.po_id into poJoin
                    from poHeader in poJoin.DefaultIfEmpty() // Left Join if GRN can exist without a PO context

                    join vendor in _context.customers
                        on grn.vendor_id equals vendor.cust_pk // Maps to your Vendor/Party table

                    join emp in _context.employees
                        on grn.received_by equals emp.employee_pk into empJoin
                    from employee in empJoin.DefaultIfEmpty() // Left Join for user accountability

                    join statusDdl in _context.ddlookups
                        on grn.status equals statusDdl.id

                    // Grouping details for row aggregations
                    join d in _context.goodsreceiptdetails
                        on grn.id equals d.grn_id into details
                   where statusDdl.lookup_type == "GRN_STATUS" 
                   

                    select new
                    {
                        GRN = grn,
                        PO = poHeader,
                        Vendor = vendor,
                        Employee = employee,
                        Status = statusDdl,
                        TotalItemsCount = details.Count(),

                        // Aggregating decimal fields securely with null checks
                        TotalQtyReceived = details.Sum(x => (decimal?)x.received_qty) ?? 0
                    };

                #region Dynamic Filtering Operations

                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    var search = filters.Search.Trim();
                    query = query.Where(x =>
                        EF.Functions.ILike(x.GRN.grn_no, $"%{search}%") ||
                        EF.Functions.ILike(x.PO.po_no, $"%{search}%") ||
                        EF.Functions.ILike(x.Vendor.company_name, $"%{search}%"));
                }

                // Using invoiceNo filter as a dynamic link parameter for explicit ID matching (e.g., specific PO ID)
                if (filters.invoiceno.HasValue)
                {
                    query = query.Where(x => x.GRN.id == filters.invoiceno.Value);
                }

                if (filters.vendor != null && filters.vendor.Any())
                {
                    query = query.Where(x => filters.vendor.Contains(x.GRN.vendor_id));
                }

                if (filters.salesperson != null && filters.salesperson.Any())
                {
                    // Maps the filter property to check against your received_by field
                    query = query.Where(x => x.GRN.received_by == null && filters.salesperson.Contains(x.GRN.received_by));
                }

                if (filters.status.HasValue)
                {
                    query = query.Where(x => x.GRN.status == filters.status.Value);
                }

                if (!string.IsNullOrWhiteSpace(filters.datefrom))
                {
                    var dateFrom = DateTimeOffset.Parse(filters.datefrom).UtcDateTime.Date;
                    var dateTo = !string.IsNullOrWhiteSpace(filters.dateto)
                        ? DateTimeOffset.Parse(filters.dateto).UtcDateTime.Date
                        : dateFrom;

                    query = query.Where(x => x.GRN.grn_date.Date >= dateFrom && x.GRN.grn_date.Date <= dateTo);
                }

                #endregion

                // Execute total element count evaluation
                var total = await query.CountAsync();

                // Paginate and load the structural flat objects array to application memory
                var data = await query
                    .OrderByDescending(x => x.GRN.grn_date)
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

                // Project backend models precisely into target DTO configurations
                var result = data.Select((x, index) => new GoodsReceiptListDto
                {
                    sno = offset + index + 1,
                    id = x.GRN.id,
                    grnNo = x.GRN.grn_no,
                    grnDate = DateOnly.Parse(x.GRN.grn_date.ToString("dd-MMM-yyyy")),
                    challanNo = x.GRN.vendor_reference_no,
                    challanDate = x.GRN.vendor_reference_date !=null
                        ? DateOnly.Parse(x.GRN.vendor_reference_date.ToString("dd-MMM-yyyy"))
                        : null,
                    

                    // Aggregation assignments
                    totalQtyRec = x.TotalQtyReceived,
                    grandTotal = x.GRN.grand_total ,

                    // Linked metadata
                    poNo = x.PO?.po_no ?? "Direct Inward",
                    vendorName = x.Vendor.company_name,
                    //receivedByName = x.Employee != null
                    //    ? $"{x.Employee.first_name} {x.Employee.last_name}".Trim()
                    //    : "Not Assigned",
                    status = x.Status.value
                })
                .ToList();

                return new GridDataResponse<GoodsReceiptListDto>
                {
                    Data = result,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetGoodsReceiptGrid Error : " + ex.Message);
                throw;
            }
        }

        public async Task<GRNPOlistDto> GetPOForGRNByIdAsync(long poId)
        {
            try
            {
                // 1. Fetch the PO Header and flat line item structures in a single clean query
                var poResult = await _context.purchaseorderheaders
                    .AsNoTracking()
                    .Where(x => x.po_id == poId)
                    .Select(h => new
                    {
                        POId = h.po_id,
                        SubTotal = h.subtotal,
                        TaxAmount = h.tax_amount,
                        DiscountAmount = h.discount_amount,
                        GrandTotal = h.total_amount,

                        Lines = _context.purchaseOrderDetails
                            .Where(d => d.po_id == h.po_id)
                            .Select(d => new
                            {
                                PoDetailId = d.po_detail_id,
                                ItemId = d.item_id,
                                PartNo = _context.itemattributes.Where(x=>x.item_id == d.item_id && x.attribute_name== "part_number").Select(z=>z.attribute_value).FirstOrDefault(),
                                PartName = _context.itemmasters.Where(x=>x.id== d.item_id).Select(z=>z.name).FirstOrDefault(), 
                                OrderedQty = d.ordered_qty,
                                PoLineReceivedQty = d.received_qty, // The line-level fallback counter from PO Details
                                UnitPrice = d.unit_price,
                                TaxPct = d.tax_percent
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (poResult == null)
                    return null;

                // 2. Collect all PO detail line primary keys to evaluate operational histories
                var poDetailIds = poResult.Lines.Select(l => l.PoDetailId).ToList();

                // 3. Gather approved/persisted quantities inside the Goods Receipt details table
                var grnHistoryMap = await _context.goodsreceiptdetails
                    .Where(gd => poDetailIds.Contains(gd.po_detail_id))
                    .GroupBy(gd => gd.po_detail_id)
                    .Select(g => new
                    {
                        PoDetailId = g.Key,
                        TotalReceivedQty = g.Sum(x => (decimal?)x.received_qty) ?? 0m
                    })
                    .ToDictionaryAsync(k => k.PoDetailId, v => v.TotalReceivedQty);

                // 4. Transform datasets into your required presentation schema configurations
                return new GRNPOlistDto
                {
                    id = poResult.POId,
                    subtotal = poResult.SubTotal,
                    taxAmount = poResult.TaxAmount,
                    discountAmount = poResult.DiscountAmount,
                    grandTotal = poResult.GrandTotal,

                    lineItems = poResult.Lines.Select(line =>
                    {
                        // Look for an entry in the Goods Receipt details history map
                        bool hasGrnHistory = grnHistoryMap.TryGetValue(line.PoDetailId, out var grnReceivedQty);

                        // 🔥 CONDITIONAL EVALUATION:
                        // Use the database sum if history exists; otherwise, fall back to the purchase detail line column
                        decimal previouslyReceived = hasGrnHistory ? grnReceivedQty : line.PoLineReceivedQty;

                        // Compute open available intake bounds
                        var openBalance = line.OrderedQty - previouslyReceived;
                        if (openBalance < 0m) openBalance = 0m; // Protects matrix states against stray calculations

                        return new POSLineItems
                        {
                            grnDetailId = 0, // 0 flags a completely brand new grid entry layer in UI
                            poDetailId = line.PoDetailId,
                            itemId = line.ItemId,
                            partNo = line.PartNo,
                            partName = line.PartName,
                            orderedQty = line.OrderedQty,
                            previouslyReceivedQty = previouslyReceived,
                            balanceQty = openBalance,
                            receivedQty = 0m,
                            unitPrice = line.UnitPrice,
                            taxPct = line.TaxPct,
                            taxAmount = 0m,
                            lineTotal = 0m,
                            remarks = string.Empty
                        };
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fetch PO Details conversion execution crash: {ex.Message}");
                throw;
            }
        }


       public async  Task<GoodsReceiptDto> GetGRNById(long id)
        {


            var header = await _context.goodsreceiptheaders.AsNoTracking().Where(x => x.id == id).FirstOrDefaultAsync();

            if (header == null)
                return null;

            var details = await (from d in _context.goodsreceiptdetails
                                 join i in _context.itemmasters
                                   on d.item_id equals i.id
                                 join att in _context.itemattributes
                                    on i.id equals att.item_id
                                 where d.grn_id == id
                                 && att.attribute_name == "part_number"

                                 select new GoodsReceiptLineItemDto
                                 {
                                     grnDetailId = d.id,
                                     poDetailId = d.po_detail_id,
                                     itemId = d.item_id,
                                     partName = i.name,
                                     partNo = att.attribute_value,
                                     orderedQty = d.ordered_qty,
                                     receivedQty= d.received_qty,
                                     previouslyReceivedQty = d.previously_received_qty,
                                     balanceQty = d.balance_qty,
                                     unitPrice = d.unit_price,
                                     taxAmount = d.tax_amount,
                                     taxPct = d.tax_pct,
                                     lineTotal = d.line_total,

                                 }).ToListAsync();

            return  new GoodsReceiptDto
                               {
                                   id = header.id,
                                   grnNo = header.grn_no,
                                   grnDate = header.grn_date,
                                   poId = header.po_id,
                                   poNo= header.po_no,
                                   vendorId = header.vendor_id,
                                   branchId = header.branch_id,
                                   warehouseId = header.warehouse_id,
                                   vendorReferenceDate = header.vendor_reference_date,
                                   vendorReferenceNo = header.vendor_reference_no,
                                   vehicleNo = header.vehicle_no,
                                   remarks=header.remarks,
                                   status = header.status,
                                   receivedBy = header.received_by,
                                   subTotal = header.sub_total,
                                   taxAmount = header.tax_amount,
                                   grandTotal = header.grand_total,

                                   lineItems = details

                               };
                               
        }



        public async Task<GoodsReceiptDto> CreateupdateGRN(GoodsReceiptDto dto)
        {
            using var transaction =
               await _context.Database.BeginTransactionAsync();

            try
            {
                if (dto.lineItems == null ||
                    !dto.lineItems.Any())
                {
                    throw new Exception(
                        "Goods Receipt must contain items.");
                }



                foreach (var item in dto.lineItems)
                {
                    if (item.orderedQty <= 0)
                        throw new Exception(
                            "Ordered quantity must be greater than zero.");

                    if (item.receivedQty > item.orderedQty)
                        throw new Exception(
                            $"Received quantity exceeds balance quantity for {item.partNo}");
                }


                goodsreceiptheader header;

                if (dto.id == 0)
                {
                    await CreateGRN(dto);
                }
                else
                {
                    await UpdateGRN(dto);
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

        private async Task CreateGRN(GoodsReceiptDto dto)
        {
            string GRNNo = await _codeGeneratorService.GenerateAsync("GoodsReceipt");

            var header =
                new goodsreceiptheader
                {
                    grn_no = GRNNo,
                    grn_date = DateTime.SpecifyKind(dto.grnDate, DateTimeKind.Utc),

                    vendor_id = dto.vendorId,
                    po_id = dto.poId,
                    po_no = dto.poNo,
                    vendor_reference_no = dto.vendorReferenceNo,
                    vendor_reference_date = DateTime.SpecifyKind(dto.vendorReferenceDate, DateTimeKind.Utc),
                    vehicle_no = dto.vehicleNo,
                    branch_id = dto.branchId,
                    warehouse_id = dto.warehouseId,
                    total_qty_received = dto.totalQtyReceived,
                    sub_total = dto.subTotal,
                    grand_total = dto.grandTotal,
                    received_by = dto.receivedBy,
                    status = dto.status,
                    remarks = dto.remarks,
                    created_at = DateTime.UtcNow,
                    created_by = 1// (int)_currentuser.UserId
                };

            _context.goodsreceiptheaders.Add(header);

            await _context.SaveChangesAsync();

            var details =
                dto.lineItems.Select(x =>
                    new goodsreceiptdetail
                    {
                        grn_id = header.id,
                        item_id = x.itemId,
                        po_detail_id = x.poDetailId,
                        ordered_qty = x.orderedQty,
                        previously_received_qty = x.previouslyReceivedQty,
                        received_qty = x.receivedQty,
                        unit_price = x.unitPrice,
                        tax_pct = x.taxPct,
                        tax_amount = (x.unitPrice * x.orderedQty) * (x.taxPct / 100),
                        line_total = x.lineTotal,
                        remarks=" ",
                    });

            _context.goodsreceiptdetails.AddRange(details);

            await _context.SaveChangesAsync();

        }

        public static class StatusIds
        {
            public const int Draft = 38;

            public const int Confirm = 39;

            public const int Cancelled = 40;

           
        }
        private async Task UpdateGRN(GoodsReceiptDto dto)
        {
            // 1. Fetch Header and include existing line items in a single SQL JOIN query
            var header = await _context.goodsreceiptheaders
                .Include(x => x.goodsreceiptdetails)
                .FirstOrDefaultAsync(x => x.id == dto.id);

            if (header == null)
                throw new Exception("Goods Receipt record not found.");

            int currentDbStatus = header.status;
            int incomingStatus = dto.status;

            // 🛑 STATE SAFEGUARDS
            // Rule A: If it is already Confirmed in the DB, it is structurally locked.
            if (currentDbStatus == StatusIds.Confirm)
            {
                throw new Exception("This Goods Receipt has already been confirmed and cannot be modified or re-posted.");
            }

            // Rule B: If it is already Cancelled in the DB, lock it permanently.
            if (currentDbStatus == StatusIds.Cancelled)
            {
                throw new Exception("A cancelled Goods Receipt cannot be modified or confirmed.");
            }

            // Rule C: Prevent moving directly from a Confirmed rule back to Cancelled without a Return workflow
            if (currentDbStatus == StatusIds.Confirm && incomingStatus == StatusIds.Cancelled)
            {
                throw new Exception("Confirmed Goods Receipts cannot be cancelled. Please raise a Purchase Return instead.");
            }

            // 2. Map updated Header properties
            header.vendor_id = dto.vendorId;
            header.po_id = dto.poId;
            header.po_no = dto.poNo;
            header.vendor_reference_no = dto.vendorReferenceNo;
            header.vendor_reference_date = dto.vendorReferenceDate;
            header.vehicle_no = dto.vehicleNo;
            header.branch_id = dto.branchId;
            header.warehouse_id = dto.warehouseId;
            header.total_qty_received = dto.totalQtyReceived;
            header.sub_total = dto.subTotal;
            header.grand_total = dto.grandTotal;
            header.remarks = dto.remarks;
            header.received_by = dto.receivedBy;
            header.status = dto.status; // Assign the incoming status

            // 3. Handle Status Changes Transitions
            if (dto.status == StatusIds.Confirm)
            {
                header.confirmed_by = 1; // Map context user here
                header.confirmed_at = DateTime.UtcNow;

                // 🚀 POST INVENTORY: Increase warehouse on-hand stock balances
                await PostGRNInventoryAsync(header.id, header.grn_no, dto.lineItems);
            }

            // 4. Line Items Syncing Logic
            if (dto.status == StatusIds.Cancelled)
            {
                // Optional: Zero out amounts upon explicit cancellation while preserving historical detail entries
                foreach (var line in header.goodsreceiptdetails)
                {
                    line.received_qty = 0;
                    line.line_total = 0;
                    line.tax_amount = 0;
                }
                header.total_qty_received = 0;
                header.grand_total = 0;
                header.sub_total = 0;
                header.tax_amount = 0;
            }
            else
            {
                // In-Memory Syncing Loop (DELETE / UPDATE / INSERT)
                var existingLines = header.goodsreceiptdetails.ToList();

                var incomingItemIds = dto.lineItems
                    .Select(x => x.grnDetailId)
                    .ToList();

                // A. DELETE: Clear details removed by user from the frontend payload
                var linesToDelete = existingLines
                    .Where(dbLine => !incomingItemIds.Contains(dbLine.id))
                    .ToList();

                if (linesToDelete.Any())
                {
                    _context.goodsreceiptdetails.RemoveRange(linesToDelete);
                }

                // B. UPDATE or INSERT: Process incoming payload lines
                foreach (var itemDto in dto.lineItems)
                {
                    goodsreceiptdetail? existingLine = null;
                    if (itemDto.grnDetailId > 0)
                    {
                        existingLine = existingLines.FirstOrDefault(x => x.id == itemDto.grnDetailId);
                    }

                    if (existingLine != null)
                    {
                        // UPDATE existing record properties in place
                        existingLine.received_qty = itemDto.receivedQty;
                        existingLine.ordered_qty = itemDto.orderedQty;
                        existingLine.previously_received_qty = itemDto.previouslyReceivedQty;
                        existingLine.unit_price = itemDto.unitPrice;
                        existingLine.tax_pct = itemDto.taxPct;
                        existingLine.tax_amount = (itemDto.unitPrice * itemDto.orderedQty) * (itemDto.taxPct / 100);
                        existingLine.line_total = itemDto.lineTotal;
                        existingLine.remarks = itemDto.remarks ?? " ";
                    }
                    else
                    {
                        // INSERT brand new item added during update session
                        var newLine = new goodsreceiptdetail
                        {
                            grn_id = header.id,
                            item_id = itemDto.itemId,
                            po_detail_id = itemDto.poDetailId,
                            ordered_qty = itemDto.orderedQty,
                            previously_received_qty = itemDto.previouslyReceivedQty,
                            received_qty = itemDto.receivedQty,
                            unit_price = itemDto.unitPrice,
                            tax_pct = itemDto.taxPct,
                            tax_amount = (itemDto.unitPrice * itemDto.orderedQty) * (itemDto.taxPct / 100),
                            line_total = itemDto.lineTotal,
                            remarks = itemDto.remarks ?? " "
                        };
                        _context.goodsreceiptdetails.Add(newLine);
                    }
                }
            }

            // 5. Single atomic transaction commit to PostgreSQL database context
            await _context.SaveChangesAsync();
        }

        private async Task PostGRNInventoryAsync(long grnId, string grnNo, List<GoodsReceiptLineItemDto> lines)
        {
      
            await _inventoryService
                .PostTransactionAsync(
                    transactionCode: "GOODS_RECEIPT",
                    referenceId: grnId,
                    referenceNo: grnNo,

                    lines: lines.Select(x =>
                        new InventoryPostingLine
                        {
                            ItemId = x.itemId,
                            Quantity = x.receivedQty,
                        }).ToList(),

                    isStockIn: true
                );
       
            }


    }
}

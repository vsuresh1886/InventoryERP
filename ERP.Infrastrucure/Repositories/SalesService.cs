using ERP.Application.DTOs;
using ERP.Application.DTOs.Quotation;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.Quotation;
using ERP.Domain.Entities.SalesInvoice;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using Microsoft.Data.SqlClient.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public  class SalesService : ISalesService
    {
        private readonly AppDbContext _context;
        private readonly ICodeGeneratorService _codeGeneratorservice;
        public readonly ICurrentUser _currentuser;
        public SalesService(AppDbContext context, ICodeGeneratorService codeGeneratorservice, ICurrentUser currentUser)
        {
            _context = context;
            _codeGeneratorservice = codeGeneratorservice;
            _currentuser = currentUser;
        }

        public async Task<List<DropdownDto>> FetchInvoiceids()
        {
            try
            {
                var result = await _context.invoiceHeaders.Select(x => new DropdownDto
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

        public async Task<GridDataResponse<InvoiceGridDto>> FetchInvoices(InvoiceGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query = from q in _context.invoiceHeaders
                            join p in _context.customers on q.party_id equals p.cust_pk
                            join e in _context.employees on q.salesperson equals e.employee_pk
                            join s in _context.ddlookups on q.status equals s.id
                            //  join il in _context.quotationsLines on q.id equals il.quotation_id
                            select new { q, p, e, s };



                if (!string.IsNullOrEmpty(filters.Search))
                {
                    var search = filters.Search.Trim();

                    query = query.Where(x =>
                        EF.Functions.ILike(x.q.invoice_no, $"%{search}%") ||
                        EF.Functions.ILike(x.p.customer_name, $"%{search}%") ||
                        EF.Functions.ILike(x.e.first_name, $"%{search}%")
                    );
                }

                if (filters.invoiceno != null)
                {
                    query = query.Where(x => x.q.id == filters.invoiceno);
                }


                if (filters.customer != null && filters.customer.Any())
                {
                    query = query.Where(x => filters.customer.Contains(x.q.party_id));
                }

                if (filters.salesperson != null && filters.salesperson.Any())
                {
                    query = query.Where(x => filters.salesperson.Contains(x.q.salesperson));
                }


                if (!String.IsNullOrWhiteSpace(filters.datefrom))
                {
                    var datefrm = DateTimeOffset.Parse(filters.datefrom).UtcDateTime;

                    DateTime dateto;

                    if (!string.IsNullOrWhiteSpace(filters.dateto))
                    {
                        dateto = DateTimeOffset.Parse(filters.dateto).UtcDateTime.AddDays(1);
                    }
                    else
                    {
                        dateto = datefrm;
                    }
                    query = query.Where(x => x.q.invoice_date >= datefrm && x.q.invoice_date <= dateto);
                }


                var total = await query.CountAsync();

                var data = await query
                            .OrderBy(x => x.q.id) // IMPORTANT: Always order before Skip
                            .Skip(offset)
                            .Take(limit).ToListAsync();
                var res = data.Select((x, index) => new InvoiceGridDto
                {
                    sno = offset + index + 1,
                    id = x.q.id,
                    invoiceno = x.q.invoice_no,
                    quotationid=x.q.quotation_id,
                    quotationno = _context.quotations.Where(y=>y.id == x.q.quotation_id).Select(z=>z.quotation_no).FirstOrDefault(),
                    customerid = x.p.company_name,
                    salesperson = x.e.first_name + ' ' + x.e.last_name,
                    validfrom = x.q.invoice_date,
                    //validto = x.q.invoice.AddDays(x.q.validity),
                    status = x.s.value,
                    totalItems = _context.invoicelines.Count(y => y.invoice_id == x.q.id),
                    amount = x.q.total_amount,
                    actions = "",

                }).ToList();

                return new GridDataResponse<InvoiceGridDto>
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
        public async Task<InvoiceDto> getInvoiceById(long id)
        {
            try
            {
                var result = await _context.invoiceHeaders.AsNoTracking().Where(x => x.id == id).
                    Select(y => new InvoiceDto
                    {
                        Id = y.id,
                        invoiceNo = y.invoice_no,
                        invoiceDate = y.invoice_date,
                        ValidUntil = y.invoice_date,
                        CustomerId = y.party_id,
                        Salesperson = y.salesperson,
                        Status = y.status ,
                        VatTotal = y.tax_amount,
                        Subtotal = y.sub_total,
                        GrandTotal = y.total_amount,
                        Discount = y.discount_amount,
                        Remarks = y.remarks,
                        LineItems = _context.invoicelines.Where(z => z.invoice_id == y.id).Select(litem => new InvoiceLineItemDto
                        {
                            dbId = litem.id,
                            invoice_id = litem.invoice_id,
                            ItemId = litem.item_id,
                            Quantity = litem.quantity,
                            UnitPrice = litem.unit_price,
                            VatPct = litem.tax,
                            TotalPrice = litem.line_total,

                        }).ToList()
                    }).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fetch quotation by id  error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<InvoiceDto> CreateUpdateInvoice(InvoiceDto InvDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                InvoiceHeader Invhead;
                QuotationLines Invlineitem;

                //validate and recalculate the totals, tax amount ..etc

                if (InvDto.LineItems == null || !InvDto.LineItems.Any())
                    throw new Exception("Invoice must have at least one line item.");

                if (InvDto.LineItems.Any(x => x.Quantity <= 0))
                    throw new Exception("Invoice must be greater than zero.");

                if (InvDto.LineItems.Any(x => x.UnitPrice < 0))
                    throw new Exception("Invalid unit price.");


                foreach (var item in InvDto.LineItems)
                {
                    var baseAmount = item.Quantity * item.UnitPrice;
                    var vat = baseAmount * item.VatPct / 100;

                    item.TotalPrice = baseAmount + vat;
                }

                InvDto.Subtotal = InvDto.LineItems.Sum(x => x.Quantity * x.UnitPrice);
                InvDto.VatTotal = InvDto.LineItems.Sum(x => (x.Quantity * x.UnitPrice * x.VatPct) / 100);
                InvDto.GrandTotal = InvDto.Subtotal + InvDto.VatTotal - InvDto.Discount;
                //end


                if (InvDto.Id == 0)
                {
                    string quotno;
                    do
                    {
                        quotno = await _codeGeneratorservice.GenerateAsync("Invoice");
                    }
                    while (await _context.invoiceHeaders.AnyAsync(x => x.invoice_no == quotno));

                    InvDto.invoiceNo = quotno;


                    Invhead = new InvoiceHeader
                    {
                        invoice_no = quotno,
                        invoice_date = DateTime.SpecifyKind(InvDto.invoiceDate, DateTimeKind.Utc),
                        quotation_id = InvDto.quotationid ?? 0,
                        party_id = InvDto.CustomerId,
                        salesperson = InvDto.Salesperson,
                        validity = 30,
                        status = (int)InvDto.Status,
                        sub_total = InvDto.Subtotal,
                        tax_amount = InvDto.VatTotal,
                        total_amount = InvDto.GrandTotal,
                        discount_amount = InvDto.Discount,
                        balance_amount = InvDto.GrandTotal,
                        remarks = InvDto.Remarks,
                        created_at = DateTime.UtcNow,
                        created_by = (int)_currentuser.UserId,
                        due_date = InvDto.ValidUntil != null
                            ? DateTime.SpecifyKind(InvDto.ValidUntil, DateTimeKind.Utc)
                            : DateTime.UtcNow
                    };
                    _context.invoiceHeaders.Add(Invhead);
                    await _context.SaveChangesAsync();


                    var lineitems = InvDto.LineItems.Select(item => new InvoiceLines
                    {
                        invoice_id = Invhead.id,
                        item_id = item.ItemId,
                        partno = item.PartNo,
                        sku = item.Sku,
                        description = item.PartName,
                        quantity = item.Quantity,
                        unit_price = item.UnitPrice,
                        tax = item.VatPct,
                        line_total = item.TotalPrice,
                    }).ToList();

                    _context.invoicelines.AddRange(lineitems);

                    await _context.SaveChangesAsync();

                    var oldStatus = StatusIds.Draft; // assume draft (no previous state)
                    var newStatus = Invhead.status; //during creation
                   //check the stock availablity before update the transaction
                    if (newStatus == StatusIds.Confirm)
                    {
                        await ValidateStockAvailability(
                                   0, // no previous invoice
                                   0,
                                   InvDto.LineItems!
                               );
                    }


                    await HandleInventoryOnInvoiceChange(
                        Invhead.id,
                        Invhead.invoice_no,
                        oldStatus,
                        newStatus,
                        InvDto.LineItems!
                    );


                }
                else //update the quotation
                {
                    Invhead = await _context.invoiceHeaders.Where(x => x.id == (long)InvDto.Id && x.invoice_no == InvDto.invoiceNo).FirstOrDefaultAsync();

                    if (Invhead == null)
                        throw new Exception("Invalid Invoice");
                    var oldStatus = Invhead.status;
                    var newStatus = InvDto.Status;
                    Invhead.quotation_id = InvDto.quotationid ?? 0;
                    Invhead.party_id = InvDto.CustomerId;
                    Invhead.salesperson = InvDto.Salesperson;
                    Invhead.validity = 30;
                    Invhead.status = (int)newStatus;//InvDto.Status == "Draft" ? 1 : InvDto.Status == "Confirm" ? 2 : 3;
                    Invhead.sub_total = InvDto.Subtotal;
                    Invhead.tax_amount = InvDto.VatTotal;
                    Invhead.total_amount = InvDto.GrandTotal;
                    Invhead.discount_amount = InvDto.Discount;
                    Invhead.balance_amount = InvDto.GrandTotal;
                    Invhead.remarks = InvDto.Remarks;
                    Invhead.updated_at = DateTime.UtcNow;
                    Invhead.updated_by = (int)_currentuser.UserId;
                    Invhead.due_date = InvDto.ValidUntil != null ? DateTime.SpecifyKind(InvDto.ValidUntil, DateTimeKind.Utc): DateTime.UtcNow;
                    await _context.SaveChangesAsync();


                    var exisitngitems = await _context.invoicelines.Where(x => x.invoice_id == InvDto.Id).ToListAsync();


                    if (exisitngitems == null)
                        throw new Exception("Items not found");

                    var itemstoDelete = exisitngitems.Where(dbitem => !InvDto.LineItems.Any(d => d.dbId == dbitem.id)).ToList();
                    if (itemstoDelete != null)
                    { _context.invoicelines.RemoveRange(itemstoDelete); }


                    foreach (var dtoItem in InvDto.LineItems)
                    {
                        var existingItem = exisitngitems
                            .FirstOrDefault(x => x.id == dtoItem.dbId);

                        if (existingItem != null)
                        {
                            //  UPDATE (A)
                            existingItem.item_id = dtoItem.ItemId;
                            existingItem.quantity = dtoItem.Quantity;
                            existingItem.unit_price = dtoItem.UnitPrice;
                            existingItem.tax = dtoItem.VatPct;

                            var baseAmount = dtoItem.Quantity * dtoItem.UnitPrice;
                            existingItem.line_total = baseAmount + (baseAmount * dtoItem.VatPct / 100);

                            _context.invoicelines.Update(existingItem);
                        }
                        else
                        {
                            //  INSERT (D, E, F)
                            var newItem = new InvoiceLines
                            {
                                invoice_id = (int)InvDto.Id,
                                partno = dtoItem.PartNo,
                                sku = dtoItem.Sku,
                                description = dtoItem.PartName,
                                item_id = dtoItem.ItemId,
                                quantity = dtoItem.Quantity,
                                unit_price = dtoItem.UnitPrice,
                                tax = dtoItem.VatPct,
                                line_total = (dtoItem.Quantity * dtoItem.UnitPrice)
                                           + (dtoItem.Quantity * dtoItem.UnitPrice * dtoItem.VatPct / 100)
                            };

                            _context.invoicelines.Add(newItem);
                        }
                    }

                    if (newStatus == StatusIds.Confirm)
                    {
                        await ValidateStockAvailability(
                            Invhead.id,
                            oldStatus,
                            InvDto.LineItems!
                        );
                    }
                    await HandleInventoryOnInvoiceChange(
                            Invhead.id,
                            Invhead.invoice_no,
                            oldStatus,
                            (int)newStatus,
                            InvDto.LineItems!
                        );


                    await _context.SaveChangesAsync();
                }  // call the transaction function to update the stock. 
                
                    await transaction.CommitAsync();

                return InvDto;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Creat Update quotation error: " + ex.Message);
                throw ex;
            }
        }


        // transaction table -- Stock update 
        private async Task HandleInventoryOnInvoiceChange(
                                        long invoiceId,
                                        string invoiceNo,
                                        int oldStatus,
                                        int newStatus,
                                        List<InvoiceLineItemDto> items)
            {
            // Draft -> Confirm
            if (oldStatus != StatusIds.Confirm && newStatus == StatusIds.Confirm)
            {
                await PostSaleTransactions(invoiceId, invoiceNo, items);
                return;
            }

            // Confirm -> Confirm (edit)
            if (oldStatus == StatusIds.Confirm && newStatus == StatusIds.Confirm)
            {
                await ReverseSaleTransactions(invoiceId, "Reversal due to edit");
                await PostSaleTransactions(invoiceId, invoiceNo, items);
                return;
            }

            // Confirm -> Cancel
            if (oldStatus == StatusIds.Confirm && newStatus == StatusIds.Cancel)
            {
                await ReverseSaleTransactions(invoiceId, "Reversal due to cancel");
                return;
            }

            // Draft -> Draft / Draft -> Cancel → do nothing
        }


        private async Task PostSaleTransactions( long invoiceId, string invoiceNo, List<InvoiceLineItemDto> items)
        {
            var txns = items.Select(item => new InventoryTransaction
            {
                item_id = item.ItemId,
                transaction_type = "OUT",
                quantity = item.Quantity,
                unit_price = item.UnitPrice,
                total_amount = item.TotalPrice,
                transaction_date = DateTime.UtcNow,

                reference_type = "SALE",
                reference_id = invoiceId,

                quantity_in = 0,
                quantity_out = item.Quantity,

                remarks = $"Invoice {invoiceNo}",
                created_at = DateTime.UtcNow,
                created_by = (int)_currentuser.UserId
            });

            _context.inventoryTransactions.AddRange(txns);
            await _context.SaveChangesAsync();
        }

        private async Task ReverseSaleTransactions(long invoiceId, string reason)
        {
            var existing = await _context.inventoryTransactions
                .Where(x => x.reference_type == "SALE" && x.reference_id == invoiceId)
                .ToListAsync();

            if (!existing.Any())
                return;

            var reversals = existing.Select(txn => new InventoryTransaction
            {
                item_id = txn.item_id,
                transaction_type = "IN",
                quantity = txn.quantity,
                unit_price = txn.unit_price,
                total_amount = txn.total_amount,
                transaction_date = DateTime.UtcNow,

                quantity_in = txn.quantity,
                quantity_out = 0,

                reference_type = "SALE_RETURN",
                reference_id = invoiceId,

                remarks = reason,
                created_at = DateTime.UtcNow,
                created_by = (int)_currentuser.UserId
            });

            _context.inventoryTransactions.AddRange(reversals);
            await _context.SaveChangesAsync();
        }

        private async Task ValidateStockAvailability(
                                             long invoiceId,
                                             int oldStatus,
                                             List<InvoiceLineItemDto> items)
        {
            var groupedItems = items
                .GroupBy(x => x.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key,
                    RequiredQty = g.Sum(x => x.Quantity)
                })
                .ToList();

            foreach (var item in groupedItems)
            {
              

                var inQty = await _context.inventoryTransactions
                        .Where(t => t.item_id == item.ItemId && t.transaction_type == "IN")
                        .SumAsync(t => t.quantity ?? 0);


                var availableStock = await _context.inventoryTransactions
                         .Where(t => t.item_id == item.ItemId)
                         .SumAsync(t =>
                             t.transaction_type == "IN"
                                 ? (t.quantity ?? 0)
                                 : t.transaction_type == "OUT"
                                     ? -(t.quantity ?? 0)
                                     : 0);

                // 🔥 If already confirmed before → add back old qty (because we will reverse)
                if (oldStatus == StatusIds.Confirm)
                {
                    var oldQty = await _context.inventoryTransactions
                        .Where(t => t.reference_type == "SALE"
                                 && t.reference_id == invoiceId
                                 && t.item_id == item.ItemId)
                        .SumAsync(t => t.quantity);

                    availableStock += (decimal)oldQty;
                }

                if (availableStock < item.RequiredQty)
                {
                    throw new Exception(
                        $"Insufficient stock for ItemId {item.ItemId}. " +
                        $"Available: {availableStock}, Required: {item.RequiredQty}"
                    );
                }
            }
        }
        public static class StatusIds
        {
            public const int Draft = 10;

            public const int Confirm = 11;

            public const int Cancel = 12;
        }

    }
}

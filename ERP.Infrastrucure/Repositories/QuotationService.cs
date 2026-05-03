using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Quotation;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Application.Models.Quotation;
using ERP.Domain.Entities.Quotation;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace ERP.Infrastructure.Repositories
{
    public class QuotationService:IQuotationService
    {
        private readonly AppDbContext _context;
        public readonly ICodeGeneratorService _codeGeneratorservice;
        public readonly ICurrentUser _currentuser;

        public QuotationService(AppDbContext dbContext, ICodeGeneratorService codeGenerator, ICurrentUser currentUser) 
        {
            _context = dbContext;
            _codeGeneratorservice = codeGenerator;
            _currentuser = currentUser;
        }

        public async Task<List<DropdownDto>> Fetchquotationid()
        {
            try
            {
                var result = await _context.quotations.Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.quotation_no
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<QuotationDto> getQuotationbyid(string id)
        {
            try
            {
                var result = await _context.quotations.AsNoTracking().Where(x => x.id == long.Parse(id)).
                    Select(y => new QuotationDto
                    {
                        Id=y.id,
                        QuotationNo = y.quotation_no,
                        QuotationDate = y.quotation_date,
                        ValidUntil = y.quotation_date.AddDays(y.validity),
                        CustomerId = y.party_id,
                        Salesperson = y.salesperson,
                        Status = y.status == 1 ? "Draft": y.status == 2 ? "Approved" : "Rejected",
                        VatTotal = y.tax_amount,
                       Subtotal=y.sub_total,
                       GrandTotal = y.total_amount,
                       Discount = y.discount_amount,
                       Remarks = y.remarks,
                       LineItems = _context.quotationsLines.Where(z=>z.quotation_id == y.id).Select(litem => new QuotationLineItemDto
                       {
                           dbId=litem.id,
                           quotation_id = litem.quotation_id,
                           ItemId = litem.item_id,
                           Quantity = litem.quantity,
                           UnitPrice = litem.unit_price,
                           VatPct = litem.tax,
                           TotalPrice = litem.line_total,

                       }).ToList()
                    }).FirstOrDefaultAsync();

                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Fetch quotation by id  error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<GridDataResponse<QuotationGridDto>> Fetchquotations(QuotationGridRequestDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query = from q in _context.quotations
                            join p in _context.customers on q.party_id equals p.cust_pk
                            join e in _context.employees on q.salesperson equals e.employee_pk
                          //  join il in _context.quotationsLines on q.id equals il.quotation_id
                            select new { q, p, e };



                if (!string.IsNullOrEmpty(filters.Search))
                {
                    var search = filters.Search.Trim();

                    query = query.Where(x =>
                        EF.Functions.ILike(x.q.quotation_no, $"%{search}%") ||
                        EF.Functions.ILike(x.p.customer_name, $"%{search}%") ||
                        EF.Functions.ILike(x.e.first_name, $"%{search}%") 
                    );
                }

                if (filters.quotationno != null)
                {
                    query = query.Where(x => x.q.id == filters.quotationno);
                }
               

                if (filters.customer != null && filters.customer.Any())
                {
                    query = query.Where(x => filters.customer.Contains(x.q.party_id));
                }

                if (filters.salesperson != null && filters.salesperson.Any())
                {
                    query = query.Where(x => filters.salesperson.Contains(x.q.salesperson));
                }

                if(!String.IsNullOrWhiteSpace(filters.datefrom) ) 
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
                    query = query.Where(x => x.q.quotation_date >= datefrm && x.q.quotation_date <= dateto);
                }

                var total = await query.CountAsync();

                var data = await query
                            .OrderBy(x => x.q.id) // IMPORTANT: Always order before Skip
                            .Skip(offset)
                            .Take(limit).ToListAsync();
                var res = data.Select((x, index) => new QuotationGridDto
                {
                    sno = offset + index + 1,
                    id = x.q.id,
                    quono = x.q.quotation_no,
                    customerid = x.p.customer_name,
                    salesperson = x.e.first_name + ' ' + x.e.last_name,
                    validfrom = x.q.quotation_date,
                    validto = x.q.quotation_date.AddDays(x.q.validity),
                    status = x.q.status == 1 ? "Draft" : x.q.status == 2 ? "Approved" : "Rejected",
                    totalItems = _context.quotationsLines.Count(y=>y.quotation_id == x.q.id),
                    amount = x.q.total_amount ,
                    actions = "",

                }).ToList();

                return new GridDataResponse<QuotationGridDto>
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

        public async Task<QuotationDto> CreateUpdateQuot(QuotationDto quotationDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                QuotationHeader quothead;
                QuotationLines lineitemss;

                //validate and recalculate the totals, tax amount ..etc

                if (quotationDto.LineItems == null || !quotationDto.LineItems.Any())
                    throw new Exception("Quotation must have at least one line item.");

                if (quotationDto.LineItems.Any(x => x.Quantity <= 0))
                    throw new Exception("Quantity must be greater than zero.");

                if (quotationDto.LineItems.Any(x => x.UnitPrice < 0))
                    throw new Exception("Invalid unit price.");


                foreach (var item in quotationDto.LineItems)
                {
                    var baseAmount = item.Quantity * item.UnitPrice;
                    var vat = baseAmount * item.VatPct / 100;

                    item.TotalPrice = baseAmount + vat;
                }

                quotationDto.Subtotal = quotationDto.LineItems.Sum(x => x.Quantity * x.UnitPrice);
                quotationDto.VatTotal = quotationDto.LineItems.Sum(x => (x.Quantity * x.UnitPrice * x.VatPct) / 100);
                quotationDto.GrandTotal = quotationDto.Subtotal + quotationDto.VatTotal - quotationDto.Discount;
                //end


                if (quotationDto.Id == 0)
                {
                    string quotno;
                    do
                    {
                        quotno = await _codeGeneratorservice.GenerateAsync("Quotation");
                    }
                    while (await _context.quotations.AnyAsync(x => x.quotation_no == quotno));

                    quotationDto.QuotationNo = quotno;


                    quothead = new QuotationHeader
                    {
                        quotation_no = quotno,
                        quotation_date = DateTime.SpecifyKind(quotationDto.QuotationDate, DateTimeKind.Utc),
                        party_id = quotationDto.CustomerId,
                        salesperson = quotationDto.Salesperson,
                        validity = 30,
                        status =quotationDto.Status == "Draft"? 1 : quotationDto.Status == "Approved"? 2 : 3,
                        sub_total = quotationDto.Subtotal,
                        tax_amount = quotationDto.VatTotal,
                        total_amount = quotationDto.GrandTotal,
                        discount_amount = quotationDto.Discount,
                        remarks = quotationDto.Remarks,
                        created_at = DateTime.UtcNow,
                        created_by = (int) _currentuser.UserId,
                    };
                    _context.quotations.Add(quothead);
                    await _context.SaveChangesAsync();


                   var lineitems = quotationDto.LineItems.Select(item => new QuotationLines
                    {
                        quotation_id = quothead.id,
                        item_id = item.ItemId,
                        description = item.PartName,
                        quantity = item.Quantity,
                        unit_price = item.UnitPrice,
                        tax = item.VatPct,
                        line_total = item.TotalPrice,
                    }).ToList();

                    _context.quotationsLines.AddRange(lineitems);
                    await _context.SaveChangesAsync();


                }else //update the quotation
                {
                    quothead = await _context.quotations.Where(x => x.id == (long)quotationDto.Id && x.quotation_no == quotationDto.QuotationNo).FirstOrDefaultAsync();

                    if (quothead == null)
                        throw new Exception("Invalid quotation");

                    quothead.party_id = quotationDto.CustomerId;
                    quothead.salesperson = quotationDto.Salesperson;
                    quothead.validity = 30;
                    quothead.status = quotationDto.Status == "Draft" ? 1 : quotationDto.Status == "Approved" ? 2 : 3;
                    quothead.sub_total = quotationDto.Subtotal;
                    quothead.tax_amount = quotationDto.VatTotal;
                    quothead.total_amount = quotationDto.GrandTotal;
                    quothead.discount_amount = quotationDto.Discount;
                    quothead.remarks = quotationDto.Remarks;
                    quothead.updated_at = DateTime.UtcNow;
                    quothead.updated_by = (int)_currentuser.UserId;
                    await _context.SaveChangesAsync();


                    var exisitngitems = await _context.quotationsLines.Where(x => x.quotation_id == quotationDto.Id).ToListAsync();


                    if (exisitngitems == null)
                        throw new Exception("Items not found");

                    var itemstoDelete = exisitngitems.Where(dbitem => !quotationDto.LineItems.Any(d => d.dbId == dbitem.id)).ToList();
                    _context.quotationsLines.RemoveRange(itemstoDelete);

                    foreach (var dtoItem in quotationDto.LineItems)
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

                            _context.quotationsLines.Update(existingItem);
                        }
                        else
                        {
                            //  INSERT (D, E, F)
                            var newItem = new QuotationLines
                            {
                                quotation_id = (int) quotationDto.Id,
                                description = dtoItem.PartName,
                                item_id = dtoItem.ItemId,
                                quantity = dtoItem.Quantity,
                                unit_price = dtoItem.UnitPrice,
                                tax = dtoItem.VatPct,
                                line_total = (dtoItem.Quantity * dtoItem.UnitPrice)
                                           + (dtoItem.Quantity * dtoItem.UnitPrice * dtoItem.VatPct / 100)
                            };

                            _context.quotationsLines.Add(newItem);
                        }
                    }

                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return quotationDto;

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine("Creat Update quotation error: " + ex.Message);
                throw ex;
            }
        }

        public async Task<QuotationModel> quotationpdfdata(int id)
        {
            try
            {
                var result = await _context.quotations.AsNoTracking().Where(x => x.id == id).
                    Select(y => new QuotationModel
                    {
                        companyname =" Test Data Company",
                        companyaddress = "Devarchikanahalli, bangalore - 560076",
                        gstin = "",
                        quotationno = y.quotation_no,
                        quotationdate = y.quotation_date,
                        validity = y.validity.ToString(),
                        customername = _context.customers.Where(x => x.cust_pk == Convert.ToInt32(value: y.party_id)).Select(z=>z.company_name).FirstOrDefault(),
                        customeraddress = _context.customers.Where(x => x.cust_pk == Convert.ToInt32(value: y.party_id)).Select(z => z.address_line1 + ", " + z.city + ", " + z.state + ", " + z.country).FirstOrDefault(),
                        contactperson ="Muthu Pandi",
                        contact = "+91 9150770932",
                        subtotal = y.sub_total,
                        total = y.total_amount,
                        discount = y.discount_amount,
                        gst = y.tax_amount,
                        amtinwords = "two thousand",
                        items = _context.quotationsLines.Where(z => z.quotation_id == y.id ).Select(litem => new QuotationItems
                        {
                            
                            partno = _context.itemattributes.Where(t=>t.attribute_name == "part_number" && t.item_id == litem.item_id).Select(v=>v.attribute_value).FirstOrDefault(),
                            partname = _context.itemmasters.Where(t=>t.id == litem.item_id).Select(v=>v.name).FirstOrDefault(),
                            quantity = (int)litem.quantity,
                            unitprice = litem.unit_price,
                            //VatPct = litem.tax,
                            totalprice = litem.line_total,

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

    }
}

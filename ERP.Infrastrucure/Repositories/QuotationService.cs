using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.Quotation;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Application.Models.common;
using ERP.Application.Models.Quotation;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.Quotation;
using ERP.Infrastructure.Document.Documents;
using ERP.Infrastructure.Document.Helpers;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using Twilio.TwiML.Voice;

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
                        Status = y.status,
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
                           units = litem.units,
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
                            join s in _context.ddlookups on q.status equals s.id
                          //  join il in _context.quotationsLines on q.id equals il.quotation_id
                            select new { q, p, e ,s};



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
                    // 1. Parse the dates from the filter strings
                    var datefrm = DateTime.Parse(filters.datefrom).Date;
                    DateTime datetoNextDay;

                    if (!string.IsNullOrWhiteSpace(filters.dateto))
                    {
                        datetoNextDay = DateTime.Parse(filters.dateto).Date.AddDays(1);
                    }
                    else
                    {
                        datetoNextDay = datefrm.AddDays(1);
                    }

                    //  THE CRITICAL FIX: Convert Kind from 'Unspecified' to 'Utc'
                    var datefrmUtc = DateTime.SpecifyKind(datefrm, DateTimeKind.Utc);
                    var datetoNextDayUtc = DateTime.SpecifyKind(datetoNextDay, DateTimeKind.Utc);

                    // 2. Use the Utc-stamped parameters in your query
                    query = query.Where(x => x.q.quotation_date >= datefrmUtc && x.q.quotation_date < datetoNextDayUtc);
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
                    customerid = x.p.company_name,
                    salesperson = x.e.first_name + ' ' + x.e.last_name,
                    validfrom = x.q.quotation_date,
                    validto = x.q.quotation_date.AddDays(x.q.validity),
                    status = x.s.value,
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


                var domainid = await _context.domainmasters.Where(x => x.code == "GEN").Select(y => y.id).FirstOrDefaultAsync();
                var categoryid = await _context.categorymasters.Where(x => x.code == "HWD").Select(y => y.id).FirstOrDefaultAsync();
                Iteminventoryconfig itminvconfig;
                List<Itemattributes> itmattrib = new List<Itemattributes>();

                foreach (var item in quotationDto.LineItems)
                {

                    // here going to create a inventory items if the item id == 0 which means users add new item for sale without purhcase.

                    if (item.ItemId == 0) // New item from freeSolo entry
                    {
                        var existingMasterItem = await _context.itemmasters
                    .FirstOrDefaultAsync(x => x.part_number == item.PartNo || x.name == item.PartName);

                        if (existingMasterItem != null)
                        {
                            item.ItemId = existingMasterItem.id; // Link to the matching established record
                        }
                        else
                        {
                            var newMasterItem = new Itemmaster
                            {
                                sku = await _codeGeneratorservice.GenerateSku(domainid, categoryid),
                                name = item.PartName,
                                description = item.PartNo,
                                unit_id = 1,
                                category_id = categoryid,
                                sub_category_id = 0,
                                domain_id = domainid,
                                is_active = true,
                                created_by = Convert.ToInt32(_currentuser.UserId),
                                created_at = DateTime.UtcNow,
                                part_number = item.PartNo,
                                is_autocreated = true // Helpful flag property to track automated origins
                            };

                            _context.itemmasters.Add(newMasterItem);
                            await _context.SaveChangesAsync(); // Essential to invoke here to pull back the new primary ID key

                            item.ItemId = newMasterItem.id; //

                            itminvconfig = await _context.iteminventoryconfigs.FirstOrDefaultAsync(x => x.item_id == item.ItemId);

                            if (itminvconfig == null)
                            {
                                itminvconfig = new Iteminventoryconfig
                                {
                                    item_id = item.ItemId,
                                    min_stock = 0,
                                    max_stock = 0,
                                    // default_location_id = inventoryItem.location_bin
                                };
                                _context.iteminventoryconfigs.Add(itminvconfig);
                                await _context.SaveChangesAsync();

                            }
                            else
                            {
                                itminvconfig.min_stock = 0;
                                itminvconfig.max_stock = 0;
                            }
                            await _context.SaveChangesAsync();


                            //handle item attributes 

                            itmattrib = await _context.itemattributes.Where(x => x.item_id == item.ItemId).ToListAsync();


                            var newValues = new Dictionary<string, string>
                                {
                                    { "part_number", item.PartNo ?? "" },
                                    { "tags", item.PartNo != null ? string.Join(",", item.PartNo) : "" }
                                };
                            var itemattadd = new List<Itemattributes>();

                            foreach (var kvp in newValues)
                            {
                                var existing = itmattrib.FirstOrDefault(x => x.attribute_name == kvp.Key);
                                if (existing != null)
                                {
                                    // 🔹 UPDATE
                                    existing.attribute_value = kvp.Value;
                                }
                                else
                                {
                                    // 🔹 INSERT
                                    itemattadd.Add(new Itemattributes
                                    {
                                        item_id = item.ItemId,
                                        attribute_name = kvp.Key,
                                        attribute_value = kvp.Value
                                    });
                                }
                            }

                            if (itemattadd.Any())
                            {
                                _context.itemattributes.AddRange(itemattadd);
                            }
                            await _context.SaveChangesAsync();

                        }


                    }
                    //end here



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
                        status =quotationDto.Status ,
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
                        units = item.units,
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
                    quothead.status = quotationDto.Status;
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
                            existingItem.units = dtoItem.units;
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
                                units = dtoItem.units,
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
                            vatamt = (litem.unit_price * (int)litem.quantity) * (1 * (litem.tax/100)) ,
                            vatper = litem.tax.ToString() + "%",
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
        public async Task<byte[]> quotationpdfdata_new(int id)
        {
            try
            {
                var result = await (from q in _context.quotations
                                    join comp in _context.companies on q.company_id equals comp.id
                                    join cust in _context.customers on q.party_id equals cust.cust_pk
                                    join bank in _context.companybanks on q.company_id equals bank.company_id into bankGroup
                                    from b in bankGroup.DefaultIfEmpty()
                                    where q.id == id
                                    select new QuotationModel
                                    {
                                        companyname = comp.company_name,
                                        companyaddress = comp.address_line1 + ", " + comp.address_line2 + ", " + comp.city + " " + comp.state,
                                        companyphone = comp.phone,
                                        companymail = comp.email,
                                        gstin = comp.gstin,
                                        quotationno = q.quotation_no,
                                        quotationdate = q.quotation_date,
                                        validity = q.validity.ToString(),
                                        customername = cust.company_name,
                                        customeraddress = cust.address_line1 + ", " + cust.address_line2 + " " + cust.city + ", " + cust.state,
                                        contactperson = cust.customer_name,
                                        contact = cust.mobile,
                                        subtotal = q.sub_total,
                                        total = q.total_amount,
                                        discount = q.discount_amount,
                                        gst = q.tax_amount,
                                        AccountHolderName = b != null ? b.account_name : "N/A",
                                        BankName = b != null ? b.bank_name : "N/A",
                                        AccountNumber = b != null ? b.account_number : "N/A",
                                        BranchName = b != null ? b.branch_name : "N/A",
                                        IfscCode = b != null ? b.ifsc_code : "N/A",


        items = _context.quotationsLines.Where(z => z.quotation_id == q.id).Select(litem => new QuotationItems
                                        {

                                            partno = _context.itemattributes.Where(t => t.attribute_name == "part_number" && t.item_id == litem.item_id).Select(v => v.attribute_value).FirstOrDefault(),
                                            partname = _context.itemmasters.Where(t => t.id == litem.item_id).Select(v => v.name).FirstOrDefault(),
                                            quantity = (int)litem.quantity,
                                            units = _context.unitmasters.Where(t => t.id == litem.units).Select(v => v.code).FirstOrDefault(),
                                            unitprice = litem.unit_price,
                                            vatamt = (litem.unit_price * (int)litem.quantity) * (1 * (litem.tax / 100)),
                                            vatper = litem.tax.ToString() + "%",
                                            totalprice = litem.line_total,

                                        }).ToList()
                                    }
                    ).FirstOrDefaultAsync();


                var model = new TaxDocumentModel
                {
                    IsGstApplicable = true,
                    Header = new DocumentHeaderInfo
                    {
                        Kind = DocumentKind.Quotation,
                        CompanyName = result.companyname,
                        CompanyAddress = result.companyaddress,
                        CompanyPhone = result.companyphone,
                        CompanyEmail = result.companymail,
                        Gstin =result.gstin,
                        DocumentNo = result.quotationno,
                        DocumentDate = result.quotationdate,
                        ValidUntilOrDueDate = result.quotationdate.AddDays(30),
                        PlaceOfSupply = " ",

                        PartyLabel = "Quote To",
                        PartyName = result.customername,
                        PartyAddress = result.customeraddress,

                        SecondPartyLabel = "Ship To :" + result.customername,
                        SecondPartyAddress = result.customeraddress,
                    },

                    Items = result.items.Select(x=>new DocumentLineItem
                    {
                        PartName = x.partname,
                        HsnSac = x.partno,
                        Quantity = x.quantity,
                        UnitPrice = x.unitprice,
                        TaxableValue = x.unitprice * x.quantity,
                        CgstAmount = Math.Round(x.vatamt / 2,2),
                        CgstRate = 9,
                        SgstAmount = Math.Round(x.vatamt / 2, 2),
                        SgstRate = 9,
                        TotalAmount = x.totalprice
                    }).ToList(),

                    Summary = new DocumentSummary
                    {
                        SubTotal = result.subtotal,
                        TotalTax = result.gst,
                        GrandTotal = result.total,
                        AmountInWords = AmountToWordsConverter.Convert(result.total, "INR")
                    },

                    Bank = new BankDetails
                    {
                        AccountHolderName = result.AccountHolderName,
                        BankName = result.BankName,
                        AccountNumber = result.AccountNumber,
                        BranchName = result.BranchName,
                        IfscCode =result.IfscCode
                    },

                    Notes = "60 percentage against PO"
                };


                var document = new QuotationDocument(model);
                byte[] pdfprint = document.GeneratePdf();
                return pdfprint;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fetch quotation by id  error: " + ex.Message);
                throw ex;
            }
        }
    }
}

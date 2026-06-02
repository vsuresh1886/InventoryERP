using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.DTOs.Inventory;
using ERP.Application.DTOs.SalesInvoice;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Application.Models;
using ERP.Domain.Entities.Accounts;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.Quotation;
using ERP.Domain.Entities.SalesInvoice;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories.CodeGeneratorservice;
using ERP.Infrastructure.Repositories.common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class CollectionService:ICollectionService
    {
        public readonly AppDbContext _context;
        public readonly ICodeGeneratorService _codegeneratorservice;
        public readonly ICurrentUser _currentuser;
        public CollectionService (AppDbContext context, ICodeGeneratorService codegeneratorservice)
        {
            _context = context; 
            _codegeneratorservice = codegeneratorservice;
        }


        public async Task<List<DropdownDto>> FetchCollectionids()
        {
            try
            {
                var result = await _context.collectionheaders.Select(x => new DropdownDto
                {
                    Id = (int)x.id,//pk as id here
                    Name = x.receipt_no
                }).ToListAsync();


                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<GridDataResponse<CollectionGridDataDto>> FetchCollections(CollectionGridDto filters)
        {
            try
            {
                var page = filters.Page <= 0 ? 1 : filters.Page;
                var limit = filters.Limit <= 0 ? 10 : filters.Limit;
                var offset = (page - 1) * limit;

                var query =  from q in _context.collectionheaders
                            join p in _context.customers
                                on q.customer_id equals p.cust_pk
                            join dd in _context.ddlookups
                                on q.status equals dd.id
                            where dd.lookup_type == "COLLECTION_STATUS"
                            select new
                            {
                                q,
                                p,
                                dd
                            };



                if (!string.IsNullOrEmpty(filters.Search))
                {
                    var search = filters.Search.Trim();

                    query = query.Where(x =>
                        EF.Functions.ILike(x.q.receipt_no, $"%{search}%") ||
                        EF.Functions.ILike(x.p.customer_name, $"%{search}%") 
                        
                    );
                }

                if (filters.invoiceno != null)
                {
                    query = query.Where(x => x.q.id == filters.invoiceno);
                }


                if (filters.customer != null && filters.customer.Any())
                {
                    query = query.Where(x => filters.customer.Contains(x.q.id));
                }


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
                    query = query.Where(x => x.q.receipt_date >= datefrm && x.q.receipt_date <= dateto);
                }


                var total = await query.CountAsync();

                var data = await query
                            .OrderBy(x => x.q.id) // IMPORTANT: Always order before Skip
                            .Skip(offset)
                            .Take(limit).ToListAsync();
                var res = data.Select((x, index) => new      CollectionGridDataDto

                    {
                        sno = offset + index + 1,
                                id = x.q.id,
                                receiptno = x.q.receipt_no,
                               // invoiceid = x.i.id,
                               // invoiceno = x.i.invoice_no,
                                customerid = x.p.company_name,
                                receiptdate = x.q.receipt_date,
                                status = x.dd.value ,
                                amount = x.q.total_amount,
                                actions = "",

                     
                }).ToList();

                return new GridDataResponse<CollectionGridDataDto>
                {
                    Data = res,
                    Total = total
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("FetchCollectionData error: " + ex.Message);
                throw ex;
            }

        }

        //public async Task<CollectionDto> FetchCollectionById(long collectionid)
        //{
        //    try
        //    {
        //        var result = await( from q in _context.collectionheaders
        //                     where q.id == collectionid
        //                     select ( new CollectionDto 
        //                     {
        //                         id = q.id,
        //                         receipt_no = q.receipt_no,
        //                         receipt_date = q.receipt_date,
        //                         customer_id = q.customer_id,
        //                         reference_no = q.reference_no, 
        //                         payment_mode = q.payment_mode,
        //                         total_amount = q.total_amount,
        //                         remarks = q.remarks,
        //                         status = q.status, 
        //                         created_by = q.created_by,
        //                         colldetail = _context.collectiondetails.Where(x=>x.collection_id == q.id).Select(y=> new CollectiondetailDto
        //                                         {
        //                                                id=y.id,
        //                                                collection_id = y.collection_id,
        //                                                sales_invoice_id = y.sales_invoice_id,
        //                                                allocated_amount = y.allocated_amount,

        //                                         }).ToList(),
                                 

        //                     })).FirstOrDefaultAsync();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Fetch collection by id  error: " + ex.Message);
        //        throw ex;
        //    }
        //}

        public async  Task<CollectionDto> FetchCollectionById(long collectionid)
        {
            try
            {


                var result = await (from q in _context.collectionheaders
                                    where q.id == collectionid
                                    select (new CollectionDto
                                    {
                                        id = q.id,
                                        receiptNo = q.receipt_no,
                                        receiptDate = q.receipt_date,
                                        customerId = q.customer_id,
                                        referenceNo = q.reference_no,
                                        paymentMode = q.payment_mode,
                                        totalAmount = q.total_amount,
                                        remarks = q.remarks,
                                        status = q.status,
                                        created_by = q.created_by,
                                        //colldetail = _context.collectiondetails.Where(x => x.collection_id == q.id).Select(y => new CollectiondetailDto
                                        //{
                                        //    Aid = y.id,
                                        //    collection_id = y.collection_id,
                                        //    invoiceId = y.sales_invoice_id,
                                        //    invoiceNo = _context.invoiceHeaders.Where(d=>d.id== y.sales_invoice_id ).Select(d=>d.invoice_no).FirstOrDefault(),
                                        //    allocateAmount = y.allocated_amount,

                                        //}).ToList(),


                                    })).FirstOrDefaultAsync();
                var existingAllocations =
                            await (
                                from d in _context.collectiondetails

                                join i in _context.invoiceHeaders
                                    on d.sales_invoice_id equals i.id

                                where d.collection_id == collectionid

                                select new CollectiondetailDto
                                {
                                    aid = d.id,

                                    collection_id = d.collection_id,

                                    invoiceId = d.sales_invoice_id,

                                    allocateAmount = d.allocated_amount,

                                    invoiceNo = i.invoice_no,

                                    invoiceDate = i.invoice_date,

                                    invoiceAmount = i.total_amount,

                                    paidAmount = i.paid_amount,

                                    balanceAmount = i.total_amount == d.allocated_amount? d.allocated_amount : 
                                        i.balance_amount
                                        + d.allocated_amount,

                                    selected = true
                                }
                            ).ToListAsync();

                var allocatedInvoiceIds =
                                existingAllocations
                                    .Select(x => x.invoiceId)
                                    .ToList();
                var pendingInvoices =
                                        await _context.invoiceHeaders

                                            .Where(x =>
                                                x.party_id == result.customerId &&
                                                x.balance_amount > 0 &&
                                                !allocatedInvoiceIds.Contains(x.id)
                                            )

                                            .Select(i => new CollectiondetailDto
                                            {
                                                aid = 0,

                                                collection_id = result.id,

                                                invoiceId = i.id,

                                                allocateAmount = 0,

                                                invoiceNo = i.invoice_no,

                                                invoiceDate = i.invoice_date,

                                                invoiceAmount = i.total_amount,

                                                paidAmount = i.paid_amount,

                                                balanceAmount = i.balance_amount,

                                                selected = false
                                            })

                                            .ToListAsync();


                result.allocations =
                    existingAllocations
                        .Concat(pendingInvoices)
                        .OrderBy(x => x.invoiceDate)
                        .ToList();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fetch collection by id  error: " + ex.Message);
                throw ex;
            }
        }




        public async Task<ApiResponse<CollectionSaveDto>> CreateUpdateCollection(CollectionSaveDto dto)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =====================================================
                // VALIDATIONS
                // =====================================================

                if (dto.customerId <= 0)
                {
                    return ApiResponseHelper.Fail<CollectionSaveDto>("Customer required");
                }

                if (dto.totalAmount <= 0)

                {
                    return ApiResponseHelper.Fail<CollectionSaveDto>("Receipt amount required");
                }
                   

                if (dto.allocations == null ||
                    !dto.allocations.Any())
                {
                    return ApiResponseHelper.Fail<CollectionSaveDto>("Allocation required");
                }
                   

                var totalAllocated =
                    dto.allocations.Sum(x => x.allocateAmount);

                if (totalAllocated > dto.totalAmount)

                {
                    return ApiResponseHelper.Fail<CollectionSaveDto>("Allocated amount exceeds receipt amount");
                }  



                var paymentStatusMap = await _context.ddlookups
                        .Where(x => x.lookup_type == "PAYMENT_STATUS")
                        .ToDictionaryAsync(
                            x => x.code,
                            x => x.id
                        );

                // =====================================================
                // LOAD / CREATE HEADER
                // =====================================================

                Collectionheader header;
                Collectiondetail details;

                var statsconfirm = await _context.ddlookups.Where(y => y.lookup_type == "COLLECTION_STATUS" && y.code == "CONFIRM").Select(z => z.id).FirstOrDefaultAsync();

                bool isNew = dto.id == 0;

                if (isNew)
                {
                    header = new Collectionheader();

                    header.receipt_no =
                        await _codegeneratorservice.GenerateAsync("Collection");

                    _context.collectionheaders.Add(header);
                   
                }
                else
                {
                    header = await _context.collectionheaders
                            .FirstOrDefaultAsync(x => x.id == dto.id);

                    if (header == null)
                        throw new Exception("Collection not found");

                    var existingDetails = await _context.collectiondetails
                                        .Where(x => x.collection_id == header.id)
                                        .ToListAsync();

                    header.updated_by = 1;// (int)_currentuser.UserId;
                    header.updated_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    // =================================================
                    // IF PREVIOUSLY CONFIRMED
                    // REVERT OLD ALLOCATIONS FIRST
                    // =================================================


                    if (header.status == statsconfirm) //"CONFIRMED"
                    {
                        foreach (var oldLine in existingDetails)
                        {
                            var oldInvoice =
                                await _context.invoiceHeaders
                                .FirstOrDefaultAsync(x =>
                                    x.id == oldLine.id);

                            if (oldInvoice != null)
                            {
                                oldInvoice.paid_amount -=
                                    oldLine.allocated_amount;

                                oldInvoice.balance_amount +=
                                    oldLine.allocated_amount;

                                if (oldInvoice.paid_amount < 0)
                                    oldInvoice.paid_amount = 0;

                                string statusCode =
                                            oldInvoice.balance_amount <= 0
                                                ? "PAID"
                                                : oldInvoice.paid_amount > 0
                                                    ? "PARTIAL"
                                                    : "PENDING";


                                oldInvoice.payment_status = (int)paymentStatusMap[statusCode];

                                
                            }
                        }
                    }
                }

                // =====================================================
                // UPDATE HEADER
                // =====================================================

                header.customer_id = dto.customerId;
                header.receipt_date = DateTime.SpecifyKind(dto.receiptDate, DateTimeKind.Utc);
                header.payment_mode = dto.paymentMode;
                header.reference_no = dto.referenceNo;
                header.total_amount = dto.totalAmount;
                header.remarks = dto.remarks;
                header.status = dto.status;
                header.created_by =  1;
                header.created_at = DateTime.SpecifyKind(dto.receiptDate, DateTimeKind.Utc);
                await _context.SaveChangesAsync();
                // =====================================================
                // REMOVE OLD DETAILS
                // =====================================================

                if (!isNew)
                {

                    var existingDetails = await _context.collectiondetails
                                       .Where(x => x.collection_id == header.id)
                                       .ToListAsync();
                    _context.collectiondetails.RemoveRange(
                        existingDetails);
                }

                // =====================================================
                // ADD NEW DETAILS
                // =====================================================

                var detailEntities =
                    new List<Collectiondetail>();

                foreach (var line in dto.allocations
                             .Where(x => x.allocateAmount > 0))
                {
                    var invoice =
                        await _context.invoiceHeaders
                        .FirstOrDefaultAsync(x =>
                            x.id == line.invoiceId);

                    if (invoice == null)
                        throw new Exception(
                            $"Invoice not found : {line.invoiceId}");

                    // ================================================
                    // VALIDATE BALANCE
                    // ================================================

                    if (line.allocateAmount >
                        invoice.balance_amount)
                    {
                        throw new Exception(
                            $"Allocation exceeds balance for invoice {invoice.invoice_no}");
                    }

                    var detail = new Collectiondetail
                    {
                        collection_id = header.id,
                        sales_invoice_id = line.invoiceId,
                        allocated_amount = line.allocateAmount,
                        created_at = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
                    };

                    detailEntities.Add(detail);
                   

                    // ================================================
                    // APPLY ONLY IF CONFIRMED
                    // ================================================

                    if (dto.status == statsconfirm)
                    {
                        invoice.paid_amount +=
                            line.allocateAmount;

                        invoice.balance_amount -=
                            line.allocateAmount;

                        if (invoice.balance_amount < 0)
                            invoice.balance_amount = 0;


                        string statusCode =
                                           invoice.balance_amount <= 0
                                               ? "PAID"
                                               : invoice.paid_amount > 0
                                                   ? "PARTIAL"
                                                   : "PENDING";
                        invoice.payment_status = (int)paymentStatusMap[statusCode];
                    }
                }

                _context.collectiondetails.AddRange(detailEntities);

                // =====================================================
                // SAVE
                // =====================================================
                await _context.SaveChangesAsync();
                

                await transaction.CommitAsync();

                dto.id = header.id;
                dto.receiptNo = header.receipt_no;

                return ApiResponseHelper.Success(dto, "Collection saved successfully"); 
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                Console.WriteLine(
                    "CreateUpdateCollection Error : " + ex.Message);

                return ApiResponseHelper.Fail<CollectionSaveDto>(ex.Message);
            }
        }

        public async Task<List<OutstandingInvoiceCDto>> GetOutstandingbyCustomer(long customerid)
        {
            try
            {
                var stats = await _context.ddlookups.Where(y => y.lookup_type == "PAYMENT_STATUS" && y.code == "PAID").Select(z => z.id).FirstOrDefaultAsync();
                var res= await _context.invoiceHeaders.Where(x => x.party_id == customerid && x.balance_amount > 0 && x.status == 2 && x.payment_status != stats ) 
                            .Select(x => new OutstandingInvoiceCDto
                            {
                                Aid = 0,
                                invid = x.id,
                                invoiceNo = x.invoice_no,
                                invoiceDate = x.invoice_date,
                                grandTotal = x.total_amount,
                                paidAmount = x.paid_amount,
                                balanceAmount = x.balance_amount
                            })
                            .ToListAsync();
              return res;

            }
            catch( Exception ex)
            {
                Console.WriteLine("outstanding invoices by customer fetch  error: " + ex.Message);
                throw ex;
            }
        }


    }
}

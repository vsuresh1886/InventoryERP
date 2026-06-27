using DocumentFormat.OpenXml.Spreadsheet;
using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Application.Models.Notification;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class OutstandingService:IOutstandingService
    {
        public readonly AppDbContext _context;
        public readonly IEmailService _emailService;
        public readonly IPdfService _pdfservice;
        public readonly ICurrentTenantService _tenantService;
      



        public OutstandingService(IPdfService pdfService, AppDbContext context, IEmailService emailService,ICurrentTenantService currenttenantservice)
        {
            _emailService = emailService;
            _pdfservice = pdfService;
            _context = context;
            _tenantService = currenttenantservice;
        }



        public async Task<List<CustomerOutstandingDto>> FetchCustomer_OS(ReceivableOutstandingFiltersDto filters)
        {
            try
            {
                long[]? customerIds = filters.CustomerIds?.ToArray();

                var companyid = _tenantService.CompanyId ?? throw new UnauthorizedAccessException("Invalid Tenant Space Context.");

                var result = await _context
                              .Set<ReceivableOutstandingRowDto>()
                              .FromSqlInterpolated($@"

                            SELECT *
                            FROM receivable_outstanding_report
                            (
                                {customerIds},
                                {filters.from_date},
                                {filters.to_date},
                                {filters.pending_only},
                                {filters.overdue_only},
                                {companyid}
                            )

                            ")
                              .AsNoTracking()
                          .ToListAsync();                                                           

                var finalresult = result
                                .GroupBy(x => new
                                {
                                    x.customer_id,
                                    x.customer_name
                                })
                                .Select(g => new CustomerOutstandingDto
                                {
                                    customer_id = g.Key.customer_id,
                                    customer_name = g.Key.customer_name,

                                    totalInvoiceAmount =
                                        g.Sum(x => x.invoice_amount),

                                    totalReceivedAmount =
                                        g.Sum(x => x.received_amount),

                                    totalReturnedAmount = 
                                        g.Sum(x=>x.returned_amount),

                                    totalPendingAmount =
                                        g.Sum(x => x.pending_amount),

                                    Invoices = g.Select(x =>
                                        new ReceivableOutstandingRowDto
                                        {
                                            invoice_ids = x.invoice_ids,
                                            invoice_no = x.invoice_no,
                                            invoice_date = x.invoice_date,
                                            due_date = x.due_date,
                                            invoice_amount = x.invoice_amount,
                                            received_amount = x.received_amount,
                                            returned_amount = x.returned_amount,
                                            pending_amount = x.pending_amount,
                                            age_days = x.age_days
                                        })
                                        .ToList()
                                })
                                .ToList();




                return finalresult;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public async Task<String> EmailCustomer_OS(ReceivableOutstandingFiltersDto filters)
        {
            try
            {


                List<long> customer = filters.CustomerIds.Count() > 0 ? filters.CustomerIds.ToList() : new List<long>();
                customer_master custmst = new customer_master();


                foreach (var cust in customer)
                {
                    custmst = await _context.customers.Where(x => x.cust_pk == cust).FirstOrDefaultAsync();
                    var customerIds = new long[]
                            {
                                (long)cust
                            };

                    var result = await _context
                          .Set<ReceivableOutstandingRowDto>()
                          .FromSqlInterpolated($@"

                        SELECT *
                        FROM receivable_outstanding_report
                        (
                            {customerIds},
                            {filters.from_date},
                            {filters.to_date},
                            {filters.pending_only},
                            {filters.overdue_only}
                        )

                        ")
                          .AsNoTracking()
                          .ToListAsync();

                    var finalresult = result
                                    .GroupBy(x => new
                                    {
                                        x.customer_id,
                                        x.customer_name
                                    })
                                    .Select(g => new CustomerOutstandingDto
                                    {
                                        customer_id = g.Key.customer_id,
                                        customer_name = g.Key.customer_name,

                                        totalInvoiceAmount =
                                            g.Sum(x => x.invoice_amount),

                                        totalReceivedAmount =
                                            g.Sum(x => x.received_amount),
                                        totalReturnedAmount = 
                                            g.Sum(x => x.returned_amount),

                                        totalPendingAmount =
                                            g.Sum(x => x.pending_amount),

                                        Invoices = g.Select(x =>
                                            new ReceivableOutstandingRowDto
                                            {
                                                invoice_ids = x.invoice_ids,
                                                invoice_no = x.invoice_no,
                                                invoice_date = x.invoice_date,
                                                due_date = x.due_date,
                                                invoice_amount = x.invoice_amount,
                                                received_amount = x.received_amount,
                                                returned_amount = x.returned_amount,
                                                pending_amount = x.pending_amount,
                                                age_days = x.age_days
                                            })
                                            .ToList()
                                    })
                                    .ToList();

                    // convert to pdf

                    var pdfbytes = await _pdfservice.Gen_customerOSPdf(finalresult);

                    var attach = new EmailAttachment
                    {
                        Data = pdfbytes,
                        FileName = $"Statement_{custmst.company_name}.pdf",
                        ContentType = "application/pdf"
                    };

                    //prepare email request
                    var request = new EmailRequest
                    {
                        To = [custmst.email],
                        Cc = null,
                        Subject = $"Statement of Account - {DateTime.Now:dd-MMM-yyyy}",
                        Body = $@"
                            Dear {custmst.company_name},
                            <br/><br/>
                            Please find attached your statement of account.
                            <br/><br/>
                            Regards,
                            ERP Team
                        ",
                        IsHtml = true,
                        Attachments = [attach]
                    };

                    //send email
                    await _emailService.SendAsync(request);

                }
                return "Email Sent Successfully";

            }
            catch (Exception ex)
            {
                throw ex;
                return "Something went wrong Mail not sent!";

            }

        }



    }
}

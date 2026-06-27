using DocumentFormat.OpenXml.Spreadsheet;
using ERP.Application.DTOs;
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
    public class AgingService:IAgingService
    {
        private readonly AppDbContext _context;
        private readonly IPdfService _pdfservice;
        private readonly IEmailService _emailservice;
        private readonly ICurrentTenantService _tenantService;

        public AgingService(AppDbContext context,IPdfService pdfService,IEmailService emailservice,ICurrentTenantService tenantService)
        {
            _context = context;
            _pdfservice = pdfService;
            _emailservice = emailservice;
            _tenantService = tenantService;
        }

       public async Task<List<CustomerAgingCustomerDto>> CustomerAging_rep(CustomerAgingFilterDto filters)
        {
            try
            {
                var companyid = _tenantService.CompanyId?? throw new UnauthorizedAccessException("Invalid Tenant Space Context.");
                var result =  await _context.Set<CustomerAgingRowDto>()
                                .FromSqlInterpolated($@"
                                       SELECT *
                                    FROM customer_aging_report
                                    (
                                        {filters.customer_ids},
                                        {filters.as_on_date},
                                        {filters.pending_only},
                                        {companyid}
                                    )

                                ")
                                .AsNoTracking()
                                .ToListAsync();
                var finalResult = result?.AsEnumerable()
                           .GroupBy(x => new
                           {
                               x.customer_id,
                               x.customer_name
                           }).Select(customer => new CustomerAgingCustomerDto
                           {
                               customer_id = customer.Key.customer_id,
                               customer_name = customer.Key.customer_name,
                               current_amount = customer.Sum(x => x.current_amount),
                               days_31_60 = customer.Sum(x => x.days_31_60),
                               days_61_90 = customer.Sum(x => x.days_61_90),
                               days_91_120 = customer.Sum(x => x.days_91_120),
                               days_120_plus = customer.Sum(x => x.days_120_plus),
                               total_outstanding = customer.Sum(x=>x.pending_amount),
                               invoices = customer.Select(x => new CustomerAgingInvoiceDto 
                                        { 
                                           invoice_ids = x.invoice_ids, 
                                           invoice_no = x.invoice_no, 
                                           invoice_date = x.invoice_date.Date, 
                                           due_date = x.due_date.Date, 
                                           age_days = x.age_days, 
                                           current_amount = x.current_amount, 
                                           days_31_60 = x.days_31_60, 
                                           days_61_90 = x.days_61_90, 
                                           days_91_120 = x.days_91_120, 
                                           days_120_plus = x.days_120_plus,
                                           pending_amount = x.pending_amount,
                               }).OrderByDescending(x => x.age_days).ToList()
                           }).OrderByDescending(x => x.total_outstanding).ToList();

                return finalResult;

            }
            catch (Exception ex)
            {
                throw ex;
                return null;    
            }

        }

        public async Task<string> CustomerAgingEmail_rep(CustomerAgingFilterDto filters)
        {
            try
            {
                List<long> customer = filters.customer_ids.Count() > 0 ? filters.customer_ids.ToList() : new List<long>();
                customer_master custmst = new customer_master();


                foreach (var cust in customer)
                {
                    custmst = await _context.customers.Where(x => x.cust_pk == cust).FirstOrDefaultAsync();
                    var customerIds = new long[]
                            {
                                (long)cust
                            };
                    var result = await _context.Set<CustomerAgingRowDto>()
                                .FromSqlInterpolated($@"
                                       SELECT *
                                    FROM customer_aging_report
                                    (
                                        {customerIds},
                                        {filters.as_on_date},
                                        {filters.pending_only}
                                    )

                                ")
                                .AsNoTracking()
                                .ToListAsync();

                    var finalResult = result?.AsEnumerable()
                               .GroupBy(x => new
                               {
                                   x.customer_id,
                                   x.customer_name
                               }).Select(customer => new CustomerAgingCustomerDto
                               {
                                   customer_id = customer.Key.customer_id,
                                   customer_name = customer.Key.customer_name,
                                   current_amount = customer.Sum(x => x.current_amount),
                                   days_31_60 = customer.Sum(x => x.days_31_60),
                                   days_61_90 = customer.Sum(x => x.days_61_90),
                                   days_91_120 = customer.Sum(x => x.days_91_120),
                                   days_120_plus = customer.Sum(x => x.days_120_plus),
                                   total_outstanding = customer.Sum(x => x.pending_amount),
                                   invoices = customer.Select(x => new CustomerAgingInvoiceDto
                                   {
                                       invoice_ids = x.invoice_ids,
                                       invoice_no = x.invoice_no,
                                       invoice_date = x.invoice_date.Date,
                                       due_date = x.due_date.Date,
                                       age_days = x.age_days,
                                       current_amount = x.current_amount,
                                       days_31_60 = x.days_31_60,
                                       days_61_90 = x.days_61_90,
                                       days_91_120 = x.days_91_120,
                                       days_120_plus = x.days_120_plus,
                                       pending_amount = x.pending_amount,
                                   }).OrderByDescending(x => x.age_days).ToList()
                               }).OrderByDescending(x => x.total_outstanding).ToList();
                

                // convert to pdf

                var pdfbytes = await _pdfservice.Gen_customerAgingPdf(finalResult);

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
                    Subject = $"Statement of Account (Aging) - {DateTime.Now:dd-MMM-yyyy}",
                    Body = $@"
                            Dear {custmst.company_name},
                            <br/><br/>
                            Please find attached your statement of account - Aging.
                            <br/><br/>
                            Regards,
                            ERP Team
                        ",
                    IsHtml = true,
                    Attachments = [attach]
                };

                //send email
                await _emailservice.SendAsync(request);

            }
                return "Email Sent Successfully";

            }
            catch (Exception ex)
            {
                throw ex;
                return null;
            }

        }

    }
}

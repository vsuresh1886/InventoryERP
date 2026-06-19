using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
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
    public class SoaService: ISoaService
    {
        private readonly AppDbContext _context;
        private readonly IPdfService _pdfservice;
        private readonly IEmailService _emailservice;


        public SoaService(AppDbContext context,IPdfService pdfservice , IEmailService emailService  )
        {
            _context = context;
            _pdfservice = pdfservice;
            _emailservice = emailService;
        }
        public async Task<List<CustomerSOADto>> FetchCustomer_rep(CustomerSOAFilterDto filters)
        {
            try
            {

            
            var result =
                await _context
                .Set<CustomerSOARowDto>()
                .FromSqlInterpolated($@"

                    SELECT *
                    FROM customer_soa_report
                    (
                        {filters.customer_ids},
                        {filters.from_date},
                        {filters.to_date},
                        {filters.pending_only}
                    )

                ")
                .AsNoTracking()
                .ToListAsync();


               

            var finalResult = result?
                            .GroupBy(x => new
                            {
                                x.customer_id,
                                x.customer_name
                            })

                            .Select(customer => new CustomerSOADto
                            {
                                customer_id =
                                    customer.Key.customer_id,

                                customer_name =
                                    customer.Key.customer_name,

                                invoice_total =
                                    customer.Sum(x =>
                                        x.debit_amount),

                                paid_total =
                                    customer.Sum(x =>
                                        x.credit_amount),

                                balance_total =
                                    customer.Sum(x =>
                                        x.debit_amount - x.credit_amount),

                                months = customer

                                    .GroupBy(x => x.month_name)

                                    .Select(month =>

                                        new CustomerSOAMonthDto
                                        {
                                            month = month.Key,

                                            invoice_total =
                                                month.Sum(x =>
                                                    x.debit_amount),

                                            paid_total =
                                                month.Sum(x =>
                                                    x.credit_amount),

                                            balance_total =
                                                month.Sum(x =>
                                                    x.debit_amount - x.credit_amount),

                                            details = month
                                                .Select(x =>
                                                    new CustomerSOADetailDto
                                                    {
                                                        id = 0,

                                                        date =
                                                            x.transaction_date,

                                                        invoice_no =
                                                            x.reference_no,

                                                        doc_type =
                                                            x.doc_type,

                                                        invoice_amount =
                                                            x.debit_amount,

                                                        paid_amount =
                                                            x.credit_amount,

                                                        balance =
                                                           x.running_balance,

                                                        running_balance =
                                                            x.running_balance
                                                    })

                                                .ToList()
                                        })

                                    .ToList()

                            }).ToList(); 




                         return finalResult;
            }
            catch (Exception ex)
            {
                return new List<CustomerSOADto>();
            }

        }

        public async Task<String> EmailCustomer_rep(CustomerSOAFilterDto filters)
        {
            try
            {
                

                List<int> customer = filters.customer_ids.Count() > 0 ? filters.customer_ids.ToList(): new List<int>();
                customer_master custmst = new customer_master();
                

                foreach (var cust in customer)
                {
                    custmst = await _context.customers.Where(x => x.cust_pk == cust).FirstOrDefaultAsync();
                    var customerIds = new long[]
                            {
                                (long)cust
                            };

                    var result =
                    await _context
                    .Set<CustomerSOARowDto>()
                    .FromSqlInterpolated($@"

                    SELECT *
                    FROM customer_soa_report
                    (
                        {customerIds},
                        {filters.from_date},
                        {filters.to_date},
                        {filters.pending_only}
                    )

                ")
                    .AsNoTracking()
                    .ToListAsync();




                 var  finalResult = result?
                                    .GroupBy(x => new
                                    {
                                        x.customer_id,
                                        x.customer_name
                                    })

                                    .Select(customer => new CustomerSOADto
                                    {
                                        customer_id =
                                            customer.Key.customer_id,

                                        customer_name =
                                            customer.Key.customer_name,

                                        invoice_total =
                                            customer.Sum(x =>
                                                x.debit_amount),

                                        paid_total =
                                            customer.Sum(x =>
                                                x.credit_amount),

                                        balance_total =
                                            customer.Sum(x =>
                                                x.debit_amount - x.credit_amount),

                                        months = customer

                                            .GroupBy(x => x.month_name)

                                            .Select(month =>

                                                new CustomerSOAMonthDto
                                                {
                                                    month = month.Key,

                                                    invoice_total =
                                                        month.Sum(x =>
                                                            x.debit_amount),

                                                    paid_total =
                                                        month.Sum(x =>
                                                            x.credit_amount),

                                                    balance_total =
                                                        month.Sum(x =>
                                                            x.debit_amount - x.credit_amount),

                                                    details = month
                                                        .Select(x =>
                                                            new CustomerSOADetailDto
                                                            {
                                                                id = 0,

                                                                date =
                                                                    x.transaction_date,

                                                                invoice_no =
                                                                    x.reference_no,

                                                                doc_type =
                                                                    x.doc_type,

                                                                invoice_amount =
                                                                    x.debit_amount,

                                                                paid_amount =
                                                                    x.credit_amount,

                                                                balance =
                                                                   x.running_balance,

                                                                running_balance =
                                                                    x.running_balance
                                                            })

                                                        .ToList()
                                                })

                                            .ToList()

                                    }).ToList();

                    // convert to pdf

                    var pdfbytes = await _pdfservice.Gen_customerSOAPdf(finalResult);

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
                        Attachments = [ attach]
                    };

                    //send email
                     await _emailservice.SendAsync(request);

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

using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public  class CustomerAgingFilterDto
    {
        public List<long>? customer_ids { get; set; }
        public DateOnly? as_on_date { get; set; }
        public bool pending_only { get; set; } = true;
    }


    public class CustomerAgingRowDto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; } = string.Empty;

        public long invoice_ids { get; set; }

        public string invoice_no { get; set; } = string.Empty;

        public DateTime invoice_date { get; set; }

        public DateTime due_date { get; set; }

        public int age_days { get; set; }

        public decimal pending_amount { get; set; }

        public decimal current_amount { get; set; }

        public decimal days_31_60 { get; set; }

        public decimal days_61_90 { get; set; }

        public decimal days_91_120 { get; set; }

        public decimal days_120_plus { get; set; }
    }

    public class CustomerAgingInvoiceDto
    {
        public long invoice_ids { get; set; }

        public string invoice_no { get; set; } = string.Empty;

        public DateTime invoice_date { get; set; }

        public DateTime due_date { get; set; }

        public int age_days { get; set; }

        public decimal pending_amount { get; set; }

        public decimal current_amount { get; set; }

        public decimal days_31_60 { get; set; }

        public decimal days_61_90 { get; set; }

        public decimal days_91_120 { get; set; }

        public decimal days_120_plus { get; set; }
    }



    public class CustomerAgingCustomerDto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; } = string.Empty;

        public decimal current_amount { get; set; }

        public decimal days_31_60 { get; set; }

        public decimal days_61_90 { get; set; }

        public decimal days_91_120 { get; set; }

        public decimal days_120_plus { get; set; }

        public decimal total_outstanding { get; set; }

        public List<CustomerAgingInvoiceDto> invoices { get; set; } = new();
    }



}

using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class CustomerSOAFilterDto
    {
        public List<int>? customer_ids { get; set; }

        //public int? branch_id { get; set; }

        //public int? salesman_id { get; set; }

        //public int? area_id { get; set; }

        public DateOnly? from_date { get; set; }

        public DateOnly? to_date { get; set; }

        public bool pending_only { get; set; }

        //public bool overdue_only { get; set; }

    }

    public class CustomerSOARowDto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; }

        public string month_name { get; set; }

        public DateTime transaction_date { get; set; }

        public string doc_type { get; set; }

        public string reference_no { get; set; }

        public decimal debit_amount { get; set; }

        public decimal credit_amount { get; set; }

        public decimal running_balance { get; set; }
    }
    public class CustomerSOADto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; }

        public decimal invoice_total { get; set; }

        public decimal paid_total { get; set; }

        public decimal balance_total { get; set; }

        public List<CustomerSOAMonthDto> months { get; set; } = new();
    }

    public class CustomerSOAMonthDto
    {
        public string month { get; set; }

        public decimal invoice_total { get; set; }

        public decimal paid_total { get; set; }

        public decimal balance_total { get; set; }

        public List<CustomerSOADetailDto> details { get; set; } = new();
    }

    public class CustomerSOADetailDto
    {
        public long id { get; set; }

        public DateTime date { get; set; }

        public string invoice_no { get; set; }

        public string doc_type { get; set; }

        public decimal invoice_amount { get; set; }

        public decimal paid_amount { get; set; }

        public decimal balance { get; set; }

        public decimal running_balance { get; set; }
    }

}

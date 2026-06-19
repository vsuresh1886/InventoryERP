using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Accounts
{
    public class ReceivableOutstandingFiltersDto
    {
        public List<long>? CustomerIds { get; set; }

        public DateOnly? from_date { get; set; }

        public DateOnly? to_date { get; set; }

        public bool pending_only { get; set; } = true;

        public bool overdue_only { get; set; } = false;
    }

    public class ReceivableOutstandingRowDto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; } = string.Empty;

        public long invoice_ids { get; set; }

        public string invoice_no { get; set; } = string.Empty;

        public DateOnly invoice_date { get; set; }

        public DateOnly due_date { get; set; }

        public decimal invoice_amount { get; set; }

        public decimal received_amount { get; set; }

        public decimal returned_amount { get; set; }

        public decimal pending_amount { get; set; }

        public int age_days { get; set; }
    }

    public class CustomerOutstandingDto
    {
        public long customer_id { get; set; }

        public string customer_name { get; set; } = string.Empty;

        public decimal totalInvoiceAmount { get; set; }

        public decimal totalReceivedAmount { get; set; }

        public decimal totalReturnedAmount { get; set; }

        public decimal totalPendingAmount { get; set; }

        public List<ReceivableOutstandingRowDto> Invoices { get; set; }
            = new();
    }

}

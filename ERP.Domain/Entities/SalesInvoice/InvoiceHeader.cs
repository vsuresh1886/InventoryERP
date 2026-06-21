using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.SalesInvoice
{
    [Table("invoice_header")]
        public class InvoiceHeader:IMustHaveTenant
        {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long id { get; set; }
            public string invoice_no { get; set; }
            public DateTime invoice_date { get; set; }
            public long quotation_id { get; set; }
            public long party_id { get; set; }
            public long salesperson { get; set; }
            public int status { get; set; }
            public decimal sub_total { get; set; }
            public decimal tax_amount { get; set; }
            public decimal discount_amount { get; set; }
            public decimal total_amount { get; set; }
            public string remarks { get; set; }
            public int validity { get; set; }
            public DateTime created_at { get; set; }
            public int created_by { get; set; }
            public DateTime updated_at { get; set; }
            public int updated_by { get; set; }
            public decimal  balance_amount { get; set; }
            public decimal paid_amount { get; set; }
            public int payment_status { get; set; }
            public long? company_id { get; set; }
        }
}

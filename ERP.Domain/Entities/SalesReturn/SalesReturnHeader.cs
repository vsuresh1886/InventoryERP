using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.SalesReturn
{
    [Table("sales_return_header")]
    public class SalesReturnHeader:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string return_no { get; set; }
        public DateTime return_date { get; set; }
        public long? company_id { get; set; }
        public long customer_id { get; set; }
        public long invoice_id { get; set; }
        public string remarks { get; set; }
        public decimal total_qty { get; set; }
        public decimal gross_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal taxable_amount { get; set; }
        public decimal tax_amount { get; set; }
        public decimal net_amount { get; set; }
        public int status { get; set; }
        public long created_by { get; set; }
        public DateTime? created_on { get; set; }
        public long modified_by { get; set; }
        public DateTime? modified_on { get; set; }

        public int? discount_percentage { get; set; }

        public int? return_reason_id { get; set; }
        
    }
}

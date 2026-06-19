using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.SalesReturn
{
    [Table("sales_return_detail")]
    public class salesreturndetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long sales_return_id { get; set; }
        public long sales_invoice_detail_id { get; set; }
        public long item_id { get; set; }
        public decimal qty { get; set; }
        public decimal rate { get; set; }
        public decimal discount_percentage { get; set; }
        public decimal discount_amount { get; set; }
        public decimal tax_percentage { get; set; }
        public decimal tax_amount { get; set; }
        public decimal amount { get; set; }
        public string? remarks { get; set; }

        public string? partno { get; set; }
        public string? partname { get; set; }
    }
}

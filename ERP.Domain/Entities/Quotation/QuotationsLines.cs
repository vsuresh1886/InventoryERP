using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Quotation
{
    [Table("quotation_lines")]
    public class QuotationLines
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long quotation_id { get; set; }
        public long item_id { get; set; }
        public string? description { get; set; }
        public decimal quantity { get; set; }
        public long units { get; set; }
        public decimal unit_price { get; set; }
        public decimal discount { get; set; }
        public decimal tax { get; set; }
        public decimal line_total { get; set; }
    }
}

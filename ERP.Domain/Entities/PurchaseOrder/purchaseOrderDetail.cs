using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.PurchaseOrder
{
    [Table("pur_purchase_order_detail")]
    public  class purchaseOrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long po_detail_id { get; set; }
        public long po_id { get; set; }
        public long item_id { get; set; }
        public string description { get; set; }
        public decimal ordered_qty { get; set; }
        public decimal received_qty { get; set; }
        public decimal balance_qty { get; set; }
        public long uom_id { get; set; }
        public decimal unit_price { get; set; }
        public decimal discount_percent { get; set; }
        public decimal discount_amount { get; set; }
        public long warehouse_id { get; set; }
        public long tax_id { get; set; }
        public decimal tax_percent { get; set; }
        public decimal tax_amount { get; set; }
        public decimal line_total { get; set; }
        public string remarks { get; set; }

    }
}

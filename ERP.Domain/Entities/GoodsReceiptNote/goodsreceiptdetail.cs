using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.GoodsReceiptNote
{
    [Table("pur_goods_receipt_detail")]
    public class goodsreceiptdetail 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long grn_id { get; set; }
        public long po_detail_id { get; set; }
        public long item_id { get; set; }
        public decimal ordered_qty { get; set; }
        public decimal previously_received_qty { get; set; }
        public decimal received_qty { get; set; }
        public decimal balance_qty { get; set; }
        public decimal unit_price { get; set; }
        public decimal tax_pct { get; set; }
        public decimal tax_amount { get; set; }
        public decimal line_total { get; set; }
        public string remarks { get; set; }

        [ForeignKey(nameof(grn_id))]
        public virtual goodsreceiptheader? goodsreceiptheader { get; set; }
    }


}

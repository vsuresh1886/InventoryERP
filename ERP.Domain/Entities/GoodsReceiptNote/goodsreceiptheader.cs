using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.GoodsReceiptNote
{
   [Table("pur_goods_receipt_header")]
    public class goodsreceiptheader:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long? company_id { get; set; }
        public string? grn_no { get; set; }
        public DateTime grn_date { get; set; }
        public long po_id { get; set; }
        public string po_no { get; set; }
        public long vendor_id { get; set; }
        public long warehouse_id { get; set; }
        public long branch_id { get; set; }
        public string vendor_reference_no { get; set; }
        public DateTime vendor_reference_date { get; set; }
        public string vehicle_no { get; set; }
        public long received_by { get; set; }
        public int status { get; set; }
        public decimal total_qty_received { get; set; }
        public decimal sub_total { get; set; }
        public decimal tax_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal grand_total { get; set; }
        public string remarks { get; set; }
        public DateTime created_at { get; set; }
        public long created_by { get; set; }
        public DateTime modified_at { get; set; }
        public long modified_by { get; set; }
        public DateTime confirmed_at { get; set; }
        public long confirmed_by { get; set; }

        public virtual ICollection<goodsreceiptdetail> goodsreceiptdetails { get; set; } = new List<goodsreceiptdetail>();
    }
}

using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.PurchaseOrder
{
    [Table("pur_purchase_order_header")]
    public class PurchaseOrderHeader:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long po_id { get; set; }
        public string po_no { get; set; }
        public DateTime po_date { get; set; }
        public long vendor_id { get; set; }
        public long requester_id { get; set; }
        public string reference_no { get; set; }
        public string payment_terms { get; set; }
        public DateTime expected_delivery_date { get; set; }
        public long currency_id { get; set; }
        public decimal exchange_rate { get; set; }
        public long warehouse_id { get; set; }
        public string remarks { get; set; }
        public decimal subtotal { get; set; }
        public decimal discount_amount { get; set; }
        public decimal taxable_amount { get; set; }
        public decimal tax_amount { get; set; }
        public decimal round_off_amount { get; set; }
        public decimal total_amount { get; set; }
        public int status { get; set; }
        public long? company_id { get; set; }
        public long created_by { get; set; }
        public DateTime created_date { get; set; }
        public long modified_by { get; set; }
        public DateTime modified_date { get; set; }
        public bool is_deleted { get; set; }
        public long? approved_by { get; set; }
        public DateTime? approved_date { get; set; }
        public string? approved_remarks { get; set; }
    }
}

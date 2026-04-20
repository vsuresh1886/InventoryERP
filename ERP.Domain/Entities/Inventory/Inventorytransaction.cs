using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("inventory_transaction")]
    public class InventoryTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long item_id { get; set; }
        public string? transaction_type { get; set; }
        public decimal? quantity { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? total_amount { get; set; }
        public DateTime transaction_date { get; set; }
        public string? reference_type { get; set; }
        public long reference_id { get; set; }
        public long warehouse_id { get; set; }
        public long location_id { get; set; }
        public string? remarks { get; set; }
        public DateTime created_at { get; set; }
        public long created_by { get; set; }
    }
}

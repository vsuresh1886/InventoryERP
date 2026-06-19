using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("inventory_stock")]
    public class InventoryStock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long stock_id { get; set; }
            public long item_id { get; set; }
            public long warehouse_id { get; set; }
            public decimal qty_on_hand { get; set; }
            public decimal qty_reserved { get; set; }
            public decimal qty_available { get; set; }
            public decimal avg_cost { get; set; }
            public decimal last_purchase_rate { get; set; }
            public TimeSpan last_updated { get; set; }
    }
}

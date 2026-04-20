using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("item_inventory_config")]
    public class Iteminventoryconfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long item_id { get; set; }
        public decimal min_stock { get; set; }
        public decimal max_stock { get; set; }
        public decimal reorder_level { get; set; }
        public long default_location_id { get; set; }
        public DateTime created_at { get; set; }
    }
}

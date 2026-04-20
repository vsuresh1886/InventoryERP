using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("item_attributes")]
    public class Itemattributes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long item_id { get; set; }
        public string? attribute_name { get; set; }
        public string? attribute_value { get; set; }
        public DateTime created_at { get; set; }
    }
}

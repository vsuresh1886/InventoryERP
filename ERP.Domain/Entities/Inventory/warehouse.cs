using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("warehouse")]
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string name { get; set; }
        public string code { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public long created_by { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("domain_master")]
    public class Domainmaster
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string? name { get; set; }
        public string? code { get; set; }
        public string? description { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}

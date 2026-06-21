using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{
    [Table("item_master")]
    public class Itemmaster:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string? sku { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public long category_id { get; set; }
        public long sub_category_id { get; set; }
        public long domain_id { get; set; }
        public long unit_id { get; set; }
        public string? brand { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public int created_by { get; set; }
        public DateTime updated_at { get; set; }
        public int updated_by { get; set; }
        public long? company_id { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("master_dd_lookup")]
    public class masterddlookup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string lookup_type { get; set; }
        public string code { get; set; }
        public string value { get; set; }
        public int sort_order { get; set; }
        public bool is_default { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
    }
}

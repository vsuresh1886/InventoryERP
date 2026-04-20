using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("party_type")]
    public class party_type
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       public int id { get; set; }
        public string? type_name { get; set; }
    }
}

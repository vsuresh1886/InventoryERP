using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("roles")]
    public class Roles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int rolepk { get; set; }
        public string? rolename { get; set; }
        public int tenantid { get; set; }
        public bool issystem { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("userroles")]
    public class UserRoles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int us_pk { get; set; }
        public int userid { get; set; }
        public int roleid { get; set; }
    }
}

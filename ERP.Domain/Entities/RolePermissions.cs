using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("rolepermissions")]
    public class RolePermissions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int rl_pk { get; set; }
        public int roleid { get; set; }
        public int permissionid { get; set; }
    }
}

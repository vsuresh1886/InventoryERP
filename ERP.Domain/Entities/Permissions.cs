using ERP.Domain.Entitiess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace ERP.Domain.Entities
{
    [Table("permissions")] 
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int permissionpk { get; set; }  

       
        public string? code { get; set; }

        
        public string? module { get; set; }

       
        public string? action { get; set; }

      
        public ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
    }
}

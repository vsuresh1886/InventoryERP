using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security;
using System.Text;

namespace ERP.Domain.Entitiess
{
    [Table("menupermissions")]
    public class MenuPermission
    {
        public int id { get; set; }

        public long menuid { get; set; }

        public Menu Menu { get; set; }   // 

        public int permissionid { get; set; }

        public Permission Permission { get; set; }  // 
    }
}

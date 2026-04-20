using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Domain.Entities
{
    public  class User
    {
        public int employee_pk { get;set; }
        public string? email { get;set;  }
        public string? password { get; set; }
    }
}

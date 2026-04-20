using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.codegeneratordto
{

   public class CodeSequenceTracker
    {
        public int id { get; set; }
        public string module_name { get; set; }
        public string prefix { get; set; }
        public int last_number { get; set; }
        public DateTime last_reset_date { get; set; }
    }
}

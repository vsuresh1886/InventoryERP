using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.CodeGenerators
{
    [Table("code_sequence_tracker")]
    public class code_sequence_tracker
    {
        [Key]
        public int id { get; set; }
        public string module_name { get; set; }
        public string prefix { get; set; }
        public int last_number { get; set; }
        public DateTime last_reset_date { get; set; }

        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.CodeGenerators
{
    [Table("code_protocol_master")]
    public class code_protocol_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? module_name { get; set; }
        public string? prefix { get; set; }
        public bool include_date { get; set; }
        public string? date_format { get; set; }
        public int serial_length { get; set; }
        public string? separator { get; set; }
        public string? reset_frequency { get; set; }
        public bool is_active { get; set; }
    }
}

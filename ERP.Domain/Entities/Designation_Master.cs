using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table ("designation_master")]
    public class designation_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int designation_pk { get; set; }
        public string? designation_id { get; set; }
        public string? designation_name { get; set; }
        public string? description { get; set; }
        public string? status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

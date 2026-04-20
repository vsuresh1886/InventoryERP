using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("department_master")]
    public class department_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int department_pk { get; set; }
        public string department_id { get; set; }
        public string department_name { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

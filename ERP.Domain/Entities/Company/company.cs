using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ERP.Domain.Entities.Company
{
    [Table("company")]
    public class company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public string company_code { get; set; }
        public string company_name { get; set; }
        public string business_type { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public DateTime trial_end_date { get; set; }
        public bool is_active { get; set; }
        public TimeSpan created_at { get; set; }
    }
}

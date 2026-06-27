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
        public DateTime? trial_end_date { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public string? legal_name { get; set; }

        public string? short_name { get; set; }

        public string? address_line1 { get; set; }

        public string? address_line2 { get; set; }

        public string? city { get; set; }

        public string? state { get; set; }

        public string? postal_code { get; set; }

        public string? contact_person { get; set; }

        public string? alternate_phone { get; set; }

        public string? website { get; set; }

        public string? gstin { get; set; }

        public string? pan_no { get; set; }

        public string? cin_no { get; set; }

        public string? tan_no { get; set; }

        public string? tax_registration_type { get; set; }

        public string? logo_path { get; set; }

        public string? favicon_path { get; set; }

        public string? currency_code { get; set; }

        public string? currency_symbol { get; set; }

        public string? timezone { get; set; }

        public string? language_code { get; set; }

        public string? default_date_format { get; set; }

        public DateTime? updated_at { get; set; }

        public long? created_by { get; set; }

        public long? updated_by { get; set; }
    }
}

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
        public DateTime created_at { get; set; }
        public string? legalname { get; set; }

        public string? shortname { get; set; }

        public string? address_line1 { get; set; }

        public string? address_line2 { get; set; }

        public string? city { get; set; }

        public string? state { get; set; }

        public string? postalcode { get; set; }

        public string? contactperson { get; set; }

        public string? alternatephone { get; set; }

        public string? website { get; set; }

        public string? gstin { get; set; }

        public string? panno { get; set; }

        public string? cinno { get; set; }

        public string? tanno { get; set; }

        public string? taxregistrationtype { get; set; }

        public string? logopath { get; set; }

        public string? faviconpath { get; set; }

        public string? currencycode { get; set; }

        public string? currencysymbol { get; set; }

        public string? timezone { get; set; }

        public string? languagecode { get; set; }

        public string? defaultdateformat { get; set; }

        public DateTime? trialenddate { get; set; }

        public bool isActive { get; set; }

        public DateTime createdat { get; set; }

        public DateTime? updatedat { get; set; }

        public long? createdby { get; set; }

        public long? updatedby { get; set; }
    }
}

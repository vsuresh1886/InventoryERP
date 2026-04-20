using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("country_master")]
    public class country_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public string? country_code { get; set; }
        public string? country_name { get; set; }
        public string? iso2_code { get; set; }
        public string? iso3_code { get; set; }
        public string? phone_code { get; set; }
        public string? currency_code { get; set; }
        public string? currency_name { get; set; }
        public string? time_zone { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}

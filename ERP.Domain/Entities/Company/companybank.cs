using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Company
{
    [Table("company_bank")]
    public class companybank:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
        public long? company_id { get; set; }
        public string? account_name { get; set; }
        public string? bank_name { get; set; }
        public string? branch_name { get; set; }
        public string? account_number { get; set; }
        public string? ifsc_code { get; set; }
        public string? swift_code { get; set; }
        public string? iban { get; set; }
        public string? upi_id { get; set; }
        public string? currency_code { get; set; }
        public bool is_default { get; set; }
        public bool is_active { get; set; }
        public string? remarks { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
}

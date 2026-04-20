using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("customer_master")]
    public class customer_master
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int cust_pk { get; set; }
        public string? customer_code { get; set; }
        public string? customer_name { get; set; }
        public string? company_name { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? mobile { get; set; }
        public string? address_line1 { get; set; }
        public string? address_line2 { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public int country { get; set; }
        public string? postal_code { get; set; }
        public string? tax_id { get; set; }
        public decimal credit_limit { get; set; }
        public string? payment_terms { get; set; }
        public string? website { get; set; }
        public string? status { get; set; }
        public int partytype { get; set; }
        public DateTime created_at { get; set; } 
        public DateTime updated_at { get; set; } 
    }
}

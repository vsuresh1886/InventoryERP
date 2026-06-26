using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Application.DTOs
{
    public class CustomergridDto
    {
        public int Sno { get; set; }
        public int Id { get; set; }
        public string? customerid { get; set; }
        public string? customername { get; set; }
        public string? companyname { get; set; }
        public string? email { get; set; }
        public string? phone { get; set; }
        public string? mobile { get; set; }
        public string? address_line1 { get; set; }
        public string? address_line2 { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? postal_code { get; set; }
        public string? tax_id { get; set; }
        public decimal credit_limit { get; set; }
        public string? payment_terms { get; set; }
        public string? website { get; set; }
        public string? status { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; }
        public string? partytype { get; set; }
        public string? Actions { get; set; }
    }

    public class CustomerDto
        {
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
        public int country { get; set; } = 1;
        public string? postal_code { get; set; }
        public string? tax_id { get; set; }
        public decimal credit_limit { get; set; }
        public string? payment_terms { get; set; }
        public string? website { get; set; }
        public string? status { get; set; }
        public int partytype { get; set; }
        public DateTime created_at { get; set; } = DateTime.Now;
        public DateTime updated_at { get; set; }
    }

    public class CustomerTempDto
    {
        public string? customer_name { get; set; }
        public string? mobile { get; set; }
        public string? address_line1 { get; set; }

        public int partytype { get; set; }

    }

}

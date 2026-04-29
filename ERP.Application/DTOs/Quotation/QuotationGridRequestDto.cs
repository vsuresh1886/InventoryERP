using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Quotation
{
    public class QuotationGridRequestDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public int? quotationno { get; set; }
        public List<long>? customer { get; set; }
         public List<long>? salesperson { get; set; }
        public string? datefrom { get; set; }
        public string? dateto { get; set; }
        public int? status { get; set; }

    }

    public class QuotationGridDto
    {
        public int sno { get; set; }
        public long id { get; set; }
        public string? quono { get; set; }
        public string? customerid { get; set; }
        public string? salesperson { get; set; }
        public DateTime? validfrom { get; set; }
        public DateTime? validto { get; set; }
        public string? status { get; set; }

        public int? totalItems { get; set; }
        public decimal amount { get; set; }
        public string? actions { get; set; }
    }
}

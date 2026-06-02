using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class AiSearchRequestDto
    {
        public string Module { get; set; }

        public string Query { get; set; }
    }

    public class AiQuotationRawDto
    {

        public string? customer { get; set; }

        public string? salesperson { get; set; }

        public string? status { get; set; }

        public string? fromDate { get; set; }

        public string? toDate { get; set; }
    }
}

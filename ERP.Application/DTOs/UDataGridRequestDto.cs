using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class UDataGridRequestDto
    {
       
            public int Page { get; set; }
            public int Limit { get; set; }
            public string? Search { get; set; }
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string? Department { get; set; }
            public string? Status { get; set; }
            public string? Role { get; set; }

    }

    public class CDataGridRequestDto
    {

        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public string? customerName { get; set; }
        public string? Email { get; set; }
        public int partytype { get; set; }
        public string? status { get; set; }
        public int country { get; set; }

    }
}

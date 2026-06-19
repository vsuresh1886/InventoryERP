using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.company
{
    public class CompanySignupDto
    {
        public string CompanyName { get; set; } = "";

        public string BusinessType { get; set; } = "";

        public string Country { get; set; } = "";

        public string Email { get; set; } = "";

        public string Phone { get; set; } = "";

        public string AdminName { get; set; } = "";

        public string Password { get; set; } = "";
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ERP.Application.DTOs.company
{
    public class CompanySignupDto
    {
        public string companyName { get; set; } = "";

        public string businessType { get; set; } = "";

        public string country { get; set; } = "";
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string companyEmail { get; set; } = "";

        public string phone { get; set; } = "";

        public string adminName { get; set; } = "";
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "The password must be at least {2} characters long.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string adminEmail { get; set; } = "";
    }
}

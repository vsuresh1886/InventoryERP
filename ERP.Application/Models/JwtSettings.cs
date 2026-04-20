using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ERP.Application.Models
{
    public  class JwtSettings
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int ExpiryMinutes { get; set; } = 120;

    }
}

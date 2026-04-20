using System;
using System.Collections.Generic;
using System.Text;
using ERP.Application.Interfaces;

namespace ERP.Application.DTOs
{
    public class Auth
    {
        public class LoginRequestDto
        {
            public string? username { get; set; }
            public string? password { get; set; }
        }
        public class AuthResponseDto
        {
            public string? Token { get; set; }
            public string? Email { get; set; }
        }

    }
}

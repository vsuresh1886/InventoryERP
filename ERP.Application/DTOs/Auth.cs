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

            public List<UserDet> user { get; set; }
            
        }

        public class UserDet
        {
            public long id { get; set; }
            public string? name { get; set; }

            public string? designation { get; set; }
            public string? photoUrl { get; set; }
            public string? companyName { get; set; }
        }

    }
}

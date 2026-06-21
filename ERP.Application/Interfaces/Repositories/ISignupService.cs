using ERP.Application.DTOs.company;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface ISignupService
    {
        Task<String> SignupCompany(CompanySignupDto dtos);

    }
}

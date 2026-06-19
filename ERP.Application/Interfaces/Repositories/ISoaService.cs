using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface ISoaService
    {
        Task<List<CustomerSOADto>> FetchCustomer_rep(CustomerSOAFilterDto filter);
        Task<string> EmailCustomer_rep(CustomerSOAFilterDto filters);

    }
}

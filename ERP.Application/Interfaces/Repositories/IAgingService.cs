using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IAgingService
    {

        Task<List<CustomerAgingCustomerDto>> CustomerAging_rep(CustomerAgingFilterDto filter);
        Task<string> CustomerAgingEmail_rep(CustomerAgingFilterDto filters);

    }
}


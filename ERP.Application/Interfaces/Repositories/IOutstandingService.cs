using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IOutstandingService
    {
        Task<List<CustomerOutstandingDto>> FetchCustomer_OS(ReceivableOutstandingFiltersDto filter);
        Task<string> EmailCustomer_OS(ReceivableOutstandingFiltersDto filters);


    }
}

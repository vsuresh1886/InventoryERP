using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IExcelService
    {
        Task<byte[]> Gen_customerSOA(dynamic customers);
        Task<byte[]> Gen_customerAging(List<CustomerAgingCustomerDto> customers);
        Task<byte[]> Gen_customerOS(List<CustomerOutstandingDto> customers);
    }
}

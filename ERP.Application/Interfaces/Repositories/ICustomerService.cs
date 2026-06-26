using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface ICustomerService
    {
        Task<GridDataResponse<CustomergridDto>> FetchGridData(CDataGridRequestDto filters);
        Task<CustomerDto> FetchCustomer(int CustomerCode);

        Task<CustomerDto> CreateUpdateCustomer(CustomerDto Customer);
        Task<List<DropdownDto>> CreateUpdateCustomerT(CustomerDto Customer);
    }
}

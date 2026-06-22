using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IUserService
    {
        Task<GridDataResponse<EmployeeDto>> FetchGridData(UDataGridRequestDto filters);
        Task<EmployeeDetailDto> FetchUser(long employeeCode);
        Task<UserDetDto>  FetchUserDet(long userId);
        Task<EmployeeSaveDto> CreateUpdateUser(EmployeeSaveDto employee);
    }
}

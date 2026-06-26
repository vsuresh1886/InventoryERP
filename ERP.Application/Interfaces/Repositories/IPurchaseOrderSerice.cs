using ERP.Application.DTOs;
using ERP.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IPurchaseOrderSerice
    {
        Task<List<DropdownDto>> GetPoIds();

        Task<List<DropdownDto>> getPObyvendorId(long id);

        Task<GridDataResponse<PurchaseOrderGridDto>> GetPurchaseOrderGrid(PurchaseGridRequestDto podtls);

        Task<PurchaseOrderDto> Createupdatepo(PurchaseOrderDto dto);

        Task<PurchaseOrderDto> GetPurchaseOrderById(long id);
    }
}

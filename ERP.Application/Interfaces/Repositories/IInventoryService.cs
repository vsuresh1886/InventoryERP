using ERP.Application.DTOs;
using ERP.Application.DTOs.Inventory;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IInventoryService
    {
        Task<GridDataResponse<InventoryGridDto>> Getinventory(InvDataGridRequestDto gridRequestDto);

        Task<InventoryItemDto> GetInvdtl(int id);
        Task<InventoryItemDto> CreateUpdateInv(InventoryItemDto inventoryItem);

    }
}

using ERP.Application.DTOs;
using ERP.Application.DTOs.GoodsReceiptNote;
using ERP.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IGRNService
    {
        Task<List<DropdownDto>> getGRNids();
        Task<GridDataResponse<GoodsReceiptListDto>> GetGRNGrid(GRNGridRequestDto  podtls);

        Task<GRNPOlistDto> GetPOForGRNByIdAsync(long poId);

        Task<GoodsReceiptDto> CreateupdateGRN(GoodsReceiptDto dto);

        Task<GoodsReceiptDto> GetGRNById(long id);
    }
}

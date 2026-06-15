using ERP.Application.DTOs;
using ERP.Application.DTOs.SalesInvoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface ISalesReturnService
    {
        Task<List<DropdownDto>> FetchInvoiceCust(long id);
        Task<GridDataResponse<SRGridDto>> FetchReturndtls(SRGridRequestsDto InvRetDto);

        Task<SalesReturnInvoiceDto> getInvoiceById(long id);

        Task<SalesReturnDto> getReturnsById(long id);

        Task<SalesReturnDto> CreateUpdateSaleReturn(SalesReturnDto salesReturnDto);

    }
}

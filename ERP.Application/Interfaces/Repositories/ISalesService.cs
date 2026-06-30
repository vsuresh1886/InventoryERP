using ERP.Application.DTOs;
using ERP.Application.DTOs.SalesInvoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface ISalesService
    {
        Task<List<DropdownDto>> FetchInvoiceids();

        Task<GridDataResponse<InvoiceGridDto>> FetchInvoices(InvoiceGridRequestDto InvReqDto);

        Task<InvoiceDto> getInvoiceById(long id);

        Task<InvoiceDto> CreateUpdateInvoice(InvoiceDto InvDto);
        Task<byte[]> Salespdfdata_new(long id);
    }
}

using ERP.Application.DTOs;
using ERP.Application.DTOs.Quotation;
using ERP.Application.Models.Quotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IQuotationService
    {
       Task<List<DropdownDto>> Fetchquotationid();
       Task<GridDataResponse<QuotationGridDto>> Fetchquotations(QuotationGridRequestDto quotationReqDto);

        Task<QuotationDto> CreateUpdateQuot(QuotationDto quotationDto);

        Task<QuotationDto> getQuotationbyid(string id);

        Task<QuotationModel> quotationpdfdata(int id);

        Task<byte[]> quotationpdfdata_new(int id);

    } 
}

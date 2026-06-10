using ERP.Application.DTOs;
using ERP.Application.Models.Quotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface IPdfService
    {
        byte[] GenerateQuotationPdf(QuotationModel model);
        Task<byte[]> Gen_customerSOAPdf(dynamic customers);
        Task<byte[]> Gen_customerAgingPdf(List<CustomerAgingCustomerDto> customers);
    }
}

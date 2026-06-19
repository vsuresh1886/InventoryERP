using ERP.Application.DTOs.Quotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface IAiService
    {
        Task<AiQuotationfilterDto> ParseQuotationQuery(string query);

    }
}

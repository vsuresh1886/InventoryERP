using ERP.Application.Models.Quotation;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public  interface IPdfService
    {
        byte[] GenerateQuotationPdf(QuotationModel model);
    }
}

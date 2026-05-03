using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models.Quotation;
using ERP.Infrastructure.Document;
using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;

namespace ERP.Infrastructure.Repositories
{
    public  class PdfService : IPdfService
    {


        public byte[] GenerateQuotationPdf(QuotationModel model)
        {
            var document = new QuotationDocument(model);
            return document.GeneratePdf();
        }


    }
}

using ERP.Application.Models;
using ERP.Application.Models.Quotation;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ERP.Infrastructure.Document
{
    public  class QuotationDocument : IDocument
    {
        private readonly QuotationModel _data;
        
        public QuotationDocument(QuotationModel data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        
        
        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(20);

                page.Header().Text("QUOTATION").Bold().FontSize(18).AlignRight();

                page.Content().Column(col =>
                {
                    col.Item().Text(_data.companyname).Bold();
                    col.Item().Text(_data.companyaddress);
                    col.Item().Text($"GSTIN: {_data.gstin}");

                   
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // 🔹 LEFT: CUSTOMER DETAILS
                        row.RelativeColumn().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("Bill To").Bold().FontSize(11);
                            c.Item().Text(_data.contactperson);
                            c.Item().Text(_data.customername);
                            c.Item().Text(_data.customeraddress);

                            if (!string.IsNullOrEmpty(_data.contactperson))
                                c.Item().Text($"Contact : {_data.contact}");
                        });

                        // 🔹 RIGHT: QUOTATION INFO
                        row.RelativeColumn().Border(1).Padding(8).Column(c =>
                        {
                            c.Item().Text("Quotation Details").Bold().FontSize(11);

                            c.Item().Row(r =>
                            {
                                r.RelativeColumn().Text("Quote No:");
                                r.ConstantColumn(120).AlignRight().Text(_data.quotationno).Bold();
                            });

                            c.Item().Row(r =>
                            {
                                r.RelativeColumn().Text("Date:");
                                r.ConstantColumn(120).AlignRight().Text(_data.quotationdate.ToString("dd-MM-yyyy"));
                            });

                            if (!string.IsNullOrEmpty(_data.validity))
                            {
                                c.Item().Row(r =>
                                {
                                    r.RelativeColumn().Text("Validity:");
                                    r.ConstantColumn(120).AlignRight().Text(_data.validity);
                                });
                            }
                        });
                    });

                    col.Item().PaddingTop(10).Element(ComposeTable);

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // 🔹 LEFT: Amount in words
                        row.RelativeColumn().Text($"Amount in words: {_data.amtinwords}");

                        // 🔹 RIGHT: Totals aligned with table
                        row.ConstantColumn(220).Column(col2 =>
                        {
                            col2.Item().Element(c=> TotalRow(c, "Sub Total", _data.subtotal));
                            col2.Item().Element(c => TotalRow(c, "tax", _data.gst));
                           // col2.Item().Element(TotalRow("SGST", _data.SGST));
                            col2.Item().Element(c => TotalRow(c, "Discount", _data.discount));
                            col2.Item().Element(c => TotalRow(c, "Grand Total", _data.total, true));
                        });
                    });
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }


        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                    columns.ConstantColumn(50);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    
                    header.Cell().Element(CellStyle).AlignCenter().Text("S.No").Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Part No").Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Description").Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Qty").Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Unit Rate").Bold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Rate").Bold();

                });

                int index = 1;

                foreach (var item in _data.items)
                {
                    table.Cell().Element(CellStyle).Text(index++.ToString());
                    table.Cell().Element(CellStyle).Text(item.partno);
                    table.Cell().Element(CellStyle).Text(item.partname);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text(item.unitprice.ToString("N2"));
                    table.Cell().Element(CellStyle).AlignRight().Text(item.totalprice.ToString("N2"));

                }
            });
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .Padding(5)
                .BorderColor("#999999");
        }
        void TotalRow(IContainer container, string label, decimal value, bool isBold = false)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Text(label).SemiBold();

                row.ConstantColumn(100)
                   .AlignRight()
                   .Text(value.ToString("N2"))
                   .SemiBold();
            });
        }

    }
}

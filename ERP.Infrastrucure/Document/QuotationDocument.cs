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

        // Use QuestPDF's Color.FromHex to ensure they are the correct type
        private readonly Color PrimaryColor = Color.FromHex("#1E293B");
        private readonly Color AccentColor = Color.FromHex("#0F766E");
        private readonly Color TextColor = Color.FromHex("#334155");
        private readonly Color LightGray = Color.FromHex("#F8FAFC");
        private readonly Color BorderColor = Color.FromHex("#E2E8F0");

        public QuotationDocument(QuotationModel data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;


        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40); // Increased margin for breathing room
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextColor));

                // --- HEADER ---
                page.Header().Row(row =>
                {
                    row.RelativeColumn().Column(col =>
                    {
                        col.Item().Text(_data.companyname).Bold().FontSize(20).FontColor(PrimaryColor);
                        col.Item().Text(_data.companyaddress).FontSize(9).FontColor("#64748B");
                        if (!string.IsNullOrEmpty(_data.gstin))
                            col.Item().Text($"GSTIN: {_data.gstin}").FontSize(9).FontColor("#64748B");
                    });

                    row.RelativeColumn().AlignRight().Column(col =>
                    {
                        col.Item().Text("QUOTATION").Bold().FontSize(24).FontColor(AccentColor);
                        col.Item().PaddingTop(5).Text($"Quote No: {_data.quotationno}").Bold();
                        col.Item().Text($"Date: {_data.quotationdate.ToString("dd-MM-yyyy")}");
                    });
                });

                // --- CONTENT ---
                page.Content().Column(col =>
                {
                    // Customer & Meta Info Cards
                    col.Item().PaddingTop(25).Row(row =>
                    {
                        // Bill To Section
                        row.RelativeColumn().Border(1).BorderColor(BorderColor).Background(LightGray).Padding(12).Column(c =>
                        {
                            c.Item().Text("BILL TO").Bold().FontSize(9).FontColor("#64748B");
                            c.Item().PaddingTop(4).Text(_data.customername).Bold().FontSize(11).FontColor(PrimaryColor);
                            if (!string.IsNullOrEmpty(_data.contactperson))
                                c.Item().Text($"Attn: {_data.contactperson}");
                            c.Item().Text(_data.customeraddress);

                            if (!string.IsNullOrEmpty(_data.contact))
                                c.Item().Text($"Contact: {_data.contact}").FontSize(9);
                        });

                        row.ConstantColumn(20); // Spacer

                        // Quotation Details Section
                        row.RelativeColumn().Border(1).BorderColor(BorderColor).Padding(12).Column(c =>
                        {
                            c.Item().Text("QUOTATION DETAILS").Bold().FontSize(9).FontColor("#64748B");

                            c.Item().PaddingTop(6).Row(r =>
                            {
                                r.RelativeColumn().Text("Validity:");
                                r.ConstantColumn(120).AlignRight().Text(!string.IsNullOrEmpty(_data.validity) ? _data.validity : "N/A").Bold();
                            });

                            // You can add more metadata rows here seamlessly
                        });
                    });

                    // Items Table
                    col.Item().PaddingTop(25).Element(ComposeTable);

                    // Totals & Summary
                    col.Item().PaddingTop(20).Row(row =>
                    {
                        // Left: Amount in words
                        row.RelativeColumn().PaddingRight(40).Column(c =>
                        {
                            if (!string.IsNullOrEmpty(_data.amtinwords))
                            {
                                c.Item().Text("Amount in Words:").Bold().FontSize(9).FontColor("#64748B");
                                c.Item().Text(_data.amtinwords).Italic().FontSize(10);
                            }
                        });

                        // Right: Financial Summary
                        row.ConstantColumn(240).Column(col2 =>
                        {
                            col2.Item().Element(c => TotalRow(c, "Sub Total", _data.subtotal));

                            if (_data.gst > 0)
                                col2.Item().Element(c => TotalRow(c, "Tax (GST)", _data.gst));

                            if (_data.discount > 0)
                                col2.Item().Element(c => TotalRow(c, "Discount", _data.discount, isNegative: true));

                            col2.Item().PaddingTop(4).BorderTop(1).BorderColor(PrimaryColor).Element(c => TotalRow(c, "Grand Total", _data.total, isBold: true));
                        });
                    });
                });

                // --- FOOTER ---
                page.Footer().AlignCenter().PaddingTop(20).Text(x =>
                {
                    x.DefaultTextStyle(t => t.FontSize(9).FontColor("#94A3B8"));
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
                    columns.ConstantColumn(40);   // S.No
                    columns.ConstantColumn(90);   // Part No
                    columns.RelativeColumn();     // Description
                    columns.ConstantColumn(50);   // Qty
                    columns.ConstantColumn(90);   // Unit Rate
                    columns.ConstantColumn(100);  // Total Rate
                });

                // Table Header Setup
                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("S.No");
                    header.Cell().Element(HeaderStyle).Text("Part No");
                    header.Cell().Element(HeaderStyle).Text("Description");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Qty");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Unit Rate");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Amount");
                });

                int index = 1;

                foreach (var item in _data.items)
                {
                    // Dynamic background row color for zebra-striping
                    var rowBg = index % 2 == 0 ? LightGray : Colors.White;

                    table.Cell().Background(rowBg).Element(CellStyle).AlignCenter().Text(index++.ToString());
                    table.Cell().Background(rowBg).Element(CellStyle).Text(item.partno ?? "-");
                    table.Cell().Background(rowBg).Element(CellStyle).Text(item.partname);
                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight().Text(item.quantity.ToString());
                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight().Text(item.unitprice.ToString("N2"));
                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight().Text(item.totalprice.ToString("N2"));
                }
            });
        }
        // Header Cell Stylist
        private IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(PrimaryColor)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .AlignMiddle()
                .DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(9));
        }

        // Body Cell Stylist
        private IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(BorderColor)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .AlignMiddle();
        }
        void TotalRow(IContainer container, string label, decimal value, bool isBold = false, bool isNegative = false)
        {
            container.PaddingVertical(3).Row(row =>
            {
                // Define standard base style
                var textStyle = TextStyle.Default.FontSize(10).FontColor(TextColor);

                // Apply conditional bolding/sizing
                if (isBold)
                {
                    textStyle = textStyle.Bold().FontSize(11).FontColor(PrimaryColor);
                }

                // Left Label
                row.RelativeColumn().AlignLeft().Text(label).Style(textStyle);

                // Right Value
                var formattedValue = isNegative ? $"-{value.ToString("N2")}" : value.ToString("N2");
                row.ConstantColumn(120).AlignRight().Text(formattedValue).Style(textStyle);
            });
        }

    }
}

using ERP.Application.Models;
using ERP.Application.Models.Quotation;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;

namespace ERP.Infrastructure.Document
{
    public class TemplateDocument : IDocument
    {
        private readonly QuotationModel _data;

        // ---- Palette ----
        // Keep every color as a QuestPDF Color so .FontColor()/.Background() always accept it directly.
        private static readonly Color PrimaryColor = Color.FromHex("#1E293B");   // slate-800 (headings, table header bg)
        private static readonly Color AccentColor = Color.FromHex("#0F766E");    // teal-700 (brand accent)
        private static readonly Color AccentSoft = Color.FromHex("#ECFDF5");    // very light teal (grand total bg)
        private static readonly Color TextColor = Color.FromHex("#334155");      // slate-700 (body text)
        private static readonly Color MutedColor = Color.FromHex("#64748B");     // slate-500 (labels/captions)
        private static readonly Color FaintColor = Color.FromHex("#94A3B8");     // slate-400 (footer)
        private static readonly Color LightGray = Color.FromHex("#F8FAFC");      // slate-50 (zebra rows / card bg)
        private static readonly Color BorderColor = Color.FromHex("#E2E8F0");    // slate-200 (rules / borders)

        public TemplateDocument(QuotationModel data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(TextColor).FontFamily(Fonts.Calibri));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        // ---------------------------------------------------------------
        // HEADER
        // ---------------------------------------------------------------
        void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Company block
                    row.RelativeColumn(1.4f).Column(c =>
                    {
                        c.Item().Text(_data.companyname)
                            .Bold().FontSize(18).FontColor(PrimaryColor);

                        c.Item().PaddingTop(3).Text(_data.companyaddress)
                            .FontSize(8.5f).FontColor(MutedColor).LineHeight(1.3f);

                        if (!string.IsNullOrEmpty(_data.gstin))
                            c.Item().PaddingTop(2).Text($"GSTIN: {_data.gstin}")
                                .FontSize(8.5f).FontColor(MutedColor);
                    });

                    // Title + meta block
                    row.RelativeColumn().AlignRight().Column(c =>
                    {
                        c.Item().Text("QUOTATION")
                            .Bold().FontSize(22).FontColor(AccentColor).LetterSpacing(0.05f);

                        c.Item().PaddingTop(8).Row(r =>
                        {
                            r.AutoItem().Text("Quote No  ").FontSize(9).FontColor(MutedColor);
                            r.AutoItem().Text(_data.quotationno).Bold().FontSize(9).FontColor(PrimaryColor);
                        });

                        c.Item().PaddingTop(2).Row(r =>
                        {
                            r.AutoItem().Text("Date  ").FontSize(9).FontColor(MutedColor);
                            r.AutoItem().Text(_data.quotationdate.ToString("dd-MM-yyyy")).Bold().FontSize(9).FontColor(PrimaryColor);
                        });
                    });
                });

                // Accent rule under the header block
                col.Item().PaddingTop(14).LineHorizontal(2).LineColor(AccentColor);
            });
        }

        // ---------------------------------------------------------------
        // CONTENT
        // ---------------------------------------------------------------
        void ComposeContent(IContainer container)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Item().Element(ComposeInfoCards);
                col.Item().PaddingTop(22).Element(ComposeTable);
                col.Item().PaddingTop(18).Element(ComposeSummary);

                if (!string.IsNullOrEmpty(_data.notes))
                {
                    col.Item().PaddingTop(20).Element(c => ComposeNotes(c, _data.notes));
                }
            });
        }

        void ComposeInfoCards(IContainer container)
        {
            container.Row(row =>
            {
                // Bill To
                row.RelativeColumn().Element(c => InfoCard(c, "BILL TO", inner =>
                {
                    inner.Item().Text(_data.customername).Bold().FontSize(11).FontColor(PrimaryColor);

                    if (!string.IsNullOrEmpty(_data.contactperson))
                        inner.Item().PaddingTop(3).Text($"Attn: {_data.contactperson}").FontSize(9);

                    inner.Item().PaddingTop(3).Text(_data.customeraddress).FontSize(9).LineHeight(1.3f);

                    if (!string.IsNullOrEmpty(_data.contact))
                        inner.Item().PaddingTop(4).Text(_data.contact).FontSize(9).FontColor(MutedColor);
                }));

                row.ConstantColumn(20);

                // Quotation Details
                row.RelativeColumn().Element(c => InfoCard(c, "QUOTATION DETAILS", inner =>
                {
                    inner.Item().Element(e => DetailLine(e, "Validity", !string.IsNullOrEmpty(_data.validity) ? _data.validity : "N/A"));
                    // Add more DetailLine(...) calls here for additional metadata (payment terms, delivery, etc.)
                }));
            });
        }

        // Reusable card shell: left accent bar + light fill, label header, then custom inner content.
        void InfoCard(IContainer container, string label, Action<ColumnDescriptor> content)
        {
            container
                .Background(LightGray)
                .BorderLeft(3).BorderColor(AccentColor)
                .Padding(14)
                .Column(c =>
                {
                    c.Item().Text(label).Bold().FontSize(8.5f).FontColor(MutedColor).LetterSpacing(0.04f);
                    c.Item().PaddingTop(6).Column(content);
                });
        }

        void DetailLine(IContainer container, string label, string value)
        {
            container.PaddingTop(2).Row(r =>
            {
                r.RelativeColumn().Text(label).FontSize(9).FontColor(MutedColor);
                r.RelativeColumn().AlignRight().Text(value).Bold().FontSize(9).FontColor(PrimaryColor);
            });
        }

        void ComposeTable(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(36);   // S.No
                    columns.ConstantColumn(85);   // Part No
                    columns.RelativeColumn();     // Description
                    columns.ConstantColumn(45);   // Qty
                    columns.ConstantColumn(85);   // Unit Rate
                    columns.ConstantColumn(95);   // Amount
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).AlignCenter().Text("#");
                    header.Cell().Element(HeaderStyle).Text("PART NO");
                    header.Cell().Element(HeaderStyle).Text("DESCRIPTION");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("QTY");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("UNIT RATE");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("GST AMOUNT");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("AMOUNT");

                    // Thin accent rule directly under the header row
                    header.Cell().ColumnSpan(6).PaddingTop(1).LineHorizontal(1.5f).LineColor(AccentColor);
                });

                int index = 1;
                foreach (var item in _data.items)
                {
                    var isEven = index % 2 == 0;
                    var rowBg = isEven ? LightGray : Colors.White;

                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven)).AlignCenter()
                        .Text(index.ToString()).FontColor(MutedColor);
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven))
                        .Text(item.partno ?? "-");
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven))
                        .Text(item.partname).FontColor(PrimaryColor);
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven)).AlignRight()
                        .Text(item.quantity.ToString());
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven)).AlignRight()
                        .Text(item.unitprice.ToString("N2"));
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven)).AlignRight()
                        .Text(item.vatamt.ToString("N2"));
                    table.Cell().Background(rowBg).Element(c => CellStyle(c, isEven)).AlignRight()
                        .Text(item.totalprice.ToString("N2")).Bold().FontColor(PrimaryColor);

                    index++;
                }
            });
        }

        private IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(PrimaryColor)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .AlignMiddle()
                .DefaultTextStyle(x => x.Bold().FontColor(Colors.White).FontSize(8.5f).LetterSpacing(0.03f));
        }

        private IContainer CellStyle(IContainer container, bool isEven)
        {
            return container
                .BorderBottom(1)
                .BorderColor(BorderColor)
                .PaddingVertical(9)
                .PaddingHorizontal(8)
                .AlignMiddle();
        }

        // ---------------------------------------------------------------
        // SUMMARY (amount in words + totals)
        // ---------------------------------------------------------------
        void ComposeSummary(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().PaddingRight(30).PaddingTop(4).Column(c =>
                {
                    if (!string.IsNullOrEmpty(_data.amtinwords))
                    {
                        c.Item().Text("AMOUNT IN WORDS").Bold().FontSize(8.5f).FontColor(MutedColor).LetterSpacing(0.04f);
                        c.Item().PaddingTop(4).Text(_data.amtinwords).Italic().FontSize(9.5f).FontColor(TextColor).LineHeight(1.3f);
                    }
                });

                row.ConstantColumn(230).Column(c =>
                {
                    c.Item().Element(e => TotalRow(e, "Sub Total", _data.subtotal));

                    if (_data.gst > 0)
                        c.Item().Element(e => TotalRow(e, "Tax (GST)", _data.gst));

                    if (_data.discount > 0)
                        c.Item().Element(e => TotalRow(e, "Discount", _data.discount, isNegative: true));

                    c.Item().PaddingTop(8).Background(AccentSoft).Padding(10)
                        .Element(e => TotalRow(e, "GRAND TOTAL", _data.total, isBold: true));
                });
            });
        }

        void TotalRow(IContainer container, string label, decimal value, bool isBold = false, bool isNegative = false)
        {
            container.PaddingVertical(isBold ? 0 : 4).Row(row =>
            {
                var labelStyle = isBold
                    ? TextStyle.Default.FontSize(11).Bold().FontColor(AccentColor).LetterSpacing(0.03f)
                    : TextStyle.Default.FontSize(9.5f).FontColor(MutedColor);

                var valueStyle = isBold
                    ? TextStyle.Default.FontSize(13).Bold().FontColor(PrimaryColor)
                    : TextStyle.Default.FontSize(9.5f).FontColor(TextColor);

                row.RelativeColumn().AlignLeft().AlignMiddle().Text(label).Style(labelStyle);

                var formattedValue = isNegative ? $"- {value:N2}" : value.ToString("N2");
                row.ConstantColumn(110).AlignRight().Text(formattedValue).Style(valueStyle);
            });
        }

        // ---------------------------------------------------------------
        // NOTES / TERMS (optional)
        // ---------------------------------------------------------------
        void ComposeNotes(IContainer container, string notes)
        {
            container.BorderTop(1).BorderColor(BorderColor).PaddingTop(12).Column(c =>
            {
                c.Item().Text("NOTES & TERMS").Bold().FontSize(8.5f).FontColor(MutedColor).LetterSpacing(0.04f);
                c.Item().PaddingTop(5).Text(notes).FontSize(8.5f).FontColor(TextColor).LineHeight(1.4f);
            });
        }

        // ---------------------------------------------------------------
        // FOOTER
        // ---------------------------------------------------------------
        void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(BorderColor);

                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeColumn().Text(_data.companyname)
                        .FontSize(8).FontColor(FaintColor);

                    row.RelativeColumn().AlignRight().Text(x =>
                    {
                        x.DefaultTextStyle(t => t.FontSize(8).FontColor(FaintColor));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
        }
    }
}
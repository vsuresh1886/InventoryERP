using ERP.Application.Models.common;
using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace ERP.Infrastructure.Document.Components
{
    public static class ItemsTableComponent
    {
        public static void Compose(IContainer container, IEnumerable<DocumentLineItem> items, bool showTax = true)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(28);   // S.No
                    columns.RelativeColumn(2.0f); // Item / Description

                    if (showTax)
                        columns.ConstantColumn(55);   // HSN/SAC — GST-specific, omitted when tax is off

                    columns.ConstantColumn(36);   // Qty
                    columns.ConstantColumn(62);   // Rate

                    if (showTax)
                    {
                        columns.ConstantColumn(72); // Taxable Value
                        columns.ConstantColumn(58); // CGST
                        columns.ConstantColumn(58); // SGST
                    }

                    columns.ConstantColumn(74);   // Amount
                });

                table.Header(header =>
                {
                    int colSpan = showTax ? 9 : 5;

                    header.Cell().Element(HeaderStyle).AlignCenter().Text("#");
                    header.Cell().Element(HeaderStyle).Text("ITEM / DESCRIPTION");

                    if (showTax)
                        header.Cell().Element(HeaderStyle).AlignCenter().Text("HSN/SAC");

                    header.Cell().Element(HeaderStyle).AlignRight().Text("QTY");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("RATE");

                    if (showTax)
                    {
                        header.Cell().Element(HeaderStyle).AlignRight().Text("TAXABLE VAL");
                        header.Cell().Element(HeaderStyle).AlignRight().Text("CGST");
                        header.Cell().Element(HeaderStyle).AlignRight().Text("SGST");
                    }

                    header.Cell().Element(HeaderStyle).AlignRight().Text("AMOUNT");

                    header.Cell().ColumnSpan((uint)colSpan).PaddingTop(1)
                        .LineHorizontal(1.5f).LineColor(Layout.DocumentStyles.AccentColor);
                });

                int index = 1;
                foreach (var item in items)
                {
                    var isEven = index % 2 == 0;
                    var rowBg = isEven ? Layout.DocumentStyles.LightGray : Colors.White;

                    table.Cell().Background(rowBg).Element(CellStyle).AlignCenter()
                        .Text(index.ToString()).FontColor(Layout.DocumentStyles.MutedColor);

                    table.Cell().Background(rowBg).Element(CellStyle).Text(item.PartName)
                        .FontColor(Layout.DocumentStyles.PrimaryColor);

                    if (showTax)
                        table.Cell().Background(rowBg).Element(CellStyle).AlignCenter()
                            .Text(item.HsnSac ?? "-").FontSize(Layout.DocumentStyles.SmallSize);

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(FormatQty(item.Quantity, item.Uom));

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(item.UnitPrice.ToString("N2"));

                    if (showTax)
                    {
                        table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                            .Text(item.TaxableValue.ToString("N2"));

                        table.Cell().Background(rowBg).Element(CellStyle).AlignRight().Column(c =>
                        {
                            c.Item().Text(item.CgstAmount.ToString("N2"));
                            if (item.CgstRate > 0)
                                c.Item().Text($"{item.CgstRate:0.##}%").FontSize(7.5f).FontColor(Layout.DocumentStyles.MutedColor);
                        });

                        table.Cell().Background(rowBg).Element(CellStyle).AlignRight().Column(c =>
                        {
                            c.Item().Text(item.SgstAmount.ToString("N2"));
                            if (item.SgstRate > 0)
                                c.Item().Text($"{item.SgstRate:0.##}%").FontSize(7.5f).FontColor(Layout.DocumentStyles.MutedColor);
                        });
                    }

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(item.TotalAmount.ToString("N2")).Bold().FontColor(Layout.DocumentStyles.PrimaryColor);

                    index++;
                }
            });
        }

        static string FormatQty(decimal qty, string uom)
        {
            var qtyText = qty.ToString("0.##");
            return string.IsNullOrEmpty(uom) ? qtyText : $"{qtyText} {uom}";
        }

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Layout.DocumentStyles.PrimaryColor)
                .PaddingVertical(9)
                .PaddingHorizontal(5)
                .AlignMiddle()
                .DefaultTextStyle(x => x.Bold().FontColor(Colors.White)
                    .FontSize(7.8f).LetterSpacing(0.02f));
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Layout.DocumentStyles.BorderColor)
                .PaddingVertical(8)
                .PaddingHorizontal(5)
                .AlignMiddle();
        }
    }
}

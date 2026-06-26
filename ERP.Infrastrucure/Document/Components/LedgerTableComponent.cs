using System;
using System.Text;
using ERP.Application.Models.common;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace ERP.Infrastructure.Document.Components
{
    public static class LedgerTableComponent
    {
        public static void Compose(IContainer container, IEnumerable<LedgerEntry> entries, decimal openingBalance)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);   // Date
                    columns.RelativeColumn(2.4f); // Description
                    columns.ConstantColumn(80);   // Reference No
                    columns.ConstantColumn(78);   // Debit
                    columns.ConstantColumn(78);   // Credit
                    columns.ConstantColumn(82);   // Balance
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("DATE");
                    header.Cell().Element(HeaderStyle).Text("DESCRIPTION");
                    header.Cell().Element(HeaderStyle).Text("REF NO");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("DEBIT");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("CREDIT");
                    header.Cell().Element(HeaderStyle).AlignRight().Text("BALANCE");

                    header.Cell().ColumnSpan(6).PaddingTop(1)
                        .LineHorizontal(1.5f).LineColor(Layout.DocumentStyles.AccentColor);
                });

                // Opening balance row
                table.Cell().ColumnSpan(3).Background(Layout.DocumentStyles.LightGray)
                    .Element(CellStyle).Text("Opening Balance").Italic().FontColor(Layout.DocumentStyles.MutedColor);
                table.Cell().Background(Layout.DocumentStyles.LightGray).Element(CellStyle).AlignRight().Text("");
                table.Cell().Background(Layout.DocumentStyles.LightGray).Element(CellStyle).AlignRight().Text("");
                table.Cell().Background(Layout.DocumentStyles.LightGray).Element(CellStyle).AlignRight()
                    .Text(openingBalance.ToString("N2")).Bold();

                int index = 1;
                foreach (var entry in entries)
                {
                    var isEven = index % 2 == 0;
                    var rowBg = isEven ? Layout.DocumentStyles.LightGray : Colors.White;

                    table.Cell().Background(rowBg).Element(CellStyle).Text(entry.Date.ToString("dd-MM-yyyy"));
                    table.Cell().Background(rowBg).Element(CellStyle).Text(entry.Description ?? "-");
                    table.Cell().Background(rowBg).Element(CellStyle).Text(entry.ReferenceNo ?? "-")
                        .FontSize(Layout.DocumentStyles.SmallSize).FontColor(Layout.DocumentStyles.MutedColor);

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(entry.Debit > 0 ? entry.Debit.ToString("N2") : "-");

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(entry.Credit > 0 ? entry.Credit.ToString("N2") : "-");

                    table.Cell().Background(rowBg).Element(CellStyle).AlignRight()
                        .Text(entry.Balance.ToString("N2")).Bold().FontColor(Layout.DocumentStyles.PrimaryColor);

                    index++;
                }
            });
        }

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background(Layout.DocumentStyles.PrimaryColor)
                .PaddingVertical(9)
                .PaddingHorizontal(6)
                .AlignMiddle()
                .DefaultTextStyle(x => x.Bold().FontColor(Colors.White)
                    .FontSize(Layout.DocumentStyles.TableHeaderSize).LetterSpacing(0.03f));
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Layout.DocumentStyles.BorderColor)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .AlignMiddle();
        }
    }
}

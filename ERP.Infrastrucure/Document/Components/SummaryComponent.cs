using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ERP.Application.Models.common;

namespace ERP.Infrastructure.Document.Components
{
    public static class SummaryComponent
    {
        /// <summary>
        /// splitTax: false (default) shows one combined "Total Tax" line —
        /// matches your "common tax is enough" requirement. Pass true only
        /// if you need CGST/SGST broken out separately in the totals box
        /// (the per-line breakdown already appears in the items table either way).
        /// </summary>
        public static void Compose(IContainer container, DocumentSummary summary, bool splitTax = false)
        {
            container.Row(row =>
            {
                row.RelativeColumn().PaddingRight(30).PaddingTop(4).Column(c =>
                {
                    if (!string.IsNullOrEmpty(summary.AmountInWords))
                    {
                        c.Item().Text("AMOUNT IN WORDS").Bold().FontSize(Layout.DocumentStyles.SectionLabelSize)
                            .FontColor(Layout.DocumentStyles.MutedColor).LetterSpacing(Layout.DocumentStyles.SectionLabelSpacing);

                        c.Item().PaddingTop(4).Text(summary.AmountInWords).Italic()
                            .FontSize(9.5f).FontColor(Layout.DocumentStyles.TextColor).LineHeight(1.3f);
                    }
                });

                row.ConstantColumn(230).Column(c =>
                {
                    c.Item().Element(e => Row(e, "Sub Total", summary.SubTotal));

                    if (splitTax)
                    {
                        if (summary.TotalCgst > 0)
                            c.Item().Element(e => Row(e, "CGST", summary.TotalCgst));
                        if (summary.TotalSgst > 0)
                            c.Item().Element(e => Row(e, "SGST", summary.TotalSgst));
                    }
                    else if (summary.TotalTax > 0)
                    {
                        c.Item().Element(e => Row(e, "Total Tax", summary.TotalTax));
                    }

                    if (summary.Discount > 0)
                        c.Item().Element(e => Row(e, "Discount", summary.Discount, isNegative: true));

                    if (summary.RoundOff != 0)
                        c.Item().Element(e => Row(e, "Round Off", summary.RoundOff));

                    c.Item().PaddingTop(8).Background(Layout.DocumentStyles.AccentSoft).Padding(10)
                        .Element(e => Row(e, "GRAND TOTAL", summary.GrandTotal, isBold: true));
                });
            });
        }

        static void Row(IContainer container, string label, decimal value, bool isBold = false, bool isNegative = false)
        {
            container.PaddingVertical(isBold ? 0 : 4).Row(row =>
            {
                var labelStyle = isBold
                    ? QuestPDF.Infrastructure.TextStyle.Default.FontSize(Layout.DocumentStyles.GrandTotalLabelSize).Bold()
                        .FontColor(Layout.DocumentStyles.AccentColor).LetterSpacing(0.03f)
                    : QuestPDF.Infrastructure.TextStyle.Default.FontSize(9.5f).FontColor(Layout.DocumentStyles.MutedColor);

                var valueStyle = isBold
                    ? QuestPDF.Infrastructure.TextStyle.Default.FontSize(Layout.DocumentStyles.GrandTotalValueSize).Bold()
                        .FontColor(Layout.DocumentStyles.PrimaryColor)
                    : QuestPDF.Infrastructure.TextStyle.Default.FontSize(9.5f).FontColor(Layout.DocumentStyles.TextColor);

                row.RelativeColumn().AlignLeft().AlignMiddle().Text(label).Style(labelStyle);

                var formatted = isNegative ? $"- {value:N2}" : value.ToString("N2");
                row.ConstantColumn(110).AlignRight().Text(formatted).Style(valueStyle);
            });
        }
    }

    /// <summary>
    /// Shared bank-details block. Used by Quotation/Invoice (collecting
    /// payment) — typically omitted on Purchase Order, Returns, and SOA by
    /// simply not calling this component.
    /// </summary>
    public static class BankDetailsComponent
    {
        public static void Compose(IContainer container, BankDetails bank)
        {
            if (bank == null) return;

            container.Column(c =>
            {
                c.Item().Text("BANK DETAILS").Bold().FontSize(Layout.DocumentStyles.SectionLabelSize)
                    .FontColor(Layout.DocumentStyles.MutedColor).LetterSpacing(Layout.DocumentStyles.SectionLabelSpacing);

                c.Item().PaddingTop(5).Element(e => Line(e, "Account Holder", bank.AccountHolderName));
                c.Item().Element(e => Line(e, "Bank Name", bank.BankName));
                c.Item().Element(e => Line(e, "Account Number", bank.AccountNumber));
                c.Item().Element(e => Line(e, "Branch Name", bank.BranchName));
                c.Item().Element(e => Line(e, "IFSC Code", bank.IfscCode));
            });
        }

        static void Line(IContainer container, string label, string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            container.PaddingTop(1.5f).Row(r =>
            {
                r.ConstantColumn(95).Text(label).FontSize(8.5f).FontColor(Layout.DocumentStyles.MutedColor);
                r.RelativeColumn().Text(value).Bold().FontSize(8.5f).FontColor(Layout.DocumentStyles.TextColor);
            });
        }
    }

    /// <summary>
    /// Renders the "Authorized Signature" block — a blank line with a label
    /// underneath, right-aligned. Used by every document type at the bottom
    /// of the content area (Quotation, Invoice, PO, Returns, SOA). Pass a
    /// company name to show "For {CompanyName}" above the line, matching
    /// standard Indian business-document convention.
    /// </summary>
    public static class SignatureComponent
    {
        public static void Compose(IContainer container, string companyName)
        {
            container.AlignRight().Width(200).Column(c =>
            {
                if (!string.IsNullOrEmpty(companyName))
                    c.Item().AlignCenter().Text($"For {companyName}")
                        .Bold().FontSize(Layout.DocumentStyles.SmallSize).FontColor(Layout.DocumentStyles.PrimaryColor);

                // Reserved blank space for the physical/digital signature or stamp
                c.Item().Height(45);

                c.Item().LineHorizontal(1).LineColor(Layout.DocumentStyles.BorderColor);

                c.Item().PaddingTop(4).AlignCenter().Text("Authorized Signature")
                    .FontSize(Layout.DocumentStyles.MicroSize).FontColor(Layout.DocumentStyles.MutedColor);
            });
        }
    }
}

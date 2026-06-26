using DocumentFormat.OpenXml.Office2010.Word;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ERP.Application.Models.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Document.Layout
{
    /// <summary>
    /// Applies the shared page setup (margin, size, default text style).
    /// Call this first inside every Document's Compose(container) method.
    /// </summary>
    public static class DocumentLayout
    {
        /// <summary>
        /// Applies the shared page setup (margin, size, default text style).
        /// Call this first inside every Document's Compose(container) method.
        /// </summary>
        public static void ApplyPageDefaults(PageDescriptor page)
        {
            page.Margin(DocumentStyles.PageMargin);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(x => x
                .FontSize(DocumentStyles.BodySize)
                .FontColor(DocumentStyles.TextColor)
                .FontFamily(DocumentStyles.FontFamily));
        }

        public static void ComposeHeader(IContainer container, DocumentHeaderInfo info)
        {
            container.Column(col =>
            {
                //col.Item().Row(row =>
                //{
                //    // ---- Left: logo (optional) + company block ----
                //    row.RelativeColumn(1.4f).Row(companyRow =>
                //    {
                //        if (!string.IsNullOrEmpty(info.LogoPath))
                //        {
                //            companyRow.ConstantColumn(44).Height(44)
                //                .Image(info.LogoPath)
                //                .FitArea();

                //            companyRow.ConstantColumn(10); // spacer
                //        }

                //        companyRow.RelativeColumn().Column(c =>
                //        {
                //            c.Item().Text(info.CompanyName ?? string.Empty)
                //                .Bold().FontSize(DocumentStyles.CompanyNameSize).FontColor(DocumentStyles.PrimaryColor);

                //            if (!string.IsNullOrEmpty(info.CompanyAddress))
                //                c.Item().PaddingTop(3).Text(info.CompanyAddress)
                //                    .FontSize(DocumentStyles.MicroSize).FontColor(DocumentStyles.MutedColor).LineHeight(1.3f);

                //            var contactLine = JoinNonEmpty("  |  ", info.CompanyPhone, info.CompanyEmail);
                //            if (!string.IsNullOrEmpty(contactLine))
                //                c.Item().PaddingTop(2).Text(contactLine)
                //                    .FontSize(DocumentStyles.MicroSize).FontColor(DocumentStyles.MutedColor);

                //            if (!string.IsNullOrEmpty(info.Gstin))
                //                c.Item().PaddingTop(2).Text($"GSTIN: {info.Gstin}")
                //                    .FontSize(DocumentStyles.MicroSize).FontColor(DocumentStyles.MutedColor);
                //        });
                //    });

                //    // ---- Right: title + meta ----
                //    row.RelativeColumn().AlignRight().Column(c =>
                //    {
                //        c.Item().AlignRight().Element(x =>
                //                    {
                //                        x.Text(info.Kind.ToTitle())
                //                            .Bold()
                //                            .FontSize(DocumentStyles.TitleSize)
                //                            .FontColor(DocumentStyles.AccentColor)
                //                            .LetterSpacing(DocumentStyles.TitleLetterSpacing);
                //                    });

                //        c.Item().PaddingTop(8).Element(e => MetaLine(e, info.Kind.ToNumberLabel(), info.DocumentNo ?? string.Empty));
                //        c.Item().PaddingTop(3).Element(e => MetaLine(e, "Date", info.DocumentDate.ToString("dd-MM-yyyy")));

                //        if (info.ValidUntilOrDueDate.HasValue)
                //        {
                //            var label = info.Kind == DocumentKind.SalesInvoice ? "Due Date" : "Valid Until";
                //            c.Item().PaddingTop(3).Element(e => MetaLine(e, label, info.ValidUntilOrDueDate.Value.ToString("dd-MM-yyyy")));
                //        }

                //        if (!string.IsNullOrEmpty(info.PlaceOfSupply))
                //        {
                //            c.Item().PaddingTop(3).Element(e => MetaLine(e, "Place of Supply", info.PlaceOfSupply));
                //        }
                //    });
                //});


                col.Item().Row(row =>
                {
                    row.RelativeColumn(7).Element(x =>
                    {
                        ComposeCompanyBlock(x, info);
                    });

                    row.ConstantColumn(260).Element(x =>
                    {
                        ComposeDocumentInfo(x, info);
                    });
                });

                // Accent rule under header block — present on every document type
                col.Item().PaddingTop(14).LineHorizontal(DocumentStyles.HeaderRuleThickness).LineColor(DocumentStyles.AccentColor);

                // Party blocks (Quote To / Ship To, Bill To / Ship To, Vendor / Deliver To, etc.)
                col.Item().PaddingTop(20).Element(c => ComposePartyBlocks(c, info));
            });
        }


        public static void ComposeCompanyBlock(IContainer container, DocumentHeaderInfo info)
        {
            container.Column(c =>
            {
                c.Item().Text(info.CompanyName)
                    .Bold()
                    .FontSize(22)
                    .FontColor(DocumentStyles.PrimaryColor);

                if (!string.IsNullOrWhiteSpace(info.CompanyAddress))
                {
                    c.Item()
                        .PaddingTop(4)
                        .Text(info.CompanyAddress)
                        .FontSize(11)
                        .FontColor(DocumentStyles.MutedColor);
                }

                var contact = JoinNonEmpty("  |  ", info.CompanyPhone, info.CompanyEmail);

                if (!string.IsNullOrWhiteSpace(contact))
                {
                    c.Item()
                        .PaddingTop(3)
                        .Text(contact)
                        .FontSize(11)
                        .FontColor(DocumentStyles.MutedColor);
                }

                if (!string.IsNullOrWhiteSpace(info.Gstin))
                {
                    c.Item()
                        .PaddingTop(3)
                        .Text($"GSTIN : {info.Gstin}")
                        .SemiBold()
                        .FontSize(11)
                        .FontColor(DocumentStyles.MutedColor);
                }
            });
        }

        public static void ComposeDocumentInfo(IContainer container, DocumentHeaderInfo info)
        {
            container.Column(c =>
            {
                c.Item()
                    .AlignRight()
                    .Text(info.Kind.ToTitle())
                    .Bold()
                    .FontSize(28)
                    .FontColor(DocumentStyles.AccentColor);
                    

                c.Item().PaddingTop(18).Element(e =>
                    MetaLine(e, info.Kind.ToNumberLabel(), info.DocumentNo));

                c.Item().PaddingTop(5).Element(e =>
                    MetaLine(e, "Date", info.DocumentDate.ToString("dd-MM-yyyy")));

                if (info.ValidUntilOrDueDate.HasValue)
                {
                    c.Item().PaddingTop(5).Element(e =>
                        MetaLine(e,
                            info.Kind == DocumentKind.SalesInvoice
                                ? "Due Date"
                                : "Valid Until",
                            info.ValidUntilOrDueDate.Value.ToString("dd-MM-yyyy")));
                }

                if (!string.IsNullOrWhiteSpace(info.PlaceOfSupply))
                {
                    c.Item().PaddingTop(5).Element(e =>
                        MetaLine(e, "Place of Supply", info.PlaceOfSupply));
                }
            });
        }


        static void MetaLine(IContainer container, string label, string value)
        {
            container.Row(row =>
            {
                row.RelativeColumn()
                    .AlignRight()
                    .Text(label)
                    .FontSize(11)
                    .FontColor(DocumentStyles.MutedColor);

                row.ConstantColumn(15)
                    .AlignCenter()
                    .Text(":")
                    .FontSize(11);

                row.ConstantColumn(125)
                    .AlignRight()
                    .Text(value)
                    .SemiBold()
                    .FontSize(11)
                    .FontColor(DocumentStyles.PrimaryColor);
            });
        }

        static void ComposePartyBlocks(IContainer container, DocumentHeaderInfo info)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Element(c => PartyCard(c, info.PartyLabel ?? "Bill To", info.PartyName, info.PartyAddress, info.PartyContactPerson, info.PartyContact, info.PartyGstin));

                if (!string.IsNullOrEmpty(info.SecondPartyLabel))
                {
                    row.ConstantColumn(20);
                    row.RelativeColumn().Element(c => PartyCard(c, info.SecondPartyLabel, null, info.SecondPartyAddress, null, null, null));
                }
            });
        }

        static void PartyCard(IContainer container, string label, string name, string address, string contactPerson, string contact, string gstin)
        {
            container
                .Background(DocumentStyles.LightGray)
                .BorderLeft(DocumentStyles.CardAccentBarWidth).BorderColor(DocumentStyles.AccentColor)
                .Padding(14)
                .Column(c =>
                {
                    c.Item().Text(label.ToUpperInvariant()).Bold().FontSize(DocumentStyles.SectionLabelSize)
                        .FontColor(DocumentStyles.MutedColor).LetterSpacing(DocumentStyles.SectionLabelSpacing);

                    if (!string.IsNullOrEmpty(name))
                        c.Item().PaddingTop(6).Text(name).Bold().FontSize(11).FontColor(DocumentStyles.PrimaryColor);

                    if (!string.IsNullOrEmpty(contactPerson))
                        c.Item().PaddingTop(3).Text($"Attn: {contactPerson}").FontSize(DocumentStyles.SmallSize);

                    if (!string.IsNullOrEmpty(address))
                        c.Item().PaddingTop(string.IsNullOrEmpty(name) ? 6 : 3).Text(address).FontSize(DocumentStyles.SmallSize).LineHeight(1.3f);

                    if (!string.IsNullOrEmpty(contact))
                        c.Item().PaddingTop(4).Text(contact).FontSize(DocumentStyles.SmallSize).FontColor(DocumentStyles.MutedColor);

                    if (!string.IsNullOrEmpty(gstin))
                        c.Item().PaddingTop(2).Text($"GSTIN: {gstin}").FontSize(DocumentStyles.SmallSize).FontColor(DocumentStyles.MutedColor);
                });
        }

        /// <summary>
        /// Shared footer for every document type: hairline rule, company name
        /// left, page numbers right. Identical across all document kinds.
        /// </summary>
        public static void ComposeFooter(IContainer container, DocumentHeaderInfo info)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(DocumentStyles.BorderColor);

                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeColumn().Text(info.CompanyName ?? string.Empty)
                        .FontSize(DocumentStyles.MicroSize).FontColor(DocumentStyles.FaintColor);

                    row.RelativeColumn().AlignRight().Text(x =>
                    {
                        x.DefaultTextStyle(t => t.FontSize(DocumentStyles.MicroSize).FontColor(DocumentStyles.FaintColor));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
        }

        static string JoinNonEmpty(string separator, params string[] parts)
        {
            var nonEmpty = Array.FindAll(parts, p => !string.IsNullOrEmpty(p));
            return string.Join(separator, nonEmpty);
        }
    }
}

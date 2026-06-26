
using ERP.Application.Models.common;
using ERP.Infrastructure.Document.Components;
using ERP.Infrastructure.Document.Layout;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace ERP.Infrastructure.Document.Documents
{
    /// <summary>
    /// Shared per-document model for everything that uses the tax-aware
    /// items table (Quotation, Sales Invoice, Purchase Order, Sales Return,
    /// Purchase Return). Statement of Account has its own model — see
    /// StatementOfAccountModel below.
    /// </summary>
    public class TaxDocumentModel
    {
        public DocumentHeaderInfo Header { get; set; }
        public List<DocumentLineItem> Items { get; set; } = new();
        public DocumentSummary Summary { get; set; }
        public BankDetails Bank { get; set; }          // null => bank block omitted
        public string Notes { get; set; }              // terms/notes — null => omitted

        /// <summary>
        /// THE single GST on/off switch for this document.
        /// true  -> items table shows HSN/SAC, Taxable Value, CGST, SGST columns;
        ///          summary box shows the Total Tax line (when summary.TotalTax > 0).
        /// false -> items table collapses to S.No, Item, Qty, Rate, Amount only;
        ///          summary box skips straight from Sub Total to Grand Total.
        /// Set this once per document — nothing else needs to change.
        /// </summary>
        public bool IsGstApplicable { get; set; } = true;

        public bool SplitTaxInSummary { get; set; } = false; // false = single "Total Tax" line (default per your request)
    }

    /// <summary>
    /// Every concrete document class below follows the identical shape:
    ///   1. ApplyPageDefaults
    ///   2. DocumentLayout.ComposeHeader     (shared, unchanged)
    ///   3. Type-specific content + Signature block (shared component)
    ///   4. DocumentLayout.ComposeFooter     (shared, unchanged)
    /// This is what guarantees Quotation, Invoice, PO, Returns, and SOA are
    /// visually identical except for their title text and body content.
    /// </summary>
    public class QuotationDocument : IDocument
    {
        private readonly TaxDocumentModel _data;
        public QuotationDocument(TaxDocumentModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().Element(ComposeContent);
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingTop(20).Column(col =>
            {
                col.Item().Element(c => ItemsTableComponent.Compose(c, _data.Items, _data.IsGstApplicable));
                col.Item().PaddingTop(18).Element(c => SummaryComponent.Compose(c, _data.Summary, _data.SplitTaxInSummary));

                if (_data.Bank != null)
                    col.Item().PaddingTop(20).Element(c => BankDetailsComponent.Compose(c, _data.Bank));

                if (!string.IsNullOrEmpty(_data.Notes))
                    col.Item().PaddingTop(16).BorderTop(1).BorderColor(DocumentStyles.BorderColor).PaddingTop(10).Column(c =>
                    {
                        c.Item().Text("TERMS & CONDITIONS").Bold().FontSize(DocumentStyles.SectionLabelSize)
                            .FontColor(DocumentStyles.MutedColor).LetterSpacing(DocumentStyles.SectionLabelSpacing);
                        c.Item().PaddingTop(5).Text(_data.Notes).FontSize(8.5f).FontColor(DocumentStyles.TextColor).LineHeight(1.4f);
                    });

                col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
            });
        }
    }

    /// <summary>Identical structure to QuotationDocument — only the model's Header.Kind differs (set to DocumentKind.SalesInvoice by the caller).</summary>
    public class SalesInvoiceDocument : IDocument
    {
        private readonly TaxDocumentModel _data;
        public SalesInvoiceDocument(TaxDocumentModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Element(c => ItemsTableComponent.Compose(c, _data.Items, _data.IsGstApplicable));
                    col.Item().PaddingTop(18).Element(c => SummaryComponent.Compose(c, _data.Summary, _data.SplitTaxInSummary));

                    if (_data.Bank != null)
                        col.Item().PaddingTop(20).Element(c => BankDetailsComponent.Compose(c, _data.Bank));

                    col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
                });
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }
    }

    /// <summary>Purchase Order — typically no bank details (leave Bank = null when building the model).</summary>
    public class PurchaseOrderDocument : IDocument
    {
        private readonly TaxDocumentModel _data;
        public PurchaseOrderDocument(TaxDocumentModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Element(c => ItemsTableComponent.Compose(c, _data.Items, _data.IsGstApplicable));
                    col.Item().PaddingTop(18).Element(c => SummaryComponent.Compose(c, _data.Summary, _data.SplitTaxInSummary));

                    if (!string.IsNullOrEmpty(_data.Notes))
                        col.Item().PaddingTop(16).BorderTop(1).BorderColor(DocumentStyles.BorderColor).PaddingTop(10).Column(c =>
                        {
                            c.Item().Text("TERMS & CONDITIONS").Bold().FontSize(DocumentStyles.SectionLabelSize)
                                .FontColor(DocumentStyles.MutedColor).LetterSpacing(DocumentStyles.SectionLabelSpacing);
                            c.Item().PaddingTop(5).Text(_data.Notes).FontSize(8.5f).FontColor(DocumentStyles.TextColor).LineHeight(1.4f);
                        });

                    col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
                });
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }
    }

    /// <summary>Sales Return — same shape as Invoice; caller sets Header.Kind = DocumentKind.SalesReturn.</summary>
    public class SalesReturnDocument : IDocument
    {
        private readonly TaxDocumentModel _data;
        public SalesReturnDocument(TaxDocumentModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Element(c => ItemsTableComponent.Compose(c, _data.Items, _data.IsGstApplicable));
                    col.Item().PaddingTop(18).Element(c => SummaryComponent.Compose(c, _data.Summary, _data.SplitTaxInSummary));
                    col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
                });
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }
    }

    /// <summary>Purchase Return — same shape as Purchase Order; caller sets Header.Kind = DocumentKind.PurchaseReturn.</summary>
    public class PurchaseReturnDocument : IDocument
    {
        private readonly TaxDocumentModel _data;
        public PurchaseReturnDocument(TaxDocumentModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Element(c => ItemsTableComponent.Compose(c, _data.Items, _data.IsGstApplicable));
                    col.Item().PaddingTop(18).Element(c => SummaryComponent.Compose(c, _data.Summary, _data.SplitTaxInSummary));
                    col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
                });
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }
    }

    /// <summary>Model for Statement of Account — uses LedgerEntry rows instead of DocumentLineItem.</summary>
    public class StatementOfAccountModel
    {
        public DocumentHeaderInfo Header { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<LedgerEntry> Entries { get; set; } = new();
        public decimal ClosingBalance { get; set; }
    }

    /// <summary>Statement of Account — same header/footer, but a ledger table instead of an items table, and no GST summary box.</summary>
    public class StatementOfAccountDocument : IDocument
    {
        private readonly StatementOfAccountModel _data;
        public StatementOfAccountDocument(StatementOfAccountModel data) => _data = data;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                DocumentLayout.ApplyPageDefaults(page);
                page.Header().Element(c => DocumentLayout.ComposeHeader(c, _data.Header));
                page.Content().PaddingTop(20).Column(col =>
                {
                    col.Item().Element(c => LedgerTableComponent.Compose(c, _data.Entries, _data.OpeningBalance));

                    col.Item().PaddingTop(14).AlignRight().Background(DocumentStyles.AccentSoft)
                        .Padding(10).Width(230).Row(r =>
                        {
                            r.RelativeColumn().Text("CLOSING BALANCE").Bold().FontSize(DocumentStyles.GrandTotalLabelSize)
                                .FontColor(DocumentStyles.AccentColor).LetterSpacing(0.03f);
                            r.AutoItem().Text(_data.ClosingBalance.ToString("N2")).Bold()
                                .FontSize(DocumentStyles.GrandTotalValueSize).FontColor(DocumentStyles.PrimaryColor);
                        });

                    col.Item().PaddingTop(30).Element(c => SignatureComponent.Compose(c, _data.Header.CompanyName));
                });
                page.Footer().Element(c => DocumentLayout.ComposeFooter(c, _data.Header));
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.common
{
    public enum DocumentKind
    {
        Quotation,
        SalesInvoice,
        PurchaseOrder,
        SalesReturn,
        PurchaseReturn,
        StatementOfAccount
    }

    public static class DocumentKindExtensions
    {
        public static string ToTitle(this DocumentKind kind) => kind switch
        {
            DocumentKind.Quotation => "QUOTATION",
            DocumentKind.SalesInvoice => "SALES INVOICE",
            DocumentKind.PurchaseOrder => "PURCHASE ORDER",
            DocumentKind.SalesReturn => "SALES RETURN",
            DocumentKind.PurchaseReturn => "PURCHASE RETURN",
            DocumentKind.StatementOfAccount => "STATEMENT OF ACCOUNT",
            _ => kind.ToString().ToUpperInvariant()
        };

        // Label shown above the doc number, e.g. "Quote No", "Invoice No".
        // Falls back sensibly for return/SOA documents.
        public static string ToNumberLabel(this DocumentKind kind) => kind switch
        {
            DocumentKind.Quotation => "Quote No",
            DocumentKind.SalesInvoice => "Invoice No",
            DocumentKind.PurchaseOrder => "PO No",
            DocumentKind.SalesReturn => "Return No",
            DocumentKind.PurchaseReturn => "Return No",
            DocumentKind.StatementOfAccount => "Statement No",
            _ => "Doc No"
        };
    }

    /// <summary>
    /// Common header/footer/party data shared by every document type.
    /// Each concrete model (InvoiceModel, PurchaseOrderModel, ...) should
    /// have a property of this type (composition) rather than re-declaring
    /// company/customer/date fields itself. This is what DocumentLayout
    /// binds to, so adding a new document type never requires touching the
    /// header/footer code.
    /// </summary>
    public class DocumentHeaderInfo
    {
        public DocumentKind Kind { get; set; }

        // Issuing company (your business)
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string Gstin { get; set; }
        public string LogoPath { get; set; }   // optional, null/empty => omitted gracefully

        // Document identity
        public string DocumentNo { get; set; }
        public DateTime DocumentDate { get; set; }
        public DateTime? ValidUntilOrDueDate { get; set; }  // "Valid Until" for quotes, "Due Date" for invoices
        public string PlaceOfSupply { get; set; }

        // Counterparty — labelled "Quote To" / "Bill To" / "Vendor" etc by the caller
        public string PartyLabel { get; set; } = "Bill To";
        public string PartyName { get; set; }
        public string PartyAddress { get; set; }
        public string PartyContactPerson { get; set; }
        public string PartyContact { get; set; }
        public string PartyGstin { get; set; }

        // Optional second party block (Ship To) — null => column omitted, first column expands
        public string SecondPartyLabel { get; set; }
        public string SecondPartyAddress { get; set; }
    }

    /// <summary>
    /// Shared line item shape for the tax-aware items table, used by
    /// Quotation, Sales Invoice, Purchase Order, Sales Return, Purchase Return.
    /// Statement of Account does NOT use this — see LedgerEntry instead.
    /// </summary>
    public class DocumentLineItem
    {
        public string PartNo { get; set; }
        public string PartName { get; set; }
        public string HsnSac { get; set; }
        public decimal Quantity { get; set; }
        public string Uom { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal CgstRate { get; set; }
        public decimal SgstAmount { get; set; }
        public decimal SgstRate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Shared totals/summary block, used by every tax-table document type.
    /// </summary>
    public class DocumentSummary
    {
        public decimal SubTotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Discount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal GrandTotal { get; set; }
        public string AmountInWords { get; set; }

        // Optional split totals — only used by SummaryComponent when
        // splitTax: true is passed. Leave at 0 if you're using the combined
        // TotalTax line (the default).
        public decimal TotalCgst { get; set; }
        public decimal TotalSgst { get; set; }
    }

    /// <summary>
    /// Optional bank details block — shown on Invoice/Quotation, typically
    /// omitted on Purchase Order/Return/SOA (caller leaves it null).
    /// </summary>
    public class BankDetails
    {
        public string AccountHolderName { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string BranchName { get; set; }
        public string IfscCode { get; set; }
    }

    /// <summary>
    /// Ledger row for Statement of Account — completely different shape from
    /// DocumentLineItem since SOA has no tax/qty/rate, just a running balance.
    /// </summary>
    public class LedgerEntry
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string ReferenceNo { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}

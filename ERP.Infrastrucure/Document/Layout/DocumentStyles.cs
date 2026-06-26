using System;
using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ERP.Infrastructure.Document.Layout
{
    /// <summary>
    /// Single source of truth for color palette, fonts, and spacing across
    /// every generated document (Quotation, Sales Invoice, Purchase Order,
    /// Sales Return, Purchase Return, Statement of Account, etc).
    ///
    /// Nothing in any Document class should hardcode a Color.FromHex(...) or
    /// a raw FontSize(...) for header/footer/title elements — pull from here
    /// so a future re-brand is a one-file change.
    /// </summary>
    public static class DocumentStyles
    {
        // ---- Palette ----
        public static readonly Color PrimaryColor = Color.FromHex("#1E293B");   // slate-800 — headings, table header bg
        public static readonly Color AccentColor = Color.FromHex("#0F766E");    // teal-700 — brand accent, rules
        public static readonly Color AccentSoft = Color.FromHex("#ECFDF5");     // light teal — grand total / highlight bg
        public static readonly Color TextColor = Color.FromHex("#334155");      // slate-700 — body text
        public static readonly Color MutedColor = Color.FromHex("#64748B");     // slate-500 — labels/captions
        public static readonly Color FaintColor = Color.FromHex("#94A3B8");     // slate-400 — footer text
        public static readonly Color LightGray = Color.FromHex("#F8FAFC");      // slate-50 — zebra rows / card bg
        public static readonly Color BorderColor = Color.FromHex("#E2E8F0");    // slate-200 — rules / borders
        public static readonly Color White = Colors.White;

        // Status / amount-due strip uses the same accent everywhere — keeps
        // "money owed" visually consistent across Invoice and Quotation.
        public static readonly Color AmountStripBg = PrimaryColor;
        public static readonly Color AmountStripText = Colors.White;

        // ---- Typography scale ----
        public const string FontFamily = Fonts.Calibri;

        public const float TitleSize = 22f;        // "QUOTATION" / "SALES INVOICE" etc.
        public const float CompanyNameSize = 18f;
        public const float SectionLabelSize = 8.5f; // "BILL TO", "QUOTATION DETAILS" etc — always upper-case + letterspaced
        public const float BodySize = 10f;
        public const float SmallSize = 9f;
        public const float MicroSize = 8.5f;
        public const float TableHeaderSize = 8.5f;
        public const float GrandTotalLabelSize = 11f;
        public const float GrandTotalValueSize = 13f;

        public const float SectionLabelSpacing = 0.04f;
        public const float TitleLetterSpacing = 0.05f;

        // ---- Spacing ----
        public const float PageMargin = 30f;
        public const float HeaderRuleThickness = 2f;
        public const float CardAccentBarWidth = 3f;
    }
}

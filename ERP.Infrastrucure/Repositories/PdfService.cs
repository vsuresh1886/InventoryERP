using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Models.Quotation;
using ERP.Infrastructure.Document;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public  class PdfService : IPdfService
    {


        public byte[] GenerateQuotationPdf(QuotationModel model)
        {
            var document = new TemplateDocument(model);
            return document.GeneratePdf();
        }

        public  async Task<byte[]> Gen_customerSOAPdf(dynamic customers)
        {
            var document = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);

                    page.Margin(20);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10)
                    );

                    // =====================================================
                    // HEADER
                    // =====================================================

                    page.Header().Column(col =>
                    {
                        col.Item().Text(
                            "CUSTOMER STATEMENT OF ACCOUNT"
                        )
                        .Bold()
                        .FontSize(18)
                        .AlignCenter();

                        col.Item().PaddingTop(5);
                    });

                    // =====================================================
                    // CONTENT
                    // =====================================================

                    page.Content().Column(column =>
                    {
                        foreach (var customer in customers)
                        {
                            // ============================================
                            // CUSTOMER HEADER
                            // ============================================

                            column.Item()
                                .Background(QuestPDF.Helpers.Colors.Blue.Darken2)
                                .Padding(5)
                                .Text(
                                    $"Customer : {customer.customer_name}"
                                )
                                .FontColor(QuestPDF.Helpers.Colors.White)
                                .Bold();

                            column.Item()
                                .PaddingBottom(5)
                                .Text(
                                    $"Invoice : {customer.invoice_total:N2}    " +
                                    $"Paid : {customer.paid_total:N2}    " +
                                    $"Balance : {customer.balance_total:N2}"
                                );

                            foreach (var month in customer.months)
                            {
                                // ========================================
                                // MONTH HEADER
                                // ========================================

                                column.Item()
                                    .Background(
                                        QuestPDF.Helpers.Colors.Grey.Lighten2
                                    )
                                    .Padding(5)
                                    .Text(
                                        $"{month.month}"
                                    )
                                    .Bold();

                                // ========================================
                                // TABLE
                                // ========================================

                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1.2f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    // ====================================
                                    // HEADER
                                    // ====================================

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Element(CellStyle)
                                            .Text("Date")
                                            .Bold();

                                        header.Cell()
                                            .Element(CellStyle)
                                            .Text("Reference")
                                            .Bold();

                                        header.Cell()
                                            .Element(CellStyle)
                                            .Text("Doc. Type")
                                            .Bold();

                                        header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Invoice Amt.")
                                            .Bold();

                                        header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Paid Amt.")
                                            .Bold();

                                        header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Balance Amt.")
                                            .Bold();
                                    });

                                    // ====================================
                                    // ROWS
                                    // ====================================

                                    foreach (var item in month.details)
                                    {
                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .Text($"{item.date.ToString("dd/MM/yyyy")}"
                                            );

                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .Text($"{item.invoice_no}");

                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .Text($"{item.doc_type}");

                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{item.invoice_amount:N2}"
                                            );

                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{item.paid_amount:N2}"
                                            );

                                        table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{item.running_balance:N2}"
                                            );
                                    }
                                });

                                // ========================================
                                // MONTH TOTAL
                                // ========================================

                                column.Item()
                                    .AlignRight()
                                    .PaddingTop(3)
                                    .Text(
                                        $"Month Balance : {month.balance_total:N2}"
                                    )
                                    .Bold();

                                column.Item()
                                    .PaddingBottom(15);
                            }

                            // ============================================
                            // CUSTOMER TOTAL
                            // ============================================

                            column.Item()
                                .AlignRight()
                                .Text(
                                    $"Customer Outstanding : {customer.balance_total:N2}"
                                )
                                .Bold()
                                .FontSize(12);

                            column.Item()
                                .PaddingBottom(20);
                        }
                    });

                    // =====================================================
                    // FOOTER
                    // =====================================================

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");

                            x.CurrentPageNumber();

                            x.Span(" of ");

                            x.TotalPages();
                        });
                });
            });

            return document.GeneratePdf();
        }

        // =============================================================
        // CELL STYLE
        // =============================================================

        static IContainer CellStyle(
            IContainer container
        )
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Blue.Lighten3)
                .Padding(4);
        }

        static IContainer DataCellStyle(
            IContainer container
        )
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Padding(4);
        }


       
        public async Task<byte[]> Gen_customerAgingPdf( List<CustomerAgingCustomerDto> customers)
        {
            var document =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers.PageSizes.A4
                            );

                            page.Margin(20);

                            page.DefaultTextStyle(x =>
                                x.FontSize(9)
                            );

                            // ============================================
                            // HEADER
                            // ============================================

                            page.Header().Column(col =>
                            {
                                col.Item().Text(
                                    "CUSTOMER AGING REPORT"
                                )
                                .Bold()
                                .FontSize(18)
                                .AlignCenter();

                                col.Item()
                                    .PaddingTop(5);
                            });

                            // ============================================
                            // CONTENT
                            // ============================================

                            page.Content().Column(column =>
                            {
                                foreach (
                                    var customer
                                    in customers
                                )
                                {
                                    // ====================================
                                    // CUSTOMER HEADER
                                    // ====================================

                                    column.Item()
                                        .Background(
                                            QuestPDF.Helpers
                                            .Colors.Blue.Darken2
                                        )
                                        .Padding(5)
                                        .Text(
                                            $"Customer : {customer.customer_name}"
                                        )
                                        .FontColor(
                                            QuestPDF.Helpers
                                            .Colors.White
                                        )
                                        .Bold();

                                    // ====================================
                                    // CUSTOMER SUMMARY
                                    // ====================================

                                    column.Item()
                                        .PaddingVertical(5)
                                        .Text(

                                            $"Current : {customer.current_amount:N2}    " +

                                            $"31-60 : {customer.days_31_60:N2}    " +

                                            $"61-90 : {customer.days_61_90:N2}    " +

                                            $"91-120 : {customer.days_91_120:N2}    " +

                                            $"120+ : {customer.days_120_plus:N2}    " +

                                            $"Outstanding : {customer.total_outstanding:N2}"
                                        )
                                        .Bold();

                                    // ====================================
                                    // TABLE
                                    // ====================================

                                    column.Item()
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(
                                        columns =>
                                            {
                                                columns.RelativeColumn(1.5f); // Invoice

                                                columns.RelativeColumn(1.3f); // Invoice Date

                                                columns.RelativeColumn(1.3f); // Due Date

                                                columns.RelativeColumn(0.8f); // Age

                                                columns.RelativeColumn(1.2f); // Current

                                                columns.RelativeColumn(1.2f); // 31-60

                                                columns.RelativeColumn(1.2f); // 61-90

                                                columns.RelativeColumn(1.2f); // 91-120

                                                columns.RelativeColumn(1.2f); // 120+

                                                columns.RelativeColumn(1.4f); // Pending
                                            });

                                            // =================================
                                            // HEADER
                                            // =================================

                                            table.Header(header =>
                                            {
                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Invoice")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Inv Date")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Due Date")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignCenter()
                                            .Text("Age")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Current")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("31-60")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("61-90")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("91-120")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("120+")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Pending")
                                            .Bold();
                                            });

                                            // =================================
                                            // ROWS
                                            // =================================

                                            foreach (
                                        var invoice
                                        in customer.invoices
                                    )
                                            {
                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.invoice_no
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.invoice_date
                                                    .ToString("dd/MM/yyyy")
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.due_date
                                                    .ToString("dd/MM/yyyy")
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignCenter()
                                            .Text(
                                                $"{invoice.age_days}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.current_amount:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.days_31_60:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.days_61_90:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.days_91_120:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.days_120_plus:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.pending_amount:N2}"
                                            );
                                            }
                                        });

                                    // ====================================
                                    // CUSTOMER TOTAL
                                    // ====================================

                                    column.Item()
                                        .AlignRight()
                                        .PaddingTop(5)
                                        .Text(
                                            $"Customer Outstanding : {customer.total_outstanding:N2}"
                                        )
                                        .Bold()
                                        .FontSize(11);

                                    column.Item()
                                        .PaddingBottom(20);
                                }
                            });

                            // ============================================
                            // FOOTER
                            // ============================================

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("Page ");

                                    x.CurrentPageNumber();

                                    x.Span(" of ");

                                    x.TotalPages();
                                });
                        });
                    });

            return document.GeneratePdf();
        }


        public async Task<byte[]> Gen_customerOSPdf(List<CustomerOutstandingDto> customers)
        {
            var document =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers.PageSizes.A4
                            );

                            page.Margin(20);

                            page.DefaultTextStyle(x =>
                                x.FontSize(9)
                            );

                            // ============================================
                            // HEADER
                            // ============================================

                            page.Header().Column(col =>
                            {
                                col.Item().Text(
                                    "CUSTOMER OUTSTANDING REPORT"
                                )
                                .Bold()
                                .FontSize(18)
                                .AlignCenter();

                                col.Item()
                                    .PaddingTop(5);
                            });

                            // ============================================
                            // CONTENT
                            // ============================================

                            page.Content().Column(column =>
                            {
                                foreach (
                                    var customer
                                    in customers
                                )
                                {
                                    // ====================================
                                    // CUSTOMER HEADER
                                    // ====================================

                                    column.Item()
                                        .Background(
                                            QuestPDF.Helpers
                                            .Colors.Blue.Darken2
                                        )
                                        .Padding(5)
                                        .Text(
                                            $"Customer : {customer.customer_name}"
                                        )
                                        .FontColor(
                                            QuestPDF.Helpers
                                            .Colors.White
                                        )
                                        .Bold();

                                    // ====================================
                                    // CUSTOMER SUMMARY
                                    // ====================================

                                    column.Item()
                                        .PaddingVertical(5)
                                        .Text(

                                            $"Invoice Amount : {customer.totalInvoiceAmount:N2}    " +

                                            $"Received : {customer.totalReceivedAmount:N2}    " +

                                            $"Returned : {customer.totalReturnedAmount:N2}    " +

                                            $"Pending : {customer.totalPendingAmount:N2}"
                                        )
                                        .Bold();

                                    // ====================================
                                    // TABLE
                                    // ====================================

                                    column.Item()
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(
                                        columns =>
                                        {
                                            columns.RelativeColumn(1.5f); // Invoice

                                            columns.RelativeColumn(1.3f); // Invoice Date

                                            columns.RelativeColumn(1.3f); // Due Date

                                            columns.RelativeColumn(0.8f); // Age

                                            columns.RelativeColumn(1.2f); // Invoice Amount

                                            columns.RelativeColumn(1.2f); // Received Amount

                                            columns.RelativeColumn(1.2f); // Returned Amount

                                            columns.RelativeColumn(1.2f); // Pending Amount
                                        });

                                            // =================================
                                            // HEADER
                                            // =================================

                                            table.Header(header =>
                                            {
                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Invoice")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Inv Date")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .Text("Due Date")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignCenter()
                                            .Text("Age")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Invoice Amount")
                                            .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Received Amount")
                                            .Bold();

                                                header.Cell()
                                          .Element(CellStyle)
                                          .AlignRight()
                                          .Text("Returned Amount")
                                          .Bold();

                                                header.Cell()
                                            .Element(CellStyle)
                                            .AlignRight()
                                            .Text("Pending Amount")
                                            .Bold();

                                              
                                            });

                                            // =================================
                                            // ROWS
                                            // =================================

                                            foreach (
                                        var invoice
                                        in customer.Invoices
                                    )
                                            {
                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.invoice_no
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.invoice_date
                                                    .ToString("dd/MM/yyyy")
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .Text(
                                                invoice.due_date
                                                    .ToString("dd/MM/yyyy")
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignCenter()
                                            .Text(
                                                $"{invoice.age_days}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.invoice_amount:N2}"
                                            );

                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.received_amount:N2}"
                                            );
                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.returned_amount:N2}"
                                            );


                                                table.Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(
                                                $"{invoice.pending_amount:N2}"
                                            );

                                            }
                                        });

                                    // ====================================
                                    // CUSTOMER TOTAL
                                    // ====================================

                                    column.Item()
                                        .AlignRight()
                                        .PaddingTop(5)
                                        .Text(
                                            $"Customer Outstanding : {customer.totalPendingAmount:N2}"
                                        )
                                        .Bold()
                                        .FontSize(11);

                                    column.Item()
                                        .PaddingBottom(20);
                                }
                            });

                            // ============================================
                            // FOOTER
                            // ============================================

                            page.Footer()
                                .AlignCenter()
                                .Text(x =>
                                {
                                    x.Span("Page ");

                                    x.CurrentPageNumber();

                                    x.Span(" of ");

                                    x.TotalPages();
                                });
                        });
                    });

            return document.GeneratePdf();
        }


    }
}

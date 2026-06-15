using ClosedXML.Excel;
using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public  class ExcelService: IExcelService
    {

        public async Task<byte[]> Gen_customerSOA(dynamic customers)
        {
            using var workbook = new XLWorkbook();

            var ws =
                workbook.Worksheets.Add(
                    "Customer SOA"
                );

            int row = 1;

            // =====================================================
            // TITLE
            // =====================================================

            ws.Range(row, 1, row, 7).Merge();

            ws.Cell(row, 1).Value =
                "CUSTOMER STATEMENT OF ACCOUNT";

            ws.Cell(row, 1).Style.Font.Bold = true;

            ws.Cell(row, 1).Style.Font.FontSize = 18;

            ws.Cell(row, 1).Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            foreach (var customer in customers)
            {
                // =====================================================
                // CUSTOMER HEADER
                // =====================================================

                ws.Range(row, 1, row, 7)
                    .Style.Fill.BackgroundColor =
                    XLColor.DarkBlue;

                ws.Range(row, 1, row, 7)
                    .Style.Font.FontColor =
                    XLColor.White;

                ws.Range(row, 1, row, 7)
                    .Style.Font.Bold = true;

                ws.Cell(row, 1).Value =
                    $"Customer : {customer.customer_name}";

                ws.Cell(row, 4).Value =
                    $"Invoice : {customer.invoice_total:N2}";

                ws.Cell(row, 5).Value =
                    $"Paid : {customer.paid_total:N2}";

                ws.Cell(row, 6).Value =
                    $"Balance : {customer.balance_total:N2}";

                row += 2;

                foreach (var month in customer.months)
                {
                    // =====================================================
                    // MONTH HEADER
                    // =====================================================

                    ws.Range(row, 1, row, 7)
                        .Style.Fill.BackgroundColor =
                        XLColor.LightBlue;

                    ws.Range(row, 1, row, 7)
                        .Style.Font.Bold = true;

                    ws.Cell(row, 1).Value =
                        month.month;

                    ws.Cell(row, 4).Value =
                        $"Invoice : {month.invoice_total:N2}";

                    ws.Cell(row, 5).Value =
                        $"Paid : {month.paid_total:N2}";

                    ws.Cell(row, 6).Value =
                        $"Balance : {month.balance_total:N2}";

                    row++;

                    // =====================================================
                    // TABLE HEADER
                    // =====================================================

                    ws.Cell(row, 1).Value = "Date";

                    ws.Cell(row, 2).Value = "Ref. No";

                    ws.Cell(row, 3).Value = "Doc. Type";

                    ws.Cell(row, 4).Value = "Invoice Amt.";

                    ws.Cell(row, 5).Value = "Paid Amt.";

                    ws.Cell(row, 6).Value = "Balance Amt.";

                    ws.Cell(row, 7).Value =
                        "Running Balance";

                    var header =
                        ws.Range(row, 1, row, 7);

                    header.Style.Font.Bold = true;

                    header.Style.Fill.BackgroundColor =
                        XLColor.Gray;

                    header.Style.Font.FontColor =
                        XLColor.White;

                    row++;

                    // =====================================================
                    // DETAILS
                    // =====================================================

                    foreach (var item in month.details)
                    {
                        ws.Cell(row, 1).Value =
                            Convert.ToDateTime(item.date);

                        ws.Cell(row, 1)
                            .Style.DateFormat.Format =
                            "dd-MM-yyyy";

                        ws.Cell(row, 2).Value =
                            item.invoice_no;

                        ws.Cell(row, 3).Value =
                            item.doc_type;

                        ws.Cell(row, 4).Value =
                            item.invoice_amount;

                        ws.Cell(row, 5).Value =
                            item.paid_amount;

                        ws.Cell(row, 6).Value =
                            item.balance;

                        ws.Cell(row, 7).Value =
                            item.running_balance;

                        ws.Range(row, 4, row, 7)
                            .Style.NumberFormat.Format =
                            "#,##0.00";

                        // Zebra rows
                        if (row % 2 == 0)
                        {
                            ws.Range(row, 1, row, 7)
                                .Style.Fill.BackgroundColor =
                                XLColor.AliceBlue;
                        }

                        row++;
                    }

                    row += 2;
                }

                row++;
            }

            // =====================================================
            // FINAL STYLING
            // =====================================================

            ws.Columns().AdjustToContents();

            ws.SheetView.FreezeRows(2);

            ws.RangeUsed().Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            ws.RangeUsed().Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

     
       public async Task<byte[]> Gen_customerAging(List<CustomerAgingCustomerDto> customers)
        {
            using var workbook =
                new XLWorkbook();

            var ws =
                workbook.Worksheets.Add(
                    "Customer Aging"
                );

            int row = 1;

            // =====================================================
            // TITLE
            // =====================================================

            ws.Range(row, 1, row, 11).Merge();

            ws.Cell(row, 1).Value =
                "CUSTOMER AGING REPORT";

            ws.Cell(row, 1)
                .Style.Font.Bold = true;

            ws.Cell(row, 1)
                .Style.Font.FontSize = 18;

            ws.Cell(row, 1)
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            foreach (var customer in customers)
            {
                // =====================================================
                // CUSTOMER HEADER
                // =====================================================

                ws.Range(row, 1, row, 11)
                    .Style.Fill.BackgroundColor =
                    XLColor.DarkBlue;

                ws.Range(row, 1, row, 11)
                    .Style.Font.FontColor =
                    XLColor.White;

                ws.Range(row, 1, row, 11)
                    .Style.Font.Bold = true;

                ws.Cell(row, 1).Value =
                    $"Customer : {customer.customer_name}";

                ws.Cell(row, 6).Value =
                    $"Current : {customer.current_amount:N2}";

                ws.Cell(row, 7).Value =
                    $"31-60 : {customer.days_31_60:N2}";

                ws.Cell(row, 8).Value =
                    $"61-90 : {customer.days_61_90:N2}";

                ws.Cell(row, 9).Value =
                    $"91-120 : {customer.days_91_120:N2}";

                ws.Cell(row, 10).Value =
                    $"120+ : {customer.days_120_plus:N2}";

                ws.Cell(row, 11).Value =
                    $"Outstanding : {customer.total_outstanding:N2}";

                row += 2;

                // =====================================================
                // TABLE HEADER
                // =====================================================

                ws.Cell(row, 1).Value =
                    "Invoice No";

                ws.Cell(row, 2).Value =
                    "Invoice Date";

                ws.Cell(row, 3).Value =
                    "Due Date";

                ws.Cell(row, 4).Value =
                    "Age Days";

                ws.Cell(row, 5).Value =
                    "Pending";

                ws.Cell(row, 6).Value =
                    "Current";

                ws.Cell(row, 7).Value =
                    "31-60";

                ws.Cell(row, 8).Value =
                    "61-90";

                ws.Cell(row, 9).Value =
                    "91-120";

                ws.Cell(row, 10).Value =
                    "120+";

                var header =
                    ws.Range(row, 1, row, 10);

                header.Style.Font.Bold = true;

                header.Style.Fill.BackgroundColor =
                    XLColor.Gray;

                header.Style.Font.FontColor =
                    XLColor.White;

                row++;

                // =====================================================
                // DETAILS
                // =====================================================

                foreach (
                    var invoice
                    in customer.invoices
                )
                {
                    ws.Cell(row, 1).Value =
                        invoice.invoice_no;

                    ws.Cell(row, 2).Value =
                        invoice.invoice_date;

                    ws.Cell(row, 3).Value =
                        invoice.due_date;

                    ws.Cell(row, 4).Value =
                        invoice.age_days;

                    ws.Cell(row, 5).Value =
                        invoice.pending_amount;

                    ws.Cell(row, 6).Value =
                        invoice.current_amount;

                    ws.Cell(row, 7).Value =
                        invoice.days_31_60;

                    ws.Cell(row, 8).Value =
                        invoice.days_61_90;

                    ws.Cell(row, 9).Value =
                        invoice.days_91_120;

                    ws.Cell(row, 10).Value =
                        invoice.days_120_plus;

                    // DATE FORMAT

                    ws.Cell(row, 2)
                        .Style.DateFormat.Format =
                        "dd-MM-yyyy";

                    ws.Cell(row, 3)
                        .Style.DateFormat.Format =
                        "dd-MM-yyyy";

                    // NUMBER FORMAT

                    ws.Range(row, 5, row, 10)
                        .Style.NumberFormat.Format =
                        "#,##0.00";

                    // ZEBRA ROWS

                    if (row % 2 == 0)
                    {
                        ws.Range(row, 1, row, 10)
                            .Style.Fill.BackgroundColor =
                            XLColor.AliceBlue;
                    }

                    row++;
                }

                row += 2;
            }

            // =====================================================
            // FINAL STYLING
            // =====================================================

            ws.Columns()
                .AdjustToContents();

            ws.SheetView
                .FreezeRows(3);

            ws.RangeUsed()
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            ws.RangeUsed()
                .Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task<byte[]> Gen_customerOS(List<CustomerOutstandingDto> customers)
        {
            using var workbook =
                new XLWorkbook();

            var ws =
                workbook.Worksheets.Add(
                    "Customer OutStanding"
                );

            int row = 1;

            // =====================================================
            // TITLE
            // =====================================================

            ws.Range(row, 1, row, 7).Merge();

            ws.Cell(row, 1).Value =
                "CUSTOMER OUTSTANDING REPORT";

            ws.Cell(row, 1)
                .Style.Font.Bold = true;

            ws.Cell(row, 1)
                .Style.Font.FontSize = 18;

            ws.Cell(row, 1)
                .Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Center;

            row += 2;

            foreach (var customer in customers)
            {
                // =====================================================
                // CUSTOMER HEADER
                // =====================================================

                ws.Range(row, 1, row, 7)
                    .Style.Fill.BackgroundColor =
                    XLColor.DarkBlue;

                ws.Range(row, 1, row, 7)
                    .Style.Font.FontColor =
                    XLColor.White;

                ws.Range(row, 1, row, 7)
                    .Style.Font.Bold = true;

                ws.Cell(row, 1).Value =
                    $"Customer : {customer.customer_name}";

                ws.Cell(row, 5).Value =
                    $"Invoice : {customer.totalInvoiceAmount:N2}";

                ws.Cell(row, 6).Value =
                    $"Received : {customer.totalReceivedAmount:N2}";

                ws.Cell(row, 7).Value =
                    $"Pending : {customer.totalPendingAmount:N2}";


                row += 2;

                // =====================================================
                // TABLE HEADER
                // =====================================================

                ws.Cell(row, 1).Value =
                    "Invoice No";

                ws.Cell(row, 2).Value =
                    "Invoice Date";

                ws.Cell(row, 3).Value =
                    "Due Date";

                ws.Cell(row, 4).Value =
                    "Age Days";

                ws.Cell(row, 5).Value =
                    "Invoice Amount";

                ws.Cell(row, 6).Value =
                    "Received Amount";

                ws.Cell(row, 7).Value =
                    "Pending Amount";

                var header =
                    ws.Range(row, 1, row, 7);

                header.Style.Font.Bold = true;

                header.Style.Fill.BackgroundColor =
                    XLColor.Gray;

                header.Style.Font.FontColor =
                    XLColor.White;

                row++;

                // =====================================================
                // DETAILS
                // =====================================================

                foreach (
                    var invoice
                    in customer.Invoices
                )
                {
                    ws.Cell(row, 1).Value =
                        invoice.invoice_no;

                    ws.Cell(row, 2).Value =
                        invoice.invoice_date.ToShortDateString();

                    ws.Cell(row, 3).Value =
                        invoice.due_date.ToShortDateString();

                    ws.Cell(row, 4).Value =
                        invoice.age_days;

                    ws.Cell(row, 5).Value =
                        invoice.invoice_amount;

                    ws.Cell(row, 6).Value =
                        invoice.received_amount;

                    ws.Cell(row, 7).Value =
                        invoice.pending_amount;

                    

                    // DATE FORMAT

                    ws.Cell(row, 2)
                        .Style.DateFormat.Format =
                        "dd-MM-yyyy";

                    ws.Cell(row, 3)
                        .Style.DateFormat.Format =
                        "dd-MM-yyyy";

                    // NUMBER FORMAT

                    ws.Range(row, 5, row, 7)
                        .Style.NumberFormat.Format =
                        "#,##0.00";

                    // ZEBRA ROWS

                    if (row % 2 == 0)
                    {
                        ws.Range(row, 1, row, 7)
                            .Style.Fill.BackgroundColor =
                            XLColor.AliceBlue;
                    }

                    row++;
                }

                row += 2;
            }

            // =====================================================
            // FINAL STYLING
            // =====================================================

            ws.Columns()
                .AdjustToContents();

            ws.SheetView
                .FreezeRows(3);

            ws.RangeUsed()
                .Style.Border.OutsideBorder =
                XLBorderStyleValues.Thin;

            ws.RangeUsed()
                .Style.Border.InsideBorder =
                XLBorderStyleValues.Thin;

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }


    }
}

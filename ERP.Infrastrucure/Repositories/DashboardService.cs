using Microsoft.EntityFrameworkCore;
using ERP.Application.DTOs.Dashboard;
using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ERP.Application.Interfaces.Repositories.Common;

namespace ERP.Infrastructure.Repositories
{
    public class DashboardService: IDashboardService
    {
        public readonly AppDbContext _context;
        public readonly ICurrentTenantService _tenantService;

        public DashboardService(AppDbContext context, ICurrentTenantService tenantservice)
        {
            _context = context;
            _tenantService= tenantservice;
        }

        public async Task<List<DashboardCardMetric>> GetCardSummaryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var agedThresholdDate = DateTime.UtcNow.AddDays(-90);

            // 🔒 Extract the current tenant/company context securely
            long currentCompanyId = _tenantService.CompanyId ?? 0;

            // 1. Fetch Inventory Aggregations from public.inventory_grid_view
            var invData = await _context.inventoryGridViews
                .Where(i => i.company_id == currentCompanyId)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    TotalCost = g.Sum(i => i.quantity * i.unit_price),
                    TotalItems = g.Sum(i => i.quantity),
                    OutOfStockCount = g.Count(i => i.quantity <= 0),
                    LowStockCount = g.Count(i => i.quantity > 0 && i.quantity <= i.minstock),
                    AgedCount = g.Count(i => i.quantity > 0 && i.last_out_date <= agedThresholdDate)
                })
                .FirstOrDefaultAsync();

            var safeInvData = invData ?? new
            {
                TotalCost = (decimal?)0m,
                TotalItems = 0m,
                OutOfStockCount = 0,
                LowStockCount = 0,
                AgedCount = 0
            };

            // 2. Fetch Quotation Aggregations (Draft, Confirm, Cancel)
            var quoteData = await _context.quotations
                .Where(q => q.company_id == currentCompanyId)
                .GroupBy(q => 1)
                .Select(g => new
                {
                    DraftCount = g.Count(q => q.status == 13),
                    DraftValue = g.Where(q => q.status == 13).Sum(q => q.total_amount),
                    ConfirmCount = g.Count(q => q.status == 14),
                    CancelCount = g.Count(q => q.status == 15),
                    TotalActive = g.Count(q => q.status == 14 || q.status == 13 || q.status == 15)
                })
                .FirstOrDefaultAsync();

            var safeQuoteData = quoteData ?? new
            {
                DraftCount = 0,
                DraftValue = 0m,
                ConfirmCount = 0,
                CancelCount = 0,
                TotalActive = 0
            };

            // 3. Fetch Today's Daily Sales Aggregations
            var salesData = await _context.invoiceHeaders
                .Where(s => s.company_id == currentCompanyId && s.invoice_date.Date == today && s.status == 11)
                .GroupBy(s => 1)
                .Select(g => new
                {
                    TodayTotalAmount = g.Sum(s => s.total_amount),
                    TodayInvoiceCount = g.Count()
                })
                .FirstOrDefaultAsync();

            var safeSalesData = salesData ?? new
            {
                TodayTotalAmount = 0m,
                TodayInvoiceCount = 0
            };

            // Calculate the Quotation Conversion Win Rate Percentage safely
            double winRate = 0;
            if (safeQuoteData.TotalActive > 0)
            {
                winRate = Math.Round(((double)safeQuoteData.ConfirmCount / safeQuoteData.TotalActive) * 100, 1);
            }

            // 4. Return unified list matching the sequence order for your frontend loop
            return new List<DashboardCardMetric>
    {

        new DashboardCardMetric
        {
            Title = "Total Inventory Value",
            Value = $"₹{(safeInvData.TotalCost ?? 0m):N0}",
            Label = $"Total Items: {safeInvData.TotalItems}",
            Color = "#2e7d32", // Success Green
            IconKey = "TrendingUp"
        },
        new DashboardCardMetric
        {
            Title = "Low Stock Alerts",
            Value = $"{safeInvData.LowStockCount + safeInvData.OutOfStockCount} Items",
            Label = $"{safeInvData.OutOfStockCount} Out of stock • {safeInvData.LowStockCount} Low level",
            Color = "#d32f2f", // Danger Red
            IconKey = "WarningAmber"
        },
        new DashboardCardMetric
        {
            Title = "Aged / Dead Stock",
            Value = $"{safeInvData.AgedCount} Items",
            Label = "No active movement in 90 days",
            Color = "#f97316", // Warning Orange
            IconKey = "HourglassEmpty"
        },

        new DashboardCardMetric
        {
            Title = "Today's Sales",
            Value = $"₹{(safeSalesData.TodayTotalAmount):N0}",
            Label = $"{safeSalesData.TodayInvoiceCount} Invoices generated today",
            Color = "#0d9488", // Teal Accent
            IconKey = "Paid"
        },
        
        new DashboardCardMetric
        {
            Title = "Active Quotations",
            Value = $"{safeQuoteData.DraftCount} Pending",
            Label = $"Value: ₹{safeQuoteData.DraftValue :N0}",
            Color = "#0288d1", // Info Blue
            IconKey = "RequestQuote"
        },
        new DashboardCardMetric
        {
            Title = "Quotation Win Rate",
            Value = $"{winRate}%",
            Label = $"{safeQuoteData.ConfirmCount} Converted • {safeQuoteData.CancelCount} Cancelled",
            Color = "#16a34a", // Bright Green
            IconKey = "Percent"
        }
        
    };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Dashboard
{
    public class DashboardDto
    {
        public InventoryValueDto TotalInventoryValue { get; set; } = new();
        public LowStockAlertDto LowStockAlert { get; set; } = new();
        public AgedDeadStockDto AgedDeadStock { get; set; } = new();
    }
    public class InventoryValueDto
    {
        public string Currency { get; set; } = "INR";
        public decimal TotalCostValue { get; set; }
        public decimal TotalRetailValue { get; set; }
        public int TotalItemsCount { get; set; }
    }

    public class LowStockAlertDto
    {
        public int TotalLowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int ReorderItems { get; set; }
    }

    public class AgedDeadStockDto
    {
        public decimal AgedStockValue { get; set; }
        public int AgedItemsCount { get; set; }
        public decimal DeadStockValue { get; set; }
        public int DeadItemsCount { get; set; }
        public int ThresholdDays { get; set; } = 90;
    }

    public class DashboardCardMetric
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string IconKey { get; set; } = string.Empty; // Send a string token like "TrendingUp", "Warning"
    }
}

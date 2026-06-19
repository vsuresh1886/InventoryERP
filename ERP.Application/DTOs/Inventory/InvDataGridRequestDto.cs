using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Inventory
{
    public  class InvDataGridRequestDto
    {
            public int Page { get; set; }
            public int Limit { get; set; }
            public string? Search { get; set; }
            public List<long>? category { get; set; }
            public Boolean lowStock { get; set; }
            public string? sku { get; set; }
            public string? status { get; set; }
            public string? itemName { get; set; }
            public List<long>? domain { get; set; }
        
    }

    public class InventoryGridDto
    {
        public int Sno { get; set; }
        public string Id { get; set; }

        public string Sku { get; set; }

        public string? part_number { get; set; }

        public string item_name { get; set; }

        public List<string>? Tags { get; set; }

        public string? category { get; set; }

        public string? sub_category { get; set; }

        public string? domain { get; set; }

        public decimal quantity { get; set; }
        public decimal? min_stock { get; set; }

        public decimal? max_stock { get; set; }

        public string? unit { get; set; }

        public decimal? unit_price { get; set; }

        public string status { get; set; }
    }

    public class InventoryItemDto
    {
        public long id { get; set; }
        public string? sku { get; set; }
        public string? item_name { get; set; }
        public string? description { get; set; }

        public long category { get; set; }
        public long sub_category { get; set; }
        public long domain { get; set; }

        public decimal? quantity { get; set; }
        public decimal? min_stock { get; set; }
        public decimal? max_stock { get; set; }

        public long? unit { get; set; }
        public decimal? unit_price { get; set; }

        public long? supplier { get; set; }
        public string? location_bin { get; set; }

        public string? part_number { get; set; }
        public string? warranty_months { get; set; }
        public string? compatible_with { get; set; }

        public string? tags { get; set; }

        public string? status { get; set; }
    }


    public class InventoryPostingRequest
    {
        public long ReferenceId { get; set; }

        public string ReferenceNo { get; set; }

        public string TransactionCode { get; set; }

        public InventoryDirection Direction { get; set; }

        public List<InventoryPostingLine> Lines { get; set; }
    }
    public class InventoryPostingLine
    {
        public long ItemId { get; set; }

        public decimal Quantity { get; set; }
    }
    public enum InventoryDirection
    {
        In,
        Out
    }



}

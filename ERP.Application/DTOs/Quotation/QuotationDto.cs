using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Quotation
{
    public class QuotationDto
    {
        public long Id { get; set; } // 0 = create

        public string? QuotationNo { get; set; }

        public long CustomerId { get; set; }

        public long Salesperson { get; set; }

        public DateTime QuotationDate { get; set; }

        public DateTime? ValidUntil { get; set; }

        public int Status { get; set; } 

        public decimal Discount { get; set; }

        public string? Remarks { get; set; }

        // Totals (response use mainly)
        public decimal Subtotal { get; set; }
        public decimal VatTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public List<QuotationLineItemDto> LineItems { get; set; } = new();
    }

    public class QuotationLineItemDto
    {
        public string Id { get; set; } // 0 = new
        public long dbId { get; set; }

        public long quotation_id { get; set; }

        public long ItemId { get; set; } // 🔴 REQUIRED for DB

        // UI support fields (optional)
        public string? PartNo { get; set; }
        public string? Sku { get; set; }
        public string? PartName { get; set; }

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal VatPct { get; set; }

        public decimal TotalPrice { get; set; } // calculated
    }
}

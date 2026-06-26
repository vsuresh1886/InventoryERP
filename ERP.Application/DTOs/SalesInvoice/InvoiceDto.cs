using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.SalesInvoice
{
    public class InvoiceDto
    {

        public long Id { get; set; } // 0 = create

        public string? invoiceNo { get; set; }

        public long? quotationid { get; set; }

        public long CustomerId { get; set; }

        public long Salesperson { get; set; }

        public DateTime invoiceDate { get; set; }

        public DateTime ValidUntil { get; set; }

        public long Status { get; set; } = 0;

        public decimal Discount { get; set; }

        public string? Remarks { get; set; }

        // Totals (response use mainly)
        public decimal Subtotal { get; set; }
        public decimal VatTotal { get; set; }
        public decimal GrandTotal { get; set; }

        public List<InvoiceLineItemDto> LineItems { get; set; } = new();
    }


    public class InvoiceLineItemDto
    {
        public string Id { get; set; } // 0 = new
        public long dbId { get; set; }

        public long invoice_id { get; set; }

        public long ItemId { get; set; } 

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

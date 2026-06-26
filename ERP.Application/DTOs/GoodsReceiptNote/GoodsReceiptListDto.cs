using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ERP.Application.DTOs.GoodsReceiptNote
{


    public class GRNGridRequestDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public int? invoiceno { get; set; }
        public List<long>? vendor { get; set; }
        public List<long>? salesperson { get; set; }
        public string? datefrom { get; set; }
        public string? dateto { get; set; }
        public int? status { get; set; }
    }

    public class GoodsReceiptListDto
    {
        public long sno { get; set; }
        public long id { get; set; }

        public long companyId { get; set; }

        public string grnNo { get; set; } = string.Empty;

        public DateOnly grnDate { get; set; }

        public long poId { get; set; }

        public string? poNo { get; set; }

        public long vendorId { get; set; }

        public string vendorName { get; set; } = string.Empty;

        public string? challanNo { get; set; }

        public DateOnly? challanDate { get; set; }

        public string status { get; set; } = string.Empty;

        public decimal totalQtyRec { get; set; }

        public decimal grandTotal { get; set; }

    }

    public class GRNPOlistDto
    {
        public long id { get; set; }
        public decimal subtotal { get; set; }
       public decimal taxAmount { get; set; }
      public decimal discountAmount { get; set; }
      public decimal grandTotal { get; set; }
        public List<POSLineItems> lineItems { get; set; }
    }

    public class POSLineItems
    {
        public long  grnDetailId { get; set; }
        public long poDetailId{ get; set; }
        public long itemId{ get; set; }
        public string partNo{ get; set; }
        public string partName{ get; set; }
        public decimal orderedQty{ get; set; }
        public decimal previouslyReceivedQty{ get; set; }
        public decimal balanceQty{ get; set; } 
        public decimal receivedQty{ get; set; }
        public decimal unitPrice{ get; set; }
        public decimal taxPct{ get; set; }
        public decimal taxAmount{ get; set; }
        public decimal lineTotal{ get; set; }
        public string? remarks{ get; set; }

    }

    public class GoodsReceiptDto
    {
     
        public long id { get; set; }
        public string? grnNo { get; set; }
        public DateTime grnDate { get; set; }
        public long poId { get; set; }
        public string poNo { get; set; } = string.Empty;
        public long vendorId { get; set; }
        public long warehouseId { get; set; }
        public long branchId { get; set; }
        public string vendorReferenceNo { get; set; } = string.Empty;
        public DateTime vendorReferenceDate { get; set; }
        public string? vehicleNo { get; set; }
        public long receivedBy { get; set; }
        public int status { get; set; }
        public decimal totalQtyReceived { get; set; }
        public decimal subTotal { get; set; }
        public decimal taxAmount { get; set; }
        public decimal discountAmount { get; set; }
        public decimal grandTotal { get; set; }

        // Safely handles empty strings or missing notes
        public string? remarks { get; set; }

        // Initializes to prevent NullReferenceException if frontend omits array
        public List<GoodsReceiptLineItemDto> lineItems { get; set; } = new();
    }
    public class GoodsReceiptLineItemDto
    {
        // Keeping it as a string allows it to easily accept the incoming GUID format 
        public long grnDetailId { get; set; } = 0;
        public long itemId { get; set; }
        public long poDetailId { get; set; }  
        public string partNo { get; set; } = string.Empty;
        public string partName { get; set; } = string.Empty;
        public decimal orderedQty { get; set; }
        public decimal previouslyReceivedQty { get; set; }
        public decimal balanceQty { get; set; }
        public decimal receivedQty { get; set; }
        public decimal unitPrice { get; set; }
        public decimal taxPct { get; set; }
        public decimal taxAmount { get; set; }
        public decimal lineTotal { get; set; }

        // Safely handles line item level remarks when empty or null
        public string? remarks { get; set; }
    }

}

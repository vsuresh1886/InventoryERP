using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Purchase
{
    public class PurchaseOrderGridDto
    {
        public int sno { get; set; }
        public long poid { get; set; }

        public string pono { get; set; } = "";

        public DateTime podate { get; set; }

        public string vendor { get; set; } = "";

        public int items { get; set; }

        public decimal qty { get; set; }

        public decimal received { get; set; }

        public decimal balance { get; set; }

        public decimal amount { get; set; }

        public DateTime? expecteddeliverydate { get; set; }

        public string status { get; set; } = "";

        public string action { get; set; } = "";
    }


    public class PurchaseGridRequestDto
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


    public class PurchaseOrderDto
    {
        public long poId { get; set; }

        public string poNo { get; set; } = string.Empty;

        public DateTime PoDate { get; set; }

        public long vendorId { get; set; }

        public long? buyerId { get; set; }

        public long? warehouseId { get; set; }

        public DateTime? expectedDeliveryDate { get; set; }
      

        public int status { get; set; }

        public decimal subTotal { get; set; }

        public decimal taxTotal { get; set; }

        public decimal discountAmount { get; set; }

        public decimal grandTotal { get; set; }

        public string? remarks { get; set; }

        public List<PurchaseOrderLineDto> LineItems { get; set; } = new();
    }

    public class PurchaseOrderLineDto
    {
        public long DbId { get; set; }

        public long itemId { get; set; }

        public string partNo { get; set; } = string.Empty;

        public string sku { get; set; } = string.Empty;

        public string partName { get; set; } = string.Empty;

        public decimal orderedQty { get; set; }

        public decimal receivedQty { get; set; }

        public decimal balanceQty { get; set; }

        public decimal unitPrice { get; set; }

        public decimal taxPct { get; set; }

        public decimal taxAmount { get; set; }

        public decimal totalPrice { get; set; }
    }



}

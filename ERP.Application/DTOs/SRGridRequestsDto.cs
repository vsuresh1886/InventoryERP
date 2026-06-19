using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ERP.Application.DTOs
{
    public class SRGridRequestsDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public int? invoiceno { get; set; }
        public List<long>? customer { get; set; }
        public List<long>? salesperson { get; set; }
        public string? datefrom { get; set; }
        public string? dateto { get; set; }
        public int? status { get; set; }
    }

    public class SRGridDto
    {
        public int sno { get; set; }
        public long id { get; set; }
        public string? returnno { get; set; }
        public DateOnly? returndate { get; set; }
        public long? invoiceid { get; set; }
        public string? invoiceno { get; set; }
        public string? customerid { get; set; }
        public decimal? returnedqty { get; set; }
        public decimal amount { get; set; }
        public string? status { get; set; }
        public string? actions { get; set; }
    }

    public class SalesReturnDto
    {
        public long id { get; set; }
        public string returnNo { get; set; } = string.Empty;

        public DateTime returnDate { get; set; }

        public long customerId { get; set; }

        public long invoiceId { get; set; }

        public long salesperson { get; set; }

        public decimal invoiceTotal { get; set; }
        public decimal returnedTotal { get; set; }
        public decimal balanceTotal { get; set; }

        public decimal  invoiceQty { get; set; }
        public decimal returnedQty { get; set; }
        public decimal balanceQty { get; set; }

        public int status { get; set; }

        public string? remarks { get; set; }

        public int? returnreason { get; set; }


        public List<SalesReturnDetailDto> LineItems { get; set; } = [];
    }

    public class SalesReturnDetailDto
    {
        public long DbId { get; set; }

        public long InvoiceDetailId { get; set; }

        public long ItemId { get; set; }
        public string partNo { get; set; }
        public string partName { get; set; }

        public decimal InvoiceQty { get; set; }

        public decimal ReturnedQty { get; set; }

        public decimal BalanceQty { get; set; }

        public decimal ReturnQty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; } // return amount for the each line item
    }

    public class SalesReturnInvoiceDto
    {
        public long InvoiceId { get; set; }

        public long CustomerId { get; set; }

        public long SalesPersonId { get; set; }

        public string InvoiceNo { get; set; }

        public decimal InvoiceTotal { get; set; }

        public decimal ReturnedTotal { get; set; }

        public decimal BalanceTotal { get; set; }

        public decimal InvoiceQty { get; set; }

        public decimal ReturnedQty { get; set; }

        public decimal BalanceQty { get; set; }

        public List<SalesReturnInvoiceDetailDto> LineItems { get; set; }
            = new();
    }
    public class SalesReturnInvoiceDetailDto
    {
        public long InvoiceDetailId { get; set; }

        public long ItemId { get; set; }

        public string? PartNo { get; set; }

        public string? PartName { get; set; }

        public decimal InvoiceQty { get; set; }

        public decimal ReturnedQty { get; set; }

        public decimal BalanceQty { get; set; }

        // Original values for reference
        public decimal UnitPrice { get; set; }

        public decimal DiscountPercentage { get; set; }

        public decimal TaxPercentage { get; set; }

        // Effective rate shown in Sales Return screen

        public decimal Rate { get; set; }

        public decimal LineAmount { get; set; }
    }


}

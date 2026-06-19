using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Accounts
{
    public class CollectionGridDto
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public string? Search { get; set; }
        public int? receiptno { get; set; }
        public int? invoiceno { get; set; }
        public List<long>? customer { get; set; }
        public string? datefrom { get; set; }
        public string? dateto { get; set; }
        public int? status { get; set; }
    }


    public class CollectionGridDataDto
    {
       public int  sno { get; set; }
        public long? id { get; set; }
       public string? receiptno { get; set; }
        public long? invoiceid { get; set; }
        public string? invoiceno { get; set; }
         public DateTime?   receiptdate { get; set; }
         public string? customerid { get; set; }
        public string? customername { get; set; }
        public string? status { get; set; }
        public decimal? amount { get; set; }
        public string? actions { get; set; }
    }

    public class OutstandingInvoiceCDto
    {
        public long Aid { get; set; }
        public long  invid { get; set; }
        public string?   invoiceNo { get; set; }
        public DateTime invoiceDate { get; set; }
        public decimal grandTotal { get; set; }
        public decimal paidAmount { get; set; }
        public decimal balanceAmount { get; set; }
    }
}

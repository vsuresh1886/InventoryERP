using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs.Accounts
{
    public class CollectionDto
    {
        public long id { get; set; }
        public string? receiptNo { get; set; }
        public long? customerId { get; set; }
        public DateTime? receiptDate { get; set; }
        public long? paymentMode { get; set; }
        public String? referenceNo { get; set; }
        public decimal? totalAmount { get; set; }
        public string? remarks { get; set; }
        public long? status { get; set; }
        public long? created_by { get; set; }
        public DateTime created_at { get; set; }
       
        public List<CollectiondetailDto>? allocations { get; set; }
    }



    public class CollectiondetailDto
    {
        public long aid { get; set; }
        public long? collection_id { get; set; }
        public long? invoiceId { get; set; }
        public decimal? allocateAmount { get; set; }
        public DateTime created_at { get; set; }

        public bool selected { get; set; }

        public string invoiceNo { get; set; }

        public DateTime invoiceDate { get; set; }

        public decimal invoiceAmount { get; set; }

        public decimal paidAmount { get; set; }

        public decimal balanceAmount { get; set; }
    }



    public class CollectionSaveDto
    {
       public long  id{ get; set; }

      public string receiptNo{ get; set; }

      public long customerId{ get; set; }

      public DateTime receiptDate{ get; set; }

      public long paymentMode { get; set; }

      public string referenceNo{ get; set; }

      public long status { get; set; }

      public decimal totalAmount{ get; set; }

    public string remarks{ get; set; }

    public List<allocation> allocations { get; set; }
    }
    public class allocation
    {
        public long Aid { get; set; }

        public long invoiceId{ get; set; }

        public decimal allocateAmount { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Quotation
{
    public class QuotationModel
    {
        public string companyname { get; set;}
        public string companyaddress { get; set; }
        public string companyphone { get; set; }
        public string companymail { get; set; }
        public string gstin { get; set; }


        public string customername { get; set; }
        public string customeraddress { get; set; }
        public string contactperson { get; set; }
        public string contact { get; set; }

        public string quotationno { get; set; }
        public DateTime quotationdate { get; set; }
        public string validity { get; set; }

        public List<QuotationItems> items { get; set; }

        public string amtinwords { get; set; }
        public decimal subtotal { get; set; }
        public decimal gst { get; set; }
        public decimal discount { get; set; }
        public decimal total { get; set; }

        public string notes { get; set; }

        public string? AccountHolderName { get; set; }
        public string? BankName { get; set; }
        public string? AccountNumber { get; set; }
        public string? BranchName { get; set; }
        public string? IfscCode { get; set; }


    }

    public class QuotationItems
    {
        public int sno { get; set; }
        public string partno { get; set; }
        public string partname { get; set; }
        public int quantity { get; set; }
        public string units { get; set; }
        public decimal unitprice { get; set; }
        public decimal vatamt { get; set; }
        public string vatper { get; set; }
        public decimal totalprice { get; set; }
    }

  



}

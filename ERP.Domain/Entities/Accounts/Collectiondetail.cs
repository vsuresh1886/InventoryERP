using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Accounts
{
    [Table("rec_collection_detail")]
    public class Collectiondetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long id { get; set; }
            public long collection_id { get; set; }
            public long sales_invoice_id { get; set; }
            public decimal allocated_amount { get; set; }
            public DateTime created_at { get; set; }
        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Accounts
{
    [Table("rec_collection_header")]
    public class Collectionheader
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long id { get; set; }
            public string? receipt_no { get; set; }
            public long? customer_id { get; set; }
            public DateTime? receipt_date { get; set; }
            public long payment_mode { get; set; }
            public string? reference_no { get; set; }
            public decimal? total_amount { get; set; }
            public string? remarks { get; set; }
            public long? status { get; set; }
            public long? created_by { get; set; }
            public DateTime created_at { get; set; }
            public long? updated_by { get; set; }
            public DateTime? updated_at { get; set; }

    }
}

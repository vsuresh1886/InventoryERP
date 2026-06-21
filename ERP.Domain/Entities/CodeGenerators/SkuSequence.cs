using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.CodeGenerators
{
    [Table("sku_sequence")]
    public  class SkuSequence:IMustHaveTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }
       
        public string? prefix { get; set; }
        public int last_number { get; set; }
        public long? company_id { get; set; }
    }
}

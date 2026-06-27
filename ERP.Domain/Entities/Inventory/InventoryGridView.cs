using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities.Inventory
{


    [Table("inventory_grid_view")]

        public class InventoryGridView:IMustHaveTenant
        {
        
            public long id { get; set; }
            public string? sku { get; set; }
            public string? itemname { get; set; }
            public long category_id { get; set; }
            public string? category { get; set; }
            public string? subcategory { get; set; } 
           public long domain_id { get; set; }
            public string? domain { get; set; }
            public string? unit { get; set; }
            public decimal quantity { get; set; }
            public decimal? minstock { get; set; }
            public decimal? maxstock { get; set; }
            public string? partnumber { get; set; }
            public string? tags { get; set; }
            public decimal? retail_price { get; set; }
            public decimal? wholesale_price { get; set; }
            public decimal? dealer_price { get; set; }
            public decimal? last_purchase_price { get; set; }
            public decimal? unit_price { get; set; }
            public long? company_id { get; set; }
            public DateTime? last_in_date { get; set; }
            public DateTime? last_out_date { get; set; }


        }

    
}

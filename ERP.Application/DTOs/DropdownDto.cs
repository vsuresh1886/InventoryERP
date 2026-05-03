using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class DropdownDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class DropdownItemsDto
    {
        public long id { get; set; }
        public string? sku { get; set; }
        public string? partNo { get; set; }
        public string? name { get; set; }
        public decimal? price { get; set; }
        public decimal gstPct { get; set; }
        public decimal availableStock { get; set; }
    }
}

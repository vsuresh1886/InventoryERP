using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class GridHeadDto
    {
        public long grid_config_id { get; set; }
        public long form_fk { get; set; }
        public string? grid_id { get; set; }
        public string? header_name { get; set; }
        public string? display_name { get; set; }
        public string? tool_tip { get; set; }
        public string? data_type { get; set; }
        public int column_width { get; set; }
        public string? text_align { get; set; }
        public bool is_visible { get; set; }
        public bool is_sortable { get; set; }
        public bool is_filterable { get; set; }
        public int display_order { get; set; }
        public DateTime created_at { get; set; }
        public long created_by { get; set; }
        public DateTime updated_at { get; set; }
        public long updated_by { get; set; }
    }
}

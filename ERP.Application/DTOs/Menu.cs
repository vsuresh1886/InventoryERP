using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class MenuDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Route { get; set; }
        public string? Icon { get; set; }

        public List<MenuDto> Children { get; set; } = new();
    }
}

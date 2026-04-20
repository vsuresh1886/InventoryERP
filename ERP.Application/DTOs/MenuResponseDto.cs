using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class MenuResponseDto
    {
        public List<MenuDto> Menu { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

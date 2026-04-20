using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.DTOs
{
    public class GridDataResponse<T>
    {
        public List<T>? Data { get; set; }
        public int Total { get; set; }
    }


}

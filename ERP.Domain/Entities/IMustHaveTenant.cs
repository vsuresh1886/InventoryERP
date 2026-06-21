using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Common
{
    public interface IMustHaveTenant
    {
        long? company_id { get; set; }
    }
}

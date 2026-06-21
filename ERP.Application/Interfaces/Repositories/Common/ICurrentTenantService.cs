using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Common
{
    public  interface ICurrentTenantService
    {
        long? CompanyId { get; }
        void SetTenantBypass();

    }
}

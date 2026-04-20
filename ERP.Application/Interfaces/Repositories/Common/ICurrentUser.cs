using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Common
{
    public  interface ICurrentUser
    {
        long UserId { get; }
        string Email { get; }
        long TenantId { get; }
        string Role { get; }


    }
}

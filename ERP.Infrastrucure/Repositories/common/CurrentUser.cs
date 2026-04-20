using ERP.Application.Interfaces.Repositories.Common;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace ERP.Infrastructure.Repositories.common
{

    public  class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _context;

        public CurrentUser(IHttpContextAccessor context)
        {
            _context = context;
        }

        private ClaimsPrincipal User => _context.HttpContext?.User;

        public long UserId =>
                     long.TryParse(
                         User?.FindFirst("UserId")?.Value,
                         out var id) ? id : 0;
        public string Email =>
                    User?.FindFirst(ClaimTypes.Email)?.Value;

        public long TenantId =>
            long.Parse(User?.FindFirst("tenantId")?.Value ?? "0");

        public string Role =>
            User?.FindFirst(ClaimTypes.Role)?.Value;

    }
}

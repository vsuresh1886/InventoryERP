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

        // Helper properties to safely access the context
        private HttpContext HttpContext => _context.HttpContext;
        private ClaimsPrincipal User => HttpContext?.User;

        public long UserId =>
            long.TryParse(User?.FindFirst("UserId")?.Value, out var id) ? id : 0;

        // FIX: Match ClaimTypes.Name because that's what your token generator uses!
        public string Email =>
            User?.FindFirst(ClaimTypes.Name)?.Value;

        // FIX: Match "CompanyId" string from your JWT generation logic
        public long TenantId =>
            long.TryParse(User?.FindFirst("CompanyId")?.Value, out var id) ? id : 0;

        public string Role =>
            User?.FindFirst(ClaimTypes.Role)?.Value;

    }
}

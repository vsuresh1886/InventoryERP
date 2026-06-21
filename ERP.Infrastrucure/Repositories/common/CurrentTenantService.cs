using ERP.Application.Interfaces.Repositories.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories.common
{
    public class CurrentTenantService: ICurrentTenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _isBypassActive = false;
        public CurrentTenantService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long? CompanyId
        {
            get
            {
                if (_isBypassActive) return 0;
                // Grabs the "CompanyId" claim baked into your JWT token
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
                return long.TryParse(claim, out var id) ? id : null;
            }
        }

        public void SetTenantBypass()
        {
            _isBypassActive = true;
        }

    }
}

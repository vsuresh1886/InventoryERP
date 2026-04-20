using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IPermissionService
    {
        public Task<List<string>> GetUserPermissions(int userId);

    }
}

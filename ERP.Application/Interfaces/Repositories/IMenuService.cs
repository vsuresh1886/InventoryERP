using ERP.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories
{
    public interface IMenuService
    {
        public Task<List<MenuDto>> GetMenuForUser(int userId);

    }
}

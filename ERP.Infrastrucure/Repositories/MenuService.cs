using ERP.Application.DTOs;
using ERP.Application.Interfaces.Repositories;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;


namespace ERP.Infrastructure.Repositories
{
    public class MenuService:IMenuService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IPermissionService _permissionService;

        public MenuService(AppDbContext context, IConfiguration config, IPermissionService permissionService    )
        {
            _context = context;
            _config = config;
            _permissionService = permissionService;
        }



        public async Task<List<MenuDto>> GetMenuForUser(int userId)
        {
            
            var menus = await GetMenus();

            var permissions = await _permissionService.GetUserPermissions(userId);

            var tree = BuildMenuTree(menus, permissions);

            return tree;
        }

        public async Task<List<Menu>> GetMenus()
        {
            return await _context.Menus
                .Include(m => m.MenuPermissions)
                    .ThenInclude(mp => mp.Permission) // ✅ FIXED
                .Where(m => m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();
        }

        private List<MenuDto> BuildMenuTree(List<Menu> menus, List<string> permissions, long? parentId = null)
        {
            return menus
                .Where(m => m.ParentId == parentId)
                .Where(m =>
                    !m.MenuPermissions.Any() ||
                    m.MenuPermissions.Any(mp => permissions.Contains(mp.Permission.code))
                )
                .Select(m => new MenuDto
                {
                    Id = m.MenuId,
                    Title = m.Title,
                    Route = m.Route,
                    Type = m.MenuType,
                    Icon = m.Icon,
                    Children = BuildMenuTree(menus, permissions, m.MenuId)
                })
                .ToList();
        }

    }
}

using ERP.Application.Interfaces.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class PermissionService:IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<string>> GetUserPermissions(int userId)
        {
            return await (
                from ur in _context.UserRoles
                join rp in _context.RolePermissions on ur.roleid equals rp.roleid
                join p in _context.Permissions on rp.permissionid equals p.permissionpk
                where ur.userid == userId
                select p.code
            ).Distinct().ToListAsync();
        }


    }
}

using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Persistence
{
    public class AuthDbContext:DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
        public DbSet<employee_master> employee_masters { get; set; }
    }
}

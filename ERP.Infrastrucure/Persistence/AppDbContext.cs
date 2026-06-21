using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Accounts;
using ERP.Domain.Entities.CodeGenerators;
using ERP.Domain.Entities.Company;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.PurchaseOrder;
using ERP.Domain.Entities.Quotation;
using ERP.Domain.Entities.SalesInvoice;
using ERP.Domain.Entities.SalesReturn;
using ERP.Domain.Entitiess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace ERP.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        private readonly ICurrentTenantService _tenantService;

        // Inject the Tenant Service alongside DB Options
        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentTenantService tenantService) : base(options) 
        {
            _tenantService = tenantService;
        }

        public DbSet<company> companies { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<Roles> Roles { get; set; } 
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<MenuPermission> MenuPermissions { get; set; } 
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<employee_master> employees { get; set; }
        public DbSet<designation_master> designation_Masters { get; set; }
        public DbSet<department_master> department_Masters { get; set; }
        public DbSet<grid_configuration_mst> grid_Configuration_Msts { get; set; }
        public DbSet<country_master> countrymasters { get; set; }
        public DbSet<masterddlookup> ddlookups { get; set; }
        public DbSet<code_protocol_master> CodeProtocolMaster { get; set; }
        public DbSet<code_sequence_tracker> CodeSequenceTracker { get; set; }
        public DbSet<customer_master> customers { get; set; }
        public DbSet<party_type> party_Types { get; set; }
        public DbSet<Itemmaster> itemmasters { get; set; }
        public DbSet<Iteminventoryconfig> iteminventoryconfigs { get; set; }
        public DbSet<Categorymaster> categorymasters { get; set; }
        public DbSet<Domainmaster> domainmasters { get; set; }
        public DbSet<InventoryTransaction> inventoryTransactions { get; set; }
        public DbSet<Itemattributes> itemattributes { get; set; }
        public DbSet<Location> locations { get; set; }
        public DbSet<Subcategorymaster> subcategories { get; set; }
        public DbSet<Unitmaster> unitmasters { get; set; }
        public DbSet<Warehouse> warehouses { get; set; }
        public DbSet<SkuSequence> SkuSequences { get; set; }

        public DbSet<InventoryGridView> inventoryGridViews { get; set; }

        public DbSet<QuotationHeader> quotations { get; set; }
        public DbSet<QuotationLines> quotationsLines { get; set; }

        public DbSet<InvoiceHeader> invoiceHeaders { get; set; }
        public DbSet<InvoiceLines> invoicelines { get; set; }

        public DbSet<SalesReturnHeader> sales_Return_Headers { get; set; }
        public DbSet<salesreturndetail> sales_Return_Details { get; set; }

        public DbSet<Collectionheader> collectionheaders { get; set; }
        public DbSet<Collectiondetail> collectiondetails { get; set; }
        public DbSet<CustomerSOARowDto> CustomerSOARowDtos { get; set; }
        public DbSet<CustomerAgingRowDto> CustomerAgingRowDtos { get; set; }
        public DbSet<ReceivableOutstandingRowDto> ReceivableOutstandingRowDtos { get; set; }
        public DbSet<MessageSetting> MessageSettings { get; set; }

        public DbSet<PurchaseOrderHeader> purchaseorderheaders { get; set; }
        public DbSet<purchaseOrderDetail> purchaseOrderDetails { get; set; }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Get all entities that are being Added or Modified
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IMustHaveTenant &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            // 2. Fetch the current company ID from your injected tenant service
            long? currentCompanyId = _tenantService.CompanyId;

            if (currentCompanyId == 0)
            {
                // Bypass is active (Signup mode). 
                // Pass control straight to EF Core to save what was manually assigned in the service.
                return base.SaveChangesAsync(cancellationToken);
            }

            if (currentCompanyId == null)
            {
                // This prevents orphaned data or system operations without a tenant context
                throw new UnauthorizedAccessException("Cannot save multi-tenant data without a valid company context.");
            }

            foreach (var entry in entries)
            {
                var entity = (IMustHaveTenant)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // Automatically assign the company ID on creation
                    entity.company_id = currentCompanyId.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Safety Catch: Ensure a tenant can never maliciously alter or swap the company_id during an update
                    entry.Property(nameof(IMustHaveTenant.company_id)).IsModified = false;
                }
            }

            // 3. Pass control back to standard EF Core saving mechanics
            return base.SaveChangesAsync(cancellationToken);
        }

        // Keep a synchronous override just in case your application calls .SaveChanges() anywhere
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IMustHaveTenant &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            long? currentCompanyId = _tenantService.CompanyId;

            if (currentCompanyId == 0)
            {
                // Bypass is active (Signup mode). 
                // Pass control straight to EF Core to save what was manually assigned in the service.
                return base.SaveChanges();
            }
            if (currentCompanyId == null)
                throw new UnauthorizedAccessException("Cannot save multi-tenant data without a valid company context.");

            foreach (var entry in entries)
            {
                var entity = (IMustHaveTenant)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.company_id = currentCompanyId.Value;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(IMustHaveTenant.company_id)).IsModified = false;
                }
            }

            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolePermissions>()
       .HasKey(rp => new { rp.roleid, rp.permissionid });
            modelBuilder.Entity<MenuPermission>()
                .HasOne(mp => mp.Menu)
                .WithMany(m => m.MenuPermissions)
                .HasForeignKey(mp => mp.menuid);

            modelBuilder.Entity<MenuPermission>()
                .HasOne(mp => mp.Permission)
                .WithMany(p => p.MenuPermissions)
                .HasForeignKey(mp => mp.permissionid);
            modelBuilder.Entity<InventoryGridView>().HasNoKey().ToView("inventory_grid_view");
            modelBuilder.Entity<CustomerSOARowDto>(entity=> { entity.HasNoKey(); entity.ToView(null); });
            modelBuilder.Entity<CustomerAgingRowDto>(entity => { entity.HasNoKey(); entity.ToView(null); });
            modelBuilder.Entity<ReceivableOutstandingRowDto>(entity => { entity.HasNoKey(); entity.ToView(null); });

            modelBuilder.Entity<code_sequence_tracker>().HasIndex(t => new { t.company_id, t.module_name }).IsUnique();
            // ==========================================
            // AUTOMATIC GLOBAL TENANT FILTER LOGIC
            // ==========================================
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Check if entity implements IMustHaveTenant
                if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(IMustHaveTenant.company_id));

                    // 🎯 FIX: Get the actual type of the company_id property (e.g., long or int)
                    var propertyType = property.Type;

                    var tenantServiceProperty = Expression.Property(Expression.Constant(_tenantService), nameof(ICurrentTenantService.CompanyId));

                    // 🎯 FIX: Convert the tenant service value to match the exact property type
                    var filterCondition = Expression.Equal(property, Expression.Convert(tenantServiceProperty, propertyType));

                    var lambda = Expression.Lambda(filterCondition, parameter);

                    // Apply the filter to this entity type
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }


        }
    }
}

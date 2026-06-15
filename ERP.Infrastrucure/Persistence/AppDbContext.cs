using ERP.Application.DTOs;
using ERP.Application.DTOs.Accounts;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Accounts;
using ERP.Domain.Entities.CodeGenerators;
using ERP.Domain.Entities.Inventory;
using ERP.Domain.Entities.Quotation;
using ERP.Domain.Entities.SalesInvoice;
using ERP.Domain.Entities.SalesReturn;
using ERP.Domain.Entitiess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ERP.Infrastructure.Persistence
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

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
        }
    }
}

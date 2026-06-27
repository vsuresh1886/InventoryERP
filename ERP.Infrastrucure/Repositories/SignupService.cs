using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;
using ERP.Application.DTOs.company;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Company;
using ERP.Domain.Entities.Inventory;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories
{
    public class SignupService:ISignupService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly ICurrentTenantService _tenantService;
        private readonly ICodeGeneratorService _codeGeneratorService;
        public SignupService(AppDbContext context, IPasswordService passwordService, ICurrentTenantService tenantService , ICodeGeneratorService codeGeneratorService   )
        {
            _context = context;
            _passwordService = passwordService;
            _tenantService = tenantService;
            _codeGeneratorService = codeGeneratorService;
        }

        public async Task<string> SignupCompany(CompanySignupDto dtos)
        {
            _tenantService.SetTenantBypass();
            if (dtos == null)
                return "Something Went Wrong! Please check the details!";

            // 1. Check if the company email or company name is already registered
            var companyExists = await _context.companies
                .AnyAsync(x => x.email == dtos.companyEmail || x.company_name == dtos.companyName);

            if (companyExists)
            {
                return "Company Already Exists!";
            }

            // 2. Check if the Admin Email is already taken by another user
            var adminExists = await _context.employees
                .AnyAsync(x => x.email == dtos.adminEmail);

            if (adminExists)
            {
                return "Admin Email is already registered!";
            }

            // Begin the database transaction contextually
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 3. Create and add the new company record
                var newCompany = new company
                {
                    company_name = dtos.companyName,
                    business_type = dtos.businessType,
                    email = dtos.companyEmail,
                    country = dtos.country,
                    phone = dtos.phone,
                    is_active = true,
                    created_at = DateTime.UtcNow,
                    short_name = string.Join("", dtos.companyName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(word => word[0])),
                };

                _context.companies.Add(newCompany);

                // Save here so PostgreSQL generates the primary Key/Identity ID for newCompany
                await _context.SaveChangesAsync();

                // 4. Create and add the Admin User using the newly generated Company ID
                // Note: Change 'User' and field names to match your exact entity configuration
                var adminUser = new employee_master
                {
                    employee_id = await _codeGeneratorService.GenerateAsync("Employee", newCompany.id),
                    company_id = newCompany.id,
                    country = dtos.country, // Foreign Key generated from SaveChangesAsync above
                    first_name = dtos.adminName,
                    email = dtos.adminEmail,
                    joining_date = DateTime.UtcNow,
                    // SECURITY WARNING: Always hash passwords using a library like BCrypt.Net or ASP.NET Core Identity!
                    password = _passwordService.HashPassword(dtos.Password),
                    department_id = _context.department_Masters.Where(x=>x.department_id== "ADMIN").Select(x=>x.department_pk).FirstOrDefault(),
                    designation_id = _context.designation_Masters.Where(x => x.designation_id == "MNG").Select(x => x.designation_pk).FirstOrDefault(),
                    status = "1",
                    created_at = DateTime.UtcNow
                };

                _context.employees.Add(adminUser);
                await _context.SaveChangesAsync();

                var userroles = new UserRoles
                {
                    userid = adminUser.employee_pk,
                    roleid = 1,
                };

                _context.UserRoles.Add(userroles);
                await _context.SaveChangesAsync();

                // create the master data for the new company

                // A. Create & Add your Core Domain Masters
                var sparePartsDomain = new Domainmaster
                {
                    company_id = newCompany.id,
                    code = "SPT",
                    name = "Spare Parts",
                    description = "Automotive and mechanical replacement components",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                var solarDomain = new Domainmaster
                {
                    company_id = newCompany.id,
                    code = "SLR",
                    name = "Solar",
                    description = "Renewable energy equipment, brackets, and solar components",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                var generalDomain = new Domainmaster
                {
                    company_id = newCompany.id,
                    code = "GEN",
                    name = "General",
                    description = "General operational miscellaneous assets",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                _context.domainmasters.Add(sparePartsDomain);
                _context.domainmasters.Add(solarDomain);
                _context.domainmasters.Add(generalDomain);
                await _context.SaveChangesAsync(); // Saves here to populate the Domain IDs

                // B. Create & Add the specified Category Masters (Mapping them to their correct Domains)
                var hardwareCategory = new Categorymaster
                {
                    company_id = newCompany.id,
                    domain_id = sparePartsDomain.id, // 👈 Maps Hardware under Spare Parts
                    code = "HWD",
                    name = "Hardware",
                    description = "Mechanical fastenings, tools, and hardware kits",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                var electricalCategory = new Categorymaster
                {
                    company_id = newCompany.id,
                    domain_id = sparePartsDomain.id, // 👈 Maps Electrical under Spare Parts (or General)
                    code = "ELE",
                    name = "Electrical",
                    description = "Switches, wiring harness modules, and fuses",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                var solarPanelsCategory = new Categorymaster
                {
                    company_id = newCompany.id,
                    domain_id = solarDomain.id, // 👈 Maps Solar Panels cleanly under the Solar Domain
                    code = "PNL",
                    name = "Solar Panels",
                    description = "Monocrystalline, Polycrystalline arrays, and cells",
                    is_active = true,
                    created_at = DateTime.UtcNow
                };

                _context.categorymasters.Add(hardwareCategory);
                _context.categorymasters.Add(electricalCategory);
                _context.categorymasters.Add(solarPanelsCategory);
                await _context.SaveChangesAsync(); // Saves here to establish the Category IDs

                // C. Create & Add default Subcategories linked to these new categories
                var defaultSubcategories = new List<Subcategorymaster>
                    {
                        // Subcategories for Hardware (Hardware Category ID)
                        new Subcategorymaster { category_id = hardwareCategory.id, code = "NTS", name = "Nut & Bolts", description = "Standard industrial fasteners", is_active = true, created_at = DateTime.UtcNow },
    
                        // Subcategories for Electrical (Electrical Category ID)
                        new Subcategorymaster { category_id = electricalCategory.id, code = "WIR", name = "Cables & Wires", description = "Copper structural connections", is_active = true, created_at = DateTime.UtcNow },
    
                        // Subcategories for Solar Panels (Solar Panels Category ID)
                        new Subcategorymaster { category_id = solarPanelsCategory.id, code = "INV", name = "Inverters & Regulators", description = "DC-to-AC conversion units", is_active = true, created_at = DateTime.UtcNow }
                    };

                _context.subcategories.AddRange(defaultSubcategories);
                await _context.SaveChangesAsync();





                // 5. Commit transaction if both operations completed successfully
                await transaction.CommitAsync();

                return "Company and Admin Account Created Successfully";
            }
            catch (Exception ex)
            {
                // If anything fails anywhere inside the try block, rollback completely
                await transaction.RollbackAsync();

                // Log your exception here (e.g., _logger.LogError(ex, "Signup failed"))
                return $"Signup Failed! Error: {ex.Message}";
            }
        }

    }
}

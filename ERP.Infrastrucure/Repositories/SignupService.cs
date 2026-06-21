using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;
using ERP.Application.DTOs.company;
using ERP.Application.Interfaces.Repositories;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities;
using ERP.Domain.Entities.Company;
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
                    created_at = DateTime.UtcNow
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

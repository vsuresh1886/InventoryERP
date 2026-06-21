using ERP.Application.DTOs.codegeneratordto;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
using ERP.Application.Interfaces.Repositories.Common;
using ERP.Domain.Entities.CodeGenerators;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static ERP.Infrastructure.Repositories.CodeGeneratorservice.CodeGeneratorService;

namespace ERP.Infrastructure.Repositories.CodeGeneratorservice
{
    public class CodeGeneratorService : ICodeGeneratorService
    {

        private readonly AppDbContext _context;
        private readonly ICurrentTenantService _currentTenantService;

        public CodeGeneratorService(AppDbContext context, ICurrentTenantService currentTenantService)
        {
            _context = context;
            _currentTenantService = currentTenantService;
        }
        

        public async Task<string> GenerateAsync1(string moduleName)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();

            var config = await _context.CodeProtocolMaster
                .FirstOrDefaultAsync(x => x.module_name == moduleName && x.is_active);

            if (config == null)
                throw new Exception($"Protocol not found for {moduleName}");

            var tracker = await _context.CodeSequenceTracker
                .FirstOrDefaultAsync(x => x.module_name == moduleName);

            if (tracker == null)
            {
                tracker = new Domain.Entities.CodeGenerators.code_sequence_tracker
                {
                    module_name = moduleName,
                    prefix = config.prefix,
                    last_number = 0,
                    last_reset_date = DateTime.Now.Date
                };
                _context.CodeSequenceTracker.Add(tracker);
            }

            var today = DateTime.Now;

            // 🔁 Reset logic
            if (ShouldReset(config.reset_frequency, tracker.last_reset_date, today))
            {
                tracker.last_number = 0;
                tracker.last_reset_date = today.Date;
            }

            tracker.last_number += 1;

            var serial = tracker.last_number
                .ToString()
                .PadLeft(config.serial_length, '0');

            var datePart = config.include_date
                ? FormatDate(today, config.date_format)
                : "";

            var code = BuildCode(config, datePart, serial);

            await _context.SaveChangesAsync();
            //await transaction.CommitAsync();

            return code;
        }

        public async Task<string> GenerateAsync2(string moduleName, long? explicitCompanyId = null)
        {
            // 1. Begin an explicit local transaction for handling the concurrency row lock
           // using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 2. Fetch the protocol rules (EF Core automatically applies WHERE company_id = X)
                var query = _context.CodeProtocolMaster.AsQueryable();
                if (explicitCompanyId.HasValue)
                {
                    query = query.IgnoreQueryFilters().Where(x => x.company_id == explicitCompanyId.Value);
                }

                var config = await _context.CodeProtocolMaster.FirstOrDefaultAsync(x => x.module_name == moduleName && x.is_active);

                if (config == null)
                    throw new Exception($"Protocol configurations not found for {moduleName}");

                // 3. Fetch tracker using a PostgreSQL Row-Level Lock (FOR UPDATE)
                // This stops thread race conditions across transactions inside the same tenant company space
                var trackerQuery = explicitCompanyId.HasValue
      ? _context.CodeSequenceTracker.FromSqlInterpolated($"SELECT * FROM code_sequence_tracker WHERE module_name = {moduleName} AND company_id = {explicitCompanyId.Value} FOR UPDATE")
      : _context.CodeSequenceTracker.FromSqlInterpolated($"SELECT * FROM code_sequence_tracker WHERE module_name = {moduleName} FOR UPDATE");

                var tracker = await trackerQuery.FirstOrDefaultAsync();

                if (tracker == null)
                {
                    tracker = new Domain.Entities.CodeGenerators.code_sequence_tracker
                    {
                        module_name = moduleName,
                        prefix = config.prefix,
                        last_number = 0,
                        last_reset_date = DateTime.UtcNow.Date,
                        // Assign explicitly if passed, otherwise let the interceptor handle it
                        company_id = explicitCompanyId ?? 0
                    };
                    _context.CodeSequenceTracker.Add(tracker);

                    // Save immediately to establish the row identity state in the DB
                    await _context.SaveChangesAsync();
                }

                var today = DateTime.UtcNow; // Standardize on UTC time for safe ERP calculations

                // 🔁 Sequence Reset logic
                if (ShouldReset(config.reset_frequency, tracker.last_reset_date, today))
                {
                    tracker.last_number = 0;
                    tracker.last_reset_date = today.Date;
                }

                // Increment sequence
                tracker.last_number += 1;

                var serial = tracker.last_number
                    .ToString()
                    .PadLeft(config.serial_length, '0');

                var datePart = config.include_date
                    ? FormatDate(today, config.date_format)
                    : "";

                var code = BuildCode(config, datePart, serial);

                // 4. Save and commit changes safely, automatically releasing the row lock
                await _context.SaveChangesAsync();
              //  await transaction.CommitAsync();

                return code;
            }
            catch (Exception)
            {
              //  await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<string> GenerateAsync(string moduleName, long? explicitCompanyId = null)
        {
            // Fallback company ID context determination
            long targetCompanyId = explicitCompanyId ?? (long)_currentTenantService.CompanyId;

            try
            {
                // 2. Fetch the protocol rules (Conditionally ignore query filters if explicit ID is passed)
                var query = _context.CodeProtocolMaster.AsQueryable();
                if (explicitCompanyId.HasValue)
                {
                    query = query.IgnoreQueryFilters().Where(x => x.company_id == explicitCompanyId.Value);
                }

                // 🎯 FIX FROM PREVIOUS STEP: Execute against the built query instead of _context.CodeProtocolMaster directly
                var config = await query.FirstOrDefaultAsync(x => x.module_name == moduleName && x.is_active);

                // ✨ NEW: Dynamically create default Protocol if not available
                if (config == null)
                {
                    config = new Domain.Entities.CodeGenerators.code_protocol_master
                    {
                        module_name = moduleName,
                        // Generate a default prefix (e.g., "EMP" for "Employee", or first 3 characters)
                        prefix = moduleName.Length >= 3 ? moduleName.Substring(0, 3).ToUpper() : moduleName.ToUpper(),
                        serial_length = 5,
                        include_date = true,
                        date_format = "yyyyMM",
                        reset_frequency = "Yearly", // or Monthly, Daily, Never
                        is_active = true,
                        company_id = targetCompanyId
                    };

                    _context.CodeProtocolMaster.Add(config);

                    // Save immediately so it has a state before generating sequences
                    await _context.SaveChangesAsync();
                }

                // 3. Fetch tracker using a PostgreSQL Row-Level Lock (FOR UPDATE)
                var trackerQuery = explicitCompanyId.HasValue
                    ? _context.CodeSequenceTracker.FromSqlInterpolated($"SELECT * FROM code_sequence_tracker WHERE module_name = {moduleName} AND company_id = {explicitCompanyId.Value} FOR UPDATE")
                    : _context.CodeSequenceTracker.FromSqlInterpolated($"SELECT * FROM code_sequence_tracker WHERE module_name = {moduleName} FOR UPDATE");

                var tracker = await trackerQuery.FirstOrDefaultAsync();

                if (tracker == null)
                {
                    tracker = new Domain.Entities.CodeGenerators.code_sequence_tracker
                    {
                        module_name = moduleName,
                        prefix = config.prefix,
                        last_number = 0,
                        last_reset_date = DateTime.UtcNow.Date,
                        company_id = targetCompanyId
                    };
                    _context.CodeSequenceTracker.Add(tracker);

                    // Save immediately to establish the row identity state in the DB
                    await _context.SaveChangesAsync();
                }

                var today = DateTime.UtcNow; // Standardize on UTC time for safe ERP calculations

                // 🔁 Sequence Reset logic
                if (ShouldReset(config.reset_frequency, tracker.last_reset_date, today))
                {
                    tracker.last_number = 0;
                    tracker.last_reset_date = today.Date;
                }

                // Increment sequence
                tracker.last_number += 1;

                var serial = tracker.last_number
                    .ToString()
                    .PadLeft(config.serial_length, '0');

                var datePart = config.include_date
                    ? FormatDate(today, config.date_format)
                    : "";

                var code = BuildCode(config, datePart, serial);

                // 4. Save and commit changes safely, automatically releasing the row lock
                await _context.SaveChangesAsync();

                return code;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool ShouldReset(string frequency, DateTime last, DateTime now)
        {
            return frequency switch
            {
                "daily" => last.Date != now.Date,
                "monthly" => last.Month != now.Month || last.Year != now.Year,
                "yearly" => last.Year != now.Year,
                _ => false
            };
        }

        private string FormatDate(DateTime date, string format)
        {
            return format switch
            {
                "ddmmyy" => date.ToString("ddMMyy"),
                "yyyymmdd" => date.ToString("yyyyMMdd"),
                "yyyy" => date.ToString("yyyy"),
                _ => ""
            };
        }

        private string BuildCode(dynamic config, string datePart, string serial)
        {
            var parts = new List<string> { config.prefix };

            if (!string.IsNullOrEmpty(datePart))
                parts.Add(datePart);

            parts.Add(serial);

            return string.Join(config.separator ?? "", parts);
        }

        public static class CodeFormatter
        {
            public static string PadSerial(int number, int length)
            {
                return number.ToString().PadLeft(length, '0');
            }
        }



        public async Task<string> GenerateSku1(long domainId, long categoryId)
        {
            

            var domain = await _context.domainmasters.FindAsync(domainId);
            var category = await _context.categorymasters.FindAsync(categoryId);

            var prefix = $"{domain.code}-{category.code}";

            var seq = await _context.SkuSequences
                .FirstOrDefaultAsync(x => x.prefix == prefix);

            if (seq == null)
            {
                seq = new SkuSequence
                {
                    prefix = prefix,
                    last_number = 1
                };
                _context.SkuSequences.Add(seq);
            }
            else
            {
                seq.last_number += 1;
            }

            await _context.SaveChangesAsync();

            return $"{prefix}-{seq.last_number.ToString("D3")}";
        }
        public async Task<string> GenerateSku(long domainId, long categoryId, long? explicitCompanyId = null)
        {
            // 1. Determine which company scope we are operating in
            long targetCompanyId = explicitCompanyId ??(long) _currentTenantService.CompanyId;

            // 2. Fetch the domain and category configurations
            // If running during a signup flow where global filters aren't active yet, 
            // you might need .IgnoreQueryFilters() here if explicitCompanyId is passed.
            var domainQuery = _context.domainmasters.AsQueryable();
            var categoryQuery = _context.categorymasters.AsQueryable();

            if (explicitCompanyId.HasValue)
            {
                domainQuery = domainQuery.IgnoreQueryFilters();
                categoryQuery = categoryQuery.IgnoreQueryFilters();
            }

            var domain = await domainQuery.FirstOrDefaultAsync(x => x.id == domainId);
            var category = await categoryQuery.FirstOrDefaultAsync(x => x.id == categoryId);

            if (domain == null || category == null)
                throw new Exception("Domain or Category configuration not found.");

            // 3. Build the unique prefix for this group
            var prefix = $"{domain.code}-{category.code}";

            // 4. Fetch the tracker specific to this prefix AND company
            var seqQuery = _context.SkuSequences.AsQueryable();
            if (explicitCompanyId.HasValue)
            {
                // Explicitly bypass the global filter to query for this specific new tenant
                seqQuery = seqQuery.IgnoreQueryFilters().Where(x => x.company_id == explicitCompanyId.Value);
            }

            var seq = await seqQuery.FirstOrDefaultAsync(x => x.prefix == prefix);

            // 5. Increment or initialize the sequence counter
            if (seq == null)
            {
                seq = new SkuSequence
                {
                    prefix = prefix,
                    last_number = 1,
                    company_id = targetCompanyId // 🎯 Make sure your SkuSequence model has this!
                };
                _context.SkuSequences.Add(seq);
            }
            else
            {
                seq.last_number += 1;
            }

            // 6. Persist changes safely
            await _context.SaveChangesAsync();

            return $"{prefix}-{seq.last_number.ToString("D3")}";
        }


    }
   
}

using ERP.Application.DTOs.codegeneratordto;
using ERP.Application.Interfaces.Repositories.CodeGenerator;
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

        public CodeGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(string moduleName)
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



        public async Task<string> GenerateSku(long domainId, long categoryId)
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

    }
   
}

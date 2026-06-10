using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Application.Models.Notification;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Infrastructure.Repositories.Notification
{
    public class WhatsAppService:IWhatsappService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<WhatsAppService> _logger;
        public WhatsAppService(
        AppDbContext context,
        ILogger<WhatsAppService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task SendAsync( SendWhatsAppRequest request)
        {
            var setting =
                await _context.MessageSettings.FirstOrDefaultAsync(x =>

                    x.ModuleName ==
                        request.ModuleName

                    &&

                    x.FormName ==
                        request.FormName

                    &&

                    x.NotificationType ==
                        "WhatsApp"

                    &&

                    x.IsActive
                );

            if (setting == null)
            {
                throw new Exception(
                    "Message setting not found.");
            }

            var customers =
                await _context.customers
                .Where(x =>
                    request.CustomerIds
                    .Contains(x.cust_pk))
                .ToListAsync();

            foreach (var customer in customers)
            {
                if (string.IsNullOrWhiteSpace(
                    customer.mobile))
                {
                    continue;
                }

                var message =  BuildMessage(setting.MessageBody, customer.customer_name, request.Variables);

                // TEMP LOGGING
                _logger.LogInformation(
                    "WhatsApp Sent To: {Mobile} Message: {Message}",
                    customer.mobile,
                    message);

                // HERE LATER:
                // Twilio API Call
            }
        }

        private string BuildMessage( string template, string customer , Dictionary<string, string> variables)
        {
            template = template.Replace(
                "{CUSTOMER_NAME}",
                customer);

            foreach (var variable in variables)
            {
                template = template.Replace(
                    "{" + variable.Key + "}",
                    variable.Value);
            }

            return template;
        }

    }
}

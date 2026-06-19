using System;
using System.Collections.Generic;
using System.Text;
using ERP.Application.Interfaces.Repositories.Notification;
using ERP.Application.Models.Notification;
using MailKit.Net.Smtp;

using MailKit.Security;

using Microsoft.Extensions.Options;

using MimeKit;

namespace ERP.Infrastructure.Repositories.Notification
{
    public class EmailService:IEmailService
    {
        private readonly SmtpSettings _smtpsettings;
        public EmailService(IOptions<SmtpSettings> settings)
        {
            _smtpsettings = settings.Value;
        }

        public async Task SendAsync(EmailRequest request, CancellationToken cancellationToken = default)
        {

            var email = new MimeMessage();
            //email from
            email.From.Add(new MailboxAddress(_smtpsettings.DisplayName, _smtpsettings.From));
            //email to
            foreach (var to in request.To)
            {
                email.To.Add(
                    MailboxAddress.Parse(to)
                );
            }
            //cc
            if (request.Cc != null)
            {
                foreach (var cc in request.Cc)
                {
                    email.Cc.Add(
                        MailboxAddress.Parse(cc)
                    );
                }
            }
            //bcc
            if (request.Bcc != null)
            {
                foreach (var bcc in request.Bcc)
                {
                    email.Bcc.Add(
                        MailboxAddress.Parse(bcc)
                    );
                }
            }
            //reply to
            if (request.ReplyTo != null)
            {
                foreach (var reply in request.ReplyTo)
                {
                    email.ReplyTo.Add(
                        MailboxAddress.Parse(reply)
                    );
                }
            }

            //subject
            email.Subject= request.Subject;
            var builder = new BodyBuilder();

            if (request.IsHtml)
            {
                builder.HtmlBody = request.Body;
            }
            else
            {
                builder.TextBody = request.Body;
            }

            //Attachement
            if (request.Attachments != null)
            {
                foreach (var attachment in request.Attachments)
                {
                    builder.Attachments.Add(
                        attachment.FileName,
                        attachment.Data,
                        ContentType.Parse(
                            attachment.ContentType
                        )
                    );
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_smtpsettings.Host, _smtpsettings.Port, SecureSocketOptions.StartTls, cancellationToken);

            await smtp.AuthenticateAsync(_smtpsettings.Username, _smtpsettings.Password, cancellationToken);

            await smtp.SendAsync(email, cancellationToken);

            await smtp.DisconnectAsync(true, cancellationToken);

        }
   


    }
}

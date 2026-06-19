using ERP.Application.Models.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Notification
{
    public  interface IEmailService
    {
        Task SendAsync(EmailRequest request , CancellationToken cancellationToken = default);

    }
}

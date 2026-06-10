using ERP.Application.Models.Notification;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Interfaces.Repositories.Notification
{
    public  interface IWhatsappService
    {
        Task SendAsync(SendWhatsAppRequest request);
    }
}

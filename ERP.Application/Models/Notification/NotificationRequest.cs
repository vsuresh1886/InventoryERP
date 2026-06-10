using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Notification
{
    public class NotificationRequest
    {

        public string Recipient { get; set; } = "";

        public string Subject { get; set; } = "";

        public string Message { get; set; } = "";

        public string? AttachmentPath { get; set; }
    }
}

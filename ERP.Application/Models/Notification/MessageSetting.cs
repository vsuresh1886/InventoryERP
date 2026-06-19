using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Notification
{
    public class MessageSetting
    {
        public long Id { get; set; }

        public string ModuleName { get; set; } = "";

        public string FormName { get; set; } = "";

        public string NotificationType { get; set; } = "";

        public string TemplateName { get; set; } = "";

        public string? Subject { get; set; }

        public string MessageBody { get; set; } = "";

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

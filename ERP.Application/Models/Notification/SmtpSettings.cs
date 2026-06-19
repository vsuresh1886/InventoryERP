using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Notification
{
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string From { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;
    }


}

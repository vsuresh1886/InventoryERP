using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Notification
{
    public class EmailRequest
    {
        public List<string> To { get; set; } = [];
        public List<string>? Cc { get; set; }

        public List<string>? Bcc { get; set; }

        public List<string>? ReplyTo { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public bool IsHtml { get; set; } = true;

        public List<EmailAttachment>? Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public byte[] Data { get; set; } = [];

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;
    }
}

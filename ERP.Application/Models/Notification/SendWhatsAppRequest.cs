using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.Application.Models.Notification
{
    public class SendWhatsAppRequest
    {
        public List<long> CustomerIds { get; set; } = [];

        public string ModuleName { get; set; } = "";

        public string FormName { get; set; } = "";

        public Dictionary<string, string> Variables  { get; set; } = [];
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("message_settings")]
    public class MessageSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

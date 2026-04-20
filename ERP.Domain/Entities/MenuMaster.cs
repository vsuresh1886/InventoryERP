using ERP.Domain.Entitiess;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ERP.Domain.Entities
{
    [Table("menu_mst_tbl")]
    public class Menu
    {
        [Column("menu_pk")]
        public long MenuId { get; set; }

        [Column("parent_fk")]
        public long? ParentId { get; set; }

        [Column("title")]
        public string? Title { get; set; }

        [Column("menu_type")]
        public string? MenuType { get; set; }

        [Column("route")]
        public string? Route { get; set; }

        [Column("external")]
        public bool External { get; set; }

        [Column("target")]
        public string? Target { get; set; }

        [Column("icon")]
        public string? Icon { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("visible")]
        public bool Visible { get; set; }

        [Column("disabled")]
        public bool Disabled { get; set; }

        [Column("badge_text")]
        public string? BadgeText { get; set; }

        [Column("badge_color")]
        public string? BadgeColor { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // 🔥 IMPORTANT
        public ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
    }
}

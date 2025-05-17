using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventorySystem.Models
{
    public class Alert
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Message { get; set; } = string.Empty;

        [Required]
        public AlertType Type { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public string? TargetRole { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public int? ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item? Item { get; set; }
    }

    public enum AlertType
    {
        Info,
        Warning,
        Danger,
        Success
    }
}

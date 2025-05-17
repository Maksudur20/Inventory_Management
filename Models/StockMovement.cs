#nullable enable
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventorySystem.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an item")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an item")]
        public int ItemId { get; set; }

        [ForeignKey("ItemId")]
        public Item? Item { get; set; }

        [Required]
        public MovementType Type { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime MovementDate { get; set; } = DateTime.Now;

        [BindNever]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public string Notes { get; set; } = string.Empty;
    }

    public enum MovementType
    {
        In,
        Out,
        Adjustment
    }
}
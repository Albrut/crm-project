using System;
using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.Models
{
    public class Deal
    {
        public int Id { get; set; }
        
        [Required]
        public int ClientId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required, MaxLength(255)]
        public string Title { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required, MaxLength(50)]
        public string Status { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
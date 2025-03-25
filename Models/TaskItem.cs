using System;
using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public int? ClientId { get; set; }
        public int? DealId { get; set; }
        
        [Required, MaxLength(255)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Required, MaxLength(50)]
        public string Status { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
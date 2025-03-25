using System;
using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public DateTime Date { get; set; }
        
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        
        [Required]
        public bool IsPresent { get; set; } 
        
        public string Notes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
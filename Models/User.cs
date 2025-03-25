using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MyAspNetApp.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; }
        
        [Required, MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required, MaxLength(50)]
        public string Role { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Deal> Deals { get; set; }
        public virtual ICollection<TaskItem> Tasks { get; set; }
        public virtual ICollection<Attendance> Attendances { get; set; }

        public User()
        {
            Deals = new List<Deal>();
            Tasks = new List<TaskItem>();
            Attendances = new List<Attendance>();
        }
    }
}

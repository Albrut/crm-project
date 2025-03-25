using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        
        [Required, MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(100)]
        public string ContactPerson { get; set; }
        
        [MaxLength(20)]
        public string Phone { get; set; }
        
        [MaxLength(255)]
        public string Email { get; set; }
        
        public string Address { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
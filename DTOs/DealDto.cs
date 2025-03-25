using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.DTOs
{
    public class DealDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDealDto
    {
        public int ClientId { get; set; }
        public int UserId { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        public decimal Amount { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}

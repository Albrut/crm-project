using System.ComponentModel.DataAnnotations;

namespace MyAspNetApp.DTOs
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? ClientId { get; set; }
        public int? DealId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class CreateTaskItemDto
    {
        public int UserId { get; set; }
        public int? ClientId { get; set; }
        public int? DealId { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}

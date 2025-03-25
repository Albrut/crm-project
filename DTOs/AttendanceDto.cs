using System;

namespace MyAspNetApp.DTOs
{
    public class CheckInOutDto
    {
        public int UserId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public bool IsPresent { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}

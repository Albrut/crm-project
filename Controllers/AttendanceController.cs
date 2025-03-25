using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Add this
using Microsoft.EntityFrameworkCore;  // Add this
using MyAspNetApp.Data;
using MyAspNetApp.DTOs;
using MyAspNetApp.Models;

namespace MyAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Добавляем атрибут авторизации
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("check-in")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckIn(CheckInOutDto dto)
        {
            var today = DateTime.UtcNow.Date;
            
            var existingAttendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.Date == today);

            if (existingAttendance != null && existingAttendance.CheckIn.HasValue)
            {
                return BadRequest("User already checked in today");
            }

            if (existingAttendance == null)
            {
                existingAttendance = new Attendance
                {
                    UserId = dto.UserId,
                    Date = today,
                    IsPresent = true,
                    Notes = dto.Notes
                };
                _context.Attendance.Add(existingAttendance);
            }

            existingAttendance.CheckIn = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new AttendanceResponseDto
            {
                Id = existingAttendance.Id,
                UserId = existingAttendance.UserId,
                Date = existingAttendance.Date,
                CheckIn = existingAttendance.CheckIn,
                CheckOut = existingAttendance.CheckOut,
                IsPresent = existingAttendance.IsPresent,
                Notes = existingAttendance.Notes
            });
        }

        [HttpPost("check-out")]
        public async Task<ActionResult<AttendanceResponseDto>> CheckOut(CheckInOutDto dto)
        {
            var today = DateTime.UtcNow.Date;
            
            var existingAttendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == dto.UserId && a.Date == today);

            if (existingAttendance == null || !existingAttendance.CheckIn.HasValue)
            {
                return BadRequest("No check-in record found for today");
            }

            if (existingAttendance.CheckOut.HasValue)
            {
                return BadRequest("User already checked out today");
            }

            existingAttendance.CheckOut = DateTime.UtcNow;
            existingAttendance.Notes = string.IsNullOrEmpty(existingAttendance.Notes) 
                ? dto.Notes 
                : $"{existingAttendance.Notes}; {dto.Notes}";

            await _context.SaveChangesAsync();

            return Ok(new AttendanceResponseDto
            {
                Id = existingAttendance.Id,
                UserId = existingAttendance.UserId,
                Date = existingAttendance.Date,
                CheckIn = existingAttendance.CheckIn,
                CheckOut = existingAttendance.CheckOut,
                IsPresent = existingAttendance.IsPresent,
                Notes = existingAttendance.Notes
            });
        }

        [HttpGet("user/{userId}/today")]
        public async Task<ActionResult<AttendanceResponseDto>> GetTodayAttendance(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var attendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            if (attendance == null)
                return NotFound();

            return Ok(new AttendanceResponseDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                Date = attendance.Date,
                CheckIn = attendance.CheckIn,
                CheckOut = attendance.CheckOut,
                IsPresent = attendance.IsPresent,
                Notes = attendance.Notes
            });
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetAttendanceStatistics(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var stats = await _context.Attendance
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .GroupBy(a => a.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = _context.Users
                        .Where(u => u.Id == g.Key)
                        .Select(u => u.Name)
                        .FirstOrDefault(),
                    TotalDays = g.Count(),
                    PresentDays = g.Count(a => a.IsPresent),
                    AbsentDays = g.Count(a => !a.IsPresent),
                    AverageCheckInTime = g.Where(a => a.CheckIn.HasValue)
                        .Select(a => a.CheckIn!.Value.TimeOfDay)
                        .DefaultIfEmpty(TimeSpan.Zero)
                        .Average(t => t.TotalMinutes),
                    LateCheckIns = g.Count(a => a.CheckIn.HasValue && 
                        a.CheckIn.Value.TimeOfDay > new TimeSpan(9, 0, 0))
                })
                .ToListAsync();

            return Ok(stats);
        }

        [HttpGet("monthly-report")]
        public async Task<ActionResult<object>> GetMonthlyReport(int month, int year)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var report = await _context.Attendance
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .GroupBy(a => a.Date.Day)
                .Select(g => new
                {
                    Day = g.Key,
                    TotalEmployees = g.Count(),
                    PresentCount = g.Count(a => a.IsPresent),
                    AbsentCount = g.Count(a => !a.IsPresent)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            return Ok(new
            {
                Month = month,
                Year = year,
                DailyStats = report,
                Summary = new
                {
                    AverageAttendance = report.Average(r => r.PresentCount),
                    TotalWorkingDays = report.Count,
                    MaxAttendance = report.Max(r => r.PresentCount)
                }
            });
        }
    }
}

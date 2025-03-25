using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAspNetApp.Data;
using MyAspNetApp.DTOs;
using MyAspNetApp.Models;

namespace MyAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Добавляем атрибут авторизации
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks([FromQuery] int? userId)
        {
            var query = _context.Tasks.AsQueryable();
            
            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId);

            return await query
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    ClientId = t.ClientId,
                    DealId = t.DealId,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Status = t.Status
                })
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<TaskItemDto>> CreateTask(CreateTaskItemDto dto)
        {
            var task = new TaskItem
            {
                UserId = dto.UserId,
                ClientId = dto.ClientId,
                DealId = dto.DealId,
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = dto.Status
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItemDto>> GetTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            return new TaskItemDto
            {
                Id = task.Id,
                UserId = task.UserId,
                ClientId = task.ClientId,
                DealId = task.DealId,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status
            };
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] string status)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-status")]
        public async Task<ActionResult<object>> GetTasksByStatus()
        {
            var tasks = await _context.Tasks
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    Tasks = g.Select(t => new TaskItemDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Status = t.Status,
                        DueDate = t.DueDate
                    })
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<List<TaskItemDto>>> GetOverdueTasks()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Tasks
                .Where(t => t.DueDate < today && t.Status != "Completed")
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    UserId = t.UserId
                })
                .ToListAsync();
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<TaskItemDto>>> GetUpcomingTasks(int days = 7)
        {
            var endDate = DateTime.UtcNow.AddDays(days);
            return await _context.Tasks
                .Where(t => t.DueDate <= endDate && t.Status != "Completed")
                .OrderBy(t => t.DueDate)
                .Select(t => new TaskItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    UserId = t.UserId
                })
                .ToListAsync();
        }
    }
}

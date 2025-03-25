using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyAspNetApp.Data;
using MyAspNetApp.DTOs;
using MyAspNetApp.Models;
using MyAspNetApp.Services;  // Add this for DealService

namespace MyAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Добавляем атрибут авторизации
    public class DealsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly DealService _dealService;

        public DealsController(ApplicationDbContext context, DealService dealService)
        {
            _context = context;
            _dealService = dealService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DealDto>>> GetDeals()
        {
            return await _context.Deals
                .Select(d => new DealDto
                {
                    Id = d.Id,
                    ClientId = d.ClientId,
                    UserId = d.UserId,
                    Title = d.Title,
                    Amount = d.Amount,
                    Status = d.Status,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DealDto>> GetDeal(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null) return NotFound();

            return new DealDto
            {
                Id = deal.Id,
                ClientId = deal.ClientId,
                UserId = deal.UserId,
                Title = deal.Title,
                Amount = deal.Amount,
                Status = deal.Status,
                CreatedAt = deal.CreatedAt
            };
        }

        [HttpPost]
        public async Task<ActionResult<DealDto>> CreateDeal(CreateDealDto dto)
        {
            var deal = new Deal
            {
                ClientId = dto.ClientId,
                UserId = dto.UserId,
                Title = dto.Title,
                Amount = dto.Amount,
                Status = dto.Status
            };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeal), new { id = deal.Id }, deal);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateDealStatus(int id, [FromBody] string status)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null) return NotFound();

            deal.Status = status;
            deal.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetAnalytics(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            // Convert to UTC if not already
            var utcStartDate = startDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc)
                : startDate.ToUniversalTime();
                
            var utcEndDate = endDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc)
                : endDate.ToUniversalTime();

            return await _dealService.GetDealsAnalytics(utcStartDate, utcEndDate);
        }

        [HttpGet("top-performers")]
        public async Task<ActionResult<List<object>>> GetTopPerformers(
            [FromQuery] int month, 
            [FromQuery] int year)
        {
            return await _dealService.GetTopPerformers(month, year);
        }

        [HttpGet("pipeline")]
        public async Task<ActionResult<object>> GetDealsPipeline()
        {
            var pipeline = await _context.Deals
                .GroupBy(d => d.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(d => d.Amount)
                })
                .ToListAsync();

            return Ok(pipeline);
        }
    }
}

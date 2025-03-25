using Microsoft.EntityFrameworkCore;
using MyAspNetApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyAspNetApp.Services
{
    public class DealService
    {
        private readonly ApplicationDbContext _context;

        public DealService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, decimal>> GetDealsAnalytics(DateTime startDate, DateTime endDate)
        {
            var deals = await _context.Deals
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
                .ToListAsync();

            return new Dictionary<string, decimal>
            {
                ["totalAmount"] = deals.Sum(d => d.Amount),
                ["avgAmount"] = deals.Any() ? deals.Average(d => d.Amount) : 0,
                ["wonAmount"] = deals.Where(d => d.Status == "Won").Sum(d => d.Amount),
                ["lostAmount"] = deals.Where(d => d.Status == "Lost").Sum(d => d.Amount),
                ["pendingAmount"] = deals.Where(d => d.Status != "Won" && d.Status != "Lost").Sum(d => d.Amount)
            };
        }

        public async Task<List<object>> GetTopPerformers(int month, int year)
        {
            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.Deals
                .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate && d.Status == "Won")
                .GroupBy(d => d.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalAmount = g.Sum(d => d.Amount),
                    DealsCount = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(5)
                .Cast<object>()
                .ToListAsync();
        }
    }
}

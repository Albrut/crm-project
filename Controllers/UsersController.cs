using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyAspNetApp.Data;
using MyAspNetApp.Models;
using MyAspNetApp.Services;
using MyAspNetApp.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace MyAspNetApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ApplicationDbContext _context;

        public UsersController(UserService userService, ApplicationDbContext context)
        {
            _userService = userService;
            _context = context;
        }

        // GET: api/Users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            return await _userService.GetUsersWithStats();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
        {
            var user = await _userService.CreateUser(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Users/{id}/performance
        [HttpGet("{id}/performance")]
        public async Task<ActionResult<object>> GetUserPerformance(int id)
        {
            var tasks = await _context.Tasks
                .Where(t => t.UserId == id)
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var deals = await _context.Deals
                .Where(d => d.UserId == id)
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, TotalAmount = g.Sum(d => d.Amount) })
                .ToListAsync();

            var attendance = await _context.Attendance
                .Where(a => a.UserId == id && a.Date >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            return new
            {
                TasksStats = tasks,
                DealsStats = deals,
                AttendanceLastMonth = attendance
            };
        }

        // GET: api/Users/{id}/deals
        [HttpGet("{id}/deals")]
        public async Task<ActionResult<object>> GetUserDeals(int id)
        {
            var deals = await _context.Deals
                .Where(d => d.UserId == id)
                .OrderByDescending(d => d.CreatedAt)
                .Take(10)
                .ToListAsync();

            return Ok(deals);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

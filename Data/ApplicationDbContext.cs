namespace MyAspNetApp.Data;
using Microsoft.EntityFrameworkCore;
using MyAspNetApp.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Deal> Deals { get; set; }
    public DbSet<TaskItem> Tasks { get; set; }
    public DbSet<Attendance> Attendance { get; set; }
}
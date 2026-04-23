using Microsoft.EntityFrameworkCore;
using Project_Hail_Mary.Models;
using System;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Users> User { get; set; }
}
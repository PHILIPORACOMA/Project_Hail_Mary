using Microsoft.EntityFrameworkCore;
using Project_Hail_Mary.Models;
using System;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options) { }

	public DbSet<Users> Users { get; set; }
    public DbSet<UserAddress> UserAddresses { get; set; }
	public DbSet<Cart> Cart { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<OrderItem> OrderItems { get; set; }
}
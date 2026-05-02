using Microsoft.EntityFrameworkCore;
using MyApp.Entities;

namespace MyApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}

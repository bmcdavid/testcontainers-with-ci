using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Entities;

namespace MyApp.Repositories;

public class ProductRepository(AppDbContext context)
{
    public async Task<Product> AddAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await context.Products.FindAsync(id);

    public async Task<List<Product>> GetAllAsync() =>
        await context.Products.ToListAsync();

    public async Task<Product?> UpdateAsync(Product updated)
    {
        var existing = await context.Products.FindAsync(updated.Id);
        if (existing is null) return null;

        existing.Name = updated.Name;
        existing.Price = updated.Price;
        existing.Stock = updated.Stock;

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await context.Products.FindAsync(id);
        if (product is null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }
}

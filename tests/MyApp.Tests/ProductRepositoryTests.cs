using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Entities;
using MyApp.Repositories;
using Testcontainers.PostgreSql;
using Xunit;

namespace MyApp.Tests;

// CI is host-gateway, local is host.docker.internal
// docker build --target build --progress plain -t myapp-build:ci .                                                   
// docker run --rm -v /var/run/docker.sock:/var/run/docker.sock -e RYUK_DISABLED=true -e TESTCONTAINERS_HOST_OVERRIDE=host.docker.internal myapp-build:ci dotnet test --no-restore

public class ProductRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = 
        new PostgreSqlBuilder("postgres:16-alpine").Build();

    private AppDbContext _context = null!;
    private ProductRepository _repository = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();
        _repository = new ProductRepository(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Add_ShouldPersistProduct()
    {
        var product = new Product { Name = "Widget", Price = 9.99m, Stock = 100 };

        var added = await _repository.AddAsync(product);

        Assert.True(added.Id > 0);
    }

    [Fact]
    public async Task GetById_ShouldReturnProduct()
    {
        var product = await _repository.AddAsync(new Product { Name = "Gadget", Price = 19.99m, Stock = 50 });

        var found = await _repository.GetByIdAsync(product.Id);

        Assert.NotNull(found);
        Assert.Equal("Gadget", found.Name);
        Assert.Equal(19.99m, found.Price);
    }

    [Fact]
    public async Task GetById_ShouldReturnNull_WhenNotFound()
    {
        var found = await _repository.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllProducts()
    {
        await _repository.AddAsync(new Product { Name = "Alpha", Price = 1.00m, Stock = 10 });
        await _repository.AddAsync(new Product { Name = "Beta", Price = 2.00m, Stock = 20 });

        var all = await _repository.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task Update_ShouldModifyProduct()
    {
        var product = await _repository.AddAsync(new Product { Name = "Old Name", Price = 5.00m, Stock = 10 });

        var updated = await _repository.UpdateAsync(new Product { Id = product.Id, Name = "New Name", Price = 7.50m, Stock = 25 });

        Assert.NotNull(updated);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal(7.50m, updated.Price);
        Assert.Equal(25, updated.Stock);
    }

    [Fact]
    public async Task Update_ShouldReturnNull_WhenNotFound()
    {
        var updated = await _repository.UpdateAsync(new Product { Id = 999, Name = "Ghost", Price = 0m, Stock = 0 });

        Assert.Null(updated);
    }

    [Fact]
    public async Task Delete_ShouldRemoveProduct()
    {
        var product = await _repository.AddAsync(new Product { Name = "Doomed", Price = 1.00m, Stock = 1 });

        var deleted = await _repository.DeleteAsync(product.Id);
        var found = await _repository.GetByIdAsync(product.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public async Task Delete_ShouldReturnFalse_WhenNotFound()
    {
        var deleted = await _repository.DeleteAsync(999);

        Assert.False(deleted);
    }
}

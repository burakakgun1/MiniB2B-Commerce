using Microsoft.EntityFrameworkCore;
using MiniB2B.Core.Entities;

namespace MiniB2B.DataAccess.Context;

public class MiniB2BDbContext : DbContext
{
    public MiniB2BDbContext(DbContextOptions<MiniB2BDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<GridColumnConfig> GridColumnConfigs => Set<GridColumnConfig>();
    public DbSet<SliderItem> SliderItems => Set<SliderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MiniB2BDbContext).Assembly);
    }
}

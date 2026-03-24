using Microsoft.EntityFrameworkCore;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<SaleSimulation> SaleSimulations => Set<SaleSimulation>();

    public DbSet<SaleSimulationDetail> SaleSimulationDetails => Set<SaleSimulationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<SaleSimulationDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId);
    }
}
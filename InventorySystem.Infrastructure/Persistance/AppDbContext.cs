using Microsoft.EntityFrameworkCore;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<SaleSimulation> SaleSimulations => Set<SaleSimulation>();

    public DbSet<SaleSimulationDetail> SaleSimulationDetails => Set<SaleSimulationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ========================
        // PRODUCT
        // ========================
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasColumnType("REAL");

        modelBuilder.Entity<Product>()
            .Property(p => p.IVA)
            .HasColumnType("REAL");

        // ========================
        // SALE SIMULATION
        // ========================
        modelBuilder.Entity<SaleSimulation>()
            .Property(s => s.Subtotal)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulation>()
            .Property(s => s.Discount)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulation>()
            .Property(s => s.IVA)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulation>()
            .Property(s => s.Total)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulationDetail>()
            .Property(d => d.Price)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulationDetail>()
            .Property(d => d.Subtotal)
            .HasColumnType("REAL");

        modelBuilder.Entity<SaleSimulationDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId);

        modelBuilder.Entity<SaleSimulationDetail>()
            .HasOne(d => d.SaleSimulation)
            .WithMany(s => s.Details)
            .HasForeignKey(d => d.SaleSimulationId);
    }
}
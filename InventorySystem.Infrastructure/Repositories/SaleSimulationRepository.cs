using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;
using InventorySystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class SaleSimulationRepository : ISaleSimulationRepository
{
    private readonly AppDbContext _context;

    public SaleSimulationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SaleSimulation>> GetAllAsync()
    {
        return await _context.SaleSimulations.Include(s => s.Details).ToListAsync();
    }

    public async Task<SaleSimulation?> CreateAsync(SaleSimulation simulation)
    {
        _context.SaleSimulations.AddAsync(simulation);
        await _context.SaveChangesAsync();
        return simulation;
    }
}
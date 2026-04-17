using InventorySystem.Domain.Entities;

public interface ISaleSimulationRepository
{
    Task<List<SaleSimulation>> GetAllAsync();
    Task<SaleSimulation?> CreateAsync(SaleSimulation simulation);
}
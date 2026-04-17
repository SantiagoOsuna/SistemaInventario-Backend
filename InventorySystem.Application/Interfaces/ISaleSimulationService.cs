using InventorySystem.Application.DTOs.SaleSimulation;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Interfaces
{
	public interface ISaleSimulationService
	{
		Task<List<SaleSimulation>> GetAllAsync();
		Task<SaleSimulationResponseDto> CreateAsync(CreateSaleSimulationDto dto);
	}
}
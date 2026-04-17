namespace InventorySystem.Application.DTOs.SaleSimulation
{
    public class CreateSaleSimulationDto
    {
        public decimal? Discount { get; set; }
        public List <CreateSaleSimulationDetailDto> Details { get; set; } = new ();
    }
}
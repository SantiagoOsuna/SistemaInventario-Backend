namespace InventorySystem.Application.DTOs.SaleSimulation
{
    public class SaleSimulationResponseDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }

        public List<SaleSimulationDetailDto> Details { get; set; } = new();
    }
}
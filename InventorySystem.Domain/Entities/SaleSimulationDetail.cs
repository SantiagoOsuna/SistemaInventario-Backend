namespace InventorySystem.Domain.Entities
{
    public class SaleSimulationDetail
    {
        public int Id { get; set; }
        public int SaleSimulationId { get; set; }
        public SaleSimulation SaleSimulation { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; } = 0;
        public decimal Subtotal { get; set; } = 0;
    }
}
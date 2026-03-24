namespace InventorySystem.Domain.Entities
{
    public class SaleSimulationDetail
    {
        public int Id { get; set; }
        public int SaleSimulationId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Subtotal { get; set; }
        public Product Product { get; set; } = null!;
    }
}
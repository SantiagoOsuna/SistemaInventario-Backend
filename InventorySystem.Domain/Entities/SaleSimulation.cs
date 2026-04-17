namespace InventorySystem.Domain.Entities
{
	public class SaleSimulation
	{
		public int Id { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
		public decimal Subtotal { get; set; } = 0;
		public decimal Discount { get; set; } = 0;
        public decimal IVA { get; set; } = 0;
        public decimal Total { get; set; } = 0;

        public List <SaleSimulationDetail> Details { get; set; } = new();
	}
}
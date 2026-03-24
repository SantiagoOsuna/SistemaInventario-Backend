namespace InventorySystem.Domain.Entities
{
	public class SaleSimulation
	{
		public int Id { get; set; }
		public DateTime Date { get; set; } = DateTime.UtcNow;
		public decimal Subtotal { get; set; }
		public decimal Discount { get; set; }
		public decimal IVA { get; set; }
		public decimal Total { get; set; }
		public List <SaleSimulationDetail> Details { get; set; } = new();
	}
}
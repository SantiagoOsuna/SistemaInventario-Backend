namespace InventorySystem.Application.DTOs.SaleSimulation
{
	public class SaleSimulationDetailDto
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }

		public decimal PriceWithoutIVA { get; set; }
		public decimal PriceWithIVA { get; set; }

		public decimal Subtotal { get; set; }
		public decimal Discount { get; set; }
		public decimal IVA { get; set; }
		public decimal Total { get; set; }
	}
}
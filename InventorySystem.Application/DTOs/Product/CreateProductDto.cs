namespace InventorySystem.Application.DTOs.Product;

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public decimal IVA { get; set; }
}
using InventorySystem.Application.Interfaces;
using InventorySystem.Application.DTOs.SaleSimulation;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Services;

public class SaleSimulationService : ISaleSimulationService
{
    private readonly ISaleSimulationRepository _repository;
    private readonly IProductRepository _productRepository;

    public SaleSimulationService(
        ISaleSimulationRepository repository,
        IProductRepository productRepository)
    {
        _repository = repository;
        _productRepository = productRepository;
    }

    public async Task<List<SaleSimulation>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<SaleSimulationResponseDto> CreateAsync(CreateSaleSimulationDto dto)
    {
        if (dto.Details == null || !dto.Details.Any())
            throw new Exception("La simulación debe tener al menos un producto");

        var groupedItems = dto.Details
            .GroupBy(d => d.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            });

        var details = new List<SaleSimulationDetail>();
        var responseDetails = new List<SaleSimulationDetailDto>();

        decimal subtotalGeneral = 0;
        decimal totalIVA = 0;
        decimal discountTotal = 0;
        decimal totalGeneral = 0;

        foreach (var item in groupedItems)
        {
            if (item.Quantity <= 0)
                throw new Exception($"Cantidad inválida para el producto {item.ProductId}");

            var product = await _productRepository.GetByIdAsync(item.ProductId);

            if (product == null)
                throw new Exception($"Producto con ID {item.ProductId} no existe");

            if (!product.IsActive)
                throw new Exception($"Producto {product.Name} está inactivo");

            if (product.Stock < item.Quantity)
                throw new Exception($"Stock insuficiente para {product.Name}");

            decimal priceWithIVA = product.Price;
            decimal ivaRate = product.IVA;

            decimal priceWithoutIVA = Math.Round(priceWithIVA / (1 + ivaRate), 2);
            decimal subtotal = Math.Round(priceWithoutIVA * item.Quantity, 2);

            decimal discountRate = dto.Discount ?? 0;
            decimal discountAmount = Math.Round(subtotal * discountRate, 2);

            decimal baseAfterDiscount = subtotal - discountAmount;
            decimal ivaAmount = Math.Round(baseAfterDiscount * ivaRate, 2);
            decimal totalItem = baseAfterDiscount + ivaAmount;

            subtotalGeneral += subtotal;
            totalIVA += ivaAmount;
            discountTotal += discountAmount;
            totalGeneral += totalItem;

            totalGeneral = Math.Round(totalGeneral, 2);

            // 🔥 Guardar en BD
            details.Add(new SaleSimulationDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = priceWithoutIVA,
                Subtotal = subtotal
            });

            // 🔥 Response
            responseDetails.Add(new SaleSimulationDetailDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceWithoutIVA = priceWithoutIVA,
                PriceWithIVA = priceWithIVA,
                Subtotal = subtotal,
                Discount = discountAmount,
                IVA = ivaAmount,
                Total = totalItem
            });

            product.Stock -= item.Quantity;
        }

        var simulation = new SaleSimulation
        {
            Date = DateTime.UtcNow,
            Subtotal = subtotalGeneral,
            Discount = discountTotal,
            IVA = totalIVA,
            Total = Math.Round(totalGeneral, 2),
            Details = details
        };

        await _repository.CreateAsync(simulation);

        return new SaleSimulationResponseDto
        {
            Id = simulation.Id,
            Date = simulation.Date,
            Subtotal = subtotalGeneral,
            Discount = discountTotal,
            IVA = totalIVA,
            Total = Math.Round(totalGeneral, 2),
            Details = responseDetails
        };
    }
}
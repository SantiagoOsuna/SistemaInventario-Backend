using InventorySystem.Application.DTOs.SaleSimulation;
using InventorySystem.Application.Exceptions;
using InventorySystem.Application.Interfaces;
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
        // VALIDAR DTO
        if (dto == null)
            throw new ValidationException(
                "La información de la simulación es requerida"
            );

        // VALIDAR DETALLES
        if (dto.Details == null || !dto.Details.Any())
            throw new ValidationException(
                "La simulación debe tener al menos un producto"
            );

        // VALIDAR DESCUENTO
        if (dto.Discount < 0)
            throw new ValidationException(
                "El descuento no puede ser negativo"
            );

        if (dto.Discount > 1)
            throw new ValidationException(
                "El descuento no puede ser mayor al 100%"
            );

        // AGRUPAR PRODUCTOS REPETIDOS
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
            // VALIDAR ID
            if (item.ProductId <= 0)
                throw new ValidationException(
                    $"El id del producto {item.ProductId} no es válido"
                );

            // VALIDAR CANTIDAD
            if (item.Quantity <= 0)
                throw new ValidationException(
                    $"La cantidad del producto {item.ProductId} debe ser mayor a cero"
                );

            // BUSCAR PRODUCTO
            var product = await _productRepository.GetByIdAsync(item.ProductId);

            // VALIDAR EXISTENCIA
            if (product == null)
                throw new NotFoundException(
                    $"Producto con ID {item.ProductId} no encontrado"
                );

            // VALIDAR ESTADO
            if (!product.IsActive)
                throw new BusinessException(
                    $"El producto {product.Name} está inactivo"
                );

            // VALIDAR STOCK
            if (product.Stock <= 0)
                throw new BusinessException(
                    $"El producto {product.Name} no tiene stock disponible"
                );

            if (product.Stock < item.Quantity)
                throw new BusinessException(
                    $"Stock insuficiente para el producto {product.Name}"
                );

            // VALIDAR PRECIO
            if (product.Price <= 0)
                throw new BusinessException(
                    $"El producto {product.Name} tiene un precio inválido"
                );

            // VALIDAR IVA
            if (product.IVA < 0)
                throw new BusinessException(
                    $"El producto {product.Name} tiene un IVA inválido"
                );

            // CALCULOS
            decimal priceWithIVA = product.Price;

            decimal ivaRate = product.IVA;

            decimal priceWithoutIVA =
                Math.Round(priceWithIVA / (1 + ivaRate), 2);

            decimal subtotal =
                Math.Round(priceWithoutIVA * item.Quantity, 2);

            decimal discountRate = dto.Discount ?? 0;

            decimal discountAmount =
                Math.Round(subtotal * discountRate, 2);

            decimal baseAfterDiscount =
                subtotal - discountAmount;

            decimal ivaAmount =
                Math.Round(baseAfterDiscount * ivaRate, 2);

            decimal totalItem =
                Math.Round(baseAfterDiscount + ivaAmount, 2);

            // ACUMULADORES
            subtotalGeneral += subtotal;

            totalIVA += ivaAmount;

            discountTotal += discountAmount;

            totalGeneral += totalItem;

            totalGeneral = Math.Round(totalGeneral, 2);

            // GUARDAR DETALLE BD
            details.Add(new SaleSimulationDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = priceWithoutIVA,
                Subtotal = subtotal
            });

            // RESPONSE DETAIL
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

            // DESCONTAR STOCK
            product.Stock -= item.Quantity;
        }

        // VALIDAR TOTALES
        if (totalGeneral <= 0)
            throw new BusinessException(
                "El total de la simulación no es válido"
            );

        // CREAR SIMULACION
        var simulation = new SaleSimulation
        {
            Date = DateTime.UtcNow,

            Subtotal = Math.Round(subtotalGeneral, 2),

            Discount = Math.Round(discountTotal, 2),

            IVA = Math.Round(totalIVA, 2),

            Total = Math.Round(totalGeneral, 2),

            Details = details
        };

        // GUARDAR
        await _repository.CreateAsync(simulation);

        // RESPONSE
        return new SaleSimulationResponseDto
        {
            Id = simulation.Id,

            Date = simulation.Date,

            Subtotal = Math.Round(subtotalGeneral, 2),

            Discount = Math.Round(discountTotal, 2),

            IVA = Math.Round(totalIVA, 2),

            Total = Math.Round(totalGeneral, 2),

            Details = responseDetails
        };
    }
}
using InventorySystem.Application.DTOs.Product;
using InventorySystem.Application.Exceptions;
using InventorySystem.Application.Interfaces;
using InventorySystem.Domain.Entities;

namespace InventorySystem.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _repository.GetAllAsync();

        return products
            .Where(p => p.IsActive)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                IVA = p.IVA,
                IsActive = p.IsActive
            })
            .ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ValidationException(
                "El id del producto no es válido"
            );

        var product = await _repository.GetByIdAsync(id);

        if (product == null || !product.IsActive)
            throw new NotFoundException(
                "Producto no encontrado"
            );

        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IVA = product.IVA,
            IsActive = product.IsActive
        };
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (dto == null)
            throw new ValidationException(
                "La información del producto es requerida"
            );

        if (string.IsNullOrWhiteSpace(dto.Code))
            throw new ValidationException(
                "El código del producto es obligatorio"
            );

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(
                "El nombre del producto es obligatorio"
            );

        if (dto.Name.Length > 100)
            throw new ValidationException(
                "El nombre del producto no puede superar los 100 caracteres"
            );

        if (dto.Description?.Length > 500)
            throw new ValidationException(
                "La descripción no puede superar los 500 caracteres"
            );

        if (dto.Price <= 0)
            throw new ValidationException(
                "El precio debe ser mayor a cero"
            );

        if (dto.Stock < 0)
            throw new ValidationException(
                "El stock no puede ser negativo"
            );

        if (dto.IVA < 0)
            throw new ValidationException(
                "El IVA no puede ser negativo"
            );

        if (dto.IVA > 100)
            throw new ValidationException(
                "El IVA no puede ser mayor al 100%"
            );

        var products = await _repository.GetAllAsync();

        var codeExists = products.Any(p =>
            p.Code.ToLower() == dto.Code.ToLower());

        if (codeExists)
            throw new BusinessException(
                "Ya existe un producto con ese código"
            );

        var product = new Product
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            Price = dto.Price,
            Stock = dto.Stock,
            IVA = dto.IVA,
            IsActive = true
        };

        await _repository.AddAsync(product);
        await _repository.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            IVA = product.IVA,
            IsActive = product.IsActive
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        if (id <= 0)
            throw new ValidationException(
                "El id del producto no es válido"
            );

        if (dto == null)
            throw new ValidationException(
                "La información del producto es requerida"
            );

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException(
                "El nombre del producto es obligatorio"
            );

        if (dto.Name.Length > 100)
            throw new ValidationException(
                "El nombre del producto no puede superar los 100 caracteres"
            );

        if (dto.Description?.Length > 500)
            throw new ValidationException(
                "La descripción no puede superar los 500 caracteres"
            );

        if (dto.Price <= 0)
            throw new ValidationException(
                "El precio debe ser mayor a cero"
            );

        if (dto.Stock < 0)
            throw new ValidationException(
                "El stock no puede ser negativo"
            );

        if (dto.IVA < 0)
            throw new ValidationException(
                "El IVA no puede ser negativo"
            );

        if (dto.IVA > 100)
            throw new ValidationException(
                "El IVA no puede ser mayor al 100%"
            );

        var product = await _repository.GetByIdAsync(id);

        if (product == null || !product.IsActive)
            throw new NotFoundException(
                "Producto no encontrado"
            );

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim();
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.IVA = dto.IVA;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
            throw new ValidationException(
                "El id del producto no es válido"
            );

        var product = await _repository.GetByIdAsync(id);

        if (product == null || !product.IsActive)
            throw new NotFoundException(
                "Producto no encontrado"
            );

        product.IsActive = false;

        await _repository.UpdateAsync(product);
        await _repository.SaveChangesAsync();

        return true;
    }
}
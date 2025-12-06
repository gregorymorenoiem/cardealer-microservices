using Microsoft.AspNetCore.Mvc;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using System.Text.Json;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<ProductsController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Buscar productos con filtros
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> Search(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        var products = await _productRepository.SearchAsync(search, categoryId, minPrice, maxPrice);
        return Ok(products);
    }

    /// <summary>
    /// Obtener producto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(product);
    }

    /// <summary>
    /// Obtener productos de un vendedor
    /// </summary>
    [HttpGet("seller/{sellerId}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetBySeller(Guid sellerId)
    {
        var products = await _productRepository.GetBySellerAsync(sellerId);
        return Ok(products);
    }

    /// <summary>
    /// Crear nuevo producto
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] CreateProductRequest request)
    {
        // Validar categoría existe
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
            return BadRequest(new { message = "Categoría no encontrada" });

        // Crear producto
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency ?? "USD",
            Status = ProductStatus.Draft,
            ImageUrl = request.ImageUrl,
            SellerId = request.SellerId,
            SellerName = request.SellerName,
            CategoryId = request.CategoryId,
            CategoryName = category.Name,
            CustomFieldsJson = JsonSerializer.Serialize(request.CustomFields ?? new Dictionary<string, object>())
        };

        // Agregar imágenes
        if (request.Images != null)
        {
            var sortOrder = 0;
            foreach (var imageUrl in request.Images)
            {
                product.Images.Add(new ProductImage
                {
                    Url = imageUrl,
                    SortOrder = sortOrder++,
                    IsPrimary = sortOrder == 1
                });
            }
        }

        // Agregar campos personalizados (EAV)
        if (request.CustomFields != null)
        {
            var sortOrder = 0;
            foreach (var field in request.CustomFields)
            {
                product.CustomFields.Add(new ProductCustomField
                {
                    Key = field.Key,
                    Value = field.Value?.ToString() ?? "",
                    DataType = InferDataType(field.Value),
                    SortOrder = sortOrder++,
                    IsSearchable = true
                });
            }
        }

        var created = await _productRepository.CreateAsync(product);

        _logger.LogInformation("Producto creado: {ProductId} - {ProductName}", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Actualizar producto
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product == null)
            return NotFound(new { message = "Producto no encontrado" });

        // Actualizar campos básicos
        product.Name = request.Name ?? product.Name;
        product.Description = request.Description ?? product.Description;
        product.Price = request.Price ?? product.Price;
        product.Status = request.Status ?? product.Status;
        product.ImageUrl = request.ImageUrl ?? product.ImageUrl;

        // Actualizar campos personalizados JSON
        if (request.CustomFields != null)
        {
            product.CustomFieldsJson = JsonSerializer.Serialize(request.CustomFields);
        }

        await _productRepository.UpdateAsync(product);

        _logger.LogInformation("Producto actualizado: {ProductId}", id);

        return Ok(product);
    }

    /// <summary>
    /// Eliminar producto (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var exists = await _productRepository.ExistsAsync(id);

        if (!exists)
            return NotFound(new { message = "Producto no encontrado" });

        await _productRepository.DeleteAsync(id);

        _logger.LogInformation("Producto eliminado: {ProductId}", id);

        return NoContent();
    }

    private string InferDataType(object? value)
    {
        return value switch
        {
            int or long or decimal or double => "number",
            bool => "boolean",
            DateTime => "date",
            _ => "string"
        };
    }
}

// ========================================
// DTOs
// ========================================

public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? Currency { get; init; }
    public string? ImageUrl { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
    public List<string>? Images { get; init; }
}

public record UpdateProductRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public ProductStatus? Status { get; init; }
    public string? ImageUrl { get; init; }
    public Dictionary<string, object>? CustomFields { get; init; }
}

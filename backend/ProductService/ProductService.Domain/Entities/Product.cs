
using CarDealer.Shared.MultiTenancy;
namespace ProductService.Domain.Entities;

/// <summary>
/// Producto genérico del marketplace
/// Soporta campos básicos + campos personalizados dinámicos
/// </summary>
public class Product : CarDealer.Shared.MultiTenancy.ITenantEntity
{
    public Guid Id { get; set; }
    // Multi-tenant: DealerId
    public Guid DealerId { get; set; }

    // ========================================
    // CAMPOS BÁSICOS (todos los productos)
    // ========================================

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public string? ImageUrl { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

    // Categorización
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Metadatos
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    // ========================================
    // CAMPOS PERSONALIZADOS DINÁMICOS
    // ========================================

    /// <summary>
    /// Campos personalizados como JSON
    /// Ejemplo para vehículos: { "make": "Toyota", "model": "Camry", "year": 2023, "mileage": 15000 }
    /// Ejemplo para inmuebles: { "bedrooms": 3, "bathrooms": 2, "sqft": 1500, "parking": true }
    /// </summary>
    public string CustomFieldsJson { get; set; } = "{}";

    // Navegación
    public Category? Category { get; set; }
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductCustomField> CustomFields { get; set; } = new List<ProductCustomField>();
}

/// <summary>
/// Estados del producto
/// </summary>
public enum ProductStatus
{
    Draft,          // Borrador
    Active,         // Activo/Publicado
    Sold,           // Vendido
    Reserved,       // Reservado
    Archived        // Archivado
}

/// <summary>
/// Imágenes del producto
/// </summary>
public class ProductImage : CarDealer.Shared.MultiTenancy.ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; } // Multi-tenant
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}

/// <summary>
/// Campos personalizados del producto (EAV pattern)
/// Alternativa a JSON para queries más complejas
/// </summary>
public class ProductCustomField
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    /// <summary>
    /// Clave del campo (ej: "make", "model", "year", "mileage")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Valor del campo (como string, se convierte según DataType)
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de dato: string, number, boolean, date
    /// </summary>
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Unidad opcional (ej: "km", "miles", "sqft", "years")
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Si es un campo indexable/buscable
    /// </summary>
    public bool IsSearchable { get; set; } = true;

    public Product? Product { get; set; }
}

/// <summary>
/// Categoría de productos
/// </summary>
public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    /// <summary>
    /// ID de la categoría padre (null = categoría raíz)
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Nivel en la jerarquía (0 = raíz)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Definición de campos personalizados para esta categoría
    /// JSON con la estructura de campos esperados
    /// </summary>
    public string CustomFieldsSchemaJson { get; set; } = "[]";

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

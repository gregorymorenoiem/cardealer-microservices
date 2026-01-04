using VehiclesSaleService.Domain.Entities;

namespace VehiclesSaleService.Domain.Interfaces;

/// <summary>
/// Repositorio para el catálogo maestro de vehículos (marcas, modelos, trims).
/// Usado para auto-completar información cuando un dealer publica un vehículo.
/// </summary>
public interface IVehicleCatalogRepository
{
    // ========================================
    // MAKES (Marcas)
    // ========================================

    /// <summary>
    /// Obtiene todas las marcas activas, ordenadas por popularidad y nombre.
    /// </summary>
    Task<IEnumerable<VehicleMake>> GetAllMakesAsync(bool includeInactive = false);

    /// <summary>
    /// Obtiene marcas populares (las más usadas).
    /// </summary>
    Task<IEnumerable<VehicleMake>> GetPopularMakesAsync(int take = 20);

    /// <summary>
    /// Busca marcas por nombre.
    /// </summary>
    Task<IEnumerable<VehicleMake>> SearchMakesAsync(string searchTerm);

    /// <summary>
    /// Obtiene una marca por ID.
    /// </summary>
    Task<VehicleMake?> GetMakeByIdAsync(Guid id);

    /// <summary>
    /// Obtiene una marca por slug.
    /// </summary>
    Task<VehicleMake?> GetMakeBySlugAsync(string slug);

    // ========================================
    // MODELS (Modelos)
    // ========================================

    /// <summary>
    /// Obtiene todos los modelos de una marca.
    /// </summary>
    Task<IEnumerable<VehicleModel>> GetModelsByMakeIdAsync(Guid makeId, bool includeInactive = false);

    /// <summary>
    /// Obtiene todos los modelos de una marca por slug.
    /// </summary>
    Task<IEnumerable<VehicleModel>> GetModelsByMakeSlugAsync(string makeSlug, bool includeInactive = false);

    /// <summary>
    /// Busca modelos por nombre dentro de una marca.
    /// </summary>
    Task<IEnumerable<VehicleModel>> SearchModelsAsync(Guid makeId, string searchTerm);

    /// <summary>
    /// Obtiene un modelo por ID.
    /// </summary>
    Task<VehicleModel?> GetModelByIdAsync(Guid id);

    /// <summary>
    /// Obtiene un modelo por slug y marca.
    /// </summary>
    Task<VehicleModel?> GetModelBySlugAsync(string makeSlug, string modelSlug);

    // ========================================
    // TRIMS (Versiones)
    // ========================================

    /// <summary>
    /// Obtiene todos los trims de un modelo para un año específico.
    /// </summary>
    Task<IEnumerable<VehicleTrim>> GetTrimsByModelAndYearAsync(Guid modelId, int year);

    /// <summary>
    /// Obtiene los años disponibles para un modelo.
    /// </summary>
    Task<IEnumerable<int>> GetAvailableYearsAsync(Guid modelId);

    /// <summary>
    /// Obtiene un trim por ID.
    /// </summary>
    Task<VehicleTrim?> GetTrimByIdAsync(Guid id);

    /// <summary>
    /// Obtiene las especificaciones de un trim específico.
    /// </summary>
    Task<VehicleTrim?> GetTrimSpecsAsync(Guid modelId, int year, string trimName);

    // ========================================
    // SEED / ADMIN
    // ========================================

    /// <summary>
    /// Agrega o actualiza una marca.
    /// </summary>
    Task<VehicleMake> UpsertMakeAsync(VehicleMake make);

    /// <summary>
    /// Agrega o actualiza un modelo.
    /// </summary>
    Task<VehicleModel> UpsertModelAsync(VehicleModel model);

    /// <summary>
    /// Agrega o actualiza un trim.
    /// </summary>
    Task<VehicleTrim> UpsertTrimAsync(VehicleTrim trim);

    /// <summary>
    /// Importa catálogo masivamente (para seed).
    /// </summary>
    Task<int> BulkImportAsync(IEnumerable<VehicleMake> makes);

    /// <summary>
    /// Obtiene estadísticas del catálogo.
    /// </summary>
    Task<CatalogStats> GetStatsAsync();
}

/// <summary>
/// Estadísticas del catálogo de vehículos.
/// </summary>
public class CatalogStats
{
    public int TotalMakes { get; set; }
    public int TotalModels { get; set; }
    public int TotalTrims { get; set; }
    public int MinYear { get; set; }
    public int MaxYear { get; set; }
    public DateTime LastUpdated { get; set; }
}

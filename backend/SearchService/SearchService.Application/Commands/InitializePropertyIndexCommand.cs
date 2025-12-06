using MediatR;

namespace SearchService.Application.Commands;

/// <summary>
/// Comando para inicializar el índice de propiedades inmobiliarias con mappings optimizados
/// </summary>
public record InitializePropertyIndexCommand : IRequest<bool>;

/// <summary>
/// Handler para crear el índice de propiedades con configuración específica para bienes raíces
/// </summary>
public class InitializePropertyIndexCommandHandler : IRequestHandler<InitializePropertyIndexCommand, bool>
{
    private readonly Domain.Interfaces.IIndexManager _indexManager;
    private readonly Domain.Interfaces.ISearchRepository _searchRepository;

    public InitializePropertyIndexCommandHandler(
        Domain.Interfaces.IIndexManager indexManager,
        Domain.Interfaces.ISearchRepository searchRepository)
    {
        _indexManager = indexManager;
        _searchRepository = searchRepository;
    }

    public async Task<bool> Handle(InitializePropertyIndexCommand request, CancellationToken cancellationToken)
    {
        const string indexName = "properties";

        // Verificar si ya existe
        if (await _indexManager.IndexExistsAsync(indexName, cancellationToken))
        {
            return true; // Ya existe, no hacer nada
        }

        // Mappings específicos para propiedades inmobiliarias
        var mappings = new Dictionary<string, object>
        {
            ["properties"] = new Dictionary<string, object>
            {
                // Identificadores
                ["id"] = new { type = "keyword" },
                ["dealerId"] = new { type = "keyword" },

                // Texto con análisis full-text
                ["title"] = new { type = "text", analyzer = "spanish", fields = new { keyword = new { type = "keyword" } } },
                ["description"] = new { type = "text", analyzer = "spanish" },

                // Tipo y categoría
                ["propertyType"] = new { type = "keyword" }, // house, apartment, condo, land, commercial
                ["listingType"] = new { type = "keyword" },  // sale, rent, sale-or-rent
                ["status"] = new { type = "keyword" },       // active, sold, rented, archived

                // Precios - números para rangos
                ["price"] = new { type = "double" },
                ["currency"] = new { type = "keyword" },
                ["pricePerSqMeter"] = new { type = "double" },
                ["maintenanceFee"] = new { type = "double" },
                ["isNegotiable"] = new { type = "boolean" },

                // Características físicas - números para filtros
                ["totalArea"] = new { type = "double" },
                ["areaUnit"] = new { type = "keyword" },
                ["builtArea"] = new { type = "double" },
                ["lotArea"] = new { type = "double" },
                ["bedrooms"] = new { type = "integer" },
                ["bathrooms"] = new { type = "integer" },
                ["halfBathrooms"] = new { type = "integer" },
                ["parkingSpaces"] = new { type = "integer" },
                ["floor"] = new { type = "integer" },
                ["totalFloors"] = new { type = "integer" },
                ["yearBuilt"] = new { type = "integer" },

                // Amenidades - booleanos para filtros rápidos
                ["hasGarden"] = new { type = "boolean" },
                ["hasPool"] = new { type = "boolean" },
                ["hasGym"] = new { type = "boolean" },
                ["hasElevator"] = new { type = "boolean" },
                ["hasSecurity"] = new { type = "boolean" },
                ["isFurnished"] = new { type = "boolean" },
                ["allowsPets"] = new { type = "boolean" },

                // Amenidades adicionales como array
                ["amenities"] = new { type = "keyword" }, // Array de strings

                // Ubicación
                ["location"] = new Dictionary<string, object>
                {
                    ["type"] = "object",
                    ["properties"] = new Dictionary<string, object>
                    {
                        ["address"] = new { type = "text", analyzer = "spanish" },
                        ["city"] = new { type = "keyword" },
                        ["state"] = new { type = "keyword" },
                        ["zipCode"] = new { type = "keyword" },
                        ["country"] = new { type = "keyword" },
                        ["neighborhood"] = new { type = "text", analyzer = "spanish", fields = new { keyword = new { type = "keyword" } } },
                        ["coordinates"] = new { type = "geo_point" } // Para búsqueda por ubicación
                    }
                },

                // Imágenes (solo metadatos para búsqueda)
                ["imageCount"] = new { type = "integer" },
                ["primaryImageUrl"] = new { type = "keyword", index = false },

                // Seller/Agent info
                ["seller"] = new Dictionary<string, object>
                {
                    ["type"] = "object",
                    ["properties"] = new Dictionary<string, object>
                    {
                        ["id"] = new { type = "keyword" },
                        ["name"] = new { type = "text" },
                        ["isVerified"] = new { type = "boolean" },
                        ["isDealership"] = new { type = "boolean" }
                    }
                },

                // Métricas
                ["viewCount"] = new { type = "integer" },
                ["favoriteCount"] = new { type = "integer" },
                ["inquiryCount"] = new { type = "integer" },
                ["isFeatured"] = new { type = "boolean" },

                // Timestamps
                ["createdAt"] = new { type = "date" },
                ["updatedAt"] = new { type = "date" },
                ["publishedAt"] = new { type = "date" }
            }
        };

        // Settings optimizados para búsqueda en español
        var settings = new Dictionary<string, object>
        {
            ["index"] = new Dictionary<string, object>
            {
                ["number_of_shards"] = 2,
                ["number_of_replicas"] = 1
            },
            ["analysis"] = new Dictionary<string, object>
            {
                ["analyzer"] = new Dictionary<string, object>
                {
                    ["spanish"] = new Dictionary<string, object>
                    {
                        ["type"] = "spanish",
                        ["stopwords"] = "_spanish_"
                    }
                }
            }
        };

        return await _indexManager.CreateIndexAsync(indexName, mappings, settings, cancellationToken);
    }
}

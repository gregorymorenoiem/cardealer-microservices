using MediatR;

namespace SearchService.Application.Queries;

/// <summary>
/// Query para listar todos los índices disponibles
/// </summary>
public class ListIndicesQuery : IRequest<List<string>>
{
}

# 📋 Template: Crear Feature CQRS

Guía para crear nuevos Commands y Queries usando MediatR.

---

## 1. Command (Escritura)

### Command Request

```csharp
// {ServiceName}.Application/Features/{Feature}/Commands/Create{Entity}Command.cs
using MediatR;

namespace {ServiceName}.Application.Features.{Feature}.Commands;

/// <summary>
/// Command para crear un nuevo {Entity}
/// </summary>
public record Create{Entity}Command(
    string Name,
    string Description,
    decimal Price
) : IRequest<Result<{Entity}Dto>>;
```

### Command Handler

```csharp
// {ServiceName}.Application/Features/{Feature}/Commands/Create{Entity}CommandHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using {ServiceName}.Domain.Entities;
using {ServiceName}.Domain.Interfaces;
using {ServiceName}.Application.DTOs;

namespace {ServiceName}.Application.Features.{Feature}.Commands;

public class Create{Entity}CommandHandler : IRequestHandler<Create{Entity}Command, Result<{Entity}Dto>>
{
    private readonly I{Entity}Repository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<Create{Entity}CommandHandler> _logger;

    public Create{Entity}CommandHandler(
        I{Entity}Repository repository,
        IEventPublisher eventPublisher,
        ILogger<Create{Entity}CommandHandler> logger)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Result<{Entity}Dto>> Handle(
        Create{Entity}Command request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating {Entity} with name: {Name}", request.Name);

            // 1. Crear entidad de dominio
            var entity = new {Entity}
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            // 2. Persistir
            await _repository.AddAsync(entity, cancellationToken);

            // 3. Publicar evento de dominio
            await _eventPublisher.PublishAsync(new {Entity}CreatedEvent
            {
                {Entity}Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt
            });

            _logger.LogInformation("{Entity} created successfully with ID: {Id}", entity.Id);

            // 4. Retornar DTO
            return Result<{Entity}Dto>.Success(new {Entity}Dto(
                entity.Id,
                entity.Name,
                entity.Description,
                entity.Price
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {Entity}");
            return Result<{Entity}Dto>.Failure($"Error creating {Entity}: {ex.Message}");
        }
    }
}
```

### Command Validator

```csharp
// {ServiceName}.Application/Features/{Feature}/Validators/Create{Entity}CommandValidator.cs
using FluentValidation;

namespace {ServiceName}.Application.Features.{Feature}.Validators;

public class Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}
```

---

## 2. Query (Lectura)

### Query Request

```csharp
// {ServiceName}.Application/Features/{Feature}/Queries/Get{Entity}ByIdQuery.cs
using MediatR;

namespace {ServiceName}.Application.Features.{Feature}.Queries;

/// <summary>
/// Query para obtener un {Entity} por ID
/// </summary>
public record Get{Entity}ByIdQuery(Guid Id) : IRequest<Result<{Entity}Dto>>;
```

### Query Handler

```csharp
// {ServiceName}.Application/Features/{Feature}/Queries/Get{Entity}ByIdQueryHandler.cs
using MediatR;
using Microsoft.Extensions.Logging;
using {ServiceName}.Domain.Interfaces;
using {ServiceName}.Application.DTOs;

namespace {ServiceName}.Application.Features.{Feature}.Queries;

public class Get{Entity}ByIdQueryHandler : IRequestHandler<Get{Entity}ByIdQuery, Result<{Entity}Dto>>
{
    private readonly I{Entity}Repository _repository;
    private readonly ILogger<Get{Entity}ByIdQueryHandler> _logger;

    public Get{Entity}ByIdQueryHandler(
        I{Entity}Repository repository,
        ILogger<Get{Entity}ByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<{Entity}Dto>> Handle(
        Get{Entity}ByIdQuery request, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Fetching {Entity} with ID: {Id}", request.Id);

        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (entity is null)
        {
            _logger.LogWarning("{Entity} not found with ID: {Id}", request.Id);
            return Result<{Entity}Dto>.Failure($"{Entity} with ID {request.Id} not found");
        }

        return Result<{Entity}Dto>.Success(new {Entity}Dto(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Price
        ));
    }
}
```

### Query Paginada

```csharp
// {ServiceName}.Application/Features/{Feature}/Queries/Get{Entity}ListQuery.cs
using MediatR;

namespace {ServiceName}.Application.Features.{Feature}.Queries;

public record Get{Entity}ListQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<Result<PaginatedList<{Entity}Dto>>>;

public class Get{Entity}ListQueryHandler : IRequestHandler<Get{Entity}ListQuery, Result<PaginatedList<{Entity}Dto>>>
{
    private readonly I{Entity}Repository _repository;

    public Get{Entity}ListQueryHandler(I{Entity}Repository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedList<{Entity}Dto>>> Handle(
        Get{Entity}ListQuery request, 
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var dtos = items.Select(e => new {Entity}Dto(
            e.Id, e.Name, e.Description, e.Price
        )).ToList();

        return Result<PaginatedList<{Entity}Dto>>.Success(
            new PaginatedList<{Entity}Dto>(dtos, totalCount, request.Page, request.PageSize)
        );
    }
}
```

---

## 3. DTO

```csharp
// {ServiceName}.Application/DTOs/{Entity}Dto.cs
namespace {ServiceName}.Application.DTOs;

/// <summary>
/// DTO para {Entity}
/// </summary>
public record {Entity}Dto(
    Guid Id,
    string Name,
    string Description,
    decimal Price
);

/// <summary>
/// DTO para crear {Entity}
/// </summary>
public record Create{Entity}Request(
    string Name,
    string Description,
    decimal Price
);

/// <summary>
/// DTO para actualizar {Entity}
/// </summary>
public record Update{Entity}Request(
    string Name,
    string Description,
    decimal Price
);
```

---

## 4. Controller

```csharp
// {ServiceName}.Api/Controllers/{Entity}Controller.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using {ServiceName}.Application.Features.{Feature}.Commands;
using {ServiceName}.Application.Features.{Feature}.Queries;
using {ServiceName}.Application.DTOs;

namespace {ServiceName}.Api.Controllers;

/// <summary>
/// Controller para gestión de {Entities}
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class {Entity}Controller : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<{Entity}Controller> _logger;

    public {Entity}Controller(IMediator mediator, ILogger<{Entity}Controller> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los {Entities} con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<{Entity}Dto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = new Get{Entity}ListQuery(page, pageSize, search);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Obtiene un {Entity} por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id, 
        CancellationToken cancellationToken)
    {
        var query = new Get{Entity}ByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Crea un nuevo {Entity}
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] Create{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var command = new Create{Entity}Command(
            request.Name,
            request.Description,
            request.Price
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetById), 
            new { id = result.Value!.Id }, 
            result.Value
        );
    }

    /// <summary>
    /// Actualiza un {Entity}
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof({Entity}Dto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] Update{Entity}Request request,
        CancellationToken cancellationToken)
    {
        var command = new Update{Entity}Command(
            id,
            request.Name,
            request.Description,
            request.Price
        );

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return result.Error!.Contains("not found") 
                ? NotFound(new { error = result.Error }) 
                : BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Elimina un {Entity}
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new Delete{Entity}Command(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
```

---

## 5. Pipeline Behaviors

### Validation Behavior

```csharp
// {ServiceName}.Application/Common/Behaviours/ValidationBehaviour.cs
using FluentValidation;
using MediatR;

namespace {ServiceName}.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Logging Behavior

```csharp
// {ServiceName}.Application/Common/Behaviours/LoggingBehaviour.cs
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace {ServiceName}.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            
            stopwatch.Stop();
            _logger.LogInformation(
                "Handled {RequestName} in {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex, 
                "Error handling {RequestName} after {ElapsedMs}ms", 
                requestName, 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
```

---

## 6. Registro en DI

```csharp
// {ServiceName}.Application/DependencyInjection.cs
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using {ServiceName}.Application.Common.Behaviours;

namespace {ServiceName}.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        return services;
    }
}
```

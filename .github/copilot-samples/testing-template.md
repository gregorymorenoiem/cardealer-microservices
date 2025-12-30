# 📋 Template: Testing

Guía para crear tests unitarios y de integración.

---

## 1. Unit Tests (xUnit + Moq + FluentAssertions)

### Test de Command Handler

```csharp
// {ServiceName}.Tests/Features/Commands/Create{Entity}CommandHandlerTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using {ServiceName}.Application.Features.{Feature}.Commands;
using {ServiceName}.Domain.Entities;
using {ServiceName}.Domain.Interfaces;

namespace {ServiceName}.Tests.Features.Commands;

public class Create{Entity}CommandHandlerTests
{
    private readonly Mock<I{Entity}Repository> _repositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly Mock<ILogger<Create{Entity}CommandHandler>> _loggerMock;
    private readonly Create{Entity}CommandHandler _handler;

    public Create{Entity}CommandHandlerTests()
    {
        _repositoryMock = new Mock<I{Entity}Repository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _loggerMock = new Mock<ILogger<Create{Entity}CommandHandler>>();

        _handler = new Create{Entity}CommandHandler(
            _repositoryMock.Object,
            _eventPublisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new Create{Entity}Command(
            Name: "Test Entity",
            Description: "Test Description",
            Price: 100.00m);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<{Entity}>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entity e, CancellationToken _) => e);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Test Entity");
        result.Value.Price.Should().Be(100.00m);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<{Entity}>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<{Entity}CreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ReturnsFailure()
    {
        // Arrange
        var command = new Create{Entity}Command("Test", "Desc", 50.00m);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<{Entity}>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Database error");
    }

    [Theory]
    [InlineData("", "Description", 100)]
    [InlineData("Name", "", 100)]
    [InlineData("Name", "Description", -1)]
    public async Task Handle_InvalidData_ShouldBeHandledByValidator(
        string name, 
        string description, 
        decimal price)
    {
        // Arrange
        var command = new Create{Entity}Command(name, description, price);

        // Act - El validador debería prevenir esto antes del handler
        // Este test documenta casos edge
        
        // Assert
        command.Should().NotBeNull();
    }
}
```

### Test de Query Handler

```csharp
// {ServiceName}.Tests/Features/Queries/Get{Entity}ByIdQueryHandlerTests.cs
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using {ServiceName}.Application.Features.{Feature}.Queries;
using {ServiceName}.Domain.Entities;
using {ServiceName}.Domain.Interfaces;

namespace {ServiceName}.Tests.Features.Queries;

public class Get{Entity}ByIdQueryHandlerTests
{
    private readonly Mock<I{Entity}Repository> _repositoryMock;
    private readonly Mock<ILogger<Get{Entity}ByIdQueryHandler>> _loggerMock;
    private readonly Get{Entity}ByIdQueryHandler _handler;

    public Get{Entity}ByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<I{Entity}Repository>();
        _loggerMock = new Mock<ILogger<Get{Entity}ByIdQueryHandler>>();
        
        _handler = new Get{Entity}ByIdQueryHandler(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingId_ReturnsEntity()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new {Entity}
        {
            Id = entityId,
            Name = "Test",
            Description = "Description",
            Price = 100m
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var query = new Get{Entity}ByIdQuery(entityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(entityId);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task Handle_NonExistingId_ReturnsFailure()
    {
        // Arrange
        var entityId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Entity?)null);

        var query = new Get{Entity}ByIdQuery(entityId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }
}
```

### Test de Validator

```csharp
// {ServiceName}.Tests/Validators/Create{Entity}CommandValidatorTests.cs
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;
using {ServiceName}.Application.Features.{Feature}.Commands;
using {ServiceName}.Application.Validators;

namespace {ServiceName}.Tests.Validators;

public class Create{Entity}CommandValidatorTests
{
    private readonly Create{Entity}CommandValidator _validator;

    public Create{Entity}CommandValidatorTests()
    {
        _validator = new Create{Entity}CommandValidator();
    }

    [Fact]
    public async Task Validate_ValidCommand_NoErrors()
    {
        // Arrange
        var command = new Create{Entity}Command(
            Name: "Valid Name",
            Description: "Valid Description",
            Price: 100.00m);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_EmptyName_HasError()
    {
        // Arrange
        var command = new Create{Entity}Command(
            Name: "",
            Description: "Description",
            Price: 100.00m);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name is required");
    }

    [Fact]
    public async Task Validate_NameTooLong_HasError()
    {
        // Arrange
        var command = new Create{Entity}Command(
            Name: new string('x', 201),
            Description: "Description",
            Price: 100.00m);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Name must not exceed 200 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_InvalidPrice_HasError(decimal price)
    {
        // Arrange
        var command = new Create{Entity}Command(
            Name: "Valid Name",
            Description: "Description",
            Price: price);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Price must be greater than 0");
    }
}
```

---

## 2. Integration Tests (Testcontainers)

### Setup

```csharp
// IntegrationTests/Fixtures/{ServiceName}WebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using {ServiceName}.Infrastructure.Persistence;

namespace IntegrationTests.Fixtures;

public class {ServiceName}WebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("testdb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remover DbContext original
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<{ServiceName}DbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            // Agregar DbContext con Testcontainer
            services.AddDbContext<{ServiceName}DbContext>(options =>
            {
                options.UseNpgsql(ConnectionString);
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
```

### Integration Test

```csharp
// IntegrationTests/{ServiceName}/{Entity}ControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;
using IntegrationTests.Fixtures;
using {ServiceName}.Application.DTOs;

namespace IntegrationTests.{ServiceName};

public class {Entity}ControllerTests : IClassFixture<{ServiceName}WebApplicationFactory>
{
    private readonly HttpClient _client;

    public {Entity}ControllerTests({ServiceName}WebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/{entities}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedList<{Entity}Dto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_ValidEntity_ReturnsCreated()
    {
        // Arrange
        var request = new Create{Entity}Request(
            Name: "Integration Test Entity",
            Description: "Created via integration test",
            Price: 150.00m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/{entities}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var result = await response.Content.ReadFromJsonAsync<{Entity}Dto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Integration Test Entity");
        result.Price.Should().Be(150.00m);
    }

    [Fact]
    public async Task Create_InvalidEntity_ReturnsBadRequest()
    {
        // Arrange
        var request = new Create{Entity}Request(
            Name: "",  // Invalid: empty name
            Description: "Description",
            Price: 100.00m);

        // Act
        var response = await _client.PostAsJsonAsync("/api/{entities}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_ExistingEntity_ReturnsOk()
    {
        // Arrange - Crear primero
        var createRequest = new Create{Entity}Request("Test", "Desc", 100m);
        var createResponse = await _client.PostAsJsonAsync("/api/{entities}", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<{Entity}Dto>();

        // Act
        var response = await _client.GetAsync($"/api/{entities}/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<{Entity}Dto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetById_NonExistingEntity_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/{entities}/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ExistingEntity_ReturnsNoContent()
    {
        // Arrange - Crear primero
        var createRequest = new Create{Entity}Request("ToDelete", "Will be deleted", 50m);
        var createResponse = await _client.PostAsJsonAsync("/api/{entities}", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<{Entity}Dto>();

        // Act
        var response = await _client.DeleteAsync($"/api/{entities}/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/{entities}/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
```

---

## 3. Test Data (Bogus)

```csharp
// IntegrationTests/TestData/{Entity}Faker.cs
using Bogus;
using {ServiceName}.Domain.Entities;

namespace IntegrationTests.TestData;

public static class {Entity}Faker
{
    private static readonly Faker<{Entity}> _faker = new Faker<{Entity}>()
        .RuleFor(e => e.Id, f => Guid.NewGuid())
        .RuleFor(e => e.DealerId, f => Guid.NewGuid())
        .RuleFor(e => e.Name, f => f.Commerce.ProductName())
        .RuleFor(e => e.Description, f => f.Lorem.Paragraph())
        .RuleFor(e => e.Price, f => f.Finance.Amount(10, 10000))
        .RuleFor(e => e.CreatedAt, f => f.Date.Past())
        .RuleFor(e => e.IsActive, f => true);

    public static {Entity} Generate() => _faker.Generate();

    public static List<{Entity}> Generate(int count) => _faker.Generate(count);

    public static {Entity} GenerateWith(Action<{Entity}> configure)
    {
        var entity = Generate();
        configure(entity);
        return entity;
    }
}
```

---

## 4. Test de Repositorio

```csharp
// {ServiceName}.Tests/Repositories/{Entity}RepositoryTests.cs
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using {ServiceName}.Infrastructure.Persistence;
using {ServiceName}.Infrastructure.Repositories;
using IntegrationTests.TestData;

namespace {ServiceName}.Tests.Repositories;

public class {Entity}RepositoryTests
{
    private {ServiceName}DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<{ServiceName}DbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new {ServiceName}DbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddAsync_ValidEntity_ReturnsEntity()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new {Entity}Repository(context);
        var entity = {Entity}Faker.Generate();

        // Act
        var result = await repository.AddAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(entity.Id);

        var saved = await context.{Entities}.FindAsync(entity.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        // Arrange
        using var context = CreateContext();
        var entity = {Entity}Faker.Generate();
        context.{Entities}.Add(entity);
        await context.SaveChangesAsync();

        var repository = new {Entity}Repository(context);

        // Act
        var result = await repository.GetByIdAsync(entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEntity_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new {Entity}Repository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingEntity_RemovesEntity()
    {
        // Arrange
        using var context = CreateContext();
        var entity = {Entity}Faker.Generate();
        context.{Entities}.Add(entity);
        await context.SaveChangesAsync();

        var repository = new {Entity}Repository(context);

        // Act
        await repository.DeleteAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        var deleted = await context.{Entities}.FindAsync(entity.Id);
        deleted.Should().BeNull();
    }
}
```

---

## 5. Test .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="FluentAssertions" Version="7.0.0" />
    <PackageReference Include="FluentValidation.TestHelper" Version="11.11.0" />
    <PackageReference Include="Bogus" Version="35.6.1" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="3.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ServiceName}.Api\{ServiceName}.Api.csproj" />
    <ProjectReference Include="..\{ServiceName}.Application\{ServiceName}.Application.csproj" />
    <ProjectReference Include="..\{ServiceName}.Infrastructure\{ServiceName}.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

---

## 6. Comandos de Ejecución

```powershell
# Ejecutar todos los tests
dotnet test

# Ejecutar con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Ejecutar solo unit tests
dotnet test --filter "FullyQualifiedName~Tests.Features"

# Ejecutar solo integration tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Ver tests en tiempo real
dotnet test --logger "console;verbosity=detailed"

# Generar reporte HTML de cobertura
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

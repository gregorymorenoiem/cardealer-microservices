# 📋 Template: Crear Nuevo Microservicio

Guía paso a paso para crear un nuevo microservicio siguiendo la arquitectura del proyecto.

## 1. Estructura de Carpetas

```
backend/{ServiceName}/
├── {ServiceName}.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── {ServiceName}.Api.csproj
├── {ServiceName}.Application/
│   ├── Features/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── DTOs/
│   ├── Validators/
│   ├── Common/
│   │   └── Behaviours/
│   └── {ServiceName}.Application.csproj
├── {ServiceName}.Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Interfaces/
│   ├── Enums/
│   ├── Exceptions/
│   └── {ServiceName}.Domain.csproj
├── {ServiceName}.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   └── Repositories/
│   ├── Services/
│   ├── Messaging/
│   ├── Extensions/
│   └── {ServiceName}.Infrastructure.csproj
├── {ServiceName}.Tests/
│   └── {ServiceName}.Tests.csproj
├── Dockerfile
└── {ServiceName}.sln
```

## 2. Archivos Base

### {ServiceName}.Api.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Consul" Version="1.7.14.9" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.11">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.8.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageReference Include="Serilog.Enrichers.Span" Version="3.1.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.14.0" />
    <PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\{ServiceName}.Application\{ServiceName}.Application.csproj" />
    <ProjectReference Include="..\{ServiceName}.Infrastructure\{ServiceName}.Infrastructure.csproj" />
    <ProjectReference Include="..\..\_Shared\CarDealer.Contracts\CarDealer.Contracts.csproj" />
    <ProjectReference Include="..\..\_Shared\CarDealer.Shared\CarDealer.Shared.csproj" />
    <ProjectReference Include="..\..\ServiceDiscovery\ServiceDiscovery.Application\ServiceDiscovery.Application.csproj" />
    <ProjectReference Include="..\..\ServiceDiscovery\ServiceDiscovery.Infrastructure\ServiceDiscovery.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

### Program.cs (Template)

```csharp
using Microsoft.EntityFrameworkCore;
using {ServiceName}.Infrastructure.Persistence;
using {ServiceName}.Infrastructure.Extensions;
using Serilog;
using Serilog.Enrichers.Span;
using System.Reflection;
using FluentValidation;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Consul;
using ServiceDiscovery.Application.Interfaces;
using ServiceDiscovery.Infrastructure.Services;
using CarDealer.Shared.Database;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithSpan()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("{ServiceName}")
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("{ServiceName}"))
            .SetSampler(new TraceIdRatioBasedSampler(
                builder.Environment.IsProduction() ? 0.1 : 1.0))
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Exporter:Otlp:Endpoint"] ?? "http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("{ServiceName}")
            .AddOtlpExporter();
    });

// Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Database
builder.Services.AddDatabaseProvider<{ServiceName}DbContext>(builder.Configuration);

// Service Discovery
builder.Services.AddSingleton<IConsulClient>(sp =>
{
    var consulAddress = builder.Configuration["Consul:Address"] ?? "http://localhost:8500";
    return new ConsulClient(config => config.Address = new Uri(consulAddress));
});
builder.Services.AddScoped<IServiceRegistry, ConsulServiceRegistry>();
builder.Services.AddScoped<IServiceDiscovery, ConsulServiceDiscovery>();

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("{ServiceName}.Application")));
builder.Services.AddValidatorsFromAssembly(Assembly.Load("{ServiceName}.Application"));

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Database": {
    "Provider": "PostgreSQL",
    "Host": "localhost",
    "Port": 5432,
    "Database": "{servicename}",
    "Username": "postgres",
    "Password": "password",
    "AutoMigrate": true,
    "MaxRetryCount": 3,
    "MaxRetryDelay": 5,
    "CommandTimeout": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": true
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database={servicename};Username=postgres;Password=password",
    "Redis": "localhost:6379"
  },
  "Consul": {
    "Address": "http://localhost:8500"
  },
  "OpenTelemetry": {
    "Exporter": {
      "Otlp": {
        "Endpoint": "http://localhost:4317"
      }
    }
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar csproj y restaurar
COPY ["{ServiceName}/{ServiceName}.Api/{ServiceName}.Api.csproj", "{ServiceName}/{ServiceName}.Api/"]
COPY ["{ServiceName}/{ServiceName}.Application/{ServiceName}.Application.csproj", "{ServiceName}/{ServiceName}.Application/"]
COPY ["{ServiceName}/{ServiceName}.Domain/{ServiceName}.Domain.csproj", "{ServiceName}/{ServiceName}.Domain/"]
COPY ["{ServiceName}/{ServiceName}.Infrastructure/{ServiceName}.Infrastructure.csproj", "{ServiceName}/{ServiceName}.Infrastructure/"]
COPY ["_Shared/CarDealer.Contracts/CarDealer.Contracts.csproj", "_Shared/CarDealer.Contracts/"]
COPY ["_Shared/CarDealer.Shared/CarDealer.Shared.csproj", "_Shared/CarDealer.Shared/"]

RUN dotnet restore "{ServiceName}/{ServiceName}.Api/{ServiceName}.Api.csproj"

# Copiar todo y compilar
COPY . .
WORKDIR "/src/{ServiceName}/{ServiceName}.Api"
RUN dotnet build "{ServiceName}.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "{ServiceName}.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Security: run as non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "{ServiceName}.Api.dll"]
```

## 3. Entidad de Dominio (Ejemplo)

```csharp
// {ServiceName}.Domain/Entities/{Entity}.cs
using CarDealer.Shared.MultiTenancy;

namespace {ServiceName}.Domain.Entities;

public class {Entity} : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid DealerId { get; set; }  // Multi-tenant
    
    // Propiedades de negocio
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
```

## 4. DbContext

```csharp
// {ServiceName}.Infrastructure/Persistence/{ServiceName}DbContext.cs
using Microsoft.EntityFrameworkCore;
using {ServiceName}.Domain.Entities;

namespace {ServiceName}.Infrastructure.Persistence;

public class {ServiceName}DbContext : DbContext
{
    public {ServiceName}DbContext(DbContextOptions<{ServiceName}DbContext> options) 
        : base(options) { }
    
    public DbSet<{Entity}> {Entities} => Set<{Entity}>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<{Entity}>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DealerId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });
    }
}
```

## 5. Agregar a docker-compose.yml

```yaml
# {servicename}-db
{servicename}-db:
  image: postgres:16
  container_name: {servicename}-db
  environment:
    POSTGRES_DB: {servicename}
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: password
  ports:
    - "25440:5432"  # Puerto único
  volumes:
    - {servicename}_data:/var/lib/postgresql/data
  networks:
    - cargurus-net
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5

# {servicename} API
{servicename}:
  build:
    context: ./backend
    dockerfile: {ServiceName}/Dockerfile
  container_name: {servicename}
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    ASPNETCORE_URLS: http://+:80
    ConnectionStrings__DefaultConnection: "Host={servicename}-db;Database={servicename};Username=postgres;Password=password"
  ports:
    - "15090:80"  # Puerto único
  depends_on:
    {servicename}-db:
      condition: service_healthy
  networks:
    - cargurus-net

# En volumes:
volumes:
  {servicename}_data:
```

## 6. Agregar ruta en Ocelot

```json
// Gateway/Gateway.Api/ocelot.dev.json
{
  "UpstreamPathTemplate": "/api/{servicename}/{everything}",
  "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
  "DownstreamPathTemplate": "/api/{servicename}/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    { "Host": "{servicename}", "Port": 80 }
  ],
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,
    "DurationOfBreak": 10,
    "TimeoutValue": 30000
  }
}
```

## 7. Checklist

- [ ] Crear estructura de carpetas
- [ ] Crear archivos .csproj para cada capa
- [ ] Crear Program.cs con configuración base
- [ ] Crear DbContext y entidades
- [ ] Crear Dockerfile
- [ ] Agregar al docker-compose.yml
- [ ] Agregar rutas en Ocelot
- [ ] Agregar al CarDealer.sln
- [ ] Crear primer Controller con health check
- [ ] Crear tests básicos

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using RoleService.Domain.Interfaces;
using RoleService.Infrastructure.Persistence;

namespace RoleService.Tests.Helpers;

/// <summary>
/// Factory personalizado para tests de integración con autenticación mock y base de datos en memoria
/// </summary>
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "TestDatabase_" + Guid.NewGuid();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        
        builder.ConfigureServices(services =>
        {
            // 1. Remover DbContext de producción
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextService = services.SingleOrDefault(
                d => d.ServiceType == typeof(ApplicationDbContext));
            if (dbContextService != null)
            {
                services.Remove(dbContextService);
            }

            // 2. Remover autenticación JWT
            services.RemoveAll(typeof(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler));
            
            // 3. Agregar DbContext con base de datos en memoria (nombre fijo para toda la instancia)
            var dbName = _databaseName;
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.EnableSensitiveDataLogging();
            });

            // 4. Mock IErrorReporter (si no está registrado)
            if (!services.Any(x => x.ServiceType == typeof(IErrorReporter)))
            {
                var mockErrorReporter = new Mock<IErrorReporter>();
                services.AddSingleton(mockErrorReporter.Object);
            }

            // 5. Agregar autenticación de test DESPUÉS de remover JWT
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.AuthScheme;
                options.DefaultChallengeScheme = TestAuthHandler.AuthScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.AuthScheme,
                options => { });

            // 6. Asegurar que la DB se crea al iniciar
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Crea un cliente HTTP con autenticación configurada
    /// </summary>
    public HttpClient CreateAuthenticatedClient()
    {
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }
}

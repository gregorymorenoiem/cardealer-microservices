using BankReconciliationService.Domain.Interfaces;
using BankReconciliationService.Infrastructure.Persistence;
using BankReconciliationService.Infrastructure.Persistence.Repositories;
using BankReconciliationService.Infrastructure.Services.Banks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace BankReconciliationService.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline
        ConfigureApp(app, app.Environment);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<BankReconciliationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(BankReconciliationDbContext).Assembly.FullName)
            ));

        // MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.Load("BankReconciliationService.Application"));
        });

        // Repositories
        services.AddScoped<IBankStatementRepository, BankStatementRepository>();
        services.AddScoped<IInternalTransactionRepository, InternalTransactionRepository>();
        services.AddScoped<IReconciliationRepository, ReconciliationRepository>();
        services.AddScoped<IBankAccountConfigRepository, BankAccountConfigRepository>();

        // Bank API Services (4 bancos dominicanos)
        services.AddBankApiServices(configuration);

        // Authentication
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        services.AddAuthorization();

        // Controllers
        services.AddControllers();

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Bank Reconciliation Service API",
                Version = "v1",
                Description = "API for automated bank reconciliation with Dominican banks (Banco Popular, Banreservas, BHD León, Scotiabank)",
                Contact = new OpenApiContact
                {
                    Name = "OKLA CarDealer",
                    Email = "support@okla.com.do"
                }
            });

            // JWT Authentication in Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // XML Comments
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        // Health Checks
        services.AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString("DefaultConnection") ?? "");

        // HttpClient for bank APIs
        services.AddHttpClient();
    }

    private static void ConfigureApp(WebApplication app, IWebHostEnvironment env)
    {
        // Exception handling
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank Reconciliation Service API v1");
                c.RoutePrefix = string.Empty; // Swagger at root
            });
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();

        // Health checks endpoint
        app.MapHealthChecks("/health");

        app.MapControllers();
    }
}

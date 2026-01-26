using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SpyneIntegrationService.Application.Validators;
using SpyneIntegrationService.Domain.Interfaces;
using SpyneIntegrationService.Infrastructure.Persistence;
using SpyneIntegrationService.Infrastructure.Persistence.Repositories;
using SpyneIntegrationService.Infrastructure.Repositories;
using SpyneIntegrationService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES ====================

// Database
builder.Services.AddDbContext<SpyneDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssemblyContaining<TransformImageCommandValidator>());

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<TransformImageCommandValidator>();

// Repositories
builder.Services.AddScoped<IImageTransformationRepository, ImageTransformationRepository>();
builder.Services.AddScoped<ISpinGenerationRepository, SpinGenerationRepository>();
builder.Services.AddScoped<IVideoGenerationRepository, VideoGenerationRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
builder.Services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
builder.Services.AddScoped<IVideo360SpinRepository, Video360SpinRepository>();

// Spyne API Client Configuration
builder.Services.Configure<SpyneApiClientOptions>(
    builder.Configuration.GetSection(SpyneApiClientOptions.SectionName));

var spyneSettings = builder.Configuration.GetSection(SpyneApiClientOptions.SectionName).Get<SpyneApiClientOptions>() 
    ?? new SpyneApiClientOptions();

builder.Services.AddHttpClient<ISpyneApiClient, SpyneApiClient>(client =>
{
    client.BaseAddress = new Uri(spyneSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(spyneSettings.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddStandardResilienceHandler();

// Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "DefaultDevelopmentKeyThatIsAtLeast32Characters";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "OKLA";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SpyneIntegrationService API",
        Version = "v1",
        Description = """
            API para integración con Spyne AI Studio.
            
            ## Funcionalidades:
            
            ### Fase 1: Transformación de Imágenes
            - Background removal/replacement
            - Image enhancement
            - License plate masking
            - Shadow generation
            
            ### Fase 2: 360° Spin (desde imágenes)
            - Generate interactive 360° views from multiple images
            - Embed code for web integration
            - Multiple background options
            
            ### Fase 2.5: 360° Spin desde Video 🆕
            - Upload video walkthrough around vehicle
            - Spyne extracts frames automatically (36-72 images)
            - Processes each frame (background, enhancement)
            - Returns interactive 360° viewer + extracted images
            - POST /api/video360spins/generate
            
            ### Fase 3: Video Generation
            - Cinematic vehicle videos
            - Multiple styles (Cinematic, Dynamic, Showcase, Social, Premium)
            - Background music integration
            
            ### Fase 4: Chat AI (Vini) - ⚠️ NOT FOR FRONTEND USE
            - AI-powered chat for vehicle inquiries
            - Lead qualification
            - Available but not consumed by frontend in this version
            """
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Example: 'Bearer {token}'",
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
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:3000",
                "https://okla.com.do",
                "https://www.okla.com.do")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// ==================== MIDDLEWARE ====================

// Swagger (always enabled for this service)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SpyneIntegrationService API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapHealthChecks("/health");

// Database migration on startup (development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SpyneDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

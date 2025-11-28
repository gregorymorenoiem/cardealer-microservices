using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using MMLib.SwaggerForOcelot.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 1. Determinar entorno
var isDevelopment = builder.Environment.IsDevelopment();
var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.prod.json";
//var configFile = isDevelopment ? "ocelot.dev.json" : "ocelot.dev.json";

// 2. Cargar configuración
builder.Configuration.AddJsonFile(configFile, optional: false, reloadOnChange: true);

// 3. Configuración esencial
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. CORS simplificado
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy => 
    {
        if (isDevelopment)
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins("https://inelcasrl.com.do")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetPreflightMaxAge(TimeSpan.FromHours(1));
        }
    });
});

// 5. Configuración Ocelot con Polly
builder.Services
    .AddOcelot(builder.Configuration)
    .AddPolly();

// 6. Swagger para Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// 7. Middleware pipeline
app.UseCors();
app.UseSwagger();
app.UseSwaggerForOcelotUI();

// 8. Agregar endpoint de salud para el Gateway
app.MapGet("/health", () => Results.Ok("Gateway is healthy"));

// 9. Ocelot como último middleware
await app.UseOcelot();

app.Run();
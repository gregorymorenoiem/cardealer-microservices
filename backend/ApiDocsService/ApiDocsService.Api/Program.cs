using ApiDocsService.Core.Interfaces;
using ApiDocsService.Core.Services;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Configure HttpClient for service communication
builder.Services.AddHttpClient<IApiAggregatorService, ApiAggregatorService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Configure services from appsettings
builder.Services.Configure<ServicesConfiguration>(
    builder.Configuration.GetSection("Services"));

// Health checks
builder.Services.AddHealthChecks();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Documentation Aggregator",
        Version = "v1",
        Description = "Centralized API documentation for all CarDealer microservices. " +
                      "This service aggregates OpenAPI specifications from all registered services " +
                      "and provides a unified documentation portal.",
        Contact = new OpenApiContact
        {
            Name = "CarDealer Team",
            Email = "dev@cardealer.com"
        }
    });

    // Include XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Docs Aggregator v1");
    options.DocumentTitle = "CarDealer API Documentation";
    options.RoutePrefix = "swagger";
    
    // Custom Swagger UI settings
    options.EnableDeepLinking();
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.ShowExtensions();
});

// Serve static files for custom UI
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors();

app.MapControllers();

app.MapHealthChecks("/health");

// Root endpoint - redirect to docs portal
app.MapGet("/", () => Results.Redirect("/portal"));

// Portal endpoint - serve custom documentation portal
app.MapGet("/portal", async (IApiAggregatorService aggregator) =>
{
    var services = await aggregator.GetAllServicesAsync();
    var dashboard = await aggregator.GetDashboardAsync();
    
    var html = GeneratePortalHtml(services, dashboard);
    return Results.Content(html, "text/html");
});

Log.Information("API Documentation Aggregator starting on {Urls}", builder.Configuration["ASPNETCORE_URLS"]);

app.Run();

// Helper method to generate portal HTML
static string GeneratePortalHtml(List<ApiDocsService.Core.Models.ServiceInfo> services, ApiDocsService.Core.Models.ApiDocsDashboard dashboard)
{
    var serviceCards = string.Join("\n", services.Select(s => $@"
        <div class='service-card {(s.IsHealthy ? "healthy" : "unhealthy")}'>
            <h3>{s.DisplayName}</h3>
            <p class='description'>{s.Description}</p>
            <div class='meta'>
                <span class='category'>{s.Category}</span>
                <span class='version'>{s.Version}</span>
            </div>
            <div class='actions'>
                <a href='{s.SwaggerUrl}' target='_blank' class='btn'>OpenAPI Spec</a>
                <a href='/api/docs/openapi/{s.Name}' target='_blank' class='btn secondary'>View JSON</a>
            </div>
            <div class='status'>
                <span class='dot {(s.IsHealthy ? "green" : "red")}'></span>
                {(s.IsHealthy ? "Healthy" : "Unhealthy")}
            </div>
        </div>
    "));

    var categoryNav = string.Join("\n", services
        .Select(s => s.Category)
        .Distinct()
        .OrderBy(c => c)
        .Select(c => $"<a href='#' class='category-link' data-category='{c}'>{c}</a>"));

    return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>CarDealer API Documentation Portal</title>
    <link href='https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap' rel='stylesheet'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: 'Inter', sans-serif; background: #f5f7fa; color: #1a1a2e; }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; padding: 2rem; text-align: center;
        }}
        .header h1 {{ font-size: 2.5rem; margin-bottom: 0.5rem; }}
        .header p {{ opacity: 0.9; }}
        .dashboard {{ 
            display: flex; justify-content: center; gap: 2rem; 
            padding: 2rem; background: white; box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .stat {{ text-align: center; }}
        .stat .number {{ font-size: 2rem; font-weight: 700; color: #667eea; }}
        .stat .label {{ color: #666; font-size: 0.875rem; }}
        .container {{ max-width: 1400px; margin: 0 auto; padding: 2rem; }}
        .filters {{ margin-bottom: 2rem; display: flex; gap: 1rem; flex-wrap: wrap; }}
        .category-link {{ 
            padding: 0.5rem 1rem; background: white; border-radius: 20px;
            text-decoration: none; color: #667eea; border: 1px solid #667eea;
            transition: all 0.2s;
        }}
        .category-link:hover, .category-link.active {{ background: #667eea; color: white; }}
        .services-grid {{ 
            display: grid; grid-template-columns: repeat(auto-fill, minmax(350px, 1fr)); 
            gap: 1.5rem; 
        }}
        .service-card {{ 
            background: white; border-radius: 12px; padding: 1.5rem;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08); transition: transform 0.2s;
            border-left: 4px solid #667eea;
        }}
        .service-card:hover {{ transform: translateY(-4px); }}
        .service-card.unhealthy {{ border-left-color: #e74c3c; opacity: 0.8; }}
        .service-card h3 {{ margin-bottom: 0.5rem; color: #1a1a2e; }}
        .service-card .description {{ color: #666; font-size: 0.875rem; margin-bottom: 1rem; }}
        .service-card .meta {{ display: flex; gap: 0.5rem; margin-bottom: 1rem; }}
        .service-card .category {{ 
            background: #e8f4fd; color: #667eea; padding: 0.25rem 0.75rem; 
            border-radius: 12px; font-size: 0.75rem; 
        }}
        .service-card .version {{ 
            background: #f0f0f0; color: #666; padding: 0.25rem 0.75rem;
            border-radius: 12px; font-size: 0.75rem;
        }}
        .service-card .actions {{ display: flex; gap: 0.5rem; margin-bottom: 1rem; }}
        .btn {{ 
            padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none;
            font-size: 0.875rem; font-weight: 500; transition: all 0.2s;
        }}
        .btn {{ background: #667eea; color: white; }}
        .btn:hover {{ background: #5a6fd6; }}
        .btn.secondary {{ background: #f0f0f0; color: #333; }}
        .btn.secondary:hover {{ background: #e0e0e0; }}
        .status {{ display: flex; align-items: center; gap: 0.5rem; font-size: 0.875rem; }}
        .dot {{ width: 8px; height: 8px; border-radius: 50%; }}
        .dot.green {{ background: #27ae60; }}
        .dot.red {{ background: #e74c3c; }}
        .search {{ margin-bottom: 2rem; }}
        .search input {{ 
            width: 100%; padding: 1rem; border: 2px solid #e0e0e0; border-radius: 8px;
            font-size: 1rem; transition: border-color 0.2s;
        }}
        .search input:focus {{ outline: none; border-color: #667eea; }}
        .nav-links {{ display: flex; gap: 1rem; margin-top: 1rem; }}
        .nav-links a {{ color: white; text-decoration: none; opacity: 0.9; }}
        .nav-links a:hover {{ opacity: 1; text-decoration: underline; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🚗 CarDealer API Documentation</h1>
        <p>Centralized documentation for all microservices</p>
        <div class='nav-links'>
            <a href='/swagger'>Swagger UI</a>
            <a href='/api/docs/dashboard'>Dashboard API</a>
            <a href='/api/docs/health/all'>Health Status</a>
        </div>
    </div>
    
    <div class='dashboard'>
        <div class='stat'>
            <div class='number'>{dashboard.TotalServices}</div>
            <div class='label'>Total Services</div>
        </div>
        <div class='stat'>
            <div class='number' style='color: #27ae60;'>{dashboard.HealthyServices}</div>
            <div class='label'>Healthy</div>
        </div>
        <div class='stat'>
            <div class='number' style='color: #e74c3c;'>{dashboard.UnhealthyServices}</div>
            <div class='label'>Unhealthy</div>
        </div>
        <div class='stat'>
            <div class='number'>{dashboard.TotalEndpoints}</div>
            <div class='label'>Total Endpoints</div>
        </div>
    </div>
    
    <div class='container'>
        <div class='search'>
            <input type='text' id='searchInput' placeholder='Search services or endpoints...' />
        </div>
        
        <div class='filters'>
            <a href='#' class='category-link active' data-category='all'>All</a>
            {categoryNav}
        </div>
        
        <div class='services-grid'>
            {serviceCards}
        </div>
    </div>
    
    <script>
        // Filter by category
        document.querySelectorAll('.category-link').forEach(link => {{
            link.addEventListener('click', (e) => {{
                e.preventDefault();
                document.querySelectorAll('.category-link').forEach(l => l.classList.remove('active'));
                link.classList.add('active');
                const category = link.dataset.category;
                document.querySelectorAll('.service-card').forEach(card => {{
                    if (category === 'all' || card.querySelector('.category').textContent === category) {{
                        card.style.display = 'block';
                    }} else {{
                        card.style.display = 'none';
                    }}
                }});
            }});
        }});
        
        // Search
        document.getElementById('searchInput').addEventListener('input', (e) => {{
            const query = e.target.value.toLowerCase();
            document.querySelectorAll('.service-card').forEach(card => {{
                const text = card.textContent.toLowerCase();
                card.style.display = text.includes(query) ? 'block' : 'none';
            }});
        }});
    </script>
</body>
</html>";
}

// Make Program class accessible for testing
public partial class Program { }

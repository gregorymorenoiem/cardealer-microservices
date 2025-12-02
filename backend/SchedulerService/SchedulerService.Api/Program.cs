using Hangfire;
using Microsoft.EntityFrameworkCore;
using SchedulerService.Application.Handlers;
using SchedulerService.Infrastructure;
using SchedulerService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SchedulerService API",
        Version = "v1",
        Description = "Distributed job scheduling service with Hangfire"
    });
});

// Add MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateJobCommandHandler).Assembly));

// Add Infrastructure layer (includes Hangfire, EF Core, repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    DashboardTitle = "SchedulerService - Job Dashboard"
});

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Simple authorization filter for Hangfire Dashboard (allow all in development)
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // In production, implement proper authorization
        return true;
    }
}

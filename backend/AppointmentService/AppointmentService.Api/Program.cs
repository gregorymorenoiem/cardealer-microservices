using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using AppointmentService.Domain.Interfaces;
using AppointmentService.Infrastructure.Persistence;
using AppointmentService.Infrastructure.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AppointmentService API", Version = "v1" });
});

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add DbContext
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();

// Add Health Checks
builder.Services.AddHealthChecks();

// Module Access (for paid feature gating) - disabled in development
// builder.Services.AddModuleAccessServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Module access verification - disabled in development
// app.UseModuleAccess("appointments");

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppointmentDbContext>();
    db.Database.Migrate();
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }

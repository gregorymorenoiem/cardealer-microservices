using Microsoft.EntityFrameworkCore;
using CarDealer.Shared.MultiTenancy;
using CarDealer.Shared.Extensions;
using CarDealer.Shared.Middleware;
using FinanceService.Domain.Interfaces;
using FinanceService.Infrastructure.Persistence;
using FinanceService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Finance Service API", Version = "v1" });
});

// Database
builder.Services.AddDbContext<FinanceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Multi-tenancy
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();

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

// Module Access (for paid feature gating) - disabled in development
// builder.Services.AddModuleAccessServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Module access verification - disabled in development
// app.UseModuleAccess("finance-advanced");

app.MapControllers();
app.MapHealthChecks("/health");

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

// Make the implicit Program class public so it can be accessed by tests
public partial class Program { }
 
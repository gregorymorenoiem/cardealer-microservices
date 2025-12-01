using AdminService.Application.Interfaces;
using AdminService.Infrastructure.External;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
    typeof(AdminService.Application.UseCases.Vehicles.ApproveVehicle.ApproveVehicleCommand).Assembly));

// Configure HttpClients for external services
builder.Services.AddHttpClient<IAuditServiceClient, AuditServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:AuditService"] ?? "https://localhost:7287";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:NotificationService"] ?? "https://localhost:45954";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IErrorServiceClient, ErrorServiceClient>(client =>
{
    var baseAddress = builder.Configuration["ServiceUrls:ErrorService"] ?? "https://localhost:45952";
    client.BaseAddress = new Uri(baseAddress);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

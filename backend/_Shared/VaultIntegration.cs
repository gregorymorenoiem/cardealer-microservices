// Vault Integration Example for CarDealer Microservices
// Add this to Program.cs or create a VaultConfigurationProvider

using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace CarDealer.Common.Vault
{
    public static class VaultConfiguration
    {
        public static IServiceCollection AddVaultConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var vaultUri = configuration["Vault:Uri"] ?? "http://vault:8200";
            var vaultToken = configuration["Vault:Token"] ?? "myroot";

            var authMethod = new TokenAuthMethodInfo(vaultToken);
            var vaultClientSettings = new VaultClientSettings(vaultUri, authMethod);
            var vaultClient = new VaultClient(vaultClientSettings);

            services.AddSingleton<IVaultClient>(vaultClient);

            return services;
        }

        public static async Task<string> GetDatabaseConnectionString(
            IVaultClient vaultClient,
            string serviceName)
        {
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync($"cardealer/database/{serviceName}");

            var data = secret.Data.Data;
            var host = data["host"].ToString();
            var port = data["port"].ToString();
            var database = data["database"].ToString();
            var username = data["username"].ToString();
            var password = data["password"].ToString();

            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        public static async Task<JwtSettings> GetJwtSettings(IVaultClient vaultClient)
        {
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync("cardealer/jwt");

            var data = secret.Data.Data;

            return new JwtSettings
            {
                SecretKey = data["secretkey"].ToString(),
                Issuer = data["issuer"].ToString(),
                Audience = data["audience"].ToString()
            };
        }

        public static async Task<string> GetRedisConnectionString(IVaultClient vaultClient)
        {
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync("cardealer/redis");

            return secret.Data.Data["connectionstring"].ToString();
        }

        public static async Task<string> GetRabbitMQConnectionString(IVaultClient vaultClient)
        {
            var secret = await vaultClient.V1.Secrets.KeyValue.V2
                .ReadSecretAsync("cardealer/rabbitmq");

            return secret.Data.Data["connectionstring"].ToString();
        }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}

// Usage in Program.cs:
/*
var builder = WebApplication.CreateBuilder(args);

// Add Vault
builder.Services.AddVaultConfiguration(builder.Configuration);

// Get secrets from Vault
var vaultClient = builder.Services.BuildServiceProvider().GetRequiredService<IVaultClient>();
var connectionString = await VaultConfiguration.GetDatabaseConnectionString(vaultClient, "errorservice");
var jwtSettings = await VaultConfiguration.GetJwtSettings(vaultClient);

// Use secrets
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience
        };
    });

var app = builder.Build();
app.Run();
*/

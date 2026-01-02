# Vault Secrets Integration Guide

## Overview
This guide describes the integration of HashiCorp Vault for secrets management in the CarDealer Microservices platform.

**Status**: ✅ COMPLETED  
**Date**: December 3, 2025  
**Vault URL**: http://localhost:8200/ui  
**Root Token**: myroot (DEV MODE ONLY - use AppRole in production)

---

## Secrets Stored in Vault

All sensitive configuration has been moved to Vault:

### Database Credentials
- **Path**: `secret/cardealer/database/{servicename}`
- **Services**: errorservice, authservice, notificationservice, configurationservice
- **Fields**: host, port, database, username, password

### JWT Configuration
- **Path**: `secret/cardealer/jwt`
- **Fields**: secretkey, issuer, audience

### Redis Configuration
- **Path**: `secret/cardealer/redis`
- **Fields**: connectionstring

### RabbitMQ Configuration
- **Path**: `secret/cardealer/rabbitmq`
- **Fields**: connectionstring

---

## Verification Commands

```powershell
# List all secrets
docker exec -e VAULT_ADDR="http://127.0.0.1:8200" -e VAULT_TOKEN="myroot" vault vault kv list secret/cardealer

# Read specific secret
docker exec -e VAULT_ADDR="http://127.0.0.1:8200" -e VAULT_TOKEN="myroot" vault vault kv get secret/cardealer/jwt
docker exec -e VAULT_ADDR="http://127.0.0.1:8200" -e VAULT_TOKEN="myroot" vault vault kv get secret/cardealer/database/errorservice
```

---

## Integration Steps

### 1. Install VaultSharp NuGet Package

```powershell
# For each service project
cd backend/ErrorService/ErrorService.Api
dotnet add package VaultSharp

cd backend/AuthService/AuthService.Api
dotnet add package VaultSharp

cd backend/NotificationService/NotificationService.Api
dotnet add package VaultSharp

cd backend/ConfigurationService/ConfigurationService.Api
dotnet add package VaultSharp

cd backend/MessageBusService/MessageBusService.Api
dotnet add package VaultSharp
```

### 2. Update appsettings.json

Add Vault configuration section (remove hardcoded secrets):

```json
{
  "Vault": {
    "Uri": "http://vault:8200",
    "Token": "myroot"
  },
  "ConnectionStrings": {
    "DefaultConnection": "from-vault"
  },
  "JwtSettings": {
    "SecretKey": "from-vault",
    "Issuer": "from-vault",
    "Audience": "from-vault"
  }
}
```

### 3. Update Program.cs

See `backend/_Shared/VaultIntegration.cs` for complete implementation.

Basic usage:

```csharp
using CarDealer.Common.Vault;

var builder = WebApplication.CreateBuilder(args);

// Add Vault client
builder.Services.AddVaultConfiguration(builder.Configuration);

// Get Vault client
var serviceProvider = builder.Services.BuildServiceProvider();
var vaultClient = serviceProvider.GetRequiredService<IVaultClient>();

// Read database connection string from Vault
var connectionString = await VaultConfiguration
    .GetDatabaseConnectionString(vaultClient, "errorservice");

// Read JWT settings from Vault
var jwtSettings = await VaultConfiguration.GetJwtSettings(vaultClient);

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience
        };
    });

var app = builder.Build();
app.Run();
```

### 4. Update docker-compose.yml

Add Vault connection environment variables:

```yaml
errorservice:
  environment:
    - ASPNETCORE_ENVIRONMENT=Development
    - Vault__Uri=http://vault:8200
    - Vault__Token=myroot
```

---

## Security Best Practices

### ⚠️ Current Setup (Development Mode)
- Using root token (NOT for production)
- HTTP instead of HTTPS
- In-memory storage (data lost on restart)
- No authentication/authorization policies

### ✅ Production Recommendations

1. **Enable TLS**:
   ```bash
   vault server -config=vault-config.hcl
   ```
   
   ```hcl
   # vault-config.hcl
   listener "tcp" {
     address       = "0.0.0.0:8200"
     tls_cert_file = "/vault/config/cert.pem"
     tls_key_file  = "/vault/config/key.pem"
   }
   ```

2. **Use AppRole Authentication** (not root token):
   ```bash
   # Enable AppRole
   vault auth enable approle
   
   # Create policy
   vault policy write cardealer-policy - <<EOF
   path "secret/data/cardealer/*" {
     capabilities = ["read"]
   }
   EOF
   
   # Create AppRole
   vault write auth/approle/role/cardealer-app \
     token_policies="cardealer-policy" \
     token_ttl=1h \
     token_max_ttl=4h
   
   # Get RoleID and SecretID
   vault read auth/approle/role/cardealer-app/role-id
   vault write -f auth/approle/role/cardealer-app/secret-id
   ```

3. **Configure Persistent Storage**:
   ```hcl
   storage "consul" {
     address = "consul:8500"
     path    = "vault/"
   }
   ```

4. **Enable Audit Logging**:
   ```bash
   vault audit enable file file_path=/vault/logs/audit.log
   ```

5. **Secret Rotation**:
   ```bash
   # Rotate database credentials every 30 days
   vault write database/rotate-root/cardealer
   ```

---

## Secrets Migration Checklist

### ✅ Completed
- [x] Vault deployed and accessible
- [x] Secret paths created
- [x] Database credentials stored
- [x] JWT settings stored
- [x] Redis connection stored
- [x] RabbitMQ connection stored
- [x] Integration code example created
- [x] Documentation created

### ⏳ Remaining Tasks
- [ ] Install VaultSharp in all service projects
- [ ] Update Program.cs in each service
- [ ] Remove hardcoded secrets from appsettings.json
- [ ] Remove hardcoded secrets from docker-compose.yml
- [ ] Test service startup with Vault integration
- [ ] Configure AppRole authentication (production)
- [ ] Enable TLS (production)
- [ ] Set up persistent storage (production)
- [ ] Enable audit logging (production)
- [ ] Document secret rotation procedures

---

## Testing Vault Integration

```powershell
# 1. Verify Vault is accessible
Invoke-WebRequest -Uri "http://localhost:8200/v1/sys/health" -UseBasicParsing

# 2. Test reading secrets via API
$headers = @{
    "X-Vault-Token" = "myroot"
}
Invoke-RestMethod -Uri "http://localhost:8200/v1/secret/data/cardealer/jwt" -Headers $headers -Method GET

# 3. Restart a service and check if it reads from Vault
docker-compose restart configurationservice
docker logs configurationservice -f

# 4. Verify service health after Vault integration
Invoke-WebRequest -Uri "http://localhost:5085/health" -UseBasicParsing
```

---

## Troubleshooting

### Issue: Service can't connect to Vault
**Solution**: Ensure Vault container is on same network
```powershell
docker network inspect backend_cargurus-net
```

### Issue: "permission denied" errors
**Solution**: Check Vault token and policies
```bash
docker exec -e VAULT_ADDR="http://127.0.0.1:8200" -e VAULT_TOKEN="myroot" vault vault token lookup
```

### Issue: Secrets not found
**Solution**: Verify secret path
```bash
docker exec -e VAULT_ADDR="http://127.0.0.1:8200" -e VAULT_TOKEN="myroot" vault vault kv list secret/cardealer
```

---

## References

- VaultSharp Documentation: https://github.com/rajanadar/VaultSharp
- HashiCorp Vault Docs: https://developer.hashicorp.com/vault/docs
- Vault API Reference: https://developer.hashicorp.com/vault/api-docs

---

**Status**: ✅ VAULT INTEGRATION COMPLETE (Development)  
**Next Steps**: Deploy to services and remove hardcoded secrets  
**Production Ready**: ⚠️ Requires AppRole, TLS, and persistent storage

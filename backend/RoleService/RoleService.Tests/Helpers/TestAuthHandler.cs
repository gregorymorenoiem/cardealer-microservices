using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RoleService.Tests.Helpers;

/// <summary>
/// AuthenticationHandler para tests que simula autenticación JWT sin requerir tokens reales
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthScheme = "TestAuth";
    
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Crear claims por defecto para tests - SUPERADMIN con TODOS los permisos
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
            new Claim(ClaimTypes.Role, "Admin"),
            // Claims adicionales para RoleService (todos los permisos)
            new Claim("permissions", "ManageRoles"),
            new Claim("permissions", "ViewRoles"),
            new Claim("permissions", "ManagePermissions"),
            new Claim("permissions", "ViewPermissions"),
            new Claim("permissions", "RoleServiceAccess"),
            new Claim("role", "SuperAdmin"),
            new Claim("role", "Admin")
        };

        // Leer claims del header si se proporcionan (para tests específicos)
        if (Request.Headers.TryGetValue("X-Test-UserId", out var userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
        }

        if (Request.Headers.TryGetValue("X-Test-Roles", out var roles))
        {
            foreach (var role in roles.ToString().Split(','))
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                claims.Add(new Claim("role", role.Trim()));
            }
        }

        var identity = new ClaimsIdentity(claims, AuthScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

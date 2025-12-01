using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace UserService.Shared.Security
{
    /// <summary>
    /// Generador de tokens JWT para testing y desarrollo
    /// </summary>
    public class JwtTokenGenerator
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secretKey;
        private readonly int _expirationMinutes;

        public JwtTokenGenerator(string issuer, string audience, string secretKey, int expirationMinutes = 60)
        {
            _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
            _audience = audience ?? throw new ArgumentNullException(nameof(audience));
            _secretKey = secretKey ?? throw new ArgumentNullException(nameof(secretKey));
            _expirationMinutes = expirationMinutes;
        }

        /// <summary>
        /// Genera un token JWT con los claims especificados
        /// </summary>
        public string GenerateToken(params Claim[] claims)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Genera un token JWT para acceso al UserService
        /// </summary>
        public string GenerateUserServiceToken(string? userId = null, string? serviceName = null)
        {
            var claims = new List<Claim>
            {
                new Claim("service", "UserService"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            if (!string.IsNullOrWhiteSpace(userId))
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            if (!string.IsNullOrWhiteSpace(serviceName))
            {
                claims.Add(new Claim("source_service", serviceName));
            }

            return GenerateToken(claims.ToArray());
        }

        /// <summary>
        /// Genera un token JWT para acceso administrativo al UserService
        /// </summary>
        public string GenerateAdminToken(string userId = "admin")
        {
            var claims = new[]
            {
                new Claim("service", "UserService"),
                new Claim("role", "admin"),
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            return GenerateToken(claims);
        }

        /// <summary>
        /// Genera un token JWT para servicios con acceso completo
        /// </summary>
        public string GenerateServiceToken(string serviceName)
        {
            var claims = new[]
            {
                new Claim("service", "all"),
                new Claim("source_service", serviceName),
                new Claim(JwtRegisteredClaimNames.Sub, $"service-{serviceName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            return GenerateToken(claims);
        }

        /// <summary>
        /// Genera un token JWT de solo lectura
        /// </summary>
        public string GenerateReadOnlyToken(string userId = "readonly-user")
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("role", "read-only"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            return GenerateToken(claims);
        }
    }
}

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

class Program
{
    static void Main()
    {
        var secretKey = "cardealer-super-secret-key-min-32-characters-long-for-production!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
            new Claim("name", "Test User"),
            new Claim("role", "admin"),
            new Claim("service", "all"),
            new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "cardealer-auth",
            audience: "cardealer-services",
            claims: claims,
            expires: DateTime.UtcNow.AddYears(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        Console.WriteLine(tokenString);
    }
}

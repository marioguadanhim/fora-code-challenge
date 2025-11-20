using Fora.Infra.Security.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fora.Infra.Security.Web;

public class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    private readonly string vaultKey = configuration["JwtSettings:SecretKey"] ?? throw new FileNotFoundException("Not possible to find JwtSettings:Key");
    private readonly string vaultExpirationInMinutes = configuration["JwtSettings:ExpirationInMinutes"] ?? throw new FileNotFoundException("Not possible to find JwtSettings:ExpirationInMinutes");
    private readonly string vaultIssuer = configuration["JwtSettings:Issuer"] ?? throw new FileNotFoundException("Not possible to find JwtSettings:ExpirationInMinutes");
    private readonly string vaultAudience = configuration["JwtSettings:Audience"] ?? throw new FileNotFoundException("Not possible to find JwtSettings:ExpirationInMinutes");

    public string GenerateForLogin(string userName, List<string> roles)
    {
        return GenerateForLogin(userName, roles.ToArray());
    }

    public string GenerateForLogin(string userName, params string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(vaultKey);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var expirationInMinutes = int.Parse(vaultExpirationInMinutes);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes),
            Issuer = vaultIssuer,
            Audience = vaultAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
}
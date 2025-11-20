using Fora.Infra.Security.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Fora.Infra.Security.Web;

public class PasswordHasher(IConfiguration configuration) : IPasswordHasher
{
    private readonly string key = configuration["JwtSettings:SecretKey"] ?? throw new FileNotFoundException("Not possible to find JwtSettings:Key");

    public string HashPasswordWithKey(string password)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedPassword);
        }
    }
}

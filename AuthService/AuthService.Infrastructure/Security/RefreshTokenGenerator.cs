using System.Security.Cryptography;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Security
{
    public class RefreshTokenGenerator(IConfiguration configuration): IRefreshTokenGenerator
    {
        private readonly IConfiguration _configuration = configuration;

        public RefreshToken GenerateToken(string userId)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = GenerateRandomTokenString(),
                DateCreated = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiresInDays"]))
            };
        }

        public string HashToken(string token)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

            byte[] hashBytes = SHA256.HashData(tokenBytes);

            return Convert.ToBase64String(hashBytes);
        }

        private static string GenerateRandomTokenString()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}

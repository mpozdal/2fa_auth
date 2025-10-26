using System.Security.Claims;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Application.Helpers
{
    public class JwtTokenGenerator(IConfiguration configuration): IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration = configuration;

        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
           var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims =
            [
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                ..roles.Select(r => new Claim(ClaimTypes.Role, r))
            ];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiresInMinutes"])),
                SigningCredentials = credentials,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
            };

            var tokenHandler = new JsonWebTokenHandler();

            return tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}

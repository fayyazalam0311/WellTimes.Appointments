using AuthService.Models;// Gives access to your existing User model.
using Microsoft.Extensions.Options; //Provides the IOptions<T> configuration pattern.
using Microsoft.IdentityModel.Tokens; // Provides signing keys, signing credentials, and algorithm definitions.
using System.IdentityModel.Tokens.Jwt; // Creates JWT objects and converts them into token strings.
using System.Security.Claims; // Represents claims about the user.
using System.Text; // Converts the secret key from text into bytes using UTF-8.

namespace AuthService.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public JwtTokenService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public TokenResult CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("role", user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer : _settings.Issuer,
                audience : _settings.Audience,
                claims : claims,
                notBefore : DateTime.UtcNow,
                expires : expiresAtUtc,
                signingCredentials : credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenResult(tokenString, expiresAtUtc);

        }
    }
}

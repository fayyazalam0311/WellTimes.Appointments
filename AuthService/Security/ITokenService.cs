using AuthService.Models;

namespace AuthService.Security
{
    public record TokenResult(string Token, DateTime ExpireAtUtc);
    public interface ITokenService
    {
        TokenResult CreateToken(User user);
    }
}

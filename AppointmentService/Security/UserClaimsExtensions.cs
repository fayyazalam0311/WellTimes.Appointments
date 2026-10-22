using System.Security.Claims;
using AppointmentService.Exceptions;

namespace AppointmentService.Security
{
    public static class UserClaimsExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(value, out var userId))
            {
                throw new InvalidTokenException();
            }

            return userId;
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            var name = user.FindFirst("name")?.Value;

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidTokenException();
            }

            return name;
        }
    }
}

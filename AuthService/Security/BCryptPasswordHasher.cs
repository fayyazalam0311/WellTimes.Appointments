
namespace AuthService.Security
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        public String Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

using AuthService.Models;

namespace AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task AddAsync(User user, CancellationToken cancellationToken = default);
    }
}

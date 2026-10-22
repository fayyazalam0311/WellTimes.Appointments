using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAccountService
    {
        Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    }
}

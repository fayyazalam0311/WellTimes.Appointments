using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAdminService
    {
        Task<UserResponse> CreateDoctorAsync(
            RegisterRequest request, 
            CancellationToken cancellationToken = default);

    }
}

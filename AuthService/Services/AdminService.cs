using AuthService.DTOs;
using AuthService.Exceptions;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Security;

namespace AuthService.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public AdminService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> CreateDoctorAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _userRepository.GetByEmailAsync(
                email, 
                cancellationToken) is not null)
            {
                throw new EmailAlreadyExistsException(email);
            }

            var doctor = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = Roles.Doctor
            };

            await _userRepository.AddAsync(doctor, cancellationToken);

            return new UserResponse
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                Email = doctor.Email,
                Role = doctor.Role
            };


        }

    }
}

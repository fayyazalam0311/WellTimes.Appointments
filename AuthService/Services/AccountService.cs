using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using AuthService.Exceptions;
using AuthService.Security;

namespace AuthService.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AccountService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            string NormalizedEmail = request.Email.Trim().ToLowerInvariant();

            User? ExistingUser = await _userRepository.GetByEmailAsync(NormalizedEmail, cancellationToken);
            
            if (ExistingUser is not null)
            {
                throw new EmailAlreadyExistsException(NormalizedEmail);
            }

            var user = new User
            {
                FullName = request.FullName,
                Email = NormalizedEmail,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = Roles.Patient
            };

            await _userRepository.AddAsync(user, cancellationToken);

            return new UserResponse
            {
                Id = user.Id,
                FullName= user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var token = _tokenService.CreateToken(user);

            return new LoginResponse
            {
                AccessToken = token.Token,
                ExpiresAtUtc = token.ExpireAtUtc,
                User = new UserResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role
                }
            };
        }
    }
}

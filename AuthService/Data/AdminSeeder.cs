using AuthService.Models;
using AuthService.Repositories;
using AuthService.Security;
using Microsoft.Extensions.Options;

namespace AuthService.Data
{
    public class AdminSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly SeedAdminSettings _settings;
        private readonly ILogger<AdminSeeder> _logger;
        public AdminSeeder(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IOptions<SeedAdminSettings> options,
            ILogger<AdminSeeder> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.Email)
                || string.IsNullOrWhiteSpace(_settings.Password))
            {
                _logger.LogInformation("No seed admin configured. Skipping.");
                return;
            }

            var email = _settings.Email.Trim().ToLowerInvariant();

            if (await _userRepository
                .GetByEmailAsync(email, cancellationToken) is not null)
            {
                return;
            }

            var admin = new User
            {
                FullName = _settings.FullName.Trim(),
                Email = email,
                PasswordHash = _passwordHasher.Hash(_settings.Password),
                Role = Roles.Admin
            };

            await _userRepository.AddAsync(admin, cancellationToken);
            _logger.LogInformation("Seed admin account created.");
        }

    }
}

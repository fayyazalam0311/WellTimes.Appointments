namespace AuthService.Models
{
    public class User
    {
        // Guid is used instead of int for the Id to ensure uniqueness across distributed systems and to avoid potential conflicts with auto-incrementing integers.
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        // Default role is set to Patient
        public string Role { get; set; } = Roles.Patient;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}

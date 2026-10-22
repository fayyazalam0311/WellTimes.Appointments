namespace AuthService.Security
{
    public class SeedAdminSettings
    {
        public const string SectionName = "SeedAdmin";

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

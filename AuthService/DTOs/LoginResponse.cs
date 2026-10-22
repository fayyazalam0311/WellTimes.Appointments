namespace AuthService.DTOs
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public UserResponse User { get; set; } = new();
    }
}

namespace BA.Server.Core.DTOs.Auth
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? Token { get; set; }
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
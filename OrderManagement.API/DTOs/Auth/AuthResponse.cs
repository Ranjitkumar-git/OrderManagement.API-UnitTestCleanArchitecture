namespace OrderManagement.API.DTOs.Authentication
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime ExpiresOn { get; set; }
    }
}
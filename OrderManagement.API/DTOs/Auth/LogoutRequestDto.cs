using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class LogoutRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class ForgotPasswordRequestDto
    {
       // [Required]
       // [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
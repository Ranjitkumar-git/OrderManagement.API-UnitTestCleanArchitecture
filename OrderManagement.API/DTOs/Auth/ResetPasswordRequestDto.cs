using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class ResetPasswordRequestDto
    {
       // [Required]
       // [EmailAddress]
        public string Email { get; set; } = string.Empty;

        //[Required]
        public string Token { get; set; } = string.Empty;

       // [Required]
        public string NewPassword { get; set; } = string.Empty;

        //[Required]
        //[Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
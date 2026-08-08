using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class ChangePasswordRequestDto
    {
       // [Required]
        public string CurrentPassword { get; set; } = string.Empty;

       // [Required]
        public string NewPassword { get; set; } = string.Empty;

       // [Required]
       // [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
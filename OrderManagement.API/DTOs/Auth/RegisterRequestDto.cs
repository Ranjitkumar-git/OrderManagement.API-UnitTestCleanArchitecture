using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class RegisterRequestDto
    {
        // [Required] // comment due to implent validation in validator class
        public string FirstName { get; set; } = string.Empty;

       // [Required]
        public string LastName { get; set; } = string.Empty;

       // [Required]
        //[EmailAddress]
        public string Email { get; set; } = string.Empty;

        //[Required]
        public string Password { get; set; } = string.Empty;

       // [Required]
       // [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
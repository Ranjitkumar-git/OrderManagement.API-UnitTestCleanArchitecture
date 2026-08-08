using System.ComponentModel.DataAnnotations;

namespace OrderManagement.API.DTOs.Authentication
{
    public class LoginRequestDto
    {
       // [Required]  // comment beacuse i used fluent validation
        //[EmailAddress]
        public string Email { get; set; } = string.Empty;

       // [Required]
        public string Password { get; set; } = string.Empty;
    }
}
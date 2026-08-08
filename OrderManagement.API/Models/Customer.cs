using System.ComponentModel.DataAnnotations;
namespace OrderManagement.API.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public List<Order> Orders { get; set; } = new();
    }
}

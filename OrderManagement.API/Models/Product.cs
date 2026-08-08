using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderManagement.API.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]

        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; }

        public int Stock { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}

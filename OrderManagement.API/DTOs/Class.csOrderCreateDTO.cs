using System.ComponentModel.DataAnnotations;
namespace OrderManagement.API.DTOs
{
    public class OrderCreateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than zero.")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Order items are required.")]
        [MinLength(1, ErrorMessage = "The order must contain at least one item.")]
        public List<OrderItemCreateDTO> Items { get; set; } = new();
    }
}

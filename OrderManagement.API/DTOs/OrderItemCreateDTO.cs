using System.ComponentModel.DataAnnotations;
namespace OrderManagement.API.DTOs
{
    public class OrderItemCreateDTO
    {
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}

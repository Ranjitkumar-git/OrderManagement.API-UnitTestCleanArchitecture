using OrderManagement.API.DTOs;
namespace OrderManagement.API.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO> CreateOrderAsync(OrderCreateDTO orderCreateDTO);
        Task<OrderResponseDTO?> GetOrderByIdAsync(int id);
    }
}

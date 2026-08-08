using OrderManagement.API.Models;
namespace OrderManagement.API.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task AddAsync(Order order);
    }
}

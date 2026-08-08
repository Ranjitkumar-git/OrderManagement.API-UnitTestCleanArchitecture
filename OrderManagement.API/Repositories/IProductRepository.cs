using OrderManagement.API.Models;
namespace OrderManagement.API.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
    }
}

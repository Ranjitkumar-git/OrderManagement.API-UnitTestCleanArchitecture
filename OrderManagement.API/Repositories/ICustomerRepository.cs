using OrderManagement.API.Models;
namespace OrderManagement.API.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
    }
}


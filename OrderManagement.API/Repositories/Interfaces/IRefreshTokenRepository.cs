using OrderManagement.API.Models.Identity;

namespace OrderManagement.API.Repositories.Interfaces
{
    public interface IRefreshTokenRepository
        : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);

        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

        Task RevokeAllUserTokensAsync(Guid userId);
    }
}
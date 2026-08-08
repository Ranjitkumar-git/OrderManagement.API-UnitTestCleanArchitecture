
using OrderManagement.API.Repositories.Interfaces;

namespace OrderManagement.API.UnitOfWork
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        IRefreshTokenRepository RefreshTokens { get; }

        IAuditLogRepository AuditLogs { get; }
        void Dispose();
    }
}


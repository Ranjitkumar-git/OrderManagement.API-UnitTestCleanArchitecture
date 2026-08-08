using OrderManagement.API.Data;
using OrderManagement.API.Repositories.Implementations;
using OrderManagement.API.Repositories.Interfaces;
namespace OrderManagement.API.UnitOfWork
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
       
        private readonly ApplicationDbContext _context;

        public IRefreshTokenRepository RefreshTokens { get; }
        public IAuditLogRepository AuditLogs { get; }

        public EfUnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            RefreshTokens = new RefreshTokenRepository(context);
            AuditLogs = new AuditLogRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public void Dispose()
        {
            _context.Dispose();
        }

    }
}

using OrderManagement.API.Data;
using OrderManagement.API.Models.Audit;
using OrderManagement.API.Repositories.Interfaces;

namespace OrderManagement.API.Repositories.Implementations
{
    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
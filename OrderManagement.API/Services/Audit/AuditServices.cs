using Microsoft.AspNetCore.Http;
using OrderManagement.API.Models.Audit;
using OrderManagement.API.UnitOfWork;

namespace OrderManagement.API.Services.Audit
{
    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TimeProvider _timeProvider;

        public AuditService(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            TimeProvider timeProvider)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _timeProvider = timeProvider;
        }

        public async Task LogAsync(
            Guid? userId,
            string? userName,
            string action,
            string module,
            string description)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                UserName = userName,
                Action = action,
                Module = module,
                Description = description,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request.Headers["User-Agent"].ToString(),
                CreatedOn = _timeProvider.GetUtcNow().UtcDateTime
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
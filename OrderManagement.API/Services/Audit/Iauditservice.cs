namespace OrderManagement.API.Services.Audit
{
    public interface IAuditService
    {
        Task LogAsync(
            Guid? userId,
            string? userName,
            string action,
            string module,
            string description);
    }
}
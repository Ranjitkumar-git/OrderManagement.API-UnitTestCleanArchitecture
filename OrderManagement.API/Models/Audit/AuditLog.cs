namespace OrderManagement.API.Models.Audit
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        public string? UserName { get; set; }

        public string Action { get; set; } = string.Empty;

        public string Module { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
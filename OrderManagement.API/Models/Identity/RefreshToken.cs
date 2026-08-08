
namespace OrderManagement.API.Models.Identity
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresOn { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedOn { get; set; }

        public bool IsRevoked { get; set; }

        // Foreign Key
        public Guid UserId { get; set; }

        // Navigation Property
        public ApplicationUser User { get; set; } = null!;
    }
}
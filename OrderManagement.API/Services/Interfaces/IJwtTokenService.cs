using OrderManagement.API.Models.Identity;

namespace OrderManagement.API.Services.Authentication
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessTokenAsync(ApplicationUser user);

        string GenerateRefreshToken();
    }
}
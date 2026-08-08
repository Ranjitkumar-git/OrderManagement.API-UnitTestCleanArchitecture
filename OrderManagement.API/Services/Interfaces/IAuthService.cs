using OrderManagement.API.DTOs.Auth;
using OrderManagement.API.DTOs.Authentication;
using OrderManagement.API.Helpers;

namespace OrderManagement.API.Services.Authentication
{
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user
        /// </summary>
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// Login user
        /// </summary>
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequestDto request);

        /// <summary>
        /// Generate new access token using refresh token
        /// </summary>
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// Logout user by revoking refresh token
        /// </summary>
        //Task<ApiResponse<string>> LogoutAsync(string refreshToken);
        Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request);

        /// <summary>
        /// Change current user's password
        /// </summary>
        Task<ApiResponse<string>> ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestDto request);


        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);

       Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.API.DTOs.Auth;
using OrderManagement.API.DTOs.Authentication;
using OrderManagement.API.Services.Authentication;
using System.Security.Claims;

namespace OrderManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register New User
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authService.RegisterAsync(request);

            return StatusCode(response.StatusCode, response);
        }
    

     [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);

            return StatusCode(response.StatusCode, response);
        }



        // Refresh Token
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        // Logout
        [HttpPost("logout")]
        //[Authorize]
        [AllowAnonymous] // Expired Access Token hone par bhi request block nahi hogi
        public async Task<IActionResult> Logout(LogoutRequestDto request)
        {
            var result = await _authService.LogoutAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        // Change Password
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)] // <-- Adds 401 to Swagger docs
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var result = await _authService.ChangePasswordAsync(
                Guid.Parse(userId),
                request);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Forgot Password
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordRequestDto request)
        {
            var result = await _authService.ForgotPasswordAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Reset Password
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordRequestDto request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            return StatusCode(result.StatusCode, result);
        }


    }

}
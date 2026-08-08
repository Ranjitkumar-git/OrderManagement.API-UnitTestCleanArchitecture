using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderManagement.API.Configurations;
using OrderManagement.API.Data;
using OrderManagement.API.DTOs.Auth;
using OrderManagement.API.DTOs.Authentication;
using OrderManagement.API.Helpers;
using OrderManagement.API.Models.Identity;
using OrderManagement.API.Services.Audit;
using OrderManagement.API.Services.Email;
using OrderManagement.API.UnitOfWork;

namespace OrderManagement.API.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        private readonly IJwtTokenService _jwtTokenService;

        //this line commen ted because we are using unit of work pattern so we don't need to use context directly
        //private readonly ApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtSettings _jwtSettings;

        private readonly TimeProvider _timeProvider;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditService _auditService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IJwtTokenService jwtTokenService,
           // ApplicationDbContext context,
           IUnitOfWork unitOfWork,
            IOptions<JwtSettings> jwtOptions,
            TimeProvider timeProvider, IEmailService emailService, ILogger<AuthService> logger
            , IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenService = jwtTokenService;
            // _context = context;
            _unitOfWork = unitOfWork; 
            _jwtSettings = jwtOptions.Value;
            _timeProvider = timeProvider;
            _emailService = emailService;
            _logger = logger;
             _auditService=auditService;
        }
        // old method
        //public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequestDto request)
        //{
        //    // 1. Check if email already exists
        //    var existingUser = await _userManager.FindByEmailAsync(request.Email);

        //    if (existingUser != null)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status400BadRequest,
        //            Message = "Email already exists.",
        //            Errors = new List<string>
        //    {
        //        "A user with this email already exists."
        //    }
        //        };
        //    }

        //    // 2. Create Application User
        //    var user = new ApplicationUser
        //    {
        //        Id = Guid.NewGuid(),
        //        FirstName = request.FirstName,
        //        LastName = request.LastName,
        //        Email = request.Email,
        //        UserName = request.Email,
        //        EmailConfirmed = true,
        //        IsActive = true,
        //        CreatedOn = _timeProvider.GetUtcNow().UtcDateTime
        //    };

        //    // 3. Save User
        //    var createUserResult = await _userManager.CreateAsync(user, request.Password);

        //    if (!createUserResult.Succeeded)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status400BadRequest,
        //            Message = "User registration failed.",
        //            Errors = createUserResult.Errors
        //                                     .Select(x => x.Description)
        //                                     .ToList()
        //        };
        //    }

        //    // 4. Assign Default Role
        //    var roleResult = await _userManager.AddToRoleAsync(user, "Employee");

        //    if (!roleResult.Succeeded)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status500InternalServerError,
        //            Message = "User created but role assignment failed.",
        //            Errors = roleResult.Errors
        //                               .Select(x => x.Description)
        //                               .ToList()
        //        };
        //    }

        //    // 5. Generate Tokens
        //    var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);

        //    var refreshToken = _jwtTokenService.GenerateRefreshToken();

        //    // 6. Save Refresh Token
        //    var refreshTokenEntity = new RefreshToken
        //    {
        //        Id = Guid.NewGuid(),
        //        Token = refreshToken,
        //        UserId = user.Id,
        //        CreatedOn = _timeProvider.GetUtcNow().UtcDateTime,
        //        ExpiresOn = _timeProvider
        //                        .GetUtcNow()
        //                        .UtcDateTime
        //                        .AddDays(_jwtSettings.RefreshTokenExpirationDays),
        //        IsRevoked = false
        //    };

        //    _context.RefreshTokens.Add(refreshTokenEntity);

        //    await _context.SaveChangesAsync();

        //    // 7. Prepare Response
        //    var authResponse = new AuthResponse
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken,
        //        ExpiresOn = _timeProvider
        //                        .GetUtcNow()
        //                        .UtcDateTime
        //                        .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        //    };

        //    // 8. Return Success
        //    return new ApiResponse<AuthResponse>
        //    {
        //        Success = true,
        //        StatusCode = StatusCodes.Status201Created,
        //        Message = "User registered successfully.",
        //        Data = authResponse
        //    };
        //}
        // new modified method with private method to avoid duplicate code


        // old code for login method
        //public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequestDto request
        // )
        //{
        //    // Find User
        //    var user = await _userManager.FindByEmailAsync(request.Email);

        //    if (user == null)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status401Unauthorized,
        //            Message = "Invalid email or password."
        //        };
        //    }

        //    // Check User Active
        //    if (!user.IsActive)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status403Forbidden,
        //            Message = "Your account is inactive. Please contact administrator."
        //        };
        //    }

        //    // Email Confirmed
        //    if (!user.EmailConfirmed)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status403Forbidden,
        //            Message = "Email is not confirmed."
        //        };
        //    }

        //    // Verify Password
        //    var signInResult = await _signInManager.CheckPasswordSignInAsync(
        //        user,
        //        request.Password,
        //        lockoutOnFailure: true);

        //    if (!signInResult.Succeeded)
        //    {
        //        return new ApiResponse<AuthResponse>
        //        {
        //            Success = false,
        //            StatusCode = StatusCodes.Status401Unauthorized,
        //            Message = "Invalid email or password."
        //        };
        //    }

        //    // Generate Access Token
        //    var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);

        //    // Generate Refresh Token
        //    var refreshToken = _jwtTokenService.GenerateRefreshToken();

        //    // Revoke Old Refresh Tokens
        //    var oldTokens = _context.RefreshTokens
        //        .Where(x => x.UserId == user.Id &&
        //                    !x.IsRevoked &&
        //                    x.ExpiresOn > _timeProvider.GetUtcNow().UtcDateTime)
        //        .ToList();

        //    foreach (var token in oldTokens)
        //    {
        //        token.IsRevoked = true;
        //        token.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;
        //    }

        //    // Save New Refresh Token
        //    var refreshTokenEntity = new RefreshToken
        //    {
        //        Id = Guid.NewGuid(),
        //        Token = refreshToken,
        //        UserId = user.Id,
        //        CreatedOn = _timeProvider.GetUtcNow().UtcDateTime,
        //        ExpiresOn = _timeProvider
        //            .GetUtcNow()
        //            .UtcDateTime
        //            .AddDays(_jwtSettings.RefreshTokenExpirationDays),
        //        IsRevoked = false
        //    };

        //    _context.RefreshTokens.Add(refreshTokenEntity);

        //    await _context.SaveChangesAsync();

        //    var response = new AuthResponse
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = refreshToken,
        //        ExpiresOn = _timeProvider
        //            .GetUtcNow()
        //            .UtcDateTime
        //            .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
        //    };

        //    return new ApiResponse<AuthResponse>
        //    {
        //        Success = true,
        //        StatusCode = StatusCodes.Status200OK,
        //        Message = "Login successful.",
        //        Data = response
        //    };
        //}

        // new modified login method using private method to avoid duplicate code

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Email already exists.",
                    Errors = new List<string>
            {
                "A user with this email already exists."
            }
                };
            }

            // 2. Create Application User
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                EmailConfirmed = true,
                IsActive = true,
                CreatedOn = _timeProvider.GetUtcNow().UtcDateTime
            };

            // 3. Create User
            var createUserResult = await _userManager.CreateAsync(user, request.Password);

            if (!createUserResult.Succeeded)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "User registration failed.",
                    Errors = createUserResult.Errors
                                             .Select(x => x.Description)
                                             .ToList()
                };
            }

            // 4. Assign Default Role
            var roleResult = await _userManager.AddToRoleAsync(user, "Employee");

            if (!roleResult.Succeeded)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Message = "User created but role assignment failed.",
                    Errors = roleResult.Errors
                                       .Select(x => x.Description)
                                       .ToList()
                };
            }

         
            // 5. Generate Access Token & Refresh Token
            var authResponse = await GenerateTokensAsync(user);

            await _auditService.LogAsync(
 user.Id,
 user.Email,
 "Register",
 "Authentication",
 "New user registered successfully.");
            // 6. Return Success
            return new ApiResponse<AuthResponse>
            {
                Success = true,
                StatusCode = StatusCodes.Status201Created,
                Message = "User registered successfully.",
                Data = authResponse
            };
        }
        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequestDto request)
        {
            // Find User
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid email or password."
                };
            }

            // Check User Active
            if (!user.IsActive)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Your account is inactive. Please contact administrator."
                };
            }

            // Email Confirmed
            if (!user.EmailConfirmed)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "Email is not confirmed."
                };
            }

            // Verify Password
            var signInResult = await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

            if (!signInResult.Succeeded)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid email or password."
                };
            }

            // Generate Tokens
            var response = await GenerateTokensAsync( user);
            // Audit Log  <-- YAHAN ADD KARNA HAI
            await _auditService.LogAsync(
                user.Id,
                user.Email,
                "Login",
                "Authentication",
                "User logged in successfully.");
            return new ApiResponse<AuthResponse>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Login successful.",
                Data = response
            };
        }
        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1. Find Refresh Token
            //var storedToken = await _context.RefreshTokens
            //    .Include(x => x.User)
            //    .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            var storedToken = await _unitOfWork
            .RefreshTokens
            .GetByTokenAsync(request.RefreshToken);
            if (storedToken == null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Invalid refresh token."
                };
            }

            // 2. Check Revoked
            if (storedToken.IsRevoked)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Refresh token has been revoked."
                };
            }

            // 3. Check Expiry
            if (storedToken.ExpiresOn <= _timeProvider.GetUtcNow().UtcDateTime)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Refresh token has expired."
                };
            }

            // 4. Check User
            var user = storedToken.User;

            if (user == null)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found."
                };
            }

            if (!user.IsActive)
            {
                return new ApiResponse<AuthResponse>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status403Forbidden,
                    Message = "User account is inactive."
                };
            }

            // 5. Revoke Current Refresh Token
            storedToken.IsRevoked = true;
            storedToken.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;

            // await _context.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();
            // 6. Generate New Tokens
            var authResponse = await GenerateTokensAsync(user);

            // 7. Return Response
            return new ApiResponse<AuthResponse>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Token refreshed successfully.",
                Data = authResponse
            };
        }
        public async Task<ApiResponse<string>> LogoutAsync(LogoutRequestDto request)
        {
            // 1. Find Refresh Token
            //var storedToken = await _context.RefreshTokens
            //    .FirstOrDefaultAsync(x => x.Token == request.RefreshToken);

            var storedToken = await _unitOfWork
    .RefreshTokens
    .GetByTokenAsync(request.RefreshToken);
            if (storedToken == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Refresh token not found."
                };
            }

            // 2. Already Revoked?
            if (storedToken.IsRevoked)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "User already logged out."
                };
            }

            // 3. Revoke Token
            storedToken.IsRevoked = true;
            storedToken.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;

            await _auditService.LogAsync(
    storedToken.UserId,
    storedToken.User?.Email,
    "Logout",
    "Authentication",
    "User logged out successfully.");
            // await _context.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();
            // 4. Return Success
            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Logout successful.",
                Data = "User logged out successfully."
            };
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(
      Guid userId,
      ChangePasswordRequestDto request)
        {
            // 1. Find User
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found."
                };
            }

            // 2. Change Password
            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Password change failed.",
                    Errors = result.Errors
                                   .Select(x => x.Description)
                                   .ToList()
                };
            }
            await _auditService.LogAsync(
    user.Id,
    user.Email,
    "Change Password",
    "Authentication",
    "Password changed successfully.");
            // 3. Revoke All Active Refresh Tokens
            //var refreshTokens = _context.RefreshTokens
            //    .Where(x => x.UserId == user.Id && !x.IsRevoked)
            //    .ToList();
            var refreshTokens = await _unitOfWork
    .RefreshTokens
    .GetActiveTokensByUserIdAsync(user.Id);

            foreach (var token in refreshTokens)
            {
                token.IsRevoked = true;
                token.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;
            }

            // await _context.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();
            // 4. Success Response
            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password changed successfully.",
                Data = "Please login again."
            };
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            // Security: Email exists hai ya nahi, same response dena
            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "If the email exists, a password reset link has been sent."
                };
            }

            // Generate Reset Token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // URL Encode Token
            token = Uri.EscapeDataString(token);

            // Angular Reset URL
            var resetLink =
                $"https://localhost:4200/reset-password?email={user.Email}&token={token}";

            var body = $@"
        <h2>Password Reset</h2>
        <p>Hello {user.FirstName},</p>

        <p>Please click the link below to reset your password.</p>

        <a href='{resetLink}'>Reset Password</a>

        <br/><br/>

        <p>If you did not request this, please ignore this email.</p>";

            await _emailService.SendEmailAsync(
                user.Email!,
                "Reset Password",
                body);
            await _auditService.LogAsync(
    user?.Id,
    user?.Email,
    "Forgot Password",
    "Authentication",
    "Password reset email sent.");

            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "If the email exists, a password reset link has been sent."
            };
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(
    ResetPasswordRequestDto request)
        {
            // 1. Find User
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found."
                };
            }

            // 2. Decode Token
            var token = Uri.UnescapeDataString(request.Token);

            // 3. Reset Password
            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                request.NewPassword);

            if (!result.Succeeded)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Password reset failed.",
                    Errors = result.Errors
                                   .Select(x => x.Description)
                                   .ToList()
                };
            }

            await _auditService.LogAsync(
    user.Id,
    user.Email,
    "Reset Password",
    "Authentication",
    "Password reset successfully.");
            // 4. Revoke All Refresh Tokens
            //var refreshTokens = _context.RefreshTokens
            //    .Where(x => x.UserId == user.Id &&
            //                !x.IsRevoked)
            //    .ToList();
            var refreshTokens = await _unitOfWork
    .RefreshTokens
    .GetActiveTokensByUserIdAsync(user.Id);

            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;
            }

            //await _context.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();
            // 5. Return Success
            return new ApiResponse<string>
            {
                Success = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Password reset successfully."
            };
        }

        // ye private method banan hai because not duplicate code 
        private async Task<AuthResponse> GenerateTokensAsync(ApplicationUser user)
        {
            // Generate Access Token
            var accessToken = await _jwtTokenService.GenerateAccessTokenAsync(user);

            // Generate Refresh Token
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            // Revoke Old Active Refresh Tokens
            //var oldTokens = _context.RefreshTokens
            //    .Where(x => x.UserId == user.Id &&
            //                !x.IsRevoked &&
            //                x.ExpiresOn > _timeProvider.GetUtcNow().UtcDateTime)
            //    .ToList();
            var oldTokens = await _unitOfWork
    .RefreshTokens
    .GetActiveTokensByUserIdAsync(user.Id);

            foreach (var token in oldTokens)
            {
                token.IsRevoked = true;
                token.RevokedOn = _timeProvider.GetUtcNow().UtcDateTime;
            }

            // Save New Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = refreshToken,
                UserId = user.Id,
                CreatedOn = _timeProvider.GetUtcNow().UtcDateTime,
                ExpiresOn = _timeProvider
                    .GetUtcNow()
                    .UtcDateTime
                    .AddDays(_jwtSettings.RefreshTokenExpirationDays),
                IsRevoked = false
            };

            //_context.RefreshTokens.Add(refreshTokenEntity);
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            // await _context.SaveChangesAsync();
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresOn = _timeProvider
                    .GetUtcNow()
                    .UtcDateTime
                    .AddMinutes(_jwtSettings.AccessTokenExpirationMinutes)
            };
        }
    }
}
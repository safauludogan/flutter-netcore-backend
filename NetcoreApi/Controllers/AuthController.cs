using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetcoreApi.Dto;
using NetcoreApi.Models;
using NetcoreApi.Services.Abstract;
using System.Security.Claims;

namespace NetcoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            IAuthService authService,
            IRefreshTokenService refreshTokenService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(
                    "Invalid email or password", 401));
            }

            // Generate tokens
            var accessToken = _authService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            var response = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = 3600, // 60 minutes
                User = user
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(
                response,
                "Login successful"));
        }

        // POST: api/auth/refresh
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken(
            RefreshTokenRequest request)
        {
            _logger.LogInformation("Token refresh attempt");

            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<RefreshTokenResponse>.ErrorResponse(
                    "Refresh token is required", 400));
            }

            // Get refresh token from database
            var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);

            if (refreshToken == null)
            {
                return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse(
                    "Invalid refresh token", 401));
            }

            if (!refreshToken.IsActive)
            {
                var reason = refreshToken.IsRevoked
                    ? $"Token revoked: {refreshToken.RevokedReason}"
                    : "Token expired";

                return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse(
                    reason, 401));
            }

            // Validate user still exists and is active
            var user = refreshToken.User;
            if (user == null || !user.IsActive)
            {
                await _refreshTokenService.RevokeRefreshTokenAsync(
                    request.RefreshToken,
                    "User no longer active");

                return Unauthorized(ApiResponse<RefreshTokenResponse>.ErrorResponse(
                    "User no longer active", 401));
            }

            // Generate new tokens
            var newAccessToken = _authService.GenerateAccessToken(user);
            var newRefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

            // Revoke old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = "Replaced by new token";
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            await _context.SaveChangesAsync();

            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = 3600
            };

            _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(
                response,
                "Token refreshed successfully"));
        }

        // POST: api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout([FromBody] LogoutRequest? request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(
                    "User not authenticated", 401));
            }

            _logger.LogInformation("Logout request for user: {UserId}", userId);

            // Revoke all user's refresh tokens
            await _refreshTokenService.RevokeAllUserTokensAsync(Guid.Parse(userId));

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Logout successful"));
        }

        // GET: api/auth/me - Get current user (requires authentication)
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<User>>> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(ApiResponse<User>.ErrorResponse(
                    "User not authenticated", 401));
            }

            var user = await _context.Users.FindAsync(Guid.Parse(userId));

            if (user == null || !user.IsActive)
            {
                return NotFound(ApiResponse<User>.ErrorResponse(
                    "User not found", 404));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }

        // POST: api/auth/revoke-token - Revoke specific refresh token
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<ActionResult<ApiResponse<bool>>> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Refresh token is required", 400));
            }

            var token = await _refreshTokenService.GetRefreshTokenAsync(request.RefreshToken);

            if (token == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse(
                    "Token not found", 404));
            }

            // Verify token belongs to current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (token.UserId.ToString() != userId)
            {
                return Forbid();
            }

            await _refreshTokenService.RevokeRefreshTokenAsync(
                request.RefreshToken,
                "Revoked by user");

            return Ok(ApiResponse<bool>.SuccessResponse(
                true,
                "Token revoked successfully"));
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetcoreApi.Dto;
using NetcoreApi.Models;
using NetcoreApi.Services.Abstract;

namespace NetcoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginReq request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse(
                    "Invalid email or password", 401));
            }

            var token = _authService.GenerateToken(user);
            var refreshToken = _authService.GenerateRefreshToken();

            var response = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600,
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

            // Validate refresh token (simplified - in real app, store refresh tokens in DB)
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(ApiResponse<RefreshTokenResponse>.ErrorResponse(
                    "Invalid refresh token", 400));
            }

            // Generate new tokens
            var newAccessToken = _authService.GenerateToken(new User
            {
                Id = Guid.NewGuid(),
                Email = "user@example.com"
            });
            var newRefreshToken = _authService.GenerateRefreshToken();

            var response = new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            };

            return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(response));
        }

        // POST: api/auth/logout
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            _logger.LogInformation("Logout");

            // In real app, invalidate refresh token

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Logout successful"));
        }
    }
}

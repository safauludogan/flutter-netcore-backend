using Microsoft.EntityFrameworkCore;
using NetcoreApi.Models;
using NetcoreApi.Services.Abstract;

namespace NetcoreApi.Services.Concrete
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RefreshTokenService> _logger;

        public RefreshTokenService(AppDbContext context, ILogger<RefreshTokenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId)
        {
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = GenerateRefreshToken(),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days validity
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created refresh token for user: {UserId}", userId);

            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeRefreshTokenAsync(string token, string reason)
        {
            var refreshToken = await GetRefreshTokenAsync(token);

            if (refreshToken == null) return;

            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = reason;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked refresh token: {Token}, Reason: {Reason}",
                token.Substring(0, 10) + "...", reason);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedReason = "User logout - all tokens revoked";
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Revoked all tokens for user: {UserId}", userId);
        }

        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired tokens", expiredTokens.Count);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}

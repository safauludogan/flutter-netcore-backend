using NetcoreApi.Models;

namespace NetcoreApi.Services.Abstract
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CreateRefreshTokenAsync(Guid userId);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string reason);
        Task RevokeAllUserTokensAsync(Guid userId);
        Task CleanupExpiredTokensAsync();
    }
}

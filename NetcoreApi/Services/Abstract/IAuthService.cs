using NetcoreApi.Models;

namespace NetcoreApi.Services.Abstract
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken();
    }
}

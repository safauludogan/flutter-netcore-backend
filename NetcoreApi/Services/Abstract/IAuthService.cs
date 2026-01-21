using NetcoreApi.Models;

namespace NetcoreApi.Services.Abstract
{
    public interface IAuthService
    {
        string GenerateAccessToken(User user);

    }
}

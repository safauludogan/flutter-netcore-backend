using NetcoreApi.Models;

namespace NetcoreApi.Dto
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public User? User { get; set; }
    }
}

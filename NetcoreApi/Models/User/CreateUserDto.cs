using System.ComponentModel.DataAnnotations;

namespace NetcoreApi.Models
{
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

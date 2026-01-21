using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetcoreApi.Models;
using System.Security.Claims;

namespace NetcoreApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase // [Authorize]'ı KALDIR - endpoint bazlı yap
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AppDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/users - Returns ApiResponse<List<User>>
        // ✅ AUTH GEREKMİYOR - Public endpoint
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<User>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            _logger.LogInformation("Getting users - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            var query = _context.Users.Where(u => u.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = ApiResponse<List<User>>.SuccessResponse(users);
            response.Metadata = new Dictionary<string, object>
            {
                { "totalCount", totalCount },
                { "page", page },
                { "pageSize", pageSize },
                { "totalPages", (int)Math.Ceiling(totalCount / (double)pageSize) }
            };

            return Ok(response);
        }

        // GET: api/users/{id} - Returns ApiResponse<User>
        // ✅ AUTH GEREKMİYOR - Public endpoint
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<User>>> GetUser(Guid id)
        {
            _logger.LogInformation("Getting user by ID: {UserId}", id);

            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found", 404));
            }

            return Ok(ApiResponse<User>.SuccessResponse(user));
        }

        // GET: api/users/{id}/simple - Returns User directly (no wrapper)
        [AllowAnonymous]
        [HttpGet("{id}/simple")]
        public async Task<ActionResult<User>> GetUserSimple(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/users/{id}/name - Returns primitive string
        [AllowAnonymous]
        [HttpGet("{id}/name")]
        public async Task<ActionResult<string>> GetUserName(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound("User not found");
            }

            return Ok(user.Name);
        }

        // GET: api/users/count - Returns primitive int
        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var count = await _context.Users.CountAsync(u => u.IsActive);
            return Ok(count);
        }

        // GET: api/users/{id}/exists - Returns primitive bool
        [AllowAnonymous]
        [HttpGet("{id}/exists")]
        public async Task<ActionResult<bool>> UserExists(Guid id)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
            return Ok(exists);
        }

        // GET: api/users/{id}/info - Returns Map<string, dynamic>
        [AllowAnonymous]
        [HttpGet("{id}/info")]
        public async Task<ActionResult<Dictionary<string, object>>> GetUserInfo(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound();
            }

            var info = new Dictionary<string, object>
            {
                { "id", user.Id },
                { "name", user.Name },
                { "email", user.Email },
                { "accountAge", (DateTime.UtcNow - user.CreatedAt).Days },
                { "isActive", user.IsActive }
            };

            return Ok(info);
        }

        // POST: api/users - Returns ApiResponse<User>
        // ✅ AUTH GEREKMİYOR - Registration endpoint
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<User>>> CreateUser(CreateUserDto dto)
        {
            _logger.LogInformation("Creating new user: {Email}", dto.Email);

            // Validation
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest(ApiResponse<User>.ErrorResponse("Email already exists", 400));
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password, // In real app, hash this!
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                ApiResponse<User>.SuccessResponse(user, "User created successfully"));
        }

        // PUT: api/users/{id} - Returns ApiResponse<User>
        // ✅ AUTH GEREKİYOR - Own profile update
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<User>>> UpdateUser(Guid id, UpdateUserDto dto)
        {
            _logger.LogInformation("Updating user: {UserId}", id);

            // Verify user can only update their own profile
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != id.ToString())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null || !user.IsActive)
            {
                return NotFound(ApiResponse<User>.ErrorResponse("User not found", 404));
            }

            if (!string.IsNullOrEmpty(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                {
                    return BadRequest(ApiResponse<User>.ErrorResponse("Email already exists", 400));
                }
                user.Email = dto.Email;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<User>.SuccessResponse(user, "User updated successfully"));
        }

        // PATCH: api/users/{id}/activate - Returns void (204 No Content)
        [AllowAnonymous]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/users/{id} - Soft delete
        // ✅ AUTH GEREKİYOR - Own account delete
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
        {
            _logger.LogInformation("Deleting user: {UserId}", id);

            // Verify user can only delete their own account
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId != id.ToString())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("User not found", 404));
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully"));
        }
    }
}
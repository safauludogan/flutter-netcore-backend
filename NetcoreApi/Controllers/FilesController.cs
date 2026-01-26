using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetcoreApi.Models;
using NetcoreApi.Services.Abstract;
using System.Security.Claims;

namespace NetcoreApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(
            AppDbContext context,
            IFileService fileService,
            ILogger<FilesController> logger)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Gets the current user's ID from JWT claims
        /// </summary>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token");
            }
            return userId;
        }

        // GET: api/files - List current user's files
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FileMetadata>>>> GetFiles()
        {
            var userId = GetCurrentUserId();

            var files = await _context.FileMetadata
                .Where(f => f.UploadedByUserId == userId)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();

            return Ok(ApiResponse<List<FileMetadata>>.SuccessResponse(files));
        }

        // GET: api/files/{id} - Get file metadata (only if it belongs to current user)
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FileMetadata>>> GetFileMetadata(Guid id)
        {
            var userId = GetCurrentUserId();

            var file = await _context.FileMetadata.FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound(ApiResponse<FileMetadata>.ErrorResponse("File not found", 404));
            }

            // Verify file belongs to current user
            if (file.UploadedByUserId != userId)
            {
                return Forbid();
            }

            return Ok(ApiResponse<FileMetadata>.SuccessResponse(file));
        }

        // POST: api/files/upload - Upload single file
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<FileMetadata>>> UploadFile(IFormFile file)
        {
            var userId = GetCurrentUserId();

            _logger.LogInformation("User {UserId} uploading file: {FileName}", userId, file.FileName);

            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<FileMetadata>.ErrorResponse("No file provided", 400));
            }

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<FileMetadata>.ErrorResponse("File too large (max 10MB)", 400));
            }

            try
            {
                // Save file with user ID (now required)
                var savedFile = await _fileService.SaveFileAsync(file, userId);

                var metadata = new FileMetadata
                {
                    Id = Guid.NewGuid(),
                    FileName = savedFile.FileName,
                    OriginalFileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    FilePath = savedFile.FilePath,
                    UploadedAt = DateTime.UtcNow,
                    UploadedByUserId = userId
                };

                _context.FileMetadata.Add(metadata);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<FileMetadata>.SuccessResponse(
                    metadata,
                    "File uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument uploading file");
                return BadRequest(ApiResponse<FileMetadata>.ErrorResponse(ex.Message, 400));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User not found during file upload");
                return BadRequest(ApiResponse<FileMetadata>.ErrorResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, ApiResponse<FileMetadata>.ErrorResponse(
                    "Failed to upload file", 500));
            }
        }

        // POST: api/files/upload-multiple - Upload multiple files
        [HttpPost("upload-multiple")]
        public async Task<ActionResult<ApiResponse<List<FileMetadata>>>> UploadMultipleFiles(
            [FromForm] List<IFormFile> files)
        {
            var userId = GetCurrentUserId();

            _logger.LogInformation("User {UserId} uploading {Count} files", userId, files?.Count ?? 0);

            if (files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<List<FileMetadata>>.ErrorResponse(
                    "No files provided", 400));
            }

            var uploadedFiles = new List<FileMetadata>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    errors.Add($"{file.FileName}: Empty file");
                    continue;
                }

                // Validate file size
                if (file.Length > 10 * 1024 * 1024)
                {
                    errors.Add($"{file.FileName}: File too large (max 10MB)");
                    continue;
                }

                try
                {
                    // Save file with user ID
                    var savedFile = await _fileService.SaveFileAsync(file, userId);

                    var metadata = new FileMetadata
                    {
                        Id = Guid.NewGuid(),
                        FileName = savedFile.FileName,
                        OriginalFileName = file.FileName,
                        ContentType = file.ContentType,
                        Size = file.Length,
                        FilePath = savedFile.FilePath,
                        UploadedAt = DateTime.UtcNow,
                        UploadedByUserId = userId
                    };

                    _context.FileMetadata.Add(metadata);
                    uploadedFiles.Add(metadata);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                    errors.Add($"{file.FileName}: {ex.Message}");
                }
            }

            if (uploadedFiles.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var message = errors.Count > 0
                ? $"{uploadedFiles.Count} files uploaded successfully with {errors.Count} error(s)"
                : $"{uploadedFiles.Count} files uploaded successfully";

            return Ok(ApiResponse<List<FileMetadata>>.SuccessResponse(uploadedFiles, message));
        }

        // GET: api/files/{id}/download - Download file (only if it belongs to current user)
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var userId = GetCurrentUserId();

            var file = await _context.FileMetadata.FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound(ApiResponse<string>.ErrorResponse("File not found", 404));
            }

            // Verify file belongs to current user
            if (file.UploadedByUserId != userId)
            {
                return Forbid();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), file.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError("Physical file not found at path: {FilePath}", filePath);
                return NotFound(ApiResponse<string>.ErrorResponse("File not found on server", 404));
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.ContentType, file.OriginalFileName);
        }

        // DELETE: api/files/{id} - Delete file (only if it belongs to current user)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(Guid id)
        {
            var userId = GetCurrentUserId();

            var file = await _context.FileMetadata.FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("File not found", 404));
            }

            // Verify file belongs to current user
            if (file.UploadedByUserId != userId)
            {
                return Forbid();
            }

            try
            {
                // Delete physical file
                await _fileService.DeleteFileAsync(file.FilePath);

                // Delete metadata
                _context.FileMetadata.Remove(file);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} deleted file {FileId}", userId, id);

                return Ok(ApiResponse<bool>.SuccessResponse(true, "File deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {FileId}", id);
                return StatusCode(500, ApiResponse<bool>.ErrorResponse("Failed to delete file", 500));
            }
        }
    }
}

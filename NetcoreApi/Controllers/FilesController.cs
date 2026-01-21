using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetcoreApi.Models;
using NetcoreApi.Services.Abstract;

namespace NetcoreApi.Controllers
{
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

        // GET: api/files - List all uploaded files
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<FileMetadata>>>> GetFiles()
        {
            var files = await _context.FileMetadata
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();

            return Ok(ApiResponse<List<FileMetadata>>.SuccessResponse(files));
        }

        // GET: api/files/{id} - Get file metadata
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<FileMetadata>>> GetFileMetadata(Guid id)
        {
            var file = await _context.FileMetadata.FindAsync(id);

            if (file == null)
            {
                return NotFound(ApiResponse<FileMetadata>.ErrorResponse("File not found", 404));
            }

            return Ok(ApiResponse<FileMetadata>.SuccessResponse(file));
        }

        // POST: api/files/upload - Upload single file
        [HttpPost("upload")]
        public async Task<ActionResult<ApiResponse<FileMetadata>>> UploadFile(
            IFormFile file,
            [FromForm] Guid? userId = null)
        {
            _logger.LogInformation("Uploading file: {FileName}", file.FileName);

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
            [FromForm] List<IFormFile> files,
            [FromForm] Guid? userId = null)
        {
            _logger.LogInformation("Uploading {Count} files", files.Count);

            if (files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse<List<FileMetadata>>.ErrorResponse(
                    "No files provided", 400));
            }

            var uploadedFiles = new List<FileMetadata>();

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                try
                {
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
                }
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<List<FileMetadata>>.SuccessResponse(
                uploadedFiles,
                $"{uploadedFiles.Count} files uploaded successfully"));
        }

        // GET: api/files/{id}/download - Download file
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadFile(Guid id)
        {
            var file = await _context.FileMetadata.FindAsync(id);

            if (file == null)
            {
                return NotFound();
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), file.FilePath);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found on server");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.ContentType, file.OriginalFileName);
        }

        // DELETE: api/files/{id} - Delete file
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteFile(Guid id)
        {
            var file = await _context.FileMetadata.FindAsync(id);

            if (file == null)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("File not found", 404));
            }

            // Delete physical file
            await _fileService.DeleteFileAsync(file.FilePath);

            // Delete metadata
            _context.FileMetadata.Remove(file);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "File deleted successfully"));
        }
    }
}

using Microsoft.EntityFrameworkCore;
using NetcoreApi.Services.Abstract;

namespace NetcoreApi.Services.Concrete
{
    public class FileService : IFileService
    {
        private readonly string _uploadFolder = "uploads";
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public FileService(IWebHostEnvironment webHostEnvironment, AppDbContext context)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;

            // Ensure upload directory exists
            var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, _uploadFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
        }

        /// <summary>
        /// Saves a file for a specific user. UserId is required and enforced.
        /// </summary>
        public async Task<SavedFileInfo> SaveFileAsync(IFormFile file, Guid userId)
        {
            // Validate userId
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("UserId is required for file upload", nameof(userId));
            }

            // Verify user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Create user-specific folder
            var relativePath = Path.Combine(_uploadFolder, userId.ToString(), uniqueFileName);
            var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, relativePath);

            // Ensure directory exists
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // Save file
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new SavedFileInfo
            {
                FileName = uniqueFileName,
                FilePath = relativePath
            };
        }

        /// <summary>
        /// Deletes a file from the file system.
        /// </summary>
        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, filePath);
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }

        /// <summary>
        /// Validates if a file belongs to a specific user.
        /// </summary>
        public async Task<bool> ValidateUserFileAsync(Guid fileId, Guid userId)
        {
            var file = await _context.FileMetadata.FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return false;
            }

            // Check if file belongs to the user
            return file.UploadedByUserId == userId;
        }
    }
}

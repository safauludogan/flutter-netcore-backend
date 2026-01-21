using NetcoreApi.Services.Abstract;

namespace NetcoreApi.Services.Concrete
{
    public class FileService : IFileService
    {
        private readonly string _uploadFolder = "uploads";
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            
            // Ensure upload directory exists
            var uploadPath = Path.Combine(_webHostEnvironment.ContentRootPath, _uploadFolder);
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
        }

        public async Task<SavedFileInfo> SaveFileAsync(IFormFile file, Guid? userId = null)
        {
            // Generate unique filename
            var extension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Create user folder if userId provided
            var relativePath = userId.HasValue
                ? Path.Combine(_uploadFolder, userId.Value.ToString(), uniqueFileName)
                : Path.Combine(_uploadFolder, uniqueFileName);

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

        public async Task DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, filePath);

            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
            }
        }
    }
}

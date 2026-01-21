namespace NetcoreApi.Services.Abstract
{
    public interface IFileService
    {
        Task<SavedFileInfo> SaveFileAsync(IFormFile file, Guid? userId = null);
        Task DeleteFileAsync(string filePath);
    }

    public class SavedFileInfo
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }
}

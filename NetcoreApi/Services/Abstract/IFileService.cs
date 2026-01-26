namespace NetcoreApi.Services.Abstract
{
    public interface IFileService
    {
        Task<SavedFileInfo> SaveFileAsync(IFormFile file, Guid userId);
        Task DeleteFileAsync(string filePath);
        Task<bool> ValidateUserFileAsync(Guid fileId, Guid userId);
    }

    public class SavedFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }
}

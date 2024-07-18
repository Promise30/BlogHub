using CloudinaryDotNet.Actions;

namespace BloggingAPI.Services.Interface
{
    public interface ICloudinaryService
    {
        Task<DeletionResult> DeleteImageAsync(string publicId);
        Task<ImageUploadResult> UploadImage(IFormFile file);
    }
}
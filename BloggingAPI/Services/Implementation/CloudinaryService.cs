using BloggingAPI.Services.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BloggingAPI.Services.Implementation
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public CloudinaryService(ILogger<CloudinaryService> logger, IConfiguration configuration)
        {

            _logger = logger;
            _configuration = configuration;
            Account account = new(
                _configuration["CloudinarySettings:CloudName"],
                _configuration["CloudinarySettings:ApiKey"],
                _configuration["CloudinarySettings:ApiSecret"]
                );
            _cloudinary = new Cloudinary(account);
        }
        public async Task<ImageUploadResult> UploadImage(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream)
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    _logger.Log(LogLevel.Information, $"Image Upload Response: {uploadResult.JsonObj}");
                }

            }
            return uploadResult;
        }
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                var result = await _cloudinary.DestroyAsync(deletionParams);

                //_logger.Log(LogLevel.Information, $"Status of deletion operation: {result.JsonObj}");
                //return result;
                if(result.Result != null)
                {
                    _logger.Log(LogLevel.Information, "Image with publicId {publicId} deleted successfully", publicId);
                }
                _logger.Log(LogLevel.Warning, $"Failed to delete image with publicId {publicId}. Deletion status: {result.Result}");
                return result;
            }
            catch (Exception ex) 
            {
                _logger.Log(LogLevel.Error, $"Error occurred while deleting image with publicId {publicId}: {ex.Message}");
                throw;
            }
        }
    }
}

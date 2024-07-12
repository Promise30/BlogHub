using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Infrastructure.Validations
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;
        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"File size must not exceed {_maxFileSize / 1024 / 1024} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}

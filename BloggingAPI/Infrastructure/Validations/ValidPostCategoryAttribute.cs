using BloggingAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Infrastructure.Validations
{
    public class ValidPostCategoryAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is string categoryString)
            {
                if (!Enum.TryParse(categoryString, true, out PostCategory result))
                {
                    return new ValidationResult($"Invalid post category type: {categoryString}");
                }
                validationContext.ObjectInstance.GetType().GetProperty(validationContext.MemberName)?.SetValue(validationContext.ObjectInstance, result.ToString());
            }

            return ValidationResult.Success;
        }
    }
}

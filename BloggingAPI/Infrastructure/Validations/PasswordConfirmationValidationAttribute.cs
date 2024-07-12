using System.ComponentModel.DataAnnotations;

namespace BloggingAPI.Infrastructure.Validations
{
    public class PasswordConfirmationValidationAttribute : ValidationAttribute
    {
        private readonly string _otherPropertyName;
        public PasswordConfirmationValidationAttribute(string otherPropertyName)
        {
            _otherPropertyName = otherPropertyName;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return new ValidationResult(ErrorMessage);
            var otherPropertyValue = validationContext.ObjectInstance.GetType().GetProperty(_otherPropertyName)?.GetValue(validationContext.ObjectInstance, null);

            if (otherPropertyValue != null && !value.Equals(otherPropertyValue))
            {
                return new ValidationResult(ErrorMessage);
            }
            return ValidationResult.Success;
        }

    }
}

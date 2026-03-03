using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VehicleRent.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    /// <summary>
    /// Represents the LicensePlateFormatAttribute component.
    /// </summary>
    public class LicensePlateFormatAttribute : ValidationAttribute
    {
        // Allowed formats:
        // 11-AA-11, AA-11-11, 11-11-AA, AA-11-AA
        private static readonly Regex Pattern = new(
            "^(?:\\d{2}-[A-Z]{2}-\\d{2}|[A-Z]{2}-\\d{2}-\\d{2}|\\d{2}-\\d{2}-[A-Z]{2}|[A-Z]{2}-\\d{2}-[A-Z]{2})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Initializes a new instance of the <see cref="LicensePlateFormatAttribute"/> class.
        /// </summary>
        public LicensePlateFormatAttribute() : base("License plate format is invalid.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success;

            var text = value.ToString()?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return ValidationResult.Success;

            return Pattern.IsMatch(text)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage);
        }
    }
}

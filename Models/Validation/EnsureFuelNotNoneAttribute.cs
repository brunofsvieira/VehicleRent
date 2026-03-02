using System;
using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Enumerators;

namespace VehicleRent.Models.Validation
{
    /// <summary>
    /// Specifies a validation attribute that ensures a fuel type property or field is not set to 'None' or an empty
    /// value.
    /// </summary>
    /// <remarks>Apply this attribute to properties or fields representing a fuel type to require that a
    /// valid, non-default value is selected. The attribute supports both enum values and string representations,
    /// returning a validation error if the value is 'None', an empty string, or null. This is commonly used in data
    /// models to enforce selection of a valid fuel type in forms or APIs.</remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EnsureFuelNotNoneAttribute : ValidationAttribute
    {
        public EnsureFuelNotNoneAttribute()
            : base("Selecione um tipo de combustível válido.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is FuelType ft)
            {
                return ft == FuelType.None ? new ValidationResult(ErrorMessage!) : ValidationResult.Success;
            }

            // If the incoming value is a string (e.g. from model binding of a select), accept non-empty and not "None"
            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s) || s.Equals(FuelType.None.ToString(), StringComparison.OrdinalIgnoreCase))
                    return new ValidationResult(ErrorMessage!);
                return ValidationResult.Success;
            }

            if (value is null)
                return new ValidationResult(ErrorMessage!);

            return ValidationResult.Success;
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Enumerators;

namespace VehicleRent.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    /// <summary>
    /// Represents the EnsureFuelNotNoneAttribute component.
    /// </summary>
    public class EnsureFuelNotNoneAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureFuelNotNoneAttribute"/> class.
        /// </summary>
        public EnsureFuelNotNoneAttribute()
            : base("Select a valid fuel type.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is FuelType ft)
            {
                return ft == FuelType.None ? new ValidationResult(ErrorMessage!) : ValidationResult.Success;
            }

            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s) || s.Equals(FuelType.None.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult(ErrorMessage!);
                }

                return ValidationResult.Success;
            }

            if (value is null)
            {
                return new ValidationResult(ErrorMessage!);
            }

            return ValidationResult.Success;
        }
    }
}

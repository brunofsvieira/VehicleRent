using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VehicleRent.Models.Validation
{
    public class ManufacturingYearRangeAttribute : ValidationAttribute, IClientModelValidator
    {
        private const int MinYear = 1900;

        public ManufacturingYearRangeAttribute()
            : base("Manufacturing year must be between {0} and {1}.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (!int.TryParse(value.ToString(), out var year))
            {
                return new ValidationResult(string.Format(ErrorMessageString, MinYear, DateTime.Now.Year));
            }

            var maxYear = DateTime.Now.Year;
            if (year < MinYear || year > maxYear)
            {
                return new ValidationResult(string.Format(ErrorMessageString, MinYear, maxYear));
            }

            return ValidationResult.Success;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var maxYear = DateTime.Now.Year;
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-range", string.Format(ErrorMessageString, MinYear, maxYear));
            MergeAttribute(context.Attributes, "data-val-range-min", MinYear.ToString());
            MergeAttribute(context.Attributes, "data-val-range-max", maxYear.ToString());
        }

        private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key)) return;
            attributes.Add(key, value);
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace VehicleRent.Models.DTOs
{
    /// <summary>
    /// Represents the CreateClientDto component.
    /// </summary>
    public class CreateClientDto
    {
        [Required, StringLength(50)]
        public required string Name { get; set; }

        [Required, StringLength(100), EmailAddress]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^\+\d{1,3}\d{9}$", ErrorMessage = "Phone number format is invalid. Use +<countrycode><9digits>.")]
        public required string PhoneNumber { get; set; }

        [Required, StringLength(30)]
        public required string DriverLicense { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.Validation;

namespace VehicleRent.Models.DTOs
{
    /// <summary>
    /// Represents the UpdateVehicleDto component.
    /// </summary>
    public class UpdateVehicleDto
    {
        [Required, StringLength(30)]
        public required string Brand { get; set; }

        [Required, StringLength(30)]
        public required string Model { get; set; }

        [Required, StringLength(8, MinimumLength = 8)]
        [LicensePlateFormat]
        public required string LicensePlate { get; set; }

        [Required]
        [ManufacturingYearRange]
        public int ManufacturingYear { get; set; }

        [Required]
        [EnsureFuelNotNone]
        public required FuelType Fuel { get; set; }
    }
}

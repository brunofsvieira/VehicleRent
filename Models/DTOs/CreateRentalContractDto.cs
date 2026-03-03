using System.ComponentModel.DataAnnotations;

namespace VehicleRent.Models.DTOs
{
    /// <summary>
    /// Represents the CreateRentalContractDto component.
    /// </summary>
    public class CreateRentalContractDto
    {
        [Required]
        public long ClientId { get; set; }

        [Required]
        public long VehicleId { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime RentalStartDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime RentalEndDate { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int InitialMileage { get; set; }
    }
}

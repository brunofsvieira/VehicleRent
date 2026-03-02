using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;

namespace VehicleRent.Models.ViewModels
{
    public class RentalContractViewModel
    {
        public long? Id { get; set; }

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

        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public bool IsFinished => RentalEndDate.Date < DateTime.UtcNow.Date;
        public string ContractStatus => IsFinished ? "Terminado" : "Em curso";

        public static RentalContractViewModel FromEntity(RentalContract contract)
        {
            return new RentalContractViewModel
            {
                Id = contract.Id == 0 ? null : contract.Id,
                ClientId = contract.ClientId,
                VehicleId = contract.VehicleId,
                RentalStartDate = contract.RentalStartDate,
                RentalEndDate = contract.RentalEndDate,
                InitialMileage = contract.InitialMileage,
                ClientName = contract.Client?.Name ?? string.Empty,
                ClientEmail = contract.Client?.Email ?? string.Empty,
                VehicleLicensePlate = contract.Vehicle?.LicensePlate ?? string.Empty
            };
        }
    }
}

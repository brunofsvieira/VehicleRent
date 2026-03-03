using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;

namespace VehicleRent.Models.ViewModels
{
    /// <summary>
    /// Represents the RentalContractViewModel component.
    /// </summary>
    public class RentalContractViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "O campo Cliente é obrigatório.")]
        public long ClientId { get; set; }

        [Required(ErrorMessage = "O campo Veículo é obrigatório.")]
        public long VehicleId { get; set; }

        [Required(ErrorMessage = "A Data de inicio é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime RentalStartDate { get; set; }

        [Required(ErrorMessage = "A Data de fim é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime RentalEndDate { get; set; }

        [Required(ErrorMessage = "A Quilometragem inicial é obrigatória.")]
        [Range(0, int.MaxValue, ErrorMessage = "A Quilometragem inicial deve ser igual ou superior a 0.")]
        public int InitialMileage { get; set; }

        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public bool IsFinished => RentalEndDate.Date < DateTime.UtcNow.Date;
        public string ContractStatus => IsFinished ? "Terminado" : "Em curso";

        /// <summary>
        /// Executes the FromEntity operation.
        /// </summary>
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

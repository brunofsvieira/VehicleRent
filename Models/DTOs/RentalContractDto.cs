namespace VehicleRent.Models.DTOs
{
    /// <summary>
    /// Represents the RentalContractDto component.
    /// </summary>
    public class RentalContractDto
    {
        public long? Id { get; set; }
        public long ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public long VehicleId { get; set; }
        public string VehicleLicensePlate { get; set; } = string.Empty;
        public DateTime RentalStartDate { get; set; }
        public DateTime RentalEndDate { get; set; }
        public int InitialMileage { get; set; }
        public bool IsFinished { get; set; }
        public string ContractStatus => IsFinished ? "Terminado" : "Em curso";
    }
}

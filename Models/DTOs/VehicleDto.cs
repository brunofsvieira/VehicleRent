using VehicleRent.Models.Enumerators;

namespace VehicleRent.Models.DTOs
{
    public class VehicleDto
    {
        public long? Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int ManufacturingYear { get; set; }
        public FuelType Fuel { get; set; }
        public bool IsCurrentlyRented { get; set; }
        public string AvailabilityStatus => IsCurrentlyRented ? "Alugado" : "Disponivel";
    }
}

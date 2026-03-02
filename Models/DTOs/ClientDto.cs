namespace VehicleRent.Models.DTOs
{
    public class ClientDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string DriverLicense { get; set; } = string.Empty;
    }
}

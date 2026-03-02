using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;

namespace VehicleRent.Models.ViewModels
{
    public class ClientViewModel
    {
        public long? Id { get; set; }

        [Required, StringLength(50)]
        public required string Name { get; set; }

        [Required, StringLength(100), EmailAddress]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^\+\d{1,3}\d{9}$", ErrorMessage = "Formato invalido. Use +<codigo-pais><9 digitos>.")]
        public required string PhoneNumber { get; set; }

        [Required, StringLength(30)]
        public required string DriverLicense { get; set; }

        public Client ToEntity() => new Client(Name, Email, PhoneNumber, DriverLicense);

        public static ClientViewModel FromEntity(Client client) =>
            new ClientViewModel
            {
                Id = client.Id == 0 ? null : client.Id,
                Name = client.Name,
                Email = client.Email,
                PhoneNumber = client.PhoneNumber,
                DriverLicense = client.DriverLicense
            };
    }
}

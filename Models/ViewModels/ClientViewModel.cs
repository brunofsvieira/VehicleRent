using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;

namespace VehicleRent.Models.ViewModels
{
    /// <summary>
    /// Represents a view model for a client, providing properties for client-related data such as name, email address,
    /// phone number, and driver license. Used to transfer client information between the user interface and the
    /// application.
    /// </summary>
    /// <remarks>This view model enforces validation rules on its properties to ensure that all required
    /// client information is provided and correctly formatted before submission. It includes methods for converting to
    /// and from the corresponding entity model, facilitating data exchange between layers of the application.</remarks>
    public class ClientViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "O campo Nome completo e obrigatório.")]
        [StringLength(50, ErrorMessage = "O Nome completo deve ter no máximo 50 caracteres.")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "O campo Email e obrigatório.")]
        [StringLength(100, ErrorMessage = "O Email deve ter no máximo 100 caracteres.")]
        [EmailAddress(ErrorMessage = "Formato de email inválido.")]
        public required string Email { get; set; }

        [Required]
        [RegularExpression(@"^\+\d{1,3}\d{9}$", ErrorMessage = "Formato inválido. Use +<código-pais><9 digitos>.")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "O campo Carta de Condução e obrigatório.")]
        [StringLength(30, ErrorMessage = "A Carta de Condução deve ter no máximo 30 caracteres.")]
        public required string DriverLicense { get; set; }

        /// <summary>
        /// Executes the ToEntity operation.
        /// </summary>
        public Client ToEntity() => new Client(Name, Email, PhoneNumber, DriverLicense);

        /// <summary>
        /// Executes the FromEntity operation.
        /// </summary>
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.Validation;

namespace VehicleRent.Models.ViewModels
{
    /// <summary>
    /// Represents the VehicleViewModel component.
    /// </summary>
    public class VehicleViewModel
    {
        /// <summary>
        /// For create leave null; for display/update contains the entity Id.
        /// </summary>
        public long? Id { get; set; }

        [Required(ErrorMessage = "O campo Marca e obrigatório.")]
        [StringLength(30, ErrorMessage = "A Marca deve ter no máximo 30 caracteres.")]
        public required string Brand { get; set; }

        [Required(ErrorMessage = "O campo Modelo e obrigatório.")]
        [StringLength(30, ErrorMessage = "O Modelo deve ter no máximo 30 caracteres.")]
        public required string Model { get; set; }

        [Required(ErrorMessage = "O campo Matrícula e obrigatório.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "A Matrícula deve ter exatamente 8 caracteres.")]
        [LicensePlateFormat(ErrorMessage = "Formato de matrícula inválido.")]
        public required string LicensePlate { get; set; }

        [Required(ErrorMessage = "O campo Ano de Fabricacao e obrigatório.")]
        [ManufacturingYearRange(ErrorMessage = "O ano de fabricacao deve estar entre 1900 e {1}.")]
        public int ManufacturingYear { get; set; }

        [Required(ErrorMessage = "O campo Combustível e obrigatório.")]
        [EnsureFuelNotNone(ErrorMessage = "Selecione um tipo de combustível válido.")]
        public required FuelType Fuel { get; set; }

        public bool IsCurrentlyRented { get; set; }
        public string AvailabilityStatus => IsCurrentlyRented ? "Alugado" : "disponível";

        // Map view model -> new entity (creates entity without Id; repository sets Id)
        /// <summary>
        /// Executes the ToEntity operation.
        /// </summary>
        public Vehicle ToEntity() =>
            new Vehicle(Brand, Model, Fuel, ManufacturingYear, LicensePlate);

        // Map entity -> view model
        /// <summary>
        /// Executes the FromEntity operation.
        /// </summary>
        public static VehicleViewModel FromEntity(Vehicle vehicle) =>
            new VehicleViewModel
            {
                Id = vehicle.Id == 0 ? null : vehicle.Id,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                LicensePlate = vehicle.LicensePlate,
                ManufacturingYear = vehicle.ManufacturingYear,
                Fuel = vehicle.Fuel,
                IsCurrentlyRented = vehicle.IsCurrentlyRented
            };
    }
}

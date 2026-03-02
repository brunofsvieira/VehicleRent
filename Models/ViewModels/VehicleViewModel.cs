using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.Validation;

namespace VehicleRent.Models.ViewModels
{
    public class VehicleViewModel
    {
        /// <summary>
        /// For create leave null; for display/update contains the entity Id.
        /// </summary>
        public long? Id { get; set; }

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

        public bool IsCurrentlyRented { get; set; }
        public string AvailabilityStatus => IsCurrentlyRented ? "Alugado" : "Disponivel";

        // Map view model -> new entity (creates entity without Id; repository sets Id)
        public Vehicle ToEntity() =>
            new Vehicle(Brand, Model, Fuel, ManufacturingYear, LicensePlate);

        // Map entity -> view model
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

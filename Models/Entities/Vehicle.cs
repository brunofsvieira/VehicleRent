using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using VehicleRent.Models.Enumerators;

namespace VehicleRent.Models.Entities
{
    public class Vehicle : BaseEntity
    {
        private static readonly Regex LicensePlatePattern = new(
            @"^(?:\d{2}-[A-Z]{2}-\d{2}|[A-Z]{2}-\d{2}-\d{2}|\d{2}-\d{2}-[A-Z]{2}|[A-Z]{2}-\d{2}-[A-Z]{2})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string Brand { get; private set; } = string.Empty;
        public string Model { get; private set; } = string.Empty;
        public string LicensePlate { get; private set; } = string.Empty;
        public int ManufacturingYear { get; private set; }
        public FuelType Fuel { get; private set; }
        [NotMapped]
        public bool IsCurrentlyRented { get; private set; }

        protected Vehicle() { }

        public Vehicle(string brand, string model, FuelType fuelType, int manufacturingYear, string licensePlate)
        {
            ValidateAndSet(brand, model, fuelType, manufacturingYear, licensePlate);
        }

        public void UpdateVehicle(string brand, string model, FuelType fuelType, int manufacturingYear, string licensePlate)
        {
            ValidateAndSet(brand, model, fuelType, manufacturingYear, licensePlate);
            TouchUpdate();
        }

        public void SetRentalStatus(bool isCurrentlyRented)
        {
            IsCurrentlyRented = isCurrentlyRented;
        }

        private void ValidateAndSet(string brand, string model, FuelType fuelType, int manufacturingYear, string licensePlate)
        {
            var normalizedPlate = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();

            switch (true)
            {
                case true when string.IsNullOrWhiteSpace(brand):
                    throw new ArgumentException("Brand cannot be null or empty.", nameof(brand));
                case true when string.IsNullOrWhiteSpace(model):
                    throw new ArgumentException("Model cannot be null or empty.", nameof(model));
                case true when string.IsNullOrWhiteSpace(normalizedPlate):
                    throw new ArgumentException("License plate cannot be null or empty.", nameof(licensePlate));
                case true when !LicensePlatePattern.IsMatch(normalizedPlate):
                    throw new ArgumentException("License plate format is invalid.", nameof(licensePlate));
                case true when fuelType == FuelType.None:
                    throw new ArgumentException("Fuel type value is not valid.", nameof(fuelType));
                case true when manufacturingYear > DateTime.UtcNow.Year || manufacturingYear < 1900:
                    throw new ArgumentOutOfRangeException(nameof(manufacturingYear), "Manufacturing year is not valid.");
                default:
                    break;
            }

            Brand = brand;
            Model = model;
            Fuel = fuelType;
            ManufacturingYear = manufacturingYear;
            LicensePlate = normalizedPlate;
        }
    }
}

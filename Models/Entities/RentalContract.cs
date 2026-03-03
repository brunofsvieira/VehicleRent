using System;

namespace VehicleRent.Models.Entities
{
    /// <summary>
    /// Represents the RentalContract component.
    /// </summary>
    public class RentalContract : BaseEntity
    {
        public long ClientId { get; private set; }
        public long VehicleId { get; private set; }
        public DateTime RentalStartDate { get; private set; }
        public DateTime RentalEndDate { get; private set; }
        public int InitialMileage { get; private set; }

        public Client? Client { get; private set; }
        public Vehicle? Vehicle { get; private set; }

        protected RentalContract() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalContract"/> class.
        /// </summary>
        public RentalContract(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
        {
            ValidateAndSet(clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
        }

        /// <summary>
        /// Executes the UpdateContract operation.
        /// </summary>
        public void UpdateContract(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
        {
            ValidateAndSet(clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
            TouchUpdate();
        }

        private static DateTime UtcToday()
        {
            return DateTime.UtcNow.Date;
        }

        private void ValidateAndSet(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
        {
            var start = rentalStartDate.Date;
            var end = rentalEndDate.Date;

            switch (true)
            {
                case true when clientId <= 0:
                    throw new ArgumentException("Client is required.", nameof(clientId));
                case true when vehicleId <= 0:
                    throw new ArgumentException("Vehicle is required.", nameof(vehicleId));
                case true when start < UtcToday():
                    throw new ArgumentException("Rental start date cannot be earlier than today.", nameof(rentalStartDate));
                case true when end <= start:
                    throw new ArgumentException("Rental end date must be later than rental start date.", nameof(rentalEndDate));
                case true when initialMileage < 0:
                    throw new ArgumentException("Initial mileage must be zero or greater.", nameof(initialMileage));
                default:
                    break;
            }

            ClientId = clientId;
            VehicleId = vehicleId;
            RentalStartDate = start;
            RentalEndDate = end;
            InitialMileage = initialMileage;
        }
    }
}

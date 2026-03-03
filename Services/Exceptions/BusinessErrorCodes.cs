namespace VehicleRent.Services.Exceptions
{
    public static class BusinessErrorCodes
    {
        public const string GenericValidation = "generic.validation";

        public const string VehicleLicensePlateAlreadyExists = "vehicle.license_plate.already_exists";
        public const string VehicleBrandRequired = "vehicle.brand.required";
        public const string VehicleModelRequired = "vehicle.model.required";
        public const string VehicleLicensePlateRequired = "vehicle.license_plate.required";
        public const string VehicleLicensePlateInvalidFormat = "vehicle.license_plate.invalid_format";
        public const string VehicleFuelInvalid = "vehicle.fuel.invalid";
        public const string VehicleManufacturingYearInvalid = "vehicle.manufacturing_year.invalid";

        public const string ClientEmailAlreadyExists = "client.email.already_exists";
        public const string ClientDriverLicenseAlreadyExists = "client.driver_license.already_exists";
        public const string ClientEmailOrDriverLicenseAlreadyExists = "client.email_or_driver_license.already_exists";
        public const string ClientNameRequired = "client.name.required";
        public const string ClientNameTooLong = "client.name.too_long";
        public const string ClientEmailRequired = "client.email.required";
        public const string ClientEmailTooLong = "client.email.too_long";
        public const string ClientEmailInvalidFormat = "client.email.invalid_format";
        public const string ClientPhoneRequired = "client.phone.required";
        public const string ClientPhoneInvalidFormat = "client.phone.invalid_format";
        public const string ClientDriverLicenseRequired = "client.driver_license.required";

        public const string RentalVehicleOverlap = "rental.vehicle.overlap";
        public const string RentalSaveFailed = "rental.save.failed";
        public const string RentalUpdateFailed = "rental.update.failed";
        public const string RentalClientNotFound = "rental.client.not_found";
        public const string RentalVehicleNotFound = "rental.vehicle.not_found";
        public const string RentalClientRequired = "rental.client.required";
        public const string RentalVehicleRequired = "rental.vehicle.required";
        public const string RentalStartDatePast = "rental.start_date.past";
        public const string RentalEndDateInvalid = "rental.end_date.invalid";
        public const string RentalInitialMileageInvalid = "rental.initial_mileage.invalid";
    }
}

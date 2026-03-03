namespace VehicleRent.Services.Exceptions
{
    public sealed class BusinessValidationException : Exception
    {
        public string ErrorCode { get; }

        public BusinessValidationException(string message)
            : this(BusinessErrorCodes.GenericValidation, message)
        {
        }

        public BusinessValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? BusinessErrorCodes.GenericValidation
                : errorCode;
        }

    }
}

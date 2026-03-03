namespace VehicleRent.Services.Exceptions
{
    /// <summary>
    /// Represents the BusinessValidationException component.
    /// </summary>
    public sealed class BusinessValidationException : Exception
    {
        public string ErrorCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessValidationException"/> class.
        /// </summary>
        public BusinessValidationException(string message)
            : this(BusinessErrorCodes.GenericValidation, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessValidationException"/> class.
        /// </summary>
        public BusinessValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = string.IsNullOrWhiteSpace(errorCode)
                ? BusinessErrorCodes.GenericValidation
                : errorCode;
        }

    }
}

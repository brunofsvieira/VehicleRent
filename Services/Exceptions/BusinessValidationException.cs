namespace VehicleRent.Services.Exceptions
{
    public sealed class BusinessValidationException : Exception
    {
        public BusinessValidationException(string message) : base(message)
        {
        }
    }
}

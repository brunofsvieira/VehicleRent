namespace VehicleRent.Services.Exceptions
{
    /// <summary>
    /// Represents the EntityNotFoundException component.
    /// </summary>
    public sealed class EntityNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        public EntityNotFoundException(string message) : base(message)
        {
        }
    }
}

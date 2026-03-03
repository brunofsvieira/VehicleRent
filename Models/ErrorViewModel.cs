namespace VehicleRent.Models
{
    /// <summary>
    /// Represents the ErrorViewModel component.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}

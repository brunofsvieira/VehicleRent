using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace VehicleRent.Models.Entities
{
    /// <summary>
    /// Represents the Client component.
    /// </summary>
    public class Client : BaseEntity
    {
        private static readonly Regex PhonePattern = new(@"^\+\d{1,3}\d{9}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string DriverLicense { get; private set; } = string.Empty;

        protected Client() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        public Client(string name, string email, string phoneNumber, string driverLicense)
        {
            ValidateAndSet(name, email, phoneNumber, driverLicense);
        }

        /// <summary>
        /// Executes the UpdateClient operation.
        /// </summary>
        public void UpdateClient(string name, string email, string phoneNumber, string driverLicense)
        {
            ValidateAndSet(name, email, phoneNumber, driverLicense);
            TouchUpdate();
        }

        private void ValidateAndSet(string name, string email, string phoneNumber, string driverLicense)
        {
            var normalizedName = (name ?? string.Empty).Trim();
            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedPhone = (phoneNumber ?? string.Empty).Trim();
            var normalizedLicense = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();

            switch (true)
            {
                case true when string.IsNullOrWhiteSpace(normalizedName):
                    throw new ArgumentException("Name is required.", nameof(name));
                case true when normalizedName.Length > 50:
                    throw new ArgumentException("Name must have at most 50 characters.", nameof(name));
                case true when string.IsNullOrWhiteSpace(normalizedEmail):
                    throw new ArgumentException("Email is required.", nameof(email));
                case true when normalizedEmail.Length > 100:
                    throw new ArgumentException("Email must have at most 100 characters.", nameof(email));
                case true when !IsValidEmail(normalizedEmail):
                    throw new ArgumentException("Email format is invalid.", nameof(email));
                case true when string.IsNullOrWhiteSpace(normalizedPhone):
                    throw new ArgumentException("Phone number is required.", nameof(phoneNumber));
                case true when !PhonePattern.IsMatch(normalizedPhone):
                    throw new ArgumentException("Phone number format is invalid. Use +<countrycode><9digits>.", nameof(phoneNumber));
                case true when string.IsNullOrWhiteSpace(normalizedLicense):
                    throw new ArgumentException("Driver license is required.", nameof(driverLicense));
                default:
                    break;
            }

            Name = normalizedName;
            Email = normalizedEmail;
            PhoneNumber = normalizedPhone;
            DriverLicense = normalizedLicense;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                _ = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

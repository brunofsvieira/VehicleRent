using VehicleRent.Models.Entities;

namespace VehicleRent.Services
{
    /// <summary>
    /// Defines operations for IClientService.
    /// </summary>
    public interface IClientService
    {
        Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null);
        Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null);
        Task<IReadOnlyList<Client>> GetAllForSelectionAsync();
        Task<Client?> GetByIdAsync(long id);
        Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense);
        Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense);
        Task DeleteAsync(long id, bool ensureExists);
    }
}

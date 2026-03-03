using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    /// <summary>
    /// Defines operations for IClientRepository.
    /// </summary>
    public interface IClientRepository
    {
        Task AddAsync(Client client);
        Task<Client?> GetByIdAsync(long id);
        Task<PagedResult<Client>> GetAllAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null);
        Task<IReadOnlyList<Client>> GetAllForSelectionAsync();
        Task<bool> ExistsByEmailAsync(string email, long? excludingId = null);
        Task<bool> ExistsByDriverLicenseAsync(string driverLicense, long? excludingId = null);
        Task UpdateAsync(Client client);
        Task DeleteAsync(long id);
    }
}

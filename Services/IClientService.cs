using VehicleRent.Models.Entities;

namespace VehicleRent.Services
{
    public interface IClientService
    {
        Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize);
        Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize);
        Task<Client?> GetByIdAsync(long id);
        Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense);
        Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense);
        Task DeleteAsync(long id, bool ensureExists);
    }
}

using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    public interface IClientRepository
    {
        Task AddAsync(Client client);
        Task<Client?> GetByIdAsync(long id);
        Task<PagedResult<Client>> GetAllAsync(int page, int pageSize);
        Task<bool> ExistsByEmailAsync(string email, long? excludingId = null);
        Task UpdateAsync(Client client);
        Task DeleteAsync(long id);
    }
}

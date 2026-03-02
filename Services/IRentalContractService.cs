using VehicleRent.Models.Entities;

namespace VehicleRent.Services
{
    public interface IRentalContractService
    {
        Task<PagedResult<RentalContract>> GetPagedForWebAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null);
        Task<PagedResult<RentalContract>> GetPagedForApiAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null);
        Task<RentalContract?> GetByIdAsync(long id);
        Task<RentalContract> CreateAsync(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage);
        Task UpdateAsync(long id, long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage);
        Task DeleteAsync(long id, bool ensureExists);
    }
}

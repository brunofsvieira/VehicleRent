using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    public interface IRentalContractRepository
    {
        Task AddAsync(RentalContract contract);
        Task<RentalContract?> GetByIdAsync(long id);
        Task<RentalContract?> GetByIdForWriteAsync(long id);
        Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize);
        Task UpdateAsync(RentalContract contract);
        Task DeleteAsync(long id);
        Task<bool> ExistsVehicleOverlapAsync(long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, long? excludingId = null);
        Task<HashSet<long>> GetCurrentlyRentedVehicleIdsAsync(DateTime onDate);
    }
}

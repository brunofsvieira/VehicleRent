using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    /// <summary>
    /// Defines operations for IRentalContractRepository.
    /// </summary>
    public interface IRentalContractRepository
    {
        Task AddAsync(RentalContract contract);
        Task<RentalContract?> GetByIdAsync(long id);
        Task<RentalContract?> GetByIdForWriteAsync(long id);
        Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null, bool? isFinished = null);
        Task UpdateAsync(RentalContract contract);
        Task DeleteAsync(long id);
        Task<bool> ExistsVehicleOverlapAsync(long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, long? excludingId = null);
        Task<HashSet<long>> GetCurrentlyRentedVehicleIdsAsync(DateTime onDate);
        Task<HashSet<long>> GetCurrentlyActiveClientIdsAsync(DateTime onDate);
        Task<bool> HasActiveRentalForVehicleAsync(long vehicleId, DateTime onDate);
        Task<bool> HasActiveRentalForClientAsync(long clientId, DateTime onDate);
        Task<bool> IsContractActiveAsync(long contractId, DateTime onDate);
    }
}

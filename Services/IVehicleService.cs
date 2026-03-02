using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;

namespace VehicleRent.Services
{
    public interface IVehicleService
    {
        Task<PagedResult<Vehicle>> GetPagedForWebAsync(int page, int pageSize);
        Task<PagedResult<Vehicle>> GetPagedForApiAsync(int page, int pageSize);
        Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync();
        Task<Vehicle?> GetByIdAsync(long id);
        Task<Vehicle> CreateAsync(string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear);
        Task UpdateAsync(long id, string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear);
        Task DeleteAsync(long id, bool ensureExists);
    }
}

using Microsoft.EntityFrameworkCore;
using VehicleRent.Data;
using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    /// <summary>
    /// Represents the VehicleRepository component.
    /// </summary>
    public class VehicleRepository : IVehicleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleRepository"/> class.
        /// </summary>
        public VehicleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Executes the AddAsync operation.
        /// </summary>
        public async Task AddAsync(Vehicle vehicle)
        {
            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Executes the GetByIdAsync operation.
        /// </summary>
        public async Task<Vehicle?> GetByIdAsync(long id)
        {
            return await _dbContext.Vehicles.FindAsync(id);
        }

        /// <summary>
        /// Executes the GetAllAsync operation.
        /// </summary>
        public async Task<PagedResult<Vehicle>> GetAllAsync(int page, int pageSize, string? licensePlate = null, long? clientId = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _dbContext.Vehicles.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                var normalizedLicensePlate = licensePlate.Trim().ToUpperInvariant();
                query = query.Where(v => v.LicensePlate.Contains(normalizedLicensePlate));
            }

            if (clientId.HasValue && clientId.Value > 0)
            {
                var cid = clientId.Value;
                query = query.Where(v => _dbContext.RentalContracts.Any(rc => rc.VehicleId == v.Id && rc.ClientId == cid));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ThenBy(v => v.LicensePlate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Vehicle>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Executes the GetAllForSelectionAsync operation.
        /// </summary>
        public async Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync()
        {
            return await _dbContext.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.Brand)
                .ThenBy(v => v.Model)
                .ThenBy(v => v.LicensePlate)
                .ToListAsync();
        }

        /// <summary>
        /// Executes the ExistsByLicensePlateAsync operation.
        /// </summary>
        public async Task<bool> ExistsByLicensePlateAsync(string licensePlate, long? excludingId = null)
        {
            var normalized = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(normalized)) return false;

            var query = _dbContext.Vehicles.AsNoTracking().Where(v => v.LicensePlate == normalized);
            if (excludingId.HasValue) query = query.Where(v => v.Id != excludingId.Value);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Executes the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(Vehicle vehicle)
        {
            _dbContext.Vehicles.Update(vehicle);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Executes the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(long id)
        {
            var existing = await _dbContext.Vehicles.FindAsync(id);
            if (existing is null) return;
            existing.MarkDeleted();
            await _dbContext.SaveChangesAsync();
        }
    }
}

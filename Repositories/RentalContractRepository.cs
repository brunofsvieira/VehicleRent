using Microsoft.EntityFrameworkCore;
using VehicleRent.Data;
using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    /// <summary>
    /// Represents the RentalContractRepository component.
    /// </summary>
    public class RentalContractRepository : IRentalContractRepository
    {
        private readonly ApplicationDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RentalContractRepository"/> class.
        /// </summary>
        public RentalContractRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Executes the AddAsync operation.
        /// </summary>
        public async Task AddAsync(RentalContract contract)
        {
            _dbContext.RentalContracts.Add(contract);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Executes the GetByIdAsync operation.
        /// </summary>
        public async Task<RentalContract?> GetByIdAsync(long id)
        {
            return await _dbContext.RentalContracts
                .AsNoTracking()
                .Include(rc => rc.Client)
                .Include(rc => rc.Vehicle)
                .FirstOrDefaultAsync(rc => rc.Id == id);
        }

        /// <summary>
        /// Executes the GetByIdForWriteAsync operation.
        /// </summary>
        public Task<RentalContract?> GetByIdForWriteAsync(long id)
        {
            return _dbContext.RentalContracts.FirstOrDefaultAsync(rc => rc.Id == id);
        }

        /// <summary>
        /// Executes the GetAllAsync operation.
        /// </summary>
        public async Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null, bool? isFinished = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _dbContext.RentalContracts
                .AsNoTracking()
                .Include(rc => rc.Client)
                .Include(rc => rc.Vehicle)
                .AsQueryable();

            if (clientId.HasValue && clientId.Value > 0)
            {
                var cid = clientId.Value;
                query = query.Where(rc => rc.ClientId == cid);
            }

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                var vid = vehicleId.Value;
                query = query.Where(rc => rc.VehicleId == vid);
            }

            if (isFinished.HasValue)
            {
                if (isFinished.Value)
                {
                    query = query.Where(rc => rc.RentalEndDate.Date < DateTime.UtcNow.Date);
                }
                else
                {
                    query = query.Where(rc => rc.RentalEndDate.Date >= DateTime.UtcNow.Date);
                }
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(rc => rc.RentalStartDate)
                .ThenBy(rc => rc.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<RentalContract>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Executes the UpdateAsync operation.
        /// </summary>
        public async Task UpdateAsync(RentalContract contract)
        {
            _dbContext.RentalContracts.Update(contract);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Executes the DeleteAsync operation.
        /// </summary>
        public async Task DeleteAsync(long id)
        {
            var existing = await _dbContext.RentalContracts.FindAsync(id);
            if (existing is null) return;

            existing.MarkDeleted();
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Executes the ExistsVehicleOverlapAsync operation.
        /// </summary>
        public async Task<bool> ExistsVehicleOverlapAsync(long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, long? excludingId = null)
        {
            var start = rentalStartDate.Date;
            var end = rentalEndDate.Date;

            var query = _dbContext.RentalContracts.AsNoTracking()
                .Where(rc => rc.VehicleId == vehicleId && rc.RentalStartDate <= end && rc.RentalEndDate >= start);

            if (excludingId.HasValue)
            {
                query = query.Where(rc => rc.Id != excludingId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Executes the GetCurrentlyRentedVehicleIdsAsync operation.
        /// </summary>
        public async Task<HashSet<long>> GetCurrentlyRentedVehicleIdsAsync(DateTime onDate)
        {
            var date = onDate.Date;

            var ids = await _dbContext.RentalContracts.AsNoTracking()
                .Where(rc => rc.RentalStartDate <= date && rc.RentalEndDate >= date)
                .Select(rc => rc.VehicleId)
                .Distinct()
                .ToListAsync();

            return ids.ToHashSet();
        }

        /// <summary>
        /// Executes the GetCurrentlyActiveClientIdsAsync operation.
        /// </summary>
        public async Task<HashSet<long>> GetCurrentlyActiveClientIdsAsync(DateTime onDate)
        {
            var date = onDate.Date;
            var ids = await _dbContext.RentalContracts.AsNoTracking()
                .Where(rc => rc.RentalStartDate <= date && rc.RentalEndDate >= date)
                .Select(rc => rc.ClientId)
                .Distinct()
                .ToListAsync();
            return ids.ToHashSet();
        }

        /// <summary>
        /// Executes the HasActiveRentalForVehicleAsync operation.
        /// </summary>
        public Task<bool> HasActiveRentalForVehicleAsync(long vehicleId, DateTime onDate)
        {
            var date = onDate.Date;
            return _dbContext.RentalContracts.AsNoTracking()
                .AnyAsync(rc => rc.VehicleId == vehicleId && rc.RentalStartDate <= date && rc.RentalEndDate >= date);
        }

        /// <summary>
        /// Executes the HasActiveRentalForClientAsync operation.
        /// </summary>
        public Task<bool> HasActiveRentalForClientAsync(long clientId, DateTime onDate)
        {
            var date = onDate.Date;
            return _dbContext.RentalContracts.AsNoTracking()
                .AnyAsync(rc => rc.ClientId == clientId && rc.RentalStartDate <= date && rc.RentalEndDate >= date);
        }

        /// <summary>
        /// Executes the IsContractActiveAsync operation.
        /// </summary>
        public Task<bool> IsContractActiveAsync(long contractId, DateTime onDate)
        {
            var date = onDate.Date;
            return _dbContext.RentalContracts.AsNoTracking()
                .AnyAsync(rc => rc.Id == contractId && rc.RentalStartDate <= date && rc.RentalEndDate >= date);
        }
    }
}

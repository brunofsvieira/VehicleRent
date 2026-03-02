using Microsoft.EntityFrameworkCore;
using VehicleRent.Data;
using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    public class RentalContractRepository : IRentalContractRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RentalContractRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(RentalContract contract)
        {
            _dbContext.RentalContracts.Add(contract);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<RentalContract?> GetByIdAsync(long id)
        {
            return await _dbContext.RentalContracts
                .AsNoTracking()
                .Include(rc => rc.Client)
                .Include(rc => rc.Vehicle)
                .FirstOrDefaultAsync(rc => rc.Id == id);
        }

        public Task<RentalContract?> GetByIdForWriteAsync(long id)
        {
            return _dbContext.RentalContracts.FirstOrDefaultAsync(rc => rc.Id == id);
        }

        public async Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
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

        public async Task UpdateAsync(RentalContract contract)
        {
            _dbContext.RentalContracts.Update(contract);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var existing = await _dbContext.RentalContracts.FindAsync(id);
            if (existing is null) return;

            _dbContext.RentalContracts.Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

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
    }
}

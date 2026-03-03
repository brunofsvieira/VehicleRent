using Microsoft.EntityFrameworkCore;
using VehicleRent.Data;
using VehicleRent.Models.Entities;

namespace VehicleRent.Repositories
{
    /// <summary>
    /// Provides methods for managing client entities, including adding, retrieving, updating, and deleting clients in
    /// the application database.
    /// </summary>
    /// <remarks>This repository supports asynchronous operations for improved scalability and responsiveness.
    /// It enables filtering and pagination when retrieving clients, and ensures that client data is accessed and
    /// modified in a consistent manner. Use this type to interact with client records without directly accessing the
    /// database context.</remarks>
    public class ClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ClientRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Client client)
        {
            _dbContext.Clients.Add(client);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Client?> GetByIdAsync(long id)
        {
            return await _dbContext.Clients.FindAsync(id);
        }

        public async Task<PagedResult<Client>> GetAllAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = _dbContext.Clients.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nameOrEmail))
            {
                var normalized = nameOrEmail.Trim().ToLowerInvariant();
                query = query.Where(c => c.Name.ToLower().Contains(normalized) || c.Email.ToLower().Contains(normalized));
            }

            if (vehicleId.HasValue && vehicleId.Value > 0)
            {
                var vid = vehicleId.Value;
                query = query.Where(c => _dbContext.RentalContracts.Any(rc => rc.ClientId == c.Id && rc.VehicleId == vid));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Client>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
        {
            return await _dbContext.Clients
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Email)
                .ToListAsync();
        }

        public async Task<bool> ExistsByEmailAsync(string email, long? excludingId = null)
        {
            var normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail)) return false;

            var query = _dbContext.Clients.AsNoTracking().Where(c => c.Email == normalizedEmail);
            if (excludingId.HasValue) query = query.Where(c => c.Id != excludingId.Value);
            return await query.AnyAsync();
        }

        public async Task<bool> ExistsByDriverLicenseAsync(string driverLicense, long? excludingId = null)
        {
            var normalizedLicense = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedLicense)) return false;

            var query = _dbContext.Clients.AsNoTracking().Where(c => c.DriverLicense == normalizedLicense);
            if (excludingId.HasValue) query = query.Where(c => c.Id != excludingId.Value);
            return await query.AnyAsync();
        }

        public async Task UpdateAsync(Client client)
        {
            _dbContext.Clients.Update(client);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var existing = await _dbContext.Clients.FindAsync(id);
            if (existing is null) return;
            existing.MarkDeleted();
            await _dbContext.SaveChangesAsync();
        }
    }
}

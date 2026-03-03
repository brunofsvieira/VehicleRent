using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryRentalContractRepository : IRentalContractRepository
{
    private readonly List<RentalContract> _items = new();
    private long _nextId = 1;
    private readonly List<(int page, int pageSize)> _pagedCalls = new();

    public bool ForceOverlap { get; set; }
    public HashSet<long> CurrentlyRentedVehicleIds { get; } = new();
    public IReadOnlyList<(int page, int pageSize)> PagedCalls => _pagedCalls;

    public InMemoryRentalContractRepository(IEnumerable<RentalContract>? seed = null)
    {
        if (seed is null) return;

        foreach (var item in seed)
        {
            _items.Add(item);
            if (item.Id >= _nextId) _nextId = item.Id + 1;
        }
    }

    public Task AddAsync(RentalContract contract)
    {
        SetId(contract, _nextId++);
        _items.Add(contract);
        return Task.CompletedTask;
    }

    public Task<RentalContract?> GetByIdAsync(long id)
    {
        return Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
    }

    public Task<RentalContract?> GetByIdForWriteAsync(long id)
    {
        return Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
    }

    public Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
    {
        _pagedCalls.Add((page, pageSize));

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        IEnumerable<RentalContract> query = _items;
        if (clientId.HasValue && clientId.Value > 0)
        {
            query = query.Where(c => c.ClientId == clientId.Value);
        }

        if (vehicleId.HasValue && vehicleId.Value > 0)
        {
            query = query.Where(c => c.VehicleId == vehicleId.Value);
        }

        var filtered = query.OrderBy(c => c.Id).ToList();
        var pageItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<RentalContract>
        {
            Items = pageItems,
            TotalCount = filtered.Count,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task UpdateAsync(RentalContract contract)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long id)
    {
        var existing = _items.FirstOrDefault(c => c.Id == id);
        if (existing is not null) _items.Remove(existing);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsVehicleOverlapAsync(long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, long? excludingId = null)
    {
        if (ForceOverlap) return Task.FromResult(true);

        var overlaps = _items.Any(c =>
            c.VehicleId == vehicleId &&
            (!excludingId.HasValue || c.Id != excludingId.Value) &&
            c.RentalStartDate <= rentalEndDate &&
            c.RentalEndDate >= rentalStartDate);

        return Task.FromResult(overlaps);
    }

    public Task<HashSet<long>> GetCurrentlyRentedVehicleIdsAsync(DateTime onDate)
    {
        return Task.FromResult(new HashSet<long>(CurrentlyRentedVehicleIds));
    }

    private static void SetId(RentalContract contract, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop!.SetValue(contract, id);
    }
}

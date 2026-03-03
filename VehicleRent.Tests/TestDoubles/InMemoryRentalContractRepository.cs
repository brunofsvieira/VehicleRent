using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryRentalContractRepository : IRentalContractRepository
{
    private readonly List<RentalContract> _items = new();
    private long _nextId = 1;
    private readonly List<(int page, int pageSize, bool? isFinished)> _pagedCalls = new();

    public bool ForceOverlap { get; set; }
    public Exception? AddException { get; set; }
    public Exception? UpdateException { get; set; }
    public HashSet<long> CurrentlyRentedVehicleIds { get; } = new();
    public IReadOnlyList<(int page, int pageSize, bool? isFinished)> PagedCalls => _pagedCalls;

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
        if (AddException is not null) throw AddException;
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

    public Task<PagedResult<RentalContract>> GetAllAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null, bool? isFinished = null)
    {
        _pagedCalls.Add((page, pageSize, isFinished));

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

        if (isFinished.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            if (isFinished.Value)
            {
                query = query.Where(c => c.RentalEndDate.Date < today);
            }
            else
            {
                query = query.Where(c => c.RentalEndDate.Date >= today);
            }
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
        if (UpdateException is not null) throw UpdateException;
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

    public Task<HashSet<long>> GetCurrentlyActiveClientIdsAsync(DateTime onDate)
    {
        var date = onDate.Date;
        var ids = _items
            .Where(c => c.RentalStartDate.Date <= date && c.RentalEndDate.Date >= date)
            .Select(c => c.ClientId)
            .Distinct()
            .ToHashSet();
        return Task.FromResult(ids);
    }

    public Task<bool> HasActiveRentalForVehicleAsync(long vehicleId, DateTime onDate)
    {
        var date = onDate.Date;
        var activeFromItems = _items.Any(c => c.VehicleId == vehicleId && c.RentalStartDate.Date <= date && c.RentalEndDate.Date >= date);
        var activeFromSet = CurrentlyRentedVehicleIds.Contains(vehicleId);
        return Task.FromResult(activeFromItems || activeFromSet);
    }

    public Task<bool> HasActiveRentalForClientAsync(long clientId, DateTime onDate)
    {
        var date = onDate.Date;
        var active = _items.Any(c => c.ClientId == clientId && c.RentalStartDate.Date <= date && c.RentalEndDate.Date >= date);
        return Task.FromResult(active);
    }

    public Task<bool> IsContractActiveAsync(long contractId, DateTime onDate)
    {
        var date = onDate.Date;
        var active = _items.Any(c => c.Id == contractId && c.RentalStartDate.Date <= date && c.RentalEndDate.Date >= date);
        return Task.FromResult(active);
    }

    private static void SetId(RentalContract contract, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop!.SetValue(contract, id);
    }
}

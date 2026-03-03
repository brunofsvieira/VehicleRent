using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _items = new();
    private long _nextId = 1;
    public Exception? AddException { get; set; }
    public Exception? UpdateException { get; set; }

    public IReadOnlyList<(int page, int pageSize)> PagedCalls => _pagedCalls;
    public int GetByIdCalls { get; private set; }

    private readonly List<(int page, int pageSize)> _pagedCalls = new();

    public InMemoryVehicleRepository(IEnumerable<Vehicle>? seed = null)
    {
        if (seed is null) return;

        foreach (var item in seed)
        {
            _items.Add(item);
            if (item.Id >= _nextId) _nextId = item.Id + 1;
        }
    }

    public Task AddAsync(Vehicle vehicle)
    {
        if (AddException is not null) throw AddException;
        SetId(vehicle, _nextId++);
        _items.Add(vehicle);
        return Task.CompletedTask;
    }

    public Task<Vehicle?> GetByIdAsync(long id)
    {
        GetByIdCalls++;
        return Task.FromResult(_items.FirstOrDefault(v => v.Id == id));
    }

    public Task<PagedResult<Vehicle>> GetAllAsync(int page, int pageSize, string? licensePlate = null, long? clientId = null)
    {
        _pagedCalls.Add((page, pageSize));

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        IEnumerable<Vehicle> query = _items;
        if (!string.IsNullOrWhiteSpace(licensePlate))
        {
            var plate = licensePlate.Trim();
            query = query.Where(v => v.LicensePlate.Contains(plate, StringComparison.OrdinalIgnoreCase));
        }

        // clientId filter is ignored in this in-memory double because there is no relation graph here.
        var filtered = query.ToList();
        var total = filtered.Count;
        var pageItems = filtered
            .OrderBy(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<Vehicle>
        {
            Items = pageItems,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<bool> ExistsByLicensePlateAsync(string licensePlate, long? excludingId = null)
    {
        var normalized = (licensePlate ?? string.Empty).Trim().ToUpperInvariant();
        var exists = _items.Any(v =>
            v.LicensePlate.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || v.Id != excludingId.Value));

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync()
    {
        return Task.FromResult<IReadOnlyList<Vehicle>>(_items.OrderBy(v => v.Id).ToList());
    }

    public Task UpdateAsync(Vehicle vehicle)
    {
        if (UpdateException is not null) throw UpdateException;
        // No-op: instance is already reference-updated in memory.
        return Task.CompletedTask;
    }

    public Task DeleteAsync(long id)
    {
        var existing = _items.FirstOrDefault(v => v.Id == id);
        if (existing is not null) _items.Remove(existing);
        return Task.CompletedTask;
    }

    public int Count() => _items.Count;

    private static void SetId(Vehicle vehicle, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop!.SetValue(vehicle, id);
    }
}

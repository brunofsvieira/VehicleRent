using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryVehicleRepository : IVehicleRepository
{
    private readonly List<Vehicle> _items = new();
    private long _nextId = 1;

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
        SetId(vehicle, _nextId++);
        _items.Add(vehicle);
        return Task.CompletedTask;
    }

    public Task<Vehicle?> GetByIdAsync(long id)
    {
        GetByIdCalls++;
        return Task.FromResult(_items.FirstOrDefault(v => v.Id == id));
    }

    public Task<PagedResult<Vehicle>> GetAllAsync(int page, int pageSize)
    {
        _pagedCalls.Add((page, pageSize));

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var total = _items.Count;
        var pageItems = _items
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

    public Task UpdateAsync(Vehicle vehicle)
    {
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

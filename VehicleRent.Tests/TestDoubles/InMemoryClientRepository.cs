using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryClientRepository : IClientRepository
{
    private readonly List<Client> _items = new();
    private long _nextId = 1;
    private readonly List<(int page, int pageSize)> _pagedCalls = new();
    public Exception? AddException { get; set; }
    public Exception? UpdateException { get; set; }

    public IReadOnlyList<(int page, int pageSize)> PagedCalls => _pagedCalls;

    public InMemoryClientRepository(IEnumerable<Client>? seed = null)
    {
        if (seed is null) return;

        foreach (var item in seed)
        {
            _items.Add(item);
            if (item.Id >= _nextId) _nextId = item.Id + 1;
        }
    }

    public Task AddAsync(Client client)
    {
        if (AddException is not null) throw AddException;
        SetId(client, _nextId++);
        _items.Add(client);
        return Task.CompletedTask;
    }

    public Task<Client?> GetByIdAsync(long id)
    {
        return Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
    }

    public Task<PagedResult<Client>> GetAllAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
    {
        _pagedCalls.Add((page, pageSize));

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        IEnumerable<Client> query = _items;

        if (!string.IsNullOrWhiteSpace(nameOrEmail))
        {
            var term = nameOrEmail.Trim();
            query = query.Where(c =>
                c.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var filtered = query.ToList();
        var total = filtered.Count;
        var pageItems = filtered
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<Client>
        {
            Items = pageItems,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public Task<bool> ExistsByEmailAsync(string email, long? excludingId = null)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var exists = _items.Any(c =>
            c.Email.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || c.Id != excludingId.Value));

        return Task.FromResult(exists);
    }

    public Task<bool> ExistsByDriverLicenseAsync(string driverLicense, long? excludingId = null)
    {
        var normalized = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();
        var exists = _items.Any(c =>
            c.DriverLicense.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || c.Id != excludingId.Value));

        return Task.FromResult(exists);
    }

    public Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
    {
        return Task.FromResult<IReadOnlyList<Client>>(_items.OrderBy(c => c.Id).ToList());
    }

    public Task UpdateAsync(Client client)
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

    public int Count() => _items.Count;

    private static void SetId(Client client, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop!.SetValue(client, id);
    }
}

using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class InMemoryClientRepository : IClientRepository
{
    private readonly List<Client> _items = new();
    private long _nextId = 1;
    private readonly List<(int page, int pageSize)> _pagedCalls = new();

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
        SetId(client, _nextId++);
        _items.Add(client);
        return Task.CompletedTask;
    }

    public Task<Client?> GetByIdAsync(long id)
    {
        return Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
    }

    public Task<PagedResult<Client>> GetAllAsync(int page, int pageSize)
    {
        _pagedCalls.Add((page, pageSize));

        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var total = _items.Count;
        var pageItems = _items
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

    public Task UpdateAsync(Client client)
    {
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

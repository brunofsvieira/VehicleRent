using System.Reflection;
using VehicleRent.Models.Entities;
using VehicleRent.Repositories;

namespace VehicleRent.Tests.TestDoubles;

/// <summary>
/// Represents a test double for InMemoryClientRepository.
/// </summary>
internal sealed class InMemoryClientRepository : IClientRepository
{
    private readonly List<Client> _items = new();
    private long _nextId = 1;
    private readonly List<(int page, int pageSize)> _pagedCalls = new();
    public Exception? AddException { get; set; }
    public Exception? UpdateException { get; set; }

    /// <summary>
    /// Executes the member test operation.
    /// </summary>
    public IReadOnlyList<(int page, int pageSize)> PagedCalls => _pagedCalls;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryClientRepository"/> class.
    /// </summary>
    public InMemoryClientRepository(IEnumerable<Client>? seed = null)
    {
        if (seed is null) return;

        foreach (var item in seed)
        {
            _items.Add(item);
            if (item.Id >= _nextId) _nextId = item.Id + 1;
        }
    }

    /// <summary>
    /// Executes the AddAsync test operation.
    /// </summary>
    public Task AddAsync(Client client)
    {
        if (AddException is not null) throw AddException;
        SetId(client, _nextId++);
        _items.Add(client);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the GetByIdAsync test operation.
    /// </summary>
    public Task<Client?> GetByIdAsync(long id)
    {
        return Task.FromResult(_items.FirstOrDefault(c => c.Id == id));
    }

    /// <summary>
    /// Executes the GetAllAsync test operation.
    /// </summary>
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

    /// <summary>
    /// Executes the ExistsByEmailAsync test operation.
    /// </summary>
    public Task<bool> ExistsByEmailAsync(string email, long? excludingId = null)
    {
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();
        var exists = _items.Any(c =>
            c.Email.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || c.Id != excludingId.Value));

        return Task.FromResult(exists);
    }

    /// <summary>
    /// Executes the ExistsByDriverLicenseAsync test operation.
    /// </summary>
    public Task<bool> ExistsByDriverLicenseAsync(string driverLicense, long? excludingId = null)
    {
        var normalized = (driverLicense ?? string.Empty).Trim().ToUpperInvariant();
        var exists = _items.Any(c =>
            c.DriverLicense.Equals(normalized, StringComparison.OrdinalIgnoreCase) &&
            (!excludingId.HasValue || c.Id != excludingId.Value));

        return Task.FromResult(exists);
    }

    /// <summary>
    /// Executes the GetAllForSelectionAsync test operation.
    /// </summary>
    public Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
    {
        return Task.FromResult<IReadOnlyList<Client>>(_items.OrderBy(c => c.Id).ToList());
    }

    /// <summary>
    /// Executes the UpdateAsync test operation.
    /// </summary>
    public Task UpdateAsync(Client client)
    {
        if (UpdateException is not null) throw UpdateException;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the DeleteAsync test operation.
    /// </summary>
    public Task DeleteAsync(long id)
    {
        var existing = _items.FirstOrDefault(c => c.Id == id);
        if (existing is not null) _items.Remove(existing);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes the Count test operation.
    /// </summary>
    public int Count() => _items.Count;

    private static void SetId(Client client, long id)
    {
        var prop = typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        prop!.SetValue(client, id);
    }
}

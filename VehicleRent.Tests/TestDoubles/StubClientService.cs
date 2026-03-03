using VehicleRent.Models.Entities;
using VehicleRent.Services;

namespace VehicleRent.Tests.TestDoubles;

/// <summary>
/// Represents a test double for StubClientService.
/// </summary>
internal sealed class StubClientService : IClientService
{
    public Func<int, int, long?, long?, Task<PagedResult<Client>>>? OnGetPagedForWeb { get; set; }
    public Func<int, int, long?, long?, Task<PagedResult<Client>>>? OnGetPagedForApi { get; set; }
    public Func<Task<IReadOnlyList<Client>>>? OnGetAllForSelection { get; set; }
    public Func<long, Task<Client?>>? OnGetById { get; set; }
    public Func<string, string, string, string, Task<Client>>? OnCreate { get; set; }
    public Func<long, string, string, string, string, Task>? OnUpdate { get; set; }
    public Func<long, bool, Task>? OnDelete { get; set; }

    /// <summary>
    /// Executes the CreateAsync test operation.
    /// </summary>
    public Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense)
    {
        if (OnCreate is null) throw new NotImplementedException();
        return OnCreate(name, email, phoneNumber, driverLicense);
    }

    /// <summary>
    /// Executes the DeleteAsync test operation.
    /// </summary>
    public Task DeleteAsync(long id, bool ensureExists)
    {
        if (OnDelete is null) throw new NotImplementedException();
        return OnDelete(id, ensureExists);
    }

    /// <summary>
    /// Executes the GetByIdAsync test operation.
    /// </summary>
    public Task<Client?> GetByIdAsync(long id)
    {
        if (OnGetById is null) throw new NotImplementedException();
        return OnGetById(id);
    }

    /// <summary>
    /// Executes the GetPagedForApiAsync test operation.
    /// </summary>
    public Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
    {
        if (OnGetPagedForApi is null) throw new NotImplementedException();
        return OnGetPagedForApi(page, pageSize, clientId, vehicleId);
    }

    /// <summary>
    /// Executes the GetPagedForWebAsync test operation.
    /// </summary>
    public Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null)
    {
        if (OnGetPagedForWeb is null) throw new NotImplementedException();
        return OnGetPagedForWeb(page, pageSize, clientId, vehicleId);
    }

    /// <summary>
    /// Executes the UpdateAsync test operation.
    /// </summary>
    public Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense)
    {
        if (OnUpdate is null) throw new NotImplementedException();
        return OnUpdate(id, name, email, phoneNumber, driverLicense);
    }

    /// <summary>
    /// Executes the GetAllForSelectionAsync test operation.
    /// </summary>
    public Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
    {
        if (OnGetAllForSelection is null) return Task.FromResult<IReadOnlyList<Client>>(Array.Empty<Client>());
        return OnGetAllForSelection();
    }
}

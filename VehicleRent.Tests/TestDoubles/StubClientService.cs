using VehicleRent.Models.Entities;
using VehicleRent.Services;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class StubClientService : IClientService
{
    public Func<int, int, string?, long?, Task<PagedResult<Client>>>? OnGetPagedForWeb { get; set; }
    public Func<int, int, string?, long?, Task<PagedResult<Client>>>? OnGetPagedForApi { get; set; }
    public Func<Task<IReadOnlyList<Client>>>? OnGetAllForSelection { get; set; }
    public Func<long, Task<Client?>>? OnGetById { get; set; }
    public Func<string, string, string, string, Task<Client>>? OnCreate { get; set; }
    public Func<long, string, string, string, string, Task>? OnUpdate { get; set; }
    public Func<long, bool, Task>? OnDelete { get; set; }

    public Task<Client> CreateAsync(string name, string email, string phoneNumber, string driverLicense)
    {
        if (OnCreate is null) throw new NotImplementedException();
        return OnCreate(name, email, phoneNumber, driverLicense);
    }

    public Task DeleteAsync(long id, bool ensureExists)
    {
        if (OnDelete is null) throw new NotImplementedException();
        return OnDelete(id, ensureExists);
    }

    public Task<Client?> GetByIdAsync(long id)
    {
        if (OnGetById is null) throw new NotImplementedException();
        return OnGetById(id);
    }

    public Task<PagedResult<Client>> GetPagedForApiAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
    {
        if (OnGetPagedForApi is null) throw new NotImplementedException();
        return OnGetPagedForApi(page, pageSize, nameOrEmail, vehicleId);
    }

    public Task<PagedResult<Client>> GetPagedForWebAsync(int page, int pageSize, string? nameOrEmail = null, long? vehicleId = null)
    {
        if (OnGetPagedForWeb is null) throw new NotImplementedException();
        return OnGetPagedForWeb(page, pageSize, nameOrEmail, vehicleId);
    }

    public Task UpdateAsync(long id, string name, string email, string phoneNumber, string driverLicense)
    {
        if (OnUpdate is null) throw new NotImplementedException();
        return OnUpdate(id, name, email, phoneNumber, driverLicense);
    }

    public Task<IReadOnlyList<Client>> GetAllForSelectionAsync()
    {
        if (OnGetAllForSelection is null) return Task.FromResult<IReadOnlyList<Client>>(Array.Empty<Client>());
        return OnGetAllForSelection();
    }
}

using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Services;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class StubVehicleService : IVehicleService
{
    public Func<int, int, string?, long?, Task<PagedResult<Vehicle>>>? OnGetPagedForWeb { get; set; }
    public Func<int, int, string?, long?, Task<PagedResult<Vehicle>>>? OnGetPagedForApi { get; set; }
    public Func<Task<IReadOnlyList<Vehicle>>>? OnGetAllForSelection { get; set; }
    public Func<long, Task<Vehicle?>>? OnGetById { get; set; }
    public Func<string, string, string, FuelType, int, Task<Vehicle>>? OnCreate { get; set; }
    public Func<long, string, string, string, FuelType, int, Task>? OnUpdate { get; set; }
    public Func<long, bool, Task>? OnDelete { get; set; }

    public Task<Vehicle> CreateAsync(string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear)
    {
        if (OnCreate is null) throw new NotImplementedException();
        return OnCreate(brand, model, licensePlate, fuel, manufacturingYear);
    }

    public Task DeleteAsync(long id, bool ensureExists)
    {
        if (OnDelete is null) throw new NotImplementedException();
        return OnDelete(id, ensureExists);
    }

    public Task<Vehicle?> GetByIdAsync(long id)
    {
        if (OnGetById is null) throw new NotImplementedException();
        return OnGetById(id);
    }

    public Task<PagedResult<Vehicle>> GetPagedForApiAsync(int page, int pageSize, string? licensePlate = null, long? clientId = null)
    {
        if (OnGetPagedForApi is null) throw new NotImplementedException();
        return OnGetPagedForApi(page, pageSize, licensePlate, clientId);
    }

    public Task<PagedResult<Vehicle>> GetPagedForWebAsync(int page, int pageSize, string? licensePlate = null, long? clientId = null)
    {
        if (OnGetPagedForWeb is null) throw new NotImplementedException();
        return OnGetPagedForWeb(page, pageSize, licensePlate, clientId);
    }

    public Task UpdateAsync(long id, string brand, string model, string licensePlate, FuelType fuel, int manufacturingYear)
    {
        if (OnUpdate is null) throw new NotImplementedException();
        return OnUpdate(id, brand, model, licensePlate, fuel, manufacturingYear);
    }

    public Task<IReadOnlyList<Vehicle>> GetAllForSelectionAsync()
    {
        if (OnGetAllForSelection is null) return Task.FromResult<IReadOnlyList<Vehicle>>(Array.Empty<Vehicle>());
        return OnGetAllForSelection();
    }
}

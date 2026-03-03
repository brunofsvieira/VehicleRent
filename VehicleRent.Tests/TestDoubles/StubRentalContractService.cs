using VehicleRent.Models.Entities;
using VehicleRent.Services;

namespace VehicleRent.Tests.TestDoubles;

internal sealed class StubRentalContractService : IRentalContractService
{
    public Func<int, int, long?, long?, Task<PagedResult<RentalContract>>>? OnGetPagedForWeb { get; set; }
    public Func<int, int, long?, long?, Task<PagedResult<RentalContract>>>? OnGetPagedForApi { get; set; }
    public Func<int, int, long?, long?, bool?, Task<PagedResult<RentalContract>>>? OnGetPagedForWebWithFinished { get; set; }
    public Func<int, int, long?, long?, bool?, Task<PagedResult<RentalContract>>>? OnGetPagedForApiWithFinished { get; set; }
    public Func<long, Task<RentalContract?>>? OnGetById { get; set; }
    public Func<long, long, DateTime, DateTime, int, Task<RentalContract>>? OnCreate { get; set; }
    public Func<long, long, long, DateTime, DateTime, int, Task>? OnUpdate { get; set; }
    public Func<long, bool, Task>? OnDelete { get; set; }

    public Task<PagedResult<RentalContract>> GetPagedForWebAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null, bool? isFinished = null)
    {
        if (OnGetPagedForWebWithFinished is not null) return OnGetPagedForWebWithFinished(page, pageSize, clientId, vehicleId, isFinished);
        if (OnGetPagedForWeb is not null) return OnGetPagedForWeb(page, pageSize, clientId, vehicleId);
        throw new NotImplementedException();
    }

    public Task<PagedResult<RentalContract>> GetPagedForApiAsync(int page, int pageSize, long? clientId = null, long? vehicleId = null, bool? isFinished = null)
    {
        if (OnGetPagedForApiWithFinished is not null) return OnGetPagedForApiWithFinished(page, pageSize, clientId, vehicleId, isFinished);
        if (OnGetPagedForApi is not null) return OnGetPagedForApi(page, pageSize, clientId, vehicleId);
        throw new NotImplementedException();
    }

    public Task<RentalContract?> GetByIdAsync(long id)
    {
        if (OnGetById is null) throw new NotImplementedException();
        return OnGetById(id);
    }

    public Task<RentalContract> CreateAsync(long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
    {
        if (OnCreate is null) throw new NotImplementedException();
        return OnCreate(clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
    }

    public Task UpdateAsync(long id, long clientId, long vehicleId, DateTime rentalStartDate, DateTime rentalEndDate, int initialMileage)
    {
        if (OnUpdate is null) throw new NotImplementedException();
        return OnUpdate(id, clientId, vehicleId, rentalStartDate, rentalEndDate, initialMileage);
    }

    public Task DeleteAsync(long id, bool ensureExists)
    {
        if (OnDelete is null) throw new NotImplementedException();
        return OnDelete(id, ensureExists);
    }
}

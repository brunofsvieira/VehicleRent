using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Repositories;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class RentalContractRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_LoadsNavigation()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var contract = new RentalContract(client.Id, vehicle.Id, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), 0);
        await repo.AddAsync(contract);

        var loaded = await repo.GetByIdAsync(contract.Id);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded!.Client);
        Assert.NotNull(loaded.Vehicle);
    }

    [Fact]
    public async Task GetAllAsync_FiltersAndOrdersByStartDateDesc()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var older = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(1), 0);
        var newer = new RentalContract(client.Id, vehicle.Id, today.AddDays(1), today.AddDays(2), 0);
        ctx.RentalContracts.AddRange(older, newer);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var result = await repo.GetAllAsync(1, 10, client.Id, vehicle.Id);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(newer.Id, result.Items.First().Id);
    }

    [Fact]
    public async Task GetAllAsync_NormalizesInvalidPagingArguments()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        ctx.RentalContracts.Add(new RentalContract(client.Id, vehicle.Id, today, today.AddDays(1), 0));
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var result = await repo.GetAllAsync(0, 0);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task ExistsVehicleOverlapAsync_RespectsExcludingId()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var contract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(2), 0);
        ctx.RentalContracts.Add(contract);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var exists = await repo.ExistsVehicleOverlapAsync(vehicle.Id, today.AddDays(1), today.AddDays(3));
        var existsExcluding = await repo.ExistsVehicleOverlapAsync(vehicle.Id, today.AddDays(1), today.AddDays(3), contract.Id);

        Assert.True(exists);
        Assert.False(existsExcluding);
    }

    [Fact]
    public async Task GetCurrentlyRentedVehicleIdsAsync_ReturnsDistinctVehicleIds()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var v1 = new Vehicle("Brand", "Model1", FuelType.Petrol, 2022, "AA-00-AA");
        var v2 = new Vehicle("Brand", "Model2", FuelType.Petrol, 2022, "BB-00-BB");
        ctx.Clients.Add(client);
        ctx.Vehicles.AddRange(v1, v2);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        ctx.RentalContracts.AddRange(
            new RentalContract(client.Id, v1.Id, today, today.AddDays(1), 0),
            new RentalContract(client.Id, v1.Id, today, today.AddDays(2), 0),
            new RentalContract(client.Id, v2.Id, today.AddDays(2), today.AddDays(3), 0));
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var ids = await repo.GetCurrentlyRentedVehicleIdsAsync(today);

        Assert.Single(ids);
        Assert.Contains(v1.Id, ids);
    }

    [Fact]
    public async Task GetByIdForWriteAsync_ReturnsTrackedEntity()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var contract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(1), 0);
        ctx.RentalContracts.Add(contract);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var loaded = await repo.GetByIdForWriteAsync(contract.Id);

        Assert.NotNull(loaded);
        Assert.Equal(contract.Id, loaded!.Id);
    }

    [Fact]
    public async Task GetAllAsync_WhenIsFinishedFilterProvided_FiltersByContractStatus()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var finishedContract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(1), 0);
        typeof(RentalContract).GetProperty(nameof(RentalContract.RentalStartDate))!
            .SetValue(finishedContract, today.AddDays(-5));
        typeof(RentalContract).GetProperty(nameof(RentalContract.RentalEndDate))!
            .SetValue(finishedContract, today.AddDays(-1));

        var activeContract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(2), 0);

        ctx.RentalContracts.AddRange(finishedContract, activeContract);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);
        var finished = await repo.GetAllAsync(1, 10, isFinished: true);
        var active = await repo.GetAllAsync(1, 10, isFinished: false);

        Assert.Single(finished.Items);
        Assert.True(finished.Items.Single().RentalEndDate.Date < today);
        Assert.Single(active.Items);
        Assert.True(active.Items.Single().RentalEndDate.Date >= today);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var contract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(2), 10);
        ctx.RentalContracts.Add(contract);
        await ctx.SaveChangesAsync();

        contract.UpdateContract(client.Id, vehicle.Id, today, today.AddDays(3), 42);

        var repo = new RentalContractRepository(ctx);
        await repo.UpdateAsync(contract);

        var reloaded = await repo.GetByIdAsync(contract.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(42, reloaded!.InitialMileage);
        Assert.Equal(today.AddDays(3), reloaded.RentalEndDate);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_DoesNothing_And_WhenExists_SoftDeletes()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL010");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.Add(client);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();

        var today = DateTime.UtcNow.Date;
        var contract = new RentalContract(client.Id, vehicle.Id, today, today.AddDays(1), 0);
        ctx.RentalContracts.Add(contract);
        await ctx.SaveChangesAsync();

        var repo = new RentalContractRepository(ctx);

        await repo.DeleteAsync(9999);
        Assert.Equal(1, await ctx.RentalContracts.CountAsync());

        await repo.DeleteAsync(contract.Id);
        Assert.Equal(0, await ctx.RentalContracts.CountAsync());
        var deleted = await ctx.RentalContracts.IgnoreQueryFilters().SingleAsync(rc => rc.Id == contract.Id);
        Assert.True(deleted.Deleted);
    }
}

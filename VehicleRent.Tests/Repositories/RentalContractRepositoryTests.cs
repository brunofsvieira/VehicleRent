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
}

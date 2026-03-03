using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Repositories;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class VehicleRepositoryTests
{
    [Fact]
    public async Task AddAndGetById_Work()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new VehicleRepository(ctx);
        var vehicle = new Vehicle("Ford", "Focus", FuelType.Petrol, 2022, "AA-11-AA");

        await repo.AddAsync(vehicle);
        var loaded = await repo.GetByIdAsync(vehicle.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Ford", loaded!.Brand);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByLicensePlateAndClient()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL100");
        var v1 = new Vehicle("B", "M1", FuelType.Petrol, 2021, "AA-00-AA");
        var v2 = new Vehicle("B", "M2", FuelType.Petrol, 2021, "BB-00-BB");
        ctx.Clients.Add(client);
        ctx.Vehicles.AddRange(v1, v2);
        await ctx.SaveChangesAsync();
        ctx.RentalContracts.Add(new RentalContract(client.Id, v2.Id, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), 0));
        await ctx.SaveChangesAsync();

        var repo = new VehicleRepository(ctx);
        var result = await repo.GetAllAsync(1, 10, "BB-00", client.Id);

        var item = Assert.Single(result.Items);
        Assert.Equal("BB-00-BB", item.LicensePlate);
    }

    [Fact]
    public async Task ExistsByLicensePlateAsync_RespectsExcludingId()
    {
        using var ctx = TestDbContextFactory.Create();
        var vehicle = new Vehicle("B", "M", FuelType.Petrol, 2021, "CC-22-CC");
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();
        var repo = new VehicleRepository(ctx);

        var existsSame = await repo.ExistsByLicensePlateAsync("cc-22-cc");
        var existsExcluding = await repo.ExistsByLicensePlateAsync("CC-22-CC", vehicle.Id);

        Assert.True(existsSame);
        Assert.False(existsExcluding);
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_DoesNotThrow()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new VehicleRepository(ctx);

        await repo.DeleteAsync(999);

        Assert.Empty(ctx.Vehicles);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesVehicle()
    {
        using var ctx = TestDbContextFactory.Create();
        var vehicle = new Vehicle("Ford", "Focus", FuelType.Petrol, 2022, "AA-11-AA");
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();
        var repo = new VehicleRepository(ctx);

        await repo.DeleteAsync(vehicle.Id);

        Assert.Empty(ctx.Vehicles);
        var deleted = await ctx.Vehicles.IgnoreQueryFilters().SingleAsync(v => v.Id == vehicle.Id);
        Assert.True(deleted.Deleted);
    }
}

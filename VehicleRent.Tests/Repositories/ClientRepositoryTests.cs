using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Repositories;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for ClientRepositoryTests.
/// </summary>
public class ClientRepositoryTests
{
    [Fact]
    /// <summary>
    /// Executes the AddAndGetById_Work test operation.
    /// </summary>
    public async Task AddAndGetById_Work()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new ClientRepository(ctx);
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL001");

        await repo.AddAsync(client);
        var loaded = await repo.GetByIdAsync(client.Id);

        Assert.NotNull(loaded);
        Assert.Equal("ana@example.com", loaded!.Email);
    }

    [Fact]
    /// <summary>
    /// Executes the GetAllAsync_FiltersByNameOrEmailAndVehicle test operation.
    /// </summary>
    public async Task GetAllAsync_FiltersByNameOrEmailAndVehicle()
    {
        using var ctx = TestDbContextFactory.Create();
        var c1 = new Client("Ana", "ana@example.com", "+351912345678", "DL001");
        var c2 = new Client("Bruno", "bruno@example.com", "+351912345679", "DL002");
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        ctx.Clients.AddRange(c1, c2);
        ctx.Vehicles.Add(vehicle);
        await ctx.SaveChangesAsync();
        ctx.RentalContracts.Add(new RentalContract(c2.Id, vehicle.Id, DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), 0));
        await ctx.SaveChangesAsync();

        var repo = new ClientRepository(ctx);
        var result = await repo.GetAllAsync(1, 10, "bruno", vehicle.Id);

        var item = Assert.Single(result.Items);
        Assert.Equal("Bruno", item.Name);
    }

    [Fact]
    /// <summary>
    /// Executes the ExistsByEmailAndDriverLicense_RespectExcludingId test operation.
    /// </summary>
    public async Task ExistsByEmailAndDriverLicense_RespectExcludingId()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL001");
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();
        var repo = new ClientRepository(ctx);

        Assert.True(await repo.ExistsByEmailAsync("ANA@EXAMPLE.COM"));
        Assert.False(await repo.ExistsByEmailAsync("ana@example.com", client.Id));
        Assert.True(await repo.ExistsByDriverLicenseAsync("dl001"));
        Assert.False(await repo.ExistsByDriverLicenseAsync("DL001", client.Id));
    }

    [Fact]
    /// <summary>
    /// Executes the DeleteAsync_WhenMissing_DoesNotThrow test operation.
    /// </summary>
    public async Task DeleteAsync_WhenMissing_DoesNotThrow()
    {
        using var ctx = TestDbContextFactory.Create();
        var repo = new ClientRepository(ctx);

        await repo.DeleteAsync(999);

        Assert.Empty(ctx.Clients);
    }

    [Fact]
    /// <summary>
    /// Executes the DeleteAsync_SoftDeletesClient test operation.
    /// </summary>
    public async Task DeleteAsync_SoftDeletesClient()
    {
        using var ctx = TestDbContextFactory.Create();
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL001");
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();
        var repo = new ClientRepository(ctx);

        await repo.DeleteAsync(client.Id);

        Assert.Empty(ctx.Clients);
        var deleted = await ctx.Clients.IgnoreQueryFilters().SingleAsync(c => c.Id == client.Id);
        Assert.True(deleted.Deleted);
    }
}

using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class VehicleServiceTests
{
    private static VehicleService CreateSut(InMemoryVehicleRepository repo, InMemoryRentalContractRepository? rentalRepo = null)
    {
        return new VehicleService(repo, rentalRepo ?? new InMemoryRentalContractRepository(), TestDistributedCacheFactory.Create());
    }

    [Fact]
    public async Task GetPagedForWebAsync_NormalizesInvalidInput()
    {
        var repo = new InMemoryVehicleRepository(SeedVehicles(12));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForWebAsync(page: 0, pageSize: 13);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task GetPagedForWebAsync_ClampsPageToLastPage()
    {
        var repo = new InMemoryVehicleRepository(SeedVehicles(15));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForWebAsync(page: 99, pageSize: 10);

        Assert.Equal(2, result.Page);
        Assert.Equal(2, repo.PagedCalls.Count);
        Assert.Equal((99, 10), repo.PagedCalls[0]);
        Assert.Equal((2, 10), repo.PagedCalls[1]);
    }

    [Fact]
    public async Task GetPagedForApiAsync_NormalizesPageSize()
    {
        var repo = new InMemoryVehicleRepository(SeedVehicles(5));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForApiAsync(page: 1, pageSize: 500);

        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task GetPagedForWebAsync_WhenAvailabilityStatusProvided_FiltersByRentalState()
    {
        var vehicles = SeedVehicles(2).ToArray();
        var repo = new InMemoryVehicleRepository(vehicles);
        var rentalRepo = new InMemoryRentalContractRepository();
        rentalRepo.CurrentlyRentedVehicleIds.Add(vehicles[0].Id);
        var sut = CreateSut(repo, rentalRepo);

        var rented = await sut.GetPagedForWebAsync(1, 10, availabilityStatus: true);
        var available = await sut.GetPagedForWebAsync(1, 10, availabilityStatus: false);

        Assert.Single(rented.Items);
        Assert.Equal(vehicles[0].Id, rented.Items.Single().Id);
        Assert.Single(available.Items);
        Assert.Equal(vehicles[1].Id, available.Items.Single().Id);
    }

    [Fact]
    public async Task GetAllForSelectionAsync_AppliesRentalStatus()
    {
        var vehicles = SeedVehicles(2).ToArray();
        var repo = new InMemoryVehicleRepository(vehicles);
        var rentalRepo = new InMemoryRentalContractRepository();
        rentalRepo.CurrentlyRentedVehicleIds.Add(vehicles[1].Id);
        var sut = CreateSut(repo, rentalRepo);

        var list = await sut.GetAllForSelectionAsync();

        Assert.False(list.Single(v => v.Id == vehicles[0].Id).IsCurrentlyRented);
        Assert.True(list.Single(v => v.Id == vehicles[1].Id).IsCurrentlyRented);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEntityWhenExists()
    {
        var vehicle = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([vehicle]);
        var sut = CreateSut(repo);

        var found = await sut.GetByIdAsync(vehicle.Id);

        Assert.NotNull(found);
        Assert.Equal(vehicle.Id, found!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        var found = await sut.GetByIdAsync(999);

        Assert.Null(found);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsEntity()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        var created = await sut.CreateAsync("Ford", "Fiesta", "AA-00-AA", FuelType.Petrol, 2020);

        Assert.True(created.Id > 0);
        Assert.Equal(1, repo.Count());
        Assert.Equal("Ford", created.Brand);
        Assert.Equal("AA-00-AA", created.LicensePlate);
    }

    [Fact]
    public async Task CreateAsync_InvalidEntity_ThrowsBusinessValidation()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("", "Fiesta", "AA-00-AA", FuelType.Petrol, 2020));

        Assert.Equal(BusinessErrorCodes.VehicleBrandRequired, ex.ErrorCode);
    }

    [Theory]
    [InlineData("Ford", "", "AA-00-AA", FuelType.Petrol, 2020, BusinessErrorCodes.VehicleModelRequired)]
    [InlineData("Ford", "Fiesta", "", FuelType.Petrol, 2020, BusinessErrorCodes.VehicleLicensePlateRequired)]
    [InlineData("Ford", "Fiesta", "AA00AA", FuelType.Petrol, 2020, BusinessErrorCodes.VehicleLicensePlateInvalidFormat)]
    [InlineData("Ford", "Fiesta", "AA-00-AA", FuelType.None, 2020, BusinessErrorCodes.VehicleFuelInvalid)]
    [InlineData("Ford", "Fiesta", "AA-00-AA", FuelType.Petrol, 1890, BusinessErrorCodes.VehicleManufacturingYearInvalid)]
    public async Task CreateAsync_InvalidPayload_ReturnsMappedBusinessError(
        string brand,
        string model,
        string licensePlate,
        FuelType fuel,
        int year,
        string expectedErrorCode)
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(brand, model, licensePlate, fuel, year));

        Assert.Equal(expectedErrorCode, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenDbUpdateFails_ReturnsDuplicatePlateCode()
    {
        var repo = new InMemoryVehicleRepository { AddException = new DbUpdateException("db") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Ford", "Fiesta", "AA-00-AA", FuelType.Petrol, 2020));

        Assert.Equal(BusinessErrorCodes.VehicleLicensePlateAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenUnexpectedArgumentException_ReturnsGenericValidationCode()
    {
        var repo = new InMemoryVehicleRepository { AddException = new ArgumentException("bad", "unknown") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Ford", "Fiesta", "AA-00-AA", FuelType.Petrol, 2020));

        Assert.Equal(BusinessErrorCodes.GenericValidation, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsEntityNotFound()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateAsync(999, "Ford", "Fiesta", "AA-00-AA", FuelType.Petrol, 2020));
    }

    [Fact]
    public async Task UpdateAsync_InvalidPayload_ThrowsBusinessValidation()
    {
        var vehicle = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([vehicle]);
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(vehicle.Id, "Ford", "Fiesta", "AA-00-AA", FuelType.None, 2020));
    }

    [Fact]
    public async Task UpdateAsync_WhenDbUpdateFails_ReturnsDuplicatePlateCode()
    {
        var vehicle = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([vehicle]) { UpdateException = new DbUpdateException("db") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(vehicle.Id, "Tesla", "Model 3", "BB-11-BB", FuelType.Electric, 2022));

        Assert.Equal(BusinessErrorCodes.VehicleLicensePlateAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_ValidPayload_UpdatesEntity()
    {
        var vehicle = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([vehicle]);
        var sut = CreateSut(repo);

        await sut.UpdateAsync(vehicle.Id, "Tesla", "Model 3", "BB-11-BB", FuelType.Electric, 2022);

        var updated = await sut.GetByIdAsync(vehicle.Id);
        Assert.NotNull(updated);
        Assert.Equal("Tesla", updated!.Brand);
        Assert.Equal("Model 3", updated.Model);
        Assert.Equal(FuelType.Electric, updated.Fuel);
        Assert.Equal(2022, updated.ManufacturingYear);
        Assert.Equal("BB-11-BB", updated.LicensePlate);
    }

    [Fact]
    public async Task CreateAsync_DuplicateLicensePlate_ThrowsBusinessValidation()
    {
        var existing = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([existing]);
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Ford", "Focus", existing.LicensePlate, FuelType.Petrol, 2021));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateLicensePlate_ThrowsBusinessValidation()
    {
        var vehicles = SeedVehicles(2).ToArray();
        var repo = new InMemoryVehicleRepository(vehicles);
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(vehicles[1].Id, "B2", "M2", vehicles[0].LicensePlate, FuelType.Petrol, 2021));
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsTrue_NotFound_Throws()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(12, ensureExists: true));
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsFalse_DoesNotThrowForMissing()
    {
        var repo = new InMemoryVehicleRepository();
        var sut = CreateSut(repo);

        await sut.DeleteAsync(12, ensureExists: false);

        Assert.Equal(0, repo.Count());
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsTrue_DeletesWhenPresent()
    {
        var vehicle = SeedVehicles(1).Single();
        var repo = new InMemoryVehicleRepository([vehicle]);
        var sut = CreateSut(repo);

        await sut.DeleteAsync(vehicle.Id, ensureExists: true);

        Assert.Equal(0, repo.Count());
    }

    private static IEnumerable<Vehicle> SeedVehicles(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            var v = new Vehicle($"Brand{i}", $"Model{i}", FuelType.Petrol, 2020, $"AA-{i:00}-BB");
            typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                .SetValue(v, (long)i);
            yield return v;
        }
    }
}

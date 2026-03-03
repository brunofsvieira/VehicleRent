using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class RentalContractServiceTests
{
    [Fact]
    public async Task GetPagedForWebAsync_NormalizesInvalidInput()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForWebAsync(page: 0, pageSize: 13);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10, null), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task GetPagedForWebAsync_ClampsPageToLastPage()
    {
        var seed = SeedContracts(15);
        var repo = new InMemoryRentalContractRepository(seed);
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForWebAsync(page: 99, pageSize: 10);

        Assert.Equal(2, result.Page);
        Assert.Equal((99, 10, null), repo.PagedCalls[0]);
        Assert.Equal((2, 10, null), repo.PagedCalls[1]);
    }

    [Fact]
    public async Task GetPagedForApiAsync_NormalizesPageSize()
    {
        var repo = new InMemoryRentalContractRepository(SeedContracts(5));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForApiAsync(page: 1, pageSize: 500);

        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10, null), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task GetPagedForWebAsync_PropagatesIsFinishedFilterOnClampedPage()
    {
        var repo = new InMemoryRentalContractRepository(SeedContracts(15));
        var sut = CreateSut(repo);

        _ = await sut.GetPagedForWebAsync(page: 99, pageSize: 10, isFinished: false);

        Assert.Equal((99, 10, false), repo.PagedCalls[0]);
        Assert.Equal((2, 10, false), repo.PagedCalls[1]);
    }

    [Fact]
    public async Task GetPagedForApiAsync_PropagatesIsFinishedFilterOnClampedPage()
    {
        var repo = new InMemoryRentalContractRepository(SeedContracts(15));
        var sut = CreateSut(repo);

        _ = await sut.GetPagedForApiAsync(page: 99, pageSize: 10, isFinished: false);

        Assert.Equal((99, 10, false), repo.PagedCalls[0]);
        Assert.Equal((2, 10, false), repo.PagedCalls[1]);
    }

    [Fact]
    public async Task CreateAsync_WhenClientDoesNotExist_ThrowsWithCode()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo, withClient: false, withVehicle: true);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalClientNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenVehicleDoesNotExist_ThrowsWithCode()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo, withClient: true, withVehicle: false);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalVehicleNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenOverlap_ThrowsWithCode()
    {
        var repo = new InMemoryRentalContractRepository { ForceOverlap = true };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalVehicleOverlap, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_Valid_CreatesContract()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var created = await sut.CreateAsync(1, 1, today, today.AddDays(1), 5);

        Assert.True(created.Id > 0);
        Assert.Equal(1, created.ClientId);
        Assert.Equal(1, created.VehicleId);
    }

    [Fact]
    public async Task CreateAsync_WhenEntityValidationFails_ReturnsMappedErrorCode()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), -1));

        Assert.Equal(BusinessErrorCodes.RentalInitialMileageInvalid, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenDbUpdateFails_ReturnsSaveFailedErrorCode()
    {
        var repo = new InMemoryRentalContractRepository { AddException = new DbUpdateException("db") };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalSaveFailed, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenUnexpectedArgumentException_ReturnsGenericValidationCode()
    {
        var repo = new InMemoryRentalContractRepository { AddException = new ArgumentException("bad", "unexpected") };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.GenericValidation, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ThrowsEntityNotFound()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateAsync(999, 1, 1, today, today.AddDays(1), 0));
    }

    [Fact]
    public async Task UpdateAsync_WhenEntityValidationFails_ReturnsMappedErrorCode()
    {
        var contract = SeedContracts(1).Single();
        var repo = new InMemoryRentalContractRepository([contract]);
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(contract.Id, 1, 1, today, today, 0));

        Assert.Equal(BusinessErrorCodes.RentalEndDateInvalid, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenOverlap_ThrowsWithCode()
    {
        var contract = SeedContracts(1).Single();
        var repo = new InMemoryRentalContractRepository([contract]) { ForceOverlap = true };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(contract.Id, 1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalVehicleOverlap, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenDbUpdateFails_ReturnsUpdateFailedErrorCode()
    {
        var contract = SeedContracts(1).Single();
        var repo = new InMemoryRentalContractRepository([contract]) { UpdateException = new DbUpdateException("db") };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(contract.Id, 1, 1, today, today.AddDays(1), 0));

        Assert.Equal(BusinessErrorCodes.RentalUpdateFailed, ex.ErrorCode);
    }

    [Theory]
    [InlineData("clientId", BusinessErrorCodes.RentalClientRequired)]
    [InlineData("vehicleId", BusinessErrorCodes.RentalVehicleRequired)]
    [InlineData("rentalStartDate", BusinessErrorCodes.RentalStartDatePast)]
    public async Task CreateAsync_WhenRepositoryThrowsMappedArgumentException_ReturnsExpectedCode(string paramName, string expectedCode)
    {
        var repo = new InMemoryRentalContractRepository { AddException = new ArgumentException("bad", paramName) };
        var sut = CreateSut(repo);
        var today = DateTime.UtcNow.Date;

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(1, 1, today, today.AddDays(1), 0));

        Assert.Equal(expectedCode, ex.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsTrue_WhenMissing_Throws()
    {
        var repo = new InMemoryRentalContractRepository();
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(55, true));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsContract()
    {
        var contract = SeedContracts(1).Single();
        var repo = new InMemoryRentalContractRepository([contract]);
        var sut = CreateSut(repo);

        var loaded = await sut.GetByIdAsync(contract.Id);

        Assert.NotNull(loaded);
        Assert.Equal(contract.Id, loaded!.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenContractIsActive_ThrowsBusinessValidation()
    {
        var contract = SeedContracts(1).Single();
        var repo = new InMemoryRentalContractRepository([contract]);
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() => sut.DeleteAsync(contract.Id, ensureExists: true));

        Assert.Equal(BusinessErrorCodes.RentalDeleteBlockedActiveContract, ex.ErrorCode);
    }

    private static RentalContractService CreateSut(InMemoryRentalContractRepository repo, bool withClient = true, bool withVehicle = true)
    {
        var clientSeed = withClient ? new[] { BuildClient(1) } : Array.Empty<Client>();
        var vehicleSeed = withVehicle ? new[] { BuildVehicle(1) } : Array.Empty<Vehicle>();
        var clientRepo = new InMemoryClientRepository(clientSeed);
        var vehicleRepo = new InMemoryVehicleRepository(vehicleSeed);

        return new RentalContractService(repo, clientRepo, vehicleRepo);
    }

    private static IEnumerable<RentalContract> SeedContracts(int count)
    {
        var today = DateTime.UtcNow.Date;
        for (var i = 1; i <= count; i++)
        {
            var c = new RentalContract(1, 1, today, today.AddDays(1), i);
            typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                .SetValue(c, (long)i);
            yield return c;
        }
    }

    private static Client BuildClient(long id)
    {
        var client = new Client("Client", "client@example.com", "+351912345678", "DL001");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, id);
        return client;
    }

    private static Vehicle BuildVehicle(long id)
    {
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2022, "AA-00-AA");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(vehicle, id);
        return vehicle;
    }
}

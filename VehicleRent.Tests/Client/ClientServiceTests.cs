using Microsoft.EntityFrameworkCore;
using VehicleRent.Models.Entities;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class ClientServiceTests
{
    private static ClientService CreateSut(InMemoryClientRepository repo)
    {
        return new ClientService(repo, new InMemoryRentalContractRepository(), TestDistributedCacheFactory.Create());
    }

    [Fact]
    public async Task GetPagedForWebAsync_NormalizesInvalidInput()
    {
        var repo = new InMemoryClientRepository(SeedClients(12));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForWebAsync(page: 0, pageSize: 13);

        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task GetPagedForWebAsync_ClampsPageToLastPage()
    {
        var repo = new InMemoryClientRepository(SeedClients(15));
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
        var repo = new InMemoryClientRepository(SeedClients(5));
        var sut = CreateSut(repo);

        var result = await sut.GetPagedForApiAsync(page: 1, pageSize: 999);

        Assert.Equal(10, result.PageSize);
        Assert.Equal((1, 10), repo.PagedCalls[0]);
    }

    [Fact]
    public async Task CreateAsync_PersistsAndReturnsEntity()
    {
        var repo = new InMemoryClientRepository();
        var sut = CreateSut(repo);

        var created = await sut.CreateAsync("Ana Silva", "ANA@EXAMPLE.COM", "+351912345678", "DL123");

        Assert.True(created.Id > 0);
        Assert.Equal(1, repo.Count());
        Assert.Equal("ana@example.com", created.Email);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsBusinessValidation()
    {
        var existing = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([existing]);
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Novo", existing.Email, "+351987654321", "DL999"));
    }

    [Fact]
    public async Task CreateAsync_DuplicateDriverLicense_ThrowsBusinessValidation()
    {
        var existing = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([existing]);
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Novo", "novo@example.com", "+351987654321", existing.DriverLicense));

        Assert.Equal(BusinessErrorCodes.ClientDriverLicenseAlreadyExists, ex.ErrorCode);
    }

    [Theory]
    [InlineData("", "ana@example.com", "+351912345678", "DL123", BusinessErrorCodes.ClientNameRequired)]
    [InlineData("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "ana@example.com", "+351912345678", "DL123", BusinessErrorCodes.ClientNameTooLong)]
    [InlineData("Ana", "", "+351912345678", "DL123", BusinessErrorCodes.ClientEmailRequired)]
    [InlineData("Ana", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@example.com", "+351912345678", "DL123", BusinessErrorCodes.ClientEmailTooLong)]
    [InlineData("Ana", "mail-invalido", "+351912345678", "DL123", BusinessErrorCodes.ClientEmailInvalidFormat)]
    [InlineData("Ana", "ana@example.com", "", "DL123", BusinessErrorCodes.ClientPhoneRequired)]
    [InlineData("Ana", "ana@example.com", "912345678", "DL123", BusinessErrorCodes.ClientPhoneInvalidFormat)]
    [InlineData("Ana", "ana@example.com", "+351912345678", "", BusinessErrorCodes.ClientDriverLicenseRequired)]
    public async Task CreateAsync_InvalidPayload_ReturnsMappedBusinessError(
        string name,
        string email,
        string phone,
        string driverLicense,
        string expectedErrorCode)
    {
        var repo = new InMemoryClientRepository();
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync(name, email, phone, driverLicense));

        Assert.Equal(expectedErrorCode, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenDbUpdateFails_ReturnsCombinedDuplicateCode()
    {
        var repo = new InMemoryClientRepository { AddException = new DbUpdateException("db") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Ana", "ana@example.com", "+351912345678", "DL123"));

        Assert.Equal(BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateAsync_WhenUnexpectedArgumentException_ReturnsGenericValidationCode()
    {
        var repo = new InMemoryClientRepository { AddException = new ArgumentException("bad", "unknown") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.CreateAsync("Ana", "ana@example.com", "+351912345678", "DL123"));

        Assert.Equal(BusinessErrorCodes.GenericValidation, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsEntityNotFound()
    {
        var repo = new InMemoryClientRepository();
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            sut.UpdateAsync(999, "Ana", "ana@example.com", "+351912345678", "DL123"));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsBusinessValidation()
    {
        var clients = SeedClients(2).ToArray();
        var repo = new InMemoryClientRepository(clients);
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(clients[1].Id, "Nome", clients[0].Email, "+351987654321", "DL999"));
    }

    [Fact]
    public async Task UpdateAsync_DuplicateDriverLicense_ThrowsBusinessValidation()
    {
        var clients = SeedClients(2).ToArray();
        var repo = new InMemoryClientRepository(clients);
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(clients[1].Id, "Nome", "novo@example.com", "+351987654321", clients[0].DriverLicense));

        Assert.Equal(BusinessErrorCodes.ClientDriverLicenseAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_ValidPayload_UpdatesEntity()
    {
        var client = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([client]);
        var sut = CreateSut(repo);

        await sut.UpdateAsync(client.Id, "Ana Maria", "ana.maria@example.com", "+351987654321", "DL777");

        var updated = await sut.GetByIdAsync(client.Id);
        Assert.NotNull(updated);
        Assert.Equal("Ana Maria", updated!.Name);
        Assert.Equal("ana.maria@example.com", updated.Email);
        Assert.Equal("+351987654321", updated.PhoneNumber);
        Assert.Equal("DL777", updated.DriverLicense);
    }

    [Fact]
    public async Task UpdateAsync_WhenDbUpdateFails_ReturnsCombinedDuplicateCode()
    {
        var client = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([client]) { UpdateException = new DbUpdateException("db") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(client.Id, "Ana Maria", "ana.maria@example.com", "+351987654321", "DL777"));

        Assert.Equal(BusinessErrorCodes.ClientEmailOrDriverLicenseAlreadyExists, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateAsync_WhenUnexpectedArgumentException_ReturnsGenericValidationCode()
    {
        var client = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([client]) { UpdateException = new ArgumentException("bad", "unknown") };
        var sut = CreateSut(repo);

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            sut.UpdateAsync(client.Id, "Ana Maria", "ana.maria@example.com", "+351987654321", "DL777"));

        Assert.Equal(BusinessErrorCodes.GenericValidation, ex.ErrorCode);
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsTrue_NotFound_Throws()
    {
        var repo = new InMemoryClientRepository();
        var sut = CreateSut(repo);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => sut.DeleteAsync(12, ensureExists: true));
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsFalse_DoesNotThrowForMissing()
    {
        var repo = new InMemoryClientRepository();
        var sut = CreateSut(repo);

        await sut.DeleteAsync(12, ensureExists: false);

        Assert.Equal(0, repo.Count());
    }

    [Fact]
    public async Task DeleteAsync_EnsureExistsTrue_DeletesWhenPresent()
    {
        var client = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([client]);
        var sut = CreateSut(repo);

        await sut.DeleteAsync(client.Id, ensureExists: true);

        Assert.Equal(0, repo.Count());
    }

    [Fact]
    public async Task DeleteAsync_WhenClientHasActiveRental_ThrowsBusinessValidation()
    {
        var client = SeedClients(1).Single();
        var repo = new InMemoryClientRepository([client]);
        var today = DateTime.UtcNow.Date;
        var activeContract = new RentalContract(client.Id, 10, today, today.AddDays(1), 0);
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(activeContract, 1L);
        var rentalRepo = new InMemoryRentalContractRepository([activeContract]);
        var sut = new ClientService(repo, rentalRepo, TestDistributedCacheFactory.Create());

        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() => sut.DeleteAsync(client.Id, ensureExists: true));

        Assert.Equal(BusinessErrorCodes.ClientDeleteBlockedActiveRental, ex.ErrorCode);
    }

    private static IEnumerable<Client> SeedClients(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            var client = new Client($"Client {i}", $"client{i}@example.com", $"+3519{i:00000000}", $"DL{i:000}");
            typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
                .SetValue(client, (long)i);
            yield return client;
        }
    }
}

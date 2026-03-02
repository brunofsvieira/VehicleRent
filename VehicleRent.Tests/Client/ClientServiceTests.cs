using VehicleRent.Models.Entities;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class ClientServiceTests
{
    private static ClientService CreateSut(InMemoryClientRepository repo)
    {
        return new ClientService(repo, TestDistributedCacheFactory.Create());
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

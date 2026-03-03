using Microsoft.EntityFrameworkCore;
using VehicleRent.Data;

namespace VehicleRent.Tests.TestDoubles;

internal static class TestDbContextFactory
{
    /// <summary>
    /// Executes the Create test operation.
    /// </summary>
    public static ApplicationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString("N"))
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

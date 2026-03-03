using VehicleRent.Models.Entities;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for RentalContractEntityTests.
/// </summary>
public class RentalContractEntityTests
{
    [Fact]
    /// <summary>
    /// Executes the Constructor_WithValidData_CreatesEntity test operation.
    /// </summary>
    public void Constructor_WithValidData_CreatesEntity()
    {
        var today = DateTime.UtcNow.Date;
        var contract = new RentalContract(1, 2, today, today.AddDays(2), 10);

        Assert.Equal(1, contract.ClientId);
        Assert.Equal(2, contract.VehicleId);
        Assert.Equal(today, contract.RentalStartDate);
        Assert.Equal(today.AddDays(2), contract.RentalEndDate);
        Assert.Equal(10, contract.InitialMileage);
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_InvalidClient_Throws test operation.
    /// </summary>
    public void Constructor_InvalidClient_Throws()
    {
        var today = DateTime.UtcNow.Date;
        var ex = Assert.Throws<ArgumentException>(() => new RentalContract(0, 2, today, today.AddDays(1), 0));
        Assert.Equal("clientId", ex.ParamName);
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_InvalidVehicle_Throws test operation.
    /// </summary>
    public void Constructor_InvalidVehicle_Throws()
    {
        var today = DateTime.UtcNow.Date;
        var ex = Assert.Throws<ArgumentException>(() => new RentalContract(1, 0, today, today.AddDays(1), 0));
        Assert.Equal("vehicleId", ex.ParamName);
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_StartDateInPast_Throws test operation.
    /// </summary>
    public void Constructor_StartDateInPast_Throws()
    {
        var today = DateTime.UtcNow.Date;
        var ex = Assert.Throws<ArgumentException>(() => new RentalContract(1, 2, today.AddDays(-1), today.AddDays(1), 0));
        Assert.Equal("rentalStartDate", ex.ParamName);
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_EndDateBeforeOrEqualStart_Throws test operation.
    /// </summary>
    public void Constructor_EndDateBeforeOrEqualStart_Throws()
    {
        var today = DateTime.UtcNow.Date;
        var ex = Assert.Throws<ArgumentException>(() => new RentalContract(1, 2, today, today, 0));
        Assert.Equal("rentalEndDate", ex.ParamName);
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_NegativeMileage_Throws test operation.
    /// </summary>
    public void Constructor_NegativeMileage_Throws()
    {
        var today = DateTime.UtcNow.Date;
        var ex = Assert.Throws<ArgumentException>(() => new RentalContract(1, 2, today, today.AddDays(1), -1));
        Assert.Equal("initialMileage", ex.ParamName);
    }
}

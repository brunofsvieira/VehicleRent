using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for VehicleEntityTests.
/// </summary>
public class VehicleEntityTests
{
    [Fact]
    /// <summary>
    /// Executes the Constructor_WithEmptyBrand_Throws test operation.
    /// </summary>
    public void Constructor_WithEmptyBrand_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Vehicle("", "M1", FuelType.Petrol, 2020, "AA-00-AA"));
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_WithNoneFuel_Throws test operation.
    /// </summary>
    public void Constructor_WithNoneFuel_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Vehicle("B1", "M1", FuelType.None, 2020, "AA-00-AA"));
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_WithYearOutOfRange_Throws test operation.
    /// </summary>
    public void Constructor_WithYearOutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Vehicle("B1", "M1", FuelType.Petrol, 1800, "AA-00-AA"));
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_WithEmptyLicensePlate_Throws test operation.
    /// </summary>
    public void Constructor_WithEmptyLicensePlate_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Vehicle("B1", "M1", FuelType.Petrol, 2020, ""));
    }

    [Fact]
    /// <summary>
    /// Executes the Constructor_WithInvalidLicensePlateFormat_Throws test operation.
    /// </summary>
    public void Constructor_WithInvalidLicensePlateFormat_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Vehicle("B1", "M1", FuelType.Petrol, 2020, "ABC-12-34"));
    }

    [Fact]
    /// <summary>
    /// Executes the UpdateVehicle_ValidData_UpdatesStateAndTouchesUpdateDate test operation.
    /// </summary>
    public void UpdateVehicle_ValidData_UpdatesStateAndTouchesUpdateDate()
    {
        var vehicle = new Vehicle("B1", "M1", FuelType.Petrol, 2020, "AA-00-AA");
        var before = vehicle.UpdateDate;

        Thread.Sleep(5);
        vehicle.UpdateVehicle("B2", "M2", FuelType.Electric, 2022, "BB-11-BB");

        Assert.Equal("B2", vehicle.Brand);
        Assert.Equal("M2", vehicle.Model);
        Assert.Equal(FuelType.Electric, vehicle.Fuel);
        Assert.Equal(2022, vehicle.ManufacturingYear);
        Assert.Equal("BB-11-BB", vehicle.LicensePlate);
        Assert.True(vehicle.UpdateDate >= before);
    }
}

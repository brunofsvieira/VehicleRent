using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Profiles;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for EntitiesProfileTests.
/// </summary>
public class EntitiesProfileTests
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntitiesProfileTests"/> class.
    /// </summary>
    public EntitiesProfileTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EntitiesProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    [Fact]
    /// <summary>
    /// Executes the MapsVehicleToVehicleDto test operation.
    /// </summary>
    public void MapsVehicleToVehicleDto()
    {
        var vehicle = new Vehicle("Toyota", "Yaris", FuelType.Hybrid, 2021, "AB-12-CD");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(vehicle, 55L);

        var dto = _mapper.Map<VehicleDto>(vehicle);

        Assert.Equal(55, dto.Id);
        Assert.Equal("Toyota", dto.Brand);
        Assert.Equal("Yaris", dto.Model);
        Assert.Equal(FuelType.Hybrid, dto.Fuel);
        Assert.Equal(2021, dto.ManufacturingYear);
        Assert.Equal("AB-12-CD", dto.LicensePlate);
    }

    [Fact]
    /// <summary>
    /// Executes the MapsCreateVehicleDtoToVehicle test operation.
    /// </summary>
    public void MapsCreateVehicleDtoToVehicle()
    {
        var dto = new CreateVehicleDto
        {
            Brand = "Tesla",
            Model = "Model Y",
            Fuel = FuelType.Electric,
            ManufacturingYear = 2023,
            LicensePlate = "ZZ-99-ZZ"
        };

        var vehicle = _mapper.Map<Vehicle>(dto);

        Assert.Equal("Tesla", vehicle.Brand);
        Assert.Equal("Model Y", vehicle.Model);
        Assert.Equal(FuelType.Electric, vehicle.Fuel);
        Assert.Equal(2023, vehicle.ManufacturingYear);
        Assert.Equal("ZZ-99-ZZ", vehicle.LicensePlate);
    }

    [Fact]
    /// <summary>
    /// Executes the MapsClientToClientDto test operation.
    /// </summary>
    public void MapsClientToClientDto()
    {
        var client = new Client("Ana Silva", "ana@example.com", "+351912345678", "DL123");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, 12L);

        var dto = _mapper.Map<ClientDto>(client);

        Assert.Equal(12, dto.Id);
        Assert.Equal("Ana Silva", dto.Name);
        Assert.Equal("ana@example.com", dto.Email);
        Assert.Equal("+351912345678", dto.PhoneNumber);
        Assert.Equal("DL123", dto.DriverLicense);
    }

    [Fact]
    /// <summary>
    /// Executes the MapsCreateClientDtoToClient test operation.
    /// </summary>
    public void MapsCreateClientDtoToClient()
    {
        var dto = new CreateClientDto
        {
            Name = "Ana Silva",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        };

        var client = _mapper.Map<Client>(dto);

        Assert.Equal("Ana Silva", client.Name);
        Assert.Equal("ana@example.com", client.Email);
        Assert.Equal("+351912345678", client.PhoneNumber);
        Assert.Equal("DL123", client.DriverLicense);
    }
}

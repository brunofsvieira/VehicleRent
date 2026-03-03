using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleRent.Controllers;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Profiles;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for VehiclesApiControllerTests.
/// </summary>
public class VehiclesApiControllerTests
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehiclesApiControllerTests"/> class.
    /// </summary>
    public VehiclesApiControllerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EntitiesProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    /// <summary>
    /// Executes the Get_ReturnsOkWithPagedDtos test operation.
    /// </summary>
    public async Task Get_ReturnsOkWithPagedDtos()
    {
        var service = new StubVehicleService
        {
            OnGetPagedForApi = (_, _, _, _) => Task.FromResult(new PagedResult<Vehicle>
            {
                Items = new[] { BuildVehicle(1, "B1", "M1") },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Get(1, 10);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<PagedResult<VehicleDto>>(ok.Value);
        Assert.Single(payload.Items);
        Assert.Equal(1, payload.TotalCount);
        Assert.Equal("AA-00-AA", payload.Items.First().LicensePlate);
    }

    [Fact]
    /// <summary>
    /// Executes the GetById_WhenMissing_ReturnsNotFound test operation.
    /// </summary>
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var service = new StubVehicleService { OnGetById = _ => Task.FromResult<Vehicle?>(null) };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.GetById(123);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    /// <summary>
    /// Executes the GetById_WhenExists_ReturnsOk test operation.
    /// </summary>
    public async Task GetById_WhenExists_ReturnsOk()
    {
        var service = new StubVehicleService { OnGetById = _ => Task.FromResult<Vehicle?>(BuildVehicle(4, "B1", "M1")) };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.GetById(4);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var dto = Assert.IsType<VehicleDto>(ok.Value);
        Assert.Equal(4, dto.Id);
    }

    [Fact]
    /// <summary>
    /// Executes the Post_WhenValid_ReturnsCreatedAtAction test operation.
    /// </summary>
    public async Task Post_WhenValid_ReturnsCreatedAtAction()
    {
        var service = new StubVehicleService
        {
            OnCreate = (_, _, _, _, _) => Task.FromResult(BuildVehicle(7, "Tesla", "Model 3", FuelType.Electric, 2024, "ZZ-77-ZZ"))
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Post(new CreateVehicleDto
        {
            Brand = "Tesla",
            Model = "Model 3",
            Fuel = FuelType.Electric,
            ManufacturingYear = 2024,
            LicensePlate = "ZZ-77-ZZ"
        });

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(VehiclesApiController.GetById), created.ActionName);
    }

    [Fact]
    /// <summary>
    /// Executes the Post_WhenBusinessValidationFails_ReturnsValidationProblem test operation.
    /// </summary>
    public async Task Post_WhenBusinessValidationFails_ReturnsValidationProblem()
    {
        var service = new StubVehicleService
        {
            OnCreate = (_, _, _, _, _) => throw new BusinessValidationException("invalid")
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Post(new CreateVehicleDto
        {
            Brand = "",
            Model = "M",
            Fuel = FuelType.Petrol,
            ManufacturingYear = 2024,
            LicensePlate = "AA-00-AA"
        });

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.Count > 0);
    }

    [Fact]
    /// <summary>
    /// Executes the Put_WhenNotFound_ReturnsNotFound test operation.
    /// </summary>
    public async Task Put_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubVehicleService
        {
            OnUpdate = (_, _, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Put(1, new UpdateVehicleDto
        {
            Brand = "B",
            Model = "M",
            Fuel = FuelType.Petrol,
            ManufacturingYear = 2023,
            LicensePlate = "AA-00-AA"
        });

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    /// <summary>
    /// Executes the Put_WhenBusinessValidationFails_ReturnsValidationProblem test operation.
    /// </summary>
    public async Task Put_WhenBusinessValidationFails_ReturnsValidationProblem()
    {
        var service = new StubVehicleService
        {
            OnUpdate = (_, _, _, _, _, _) => throw new BusinessValidationException("bad")
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Put(1, new UpdateVehicleDto
        {
            Brand = "",
            Model = "M",
            Fuel = FuelType.Petrol,
            ManufacturingYear = 2023,
            LicensePlate = "AA-00-AA"
        });

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.Count > 0);
    }

    [Fact]
    /// <summary>
    /// Executes the Delete_WhenNotFound_ReturnsNotFound test operation.
    /// </summary>
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubVehicleService
        {
            OnDelete = (_, _) => throw new EntityNotFoundException("missing")
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Delete(3);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    /// <summary>
    /// Executes the Delete_WhenFound_ReturnsNoContent test operation.
    /// </summary>
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        var service = new StubVehicleService
        {
            OnDelete = (_, _) => Task.CompletedTask
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Delete(3);

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    /// <summary>
    /// Executes the Delete_WhenBlockedByBusinessRule_ReturnsValidationProblem test operation.
    /// </summary>
    public async Task Delete_WhenBlockedByBusinessRule_ReturnsValidationProblem()
    {
        var service = new StubVehicleService
        {
            OnDelete = (_, _) => throw new BusinessValidationException(BusinessErrorCodes.VehicleDeleteBlockedActiveRental, "blocked")
        };
        var sut = new VehiclesApiController(service, _mapper);

        var response = await sut.Delete(3);

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.ContainsKey(BusinessErrorCodes.VehicleDeleteBlockedActiveRental));
    }

    private static Vehicle BuildVehicle(long id, string brand, string model, FuelType fuel = FuelType.Petrol, int year = 2022, string licensePlate = "AA-00-AA")
    {
        var vehicle = new Vehicle(brand, model, fuel, year, licensePlate);
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(vehicle, id);
        return vehicle;
    }
}

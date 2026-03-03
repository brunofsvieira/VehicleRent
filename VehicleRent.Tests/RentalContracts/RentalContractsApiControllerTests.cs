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

public class RentalContractsApiControllerTests
{
    private readonly IMapper _mapper;

    public RentalContractsApiControllerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EntitiesProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_ReturnsOkWithPagedDtos()
    {
        var today = DateTime.UtcNow.Date;
        var service = new StubRentalContractService
        {
            OnGetPagedForApi = (_, _, _, _) => Task.FromResult(new PagedResult<RentalContract>
            {
                Items = new[] { BuildContract(1, 1, 1, today, today.AddDays(1), 0) },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = new RentalContractsApiController(service, _mapper);

        var response = await sut.Get();

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<PagedResult<RentalContractDto>>(ok.Value);
        Assert.Single(payload.Items);
    }

    [Fact]
    public async Task Post_WhenBusinessValidationFails_ReturnsValidationProblemWithErrorCodeKey()
    {
        var service = new StubRentalContractService
        {
            OnCreate = (_, _, _, _, _) => throw new BusinessValidationException(BusinessErrorCodes.RentalVehicleOverlap, "overlap")
        };
        var sut = new RentalContractsApiController(service, _mapper);
        var today = DateTime.UtcNow.Date;

        var response = await sut.Post(new CreateRentalContractDto
        {
            ClientId = 1,
            VehicleId = 1,
            RentalStartDate = today,
            RentalEndDate = today.AddDays(1),
            InitialMileage = 0
        });

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.ContainsKey(BusinessErrorCodes.RentalVehicleOverlap));
    }

    [Fact]
    public async Task Put_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubRentalContractService
        {
            OnUpdate = (_, _, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = new RentalContractsApiController(service, _mapper);
        var today = DateTime.UtcNow.Date;

        var response = await sut.Put(1, new UpdateRentalContractDto
        {
            ClientId = 1,
            VehicleId = 1,
            RentalStartDate = today,
            RentalEndDate = today.AddDays(1),
            InitialMileage = 0
        });

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        var service = new StubRentalContractService
        {
            OnDelete = (_, _) => Task.CompletedTask
        };
        var sut = new RentalContractsApiController(service, _mapper);

        var response = await sut.Delete(1);

        Assert.IsType<NoContentResult>(response);
    }

    private static RentalContract BuildContract(long id, long clientId, long vehicleId, DateTime start, DateTime end, int mileage)
    {
        var contract = new RentalContract(clientId, vehicleId, start, end, mileage);
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(contract, id);
        return contract;
    }
}

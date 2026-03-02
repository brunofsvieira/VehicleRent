using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using VehicleRent.Controllers;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Profiles;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class ClientsApiControllerTests
{
    private readonly IMapper _mapper;

    public ClientsApiControllerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<EntitiesProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Get_ReturnsOkWithPagedDtos()
    {
        var service = new StubClientService
        {
            OnGetPagedForApi = (_, _) => Task.FromResult(new PagedResult<Client>
            {
                Items = new[] { BuildClient(1, "Ana", "ana@example.com") },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Get(1, 10);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<PagedResult<ClientDto>>(ok.Value);
        Assert.Single(payload.Items);
        Assert.Equal("ana@example.com", payload.Items.First().Email);
    }

    [Fact]
    public async Task GetById_WhenMissing_ReturnsNotFound()
    {
        var service = new StubClientService { OnGetById = _ => Task.FromResult<Client?>(null) };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.GetById(123);

        Assert.IsType<NotFoundResult>(response.Result);
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsOk()
    {
        var service = new StubClientService { OnGetById = _ => Task.FromResult<Client?>(BuildClient(4, "Ana", "ana@example.com")) };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.GetById(4);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var dto = Assert.IsType<ClientDto>(ok.Value);
        Assert.Equal(4, dto.Id);
    }

    [Fact]
    public async Task Post_WhenValid_ReturnsCreatedAtAction()
    {
        var service = new StubClientService
        {
            OnCreate = (_, _, _, _) => Task.FromResult(BuildClient(7, "Ana", "ana@example.com"))
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Post(new CreateClientDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        });

        var created = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(ClientsApiController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Post_WhenBusinessValidationFails_ReturnsValidationProblem()
    {
        var service = new StubClientService
        {
            OnCreate = (_, _, _, _) => throw new BusinessValidationException("invalid")
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Post(new CreateClientDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        });

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.Count > 0);
    }

    [Fact]
    public async Task Put_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubClientService
        {
            OnUpdate = (_, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Put(1, new UpdateClientDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        });

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Put_WhenBusinessValidationFails_ReturnsValidationProblem()
    {
        var service = new StubClientService
        {
            OnUpdate = (_, _, _, _, _) => throw new BusinessValidationException("bad")
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Put(1, new UpdateClientDto
        {
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        });

        var bad = Assert.IsType<ObjectResult>(response);
        var details = Assert.IsType<ValidationProblemDetails>(bad.Value);
        Assert.True(details.Errors.Count > 0);
    }

    [Fact]
    public async Task Delete_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubClientService
        {
            OnDelete = (_, _) => throw new EntityNotFoundException("missing")
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Delete(3);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task Delete_WhenFound_ReturnsNoContent()
    {
        var service = new StubClientService
        {
            OnDelete = (_, _) => Task.CompletedTask
        };
        var sut = new ClientsApiController(service, _mapper);

        var response = await sut.Delete(3);

        Assert.IsType<NoContentResult>(response);
    }

    private static Client BuildClient(long id, string name, string email)
    {
        var client = new Client(name, email, "+351912345678", "DL123");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, id);
        return client;
    }
}

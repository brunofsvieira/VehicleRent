using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VehicleRent.Controllers;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.ViewModels;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class ClientsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithPagedViewModel()
    {
        var service = new StubClientService
        {
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<Client>
            {
                Items = new[] { BuildClient(1, "Ana", "ana@example.com") },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = CreateSut(service);

        var result = await sut.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PagedResult<ClientViewModel>>(view.Model);
        Assert.Single(model.Items);
    }

    [Fact]
    public void Create_Get_ReturnsDefaultModel()
    {
        var service = new StubClientService
        {
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<Client>())
        };
        var sut = CreateSut(service);

        var result = sut.Create();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ClientViewModel>(view.Model);
        Assert.Equal(string.Empty, model.Name);
        Assert.Equal("+351", model.PhoneNumber);
    }

    [Fact]
    public async Task Create_Post_WhenBusinessValidationFails_ReturnsViewWithModelError()
    {
        var service = new StubClientService
        {
            OnCreate = (_, _, _, _) => throw new BusinessValidationException(BusinessErrorCodes.ClientEmailAlreadyExists, "duplicate")
        };
        var sut = CreateSut(service);
        var vm = new ClientViewModel
        {
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        };

        var result = await sut.Create(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(sut.ModelState.IsValid);
        Assert.Same(vm, view.Model);
    }

    [Fact]
    public async Task Update_Post_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubClientService
        {
            OnUpdate = (_, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = CreateSut(service);
        var vm = new ClientViewModel
        {
            Id = 9,
            Name = "Ana",
            Email = "ana@example.com",
            PhoneNumber = "+351912345678",
            DriverLicense = "DL123"
        };

        var result = await sut.Update(vm);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Post_RedirectsToIndex()
    {
        var service = new StubClientService
        {
            OnDelete = (_, _) => Task.CompletedTask
        };
        var sut = CreateSut(service);

        var result = await sut.Delete(3, 2, 20);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(2, redirect.RouteValues!["page"]);
        Assert.Equal(20, redirect.RouteValues["pageSize"]);
    }

    [Fact]
    public async Task Delete_Post_WhenBlockedByBusinessRule_RedirectsWithTempDataError()
    {
        var service = new StubClientService
        {
            OnDelete = (_, _) => throw new BusinessValidationException(BusinessErrorCodes.ClientDeleteBlockedActiveRental, "blocked")
        };
        var sut = CreateSut(service);

        var tempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
            new DefaultHttpContext(),
            new NullTempDataProvider());
        sut.TempData = tempData;

        var result = await sut.Delete(3, 2, 20);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.True(sut.TempData.ContainsKey("ErrorMessage"));
    }

    private static ClientsController CreateSut(StubClientService service)
    {
        var vehicleService = new StubVehicleService
        {
            OnGetAllForSelection = () => Task.FromResult<IReadOnlyList<Vehicle>>(Array.Empty<Vehicle>()),
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<Vehicle>
            {
                Items = Array.Empty<Vehicle>(),
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            }),
            OnGetPagedForApi = (_, _, _, _) => Task.FromResult(new PagedResult<Vehicle>
            {
                Items = Array.Empty<Vehicle>(),
                Page = 1,
                PageSize = 10,
                TotalCount = 0
            }),
            OnGetById = _ => Task.FromResult<Vehicle?>(null),
            OnCreate = (_, _, _, _, _) => Task.FromResult(new Vehicle("B", "M", FuelType.Petrol, 2020, "AA-00-AA")),
            OnUpdate = (_, _, _, _, _, _) => Task.CompletedTask,
            OnDelete = (_, _) => Task.CompletedTask
        };

        return new ClientsController(service, vehicleService, new InMemoryRentalContractRepository(), TestDistributedCacheFactory.Create());
    }

    private static Client BuildClient(long id, string name, string email)
    {
        var client = new Client(name, email, "+351912345678", "DL123");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, id);
        return client;
    }

    private sealed class NullTempDataProvider : Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}

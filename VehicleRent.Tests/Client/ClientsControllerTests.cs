using Microsoft.AspNetCore.Mvc;
using VehicleRent.Controllers;
using VehicleRent.Models.Entities;
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
            OnGetPagedForWeb = (_, _) => Task.FromResult(new PagedResult<Client>
            {
                Items = new[] { BuildClient(1, "Ana", "ana@example.com") },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = new ClientsController(service);

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
            OnGetPagedForWeb = (_, _) => Task.FromResult(new PagedResult<Client>())
        };
        var sut = new ClientsController(service);

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
            OnCreate = (_, _, _, _) => throw new BusinessValidationException("invalid")
        };
        var sut = new ClientsController(service);
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
        var sut = new ClientsController(service);
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
        var sut = new ClientsController(service);

        var result = await sut.Delete(3, 2, 20);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(2, redirect.RouteValues!["page"]);
        Assert.Equal(20, redirect.RouteValues["pageSize"]);
    }

    private static Client BuildClient(long id, string name, string email)
    {
        var client = new Client(name, email, "+351912345678", "DL123");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, id);
        return client;
    }
}

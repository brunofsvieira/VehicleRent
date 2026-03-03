using Microsoft.AspNetCore.Mvc;
using VehicleRent.Controllers;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.ViewModels;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

/// <summary>
/// Represents unit tests for VehiclesControllerTests.
/// </summary>
public class VehiclesControllerTests
{
    [Fact]
    /// <summary>
    /// Executes the Index_ReturnsViewWithPagedViewModel test operation.
    /// </summary>
    public async Task Index_ReturnsViewWithPagedViewModel()
    {
        var service = new StubVehicleService
        {
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<Vehicle>
            {
                Items = new[] { BuildVehicle(1, "B1", "M1") },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };

        var sut = CreateSut(service);
        var result = await sut.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PagedResult<VehicleViewModel>>(view.Model);
        Assert.Single(model.Items);
    }

    [Fact]
    /// <summary>
    /// Executes the Create_Get_ReturnsDefaultModel test operation.
    /// </summary>
    public void Create_Get_ReturnsDefaultModel()
    {
        var service = new StubVehicleService
        {
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<Vehicle>())
        };
        var sut = CreateSut(service);

        var result = sut.Create();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<VehicleViewModel>(view.Model);
        Assert.Equal(string.Empty, model.Brand);
        Assert.Equal(string.Empty, model.LicensePlate);
    }

    [Fact]
    /// <summary>
    /// Executes the Create_Post_WhenBusinessValidationFails_ReturnsViewWithModelError test operation.
    /// </summary>
    public async Task Create_Post_WhenBusinessValidationFails_ReturnsViewWithModelError()
    {
        var service = new StubVehicleService
        {
            OnCreate = (_, _, _, _, _) => throw new BusinessValidationException(BusinessErrorCodes.VehicleLicensePlateAlreadyExists, "duplicate")
        };
        var sut = CreateSut(service);
        var vm = new VehicleViewModel { Brand = "B", Model = "M", LicensePlate = "AA-00-AA", Fuel = FuelType.Petrol, ManufacturingYear = 2022 };

        var result = await sut.Create(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.False(sut.ModelState.IsValid);
        Assert.Same(vm, view.Model);
    }

    [Fact]
    /// <summary>
    /// Executes the Update_Post_WhenNotFound_ReturnsNotFound test operation.
    /// </summary>
    public async Task Update_Post_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubVehicleService
        {
            OnUpdate = (_, _, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = CreateSut(service);
        var vm = new VehicleViewModel { Id = 9, Brand = "B", Model = "M", LicensePlate = "AA-00-AA", Fuel = FuelType.Petrol, ManufacturingYear = 2022 };

        var result = await sut.Update(vm);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    /// <summary>
    /// Executes the Delete_Post_RedirectsToIndex test operation.
    /// </summary>
    public async Task Delete_Post_RedirectsToIndex()
    {
        var service = new StubVehicleService
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

    private static VehiclesController CreateSut(StubVehicleService service)
    {
        var clientService = new StubClientService
        {
            OnGetAllForSelection = () => Task.FromResult<IReadOnlyList<Client>>(Array.Empty<Client>())
        };
        return new VehiclesController(service, clientService, TestDistributedCacheFactory.Create());
    }

    private static Vehicle BuildVehicle(long id, string brand, string model)
    {
        var vehicle = new Vehicle(brand, model, FuelType.Petrol, 2022, "AA-00-AA");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(vehicle, id);
        return vehicle;
    }
}

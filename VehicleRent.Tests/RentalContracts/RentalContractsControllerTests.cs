using Microsoft.AspNetCore.Mvc;
using VehicleRent.Controllers;
using VehicleRent.Models.Entities;
using VehicleRent.Models.Enumerators;
using VehicleRent.Models.ViewModels;
using VehicleRent.Services.Exceptions;
using VehicleRent.Tests.TestDoubles;

namespace VehicleRent.Tests;

public class RentalContractsControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithPagedViewModel()
    {
        var today = DateTime.UtcNow.Date;
        var service = new StubRentalContractService
        {
            OnGetPagedForWeb = (_, _, _, _) => Task.FromResult(new PagedResult<RentalContract>
            {
                Items = new[] { BuildContract(1, 1, 1, today, today.AddDays(1), 0) },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            })
        };
        var sut = CreateSut(service);

        var result = await sut.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<PagedResult<RentalContractViewModel>>(view.Model);
        Assert.Single(model.Items);
    }

    [Fact]
    public async Task Create_Post_WhenBusinessValidationFails_AddsTranslatedModelError()
    {
        var service = new StubRentalContractService
        {
            OnCreate = (_, _, _, _, _) => throw new BusinessValidationException(BusinessErrorCodes.RentalVehicleOverlap, "overlap")
        };
        var sut = CreateSut(service);
        var today = DateTime.UtcNow.Date;
        var vm = new RentalContractViewModel
        {
            ClientId = 1,
            VehicleId = 1,
            RentalStartDate = today,
            RentalEndDate = today.AddDays(1),
            InitialMileage = 0
        };

        var result = await sut.Create(vm);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(vm, view.Model);
        var error = Assert.Single(sut.ModelState[string.Empty]!.Errors);
        Assert.Equal("O veículo ja tem um contrato sobreposto neste periodo.", error.ErrorMessage);
    }

    [Fact]
    public async Task Update_Post_WhenNotFound_ReturnsNotFound()
    {
        var service = new StubRentalContractService
        {
            OnUpdate = (_, _, _, _, _, _) => throw new EntityNotFoundException("missing")
        };
        var sut = CreateSut(service);
        var today = DateTime.UtcNow.Date;
        var vm = new RentalContractViewModel
        {
            Id = 1,
            ClientId = 1,
            VehicleId = 1,
            RentalStartDate = today,
            RentalEndDate = today.AddDays(1),
            InitialMileage = 0
        };

        var result = await sut.Update(vm);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Post_RedirectsToIndex()
    {
        var service = new StubRentalContractService
        {
            OnDelete = (_, _) => Task.CompletedTask
        };
        var sut = CreateSut(service);

        var result = await sut.Delete(3, 2, 20, 1, 1);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal(2, redirect.RouteValues!["page"]);
        Assert.Equal(20, redirect.RouteValues["pageSize"]);
        Assert.Equal(1L, redirect.RouteValues["clientId"]);
        Assert.Equal(1L, redirect.RouteValues["vehicleId"]);
    }

    private static RentalContractsController CreateSut(StubRentalContractService rentalService)
    {
        var clients = new[] { BuildClient(1) };
        var vehicles = new[] { BuildVehicle(1) };

        var clientService = new StubClientService
        {
            OnGetAllForSelection = () => Task.FromResult<IReadOnlyList<Client>>(clients)
        };

        var vehicleService = new StubVehicleService
        {
            OnGetAllForSelection = () => Task.FromResult<IReadOnlyList<Vehicle>>(vehicles)
        };

        return new RentalContractsController(rentalService, clientService, vehicleService, TestDistributedCacheFactory.Create());
    }

    private static RentalContract BuildContract(long id, long clientId, long vehicleId, DateTime start, DateTime end, int mileage)
    {
        var contract = new RentalContract(clientId, vehicleId, start, end, mileage);
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(contract, id);
        return contract;
    }

    private static Client BuildClient(long id)
    {
        var client = new Client("Ana", "ana@example.com", "+351912345678", "DL123");
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(client, id);
        return client;
    }

    private static Vehicle BuildVehicle(long id)
    {
        var vehicle = new Vehicle("Brand", "Model", FuelType.Petrol, 2020, "AA-00-AA");
        vehicle.SetRentalStatus(false);
        typeof(BaseEntity).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)!
            .SetValue(vehicle, id);
        return vehicle;
    }
}

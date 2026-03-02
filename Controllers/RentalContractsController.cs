using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleRent.Models.Entities;
using VehicleRent.Models.ViewModels;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [Route("RentalContracts")]
    public class RentalContractsController : Controller
    {
        private readonly IRentalContractService _service;
        private readonly IClientService _clientService;
        private readonly IVehicleService _vehicleService;

        public RentalContractsController(
            IRentalContractService service,
            IClientService clientService,
            IVehicleService vehicleService)
        {
            _service = service;
            _clientService = clientService;
            _vehicleService = vehicleService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetPagedForWebAsync(page, pageSize);

            var vmPaged = new PagedResult<RentalContractViewModel>
            {
                Items = paged.Items.Select(RentalContractViewModel.FromEntity),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };

            return View(vmPaged);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var vm = NewDefaults();
            await PopulateSelectionsAsync(vm.ClientId, vm.VehicleId);
            return View(vm);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] RentalContractViewModel contract)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(contract.ClientId, contract.VehicleId);
                return View(contract);
            }

            try
            {
                await _service.CreateAsync(contract.ClientId, contract.VehicleId, contract.RentalStartDate, contract.RentalEndDate, contract.InitialMileage);
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(contract.ClientId, contract.VehicleId);
                return View(contract);
            }
        }

        [HttpGet("Update")]
        public async Task<IActionResult> Update(long? id)
        {
            if (!id.HasValue)
            {
                var vm = NewDefaults();
                await PopulateSelectionsAsync(vm.ClientId, vm.VehicleId);
                return View(vm);
            }

            var entity = await _service.GetByIdAsync(id.Value);
            if (entity is null) return NotFound();

            var model = RentalContractViewModel.FromEntity(entity);
            await PopulateSelectionsAsync(model.ClientId, model.VehicleId);
            return View(model);
        }

        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] RentalContractViewModel contract)
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectionsAsync(contract.ClientId, contract.VehicleId);
                return View(contract);
            }
            if (!contract.Id.HasValue) return BadRequest();

            try
            {
                await _service.UpdateAsync(contract.Id.Value, contract.ClientId, contract.VehicleId, contract.RentalStartDate, contract.RentalEndDate, contract.InitialMileage);
                return RedirectToAction(nameof(Index));
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectionsAsync(contract.ClientId, contract.VehicleId);
                return View(contract);
            }
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] long id, [FromForm] int page = 1, [FromForm] int pageSize = 10)
        {
            await _service.DeleteAsync(id, ensureExists: false);
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }

        private async Task PopulateSelectionsAsync(long selectedClientId, long selectedVehicleId)
        {
            var clients = await _clientService.GetAllForSelectionAsync();
            var vehicles = await _vehicleService.GetAllForSelectionAsync();
            var availableVehicles = vehicles
                .Where(v => !v.IsCurrentlyRented || v.Id == selectedVehicleId)
                .OrderBy(v => v.LicensePlate)
                .ToList();

            ViewBag.Clients = clients.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Name} ({c.Email})",
                Selected = c.Id == selectedClientId
            }).ToList();

            ViewBag.Vehicles = availableVehicles.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.LicensePlate,
                Selected = v.Id == selectedVehicleId
            }).ToList();
        }

        private static RentalContractViewModel NewDefaults()
        {
            var today = DateTime.UtcNow.Date;
            return new RentalContractViewModel
            {
                ClientId = 0,
                VehicleId = 0,
                RentalStartDate = today,
                RentalEndDate = today.AddDays(1),
                InitialMileage = 0
            };
        }
    }
}

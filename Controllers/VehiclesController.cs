using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VehicleRent.Infrastructure;
using VehicleRent.Models.Entities;
using VehicleRent.Models.ViewModels;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [Route("Vehicles")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _service;
        private readonly IClientService _clientService;
        private readonly IDistributedCache _cache;

        public VehiclesController(IVehicleService service, IClientService clientService, IDistributedCache cache)
        {
            _service = service;
            _clientService = clientService;
            _cache = cache;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] long? clientId = null, [FromQuery] string? licensePlate = null)
        {
            var paged = await _service.GetPagedForWebAsync(page, pageSize, licensePlate, clientId);

            var cached = await _cache.GetStringAsync(CacheKeys.VehicleClientFilterOptions);
            List<ClientFilterCacheItem>? clientOptions = null;

            if (!string.IsNullOrWhiteSpace(cached))
            {
                clientOptions = JsonSerializer.Deserialize<List<ClientFilterCacheItem>>(cached);
            }

            if (clientOptions is null)
            {
                var clients = await _clientService.GetAllForSelectionAsync();
                clientOptions = clients
                    .OrderBy(c => c.Name)
                    .ThenBy(c => c.Email)
                    .Select(c => new ClientFilterCacheItem(c.Id, $"{c.Name} ({c.Email})"))
                    .ToList();

                var payload = JsonSerializer.Serialize(clientOptions);
                await _cache.SetStringAsync(
                    CacheKeys.VehicleClientFilterOptions,
                    payload,
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(20),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                    });
            }

            ViewBag.ClientFilterOptions = clientOptions.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Display,
                Selected = clientId.HasValue && clientId.Value == c.Id
            }).ToList();
            ViewBag.CurrentClientId = clientId;
            ViewBag.CurrentLicensePlate = licensePlate ?? string.Empty;

            var vmPaged = new PagedResult<VehicleViewModel>
            {
                Items = paged.Items.Select(VehicleViewModel.FromEntity),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };

            return View(vmPaged);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(NewVehicleDefaults());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] VehicleViewModel vehicle)
        {
            if (!ModelState.IsValid) return View(vehicle);

            try
            {
                await _service.CreateAsync(vehicle.Brand, vehicle.Model, vehicle.LicensePlate, vehicle.Fuel, vehicle.ManufacturingYear);
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vehicle);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Update")]
        public async Task<IActionResult> Update(long? id)
        {
            if (!id.HasValue) return View(NewVehicleDefaults());

            var entity = await _service.GetByIdAsync(id.Value);
            if (entity is null) return NotFound();

            return View(VehicleViewModel.FromEntity(entity));
        }

        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] VehicleViewModel vehicle)
        {
            if (!ModelState.IsValid) return View(vehicle);
            if (!vehicle.Id.HasValue) return BadRequest();

            try
            {
                await _service.UpdateAsync(vehicle.Id.Value, vehicle.Brand, vehicle.Model, vehicle.LicensePlate, vehicle.Fuel, vehicle.ManufacturingYear);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vehicle);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] long id, [FromForm] int page = 1, [FromForm] int pageSize = 10, [FromForm] long? clientId = null, [FromForm] string? licensePlate = null)
        {
            await _service.DeleteAsync(id, ensureExists: false);
            return RedirectToAction(nameof(Index), new { page, pageSize, clientId, licensePlate });
        }

        private static VehicleViewModel NewVehicleDefaults()
        {
            return new VehicleViewModel
            {
                Brand = string.Empty,
                Model = string.Empty,
                LicensePlate = string.Empty,
                Fuel = Models.Enumerators.FuelType.None,
                ManufacturingYear = DateTime.UtcNow.Year
            };
        }

        private sealed record ClientFilterCacheItem(long Id, string Display);
    }
}

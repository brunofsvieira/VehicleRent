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
    [Route("Clients")]
    public class ClientsController : Controller
    {
        private readonly IClientService _service;
        private readonly IVehicleService _vehicleService;
        private readonly IDistributedCache _cache;

        public ClientsController(IClientService service, IVehicleService vehicleService, IDistributedCache cache)
        {
            _service = service;
            _vehicleService = vehicleService;
            _cache = cache;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] long? vehicleId = null, [FromQuery] string? nameOrEmail = null)
        {
            var paged = await _service.GetPagedForWebAsync(page, pageSize, nameOrEmail, vehicleId);

            var cached = await _cache.GetStringAsync(CacheKeys.ClientVehicleFilterOptions);
            List<VehicleFilterCacheItem>? vehicleOptions = null;

            if (!string.IsNullOrWhiteSpace(cached))
            {
                vehicleOptions = JsonSerializer.Deserialize<List<VehicleFilterCacheItem>>(cached);
            }

            if (vehicleOptions is null)
            {
                var vehicles = await _vehicleService.GetAllForSelectionAsync();
                vehicleOptions = vehicles
                    .OrderBy(v => v.LicensePlate)
                    .Select(v => new VehicleFilterCacheItem(v.Id, v.LicensePlate))
                    .ToList();

                var payload = JsonSerializer.Serialize(vehicleOptions);
                await _cache.SetStringAsync(
                    CacheKeys.ClientVehicleFilterOptions,
                    payload,
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = TimeSpan.FromMinutes(20),
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
                    });
            }

            ViewBag.VehicleFilterOptions = vehicleOptions.Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = v.Id.ToString(),
                Text = v.LicensePlate,
                Selected = vehicleId.HasValue && vehicleId.Value == v.Id
            }).ToList();
            ViewBag.CurrentVehicleId = vehicleId;
            ViewBag.CurrentNameOrEmail = nameOrEmail ?? string.Empty;

            var vmPaged = new PagedResult<ClientViewModel>
            {
                Items = paged.Items.Select(ClientViewModel.FromEntity),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };

            return View(vmPaged);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(NewClientDefaults());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] ClientViewModel client)
        {
            if (!ModelState.IsValid) return View(client);

            try
            {
                await _service.CreateAsync(client.Name, client.Email, client.PhoneNumber, client.DriverLicense);
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, FrontendErrorMessages.ToPt(ex.ErrorCode));
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Update")]
        public async Task<IActionResult> Update(long? id)
        {
            if (!id.HasValue) return View(NewClientDefaults());

            var entity = await _service.GetByIdAsync(id.Value);
            if (entity is null) return NotFound();

            return View(ClientViewModel.FromEntity(entity));
        }

        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] ClientViewModel client)
        {
            if (!ModelState.IsValid) return View(client);
            if (!client.Id.HasValue) return BadRequest();

            try
            {
                await _service.UpdateAsync(client.Id.Value, client.Name, client.Email, client.PhoneNumber, client.DriverLicense);
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, FrontendErrorMessages.ToPt(ex.ErrorCode));
                return View(client);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete([FromForm] long id, [FromForm] int page = 1, [FromForm] int pageSize = 10, [FromForm] long? vehicleId = null, [FromForm] string? nameOrEmail = null)
        {
            await _service.DeleteAsync(id, ensureExists: false);
            return RedirectToAction(nameof(Index), new { page, pageSize, vehicleId, nameOrEmail });
        }

        private static ClientViewModel NewClientDefaults()
        {
            return new ClientViewModel
            {
                Name = string.Empty,
                Email = string.Empty,
                PhoneNumber = "+351",
                DriverLicense = string.Empty
            };
        }

        private sealed record VehicleFilterCacheItem(long Id, string LicensePlate);
    }
}

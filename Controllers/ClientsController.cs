using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using VehicleRent.Infrastructure;
using VehicleRent.Models.Entities;
using VehicleRent.Models.ViewModels;
using VehicleRent.Repositories;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [Route("Clients")]
    /// <summary>
    /// Represents the ClientsController component.
    /// </summary>
    public class ClientsController : Controller
    {
        private readonly IClientService _service;
        private readonly IVehicleService _vehicleService;
        private readonly IRentalContractRepository _rentalContractRepository;
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsController"/> class.
        /// </summary>
        public ClientsController(IClientService service, IVehicleService vehicleService, IRentalContractRepository rentalContractRepository, IDistributedCache cache)
        {
            _service = service;
            _vehicleService = vehicleService;
            _rentalContractRepository = rentalContractRepository;
            _cache = cache;
        }

        [HttpGet("")]
        /// <summary>
        /// Executes the Index operation.
        /// </summary>
        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] long? vehicleId = null, [FromQuery] long? clientId = null)
        {
            var paged = await _service.GetPagedForWebAsync(page, pageSize, clientId, vehicleId);

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

            var cachedClients = await _cache.GetStringAsync(CacheKeys.RentalContractClientFilterOptions);
            List<ClientFilterCacheItem>? clientOptions = null;
            if (!string.IsNullOrWhiteSpace(cachedClients))
            {
                clientOptions = JsonSerializer.Deserialize<List<ClientFilterCacheItem>>(cachedClients);
            }
            if (clientOptions is null)
            {
                var clients = await _service.GetAllForSelectionAsync();
                clientOptions = clients
                    .OrderBy(c => c.Name)
                    .ThenBy(c => c.Email)
                    .Select(c => new ClientFilterCacheItem(c.Id, $"{c.Name} ({c.Email})"))
                    .ToList();

                await _cache.SetStringAsync(
                    CacheKeys.RentalContractClientFilterOptions,
                    JsonSerializer.Serialize(clientOptions),
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
            ViewBag.CurrentClientId = clientId;
            ViewBag.ActiveClientIds = await _rentalContractRepository.GetCurrentlyActiveClientIdsAsync(DateTime.UtcNow.Date);


            ViewBag.ClientFilterOptions = clientOptions.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Display,
                Selected = clientId.HasValue && clientId.Value == c.Id
            }).ToList();

            ViewBag.VehicleFilterOptions = vehicleOptions
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = v.LicensePlate,
                    Selected = vehicleId.HasValue && vehicleId.Value == v.Id
                })
                .ToList();

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
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
        public IActionResult Create()
        {
            return View(NewClientDefaults());
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Executes the Create operation.
        /// </summary>
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
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
        public async Task<IActionResult> Update(long? id)
        {
            if (!id.HasValue) return View(NewClientDefaults());

            var entity = await _service.GetByIdAsync(id.Value);
            if (entity is null) return NotFound();

            return View(ClientViewModel.FromEntity(entity));
        }

        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Executes the Update operation.
        /// </summary>
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
        /// <summary>
        /// Executes the Delete operation.
        /// </summary>
        public async Task<IActionResult> Delete([FromForm] long id, [FromForm] int page = 1, [FromForm] int pageSize = 10, [FromForm] long? vehicleId = null, [FromForm] long? clientId = null)
        {
            try
            {
                await _service.DeleteAsync(id, ensureExists: false);
            }
            catch (BusinessValidationException ex)
            {
                TempData["ErrorMessage"] = FrontendErrorMessages.ToPt(ex.ErrorCode);
            }
            return RedirectToAction(nameof(Index), new { page, pageSize, vehicleId, clientId });
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
        private sealed record ClientFilterCacheItem(long Id, string Display);
    }
}

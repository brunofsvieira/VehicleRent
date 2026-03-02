using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    public class VehiclesApiController : ControllerBase
    {
        private readonly IVehicleService _service;
        private readonly IMapper _mapper;

        public VehiclesApiController(IVehicleService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<VehicleDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetPagedForApiAsync(page, pageSize);

            return Ok(new PagedResult<VehicleDto>
            {
                Items = paged.Items.Select(v => _mapper.Map<VehicleDto>(v)),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<VehicleDto>> GetById(long id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is null) return NotFound();
            return Ok(_mapper.Map<VehicleDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateVehicleDto dto)
        {
            try
            {
                var entity = await _service.CreateAsync(dto.Brand, dto.Model, dto.LicensePlate, dto.Fuel, dto.ManufacturingYear);
                var result = _mapper.Map<VehicleDto>(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Put(long id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto.Brand, dto.Model, dto.LicensePlate, dto.Fuel, dto.ManufacturingYear);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _service.DeleteAsync(id, ensureExists: true);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }
    }
}

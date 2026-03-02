using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [ApiController]
    [Route("api/rental-contracts")]
    public class RentalContractsApiController : ControllerBase
    {
        private readonly IRentalContractService _service;
        private readonly IMapper _mapper;

        public RentalContractsApiController(IRentalContractService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<RentalContractDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetPagedForApiAsync(page, pageSize);

            return Ok(new PagedResult<RentalContractDto>
            {
                Items = paged.Items.Select(rc => _mapper.Map<RentalContractDto>(rc)),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RentalContractDto>> GetById(long id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is null) return NotFound();
            return Ok(_mapper.Map<RentalContractDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateRentalContractDto dto)
        {
            try
            {
                var entity = await _service.CreateAsync(dto.ClientId, dto.VehicleId, dto.RentalStartDate, dto.RentalEndDate, dto.InitialMileage);
                var result = _mapper.Map<RentalContractDto>(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Put(long id, [FromBody] UpdateRentalContractDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto.ClientId, dto.VehicleId, dto.RentalStartDate, dto.RentalEndDate, dto.InitialMileage);
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

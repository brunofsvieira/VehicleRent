using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VehicleRent.Models.DTOs;
using VehicleRent.Models.Entities;
using VehicleRent.Services;
using VehicleRent.Services.Exceptions;

namespace VehicleRent.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsApiController : ControllerBase
    {
        private readonly IClientService _service;
        private readonly IMapper _mapper;

        public ClientsApiController(IClientService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<ClientDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var paged = await _service.GetPagedForApiAsync(page, pageSize);

            return Ok(new PagedResult<ClientDto>
            {
                Items = paged.Items.Select(c => _mapper.Map<ClientDto>(c)),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ClientDto>> GetById(long id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is null) return NotFound();
            return Ok(_mapper.Map<ClientDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateClientDto dto)
        {
            try
            {
                var entity = await _service.CreateAsync(dto.Name, dto.Email, dto.PhoneNumber, dto.DriverLicense);
                var result = _mapper.Map<ClientDto>(entity);
                return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
            }
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Put(long id, [FromBody] UpdateClientDto dto)
        {
            try
            {
                await _service.UpdateAsync(id, dto.Name, dto.Email, dto.PhoneNumber, dto.DriverLicense);
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

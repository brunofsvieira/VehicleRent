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
    /// <summary>
    /// Represents the ClientsApiController component.
    /// </summary>
    public class ClientsApiController : ControllerBase
    {
        private readonly IClientService _service;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientsApiController"/> class.
        /// </summary>
        public ClientsApiController(IClientService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        /// <summary>
        /// Executes the Get operation.
        /// </summary>
        public async Task<ActionResult<PagedResult<ClientDto>>> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] long? clientId = null, [FromQuery] long? vehicleId = null)
        {
            var paged = await _service.GetPagedForApiAsync(page, pageSize, clientId, vehicleId);

            return Ok(new PagedResult<ClientDto>
            {
                Items = paged.Items.Select(c => _mapper.Map<ClientDto>(c)),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id:long}")]
        /// <summary>
        /// Executes the GetById operation.
        /// </summary>
        public async Task<ActionResult<ClientDto>> GetById(long id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity is null) return NotFound();
            return Ok(_mapper.Map<ClientDto>(entity));
        }

        [HttpPost]
        /// <summary>
        /// Executes the Post operation.
        /// </summary>
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
                ModelState.AddModelError(ex.ErrorCode, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPut("{id:long}")]
        /// <summary>
        /// Executes the Put operation.
        /// </summary>
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
                ModelState.AddModelError(ex.ErrorCode, ex.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpDelete("{id:long}")]
        /// <summary>
        /// Executes the Delete operation.
        /// </summary>
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
            catch (BusinessValidationException ex)
            {
                ModelState.AddModelError(ex.ErrorCode, ex.Message);
                return ValidationProblem(ModelState);
            }
        }
    }
}

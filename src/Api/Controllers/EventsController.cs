using System;
using System.Threading.Tasks;
using Api.Dto;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly ICreateEventService _createEventService;
        private readonly IEventRepository _eventRepository;

        public EventsController(ICreateEventService createEventService, IEventRepository eventRepository)
        {
            _createEventService = createEventService;
            _eventRepository = eventRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EventCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var id = await _createEventService.CreateEventAsync(dto.Title, dto.Description, dto.Location, dto.StartTime, dto.EndTime, dto.Capacity, dto.RegistrationOpen, dto.RegistrationClose, dto.Timezone ?? "UTC");
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var ev = await _eventRepository.GetByIdAsync(id);
            if (ev == null) return NotFound();
            return Ok(ev);
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            try
            {
                var cancelService = (Application.Services.CancelEventService)HttpContext.RequestServices.GetService(typeof(Application.Services.CancelEventService))!;
                await cancelService.CancelAsync(id);
                return Ok();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }
    }
}

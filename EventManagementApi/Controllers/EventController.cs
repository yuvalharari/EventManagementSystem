using EventManagementApi.DTOs;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementApi.Controllers
{
    [ApiController]
    [Route("api/event")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IWeatherService _weatherService;
        private readonly ILogger<EventController> _logger;

        public EventController(IEventService eventService, IWeatherService weatherService, ILogger<EventController> logger)
        {
            _eventService = eventService;
            _weatherService = weatherService;
            _logger = logger;
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<IEnumerable<EventResponseDto>>> GetSchedule()
        {
            return Ok(await _eventService.GetEventsScheduleAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventResponseDto>> GetEventById(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound("Event not found");

            return ev;
        }

        [HttpPost]
        public async Task<ActionResult<EventResponseDto>> CreateEvent([FromBody] EventCreateDto dto)
        {
            var created = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
        {
            var updated = await _eventService.UpdateEventAsync(id, dto);
            if (!updated)
                return NotFound("Event not found");

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var deleted = await _eventService.DeleteEventAsync(id);
            if (!deleted)
                return NotFound("Event not found");

            return NoContent();
        }

        [HttpPost("{id:int}/registration")]
        public async Task<IActionResult> RegisterUserToEvent(int id, [FromBody] UserRegisterDto dto)
        {
            var result = await _eventService.RegisterUserToEventAsync(id, dto);

            return result switch
            {
                RegistrationResult.Registered => Ok("User registered successfully"),
                RegistrationResult.EventNotFound => NotFound("Event not found"),
                RegistrationResult.AlreadyRegistered => Conflict("User is already registered to this event"),
                RegistrationResult.EventFull => Conflict("Event is full"),
                _ => StatusCode(StatusCodes.Status500InternalServerError)
            };
        }

        [HttpGet("{id:int}/registration")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetEventUsers(int id)
        {
            var users = await _eventService.GetUsersForEventAsync(id);
            if (users == null)
                return NotFound("Event not found");

            return Ok(users);
        }

        [HttpGet("{id:int}/weather")]
        public async Task<ActionResult<WeatherResponseDto>> GetEventWeather(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound("Event not found");

            try
            {
                return await _weatherService.GetWeatherAsync(ev.Location, DateOnly.FromDateTime(ev.StartDate));
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Weather service request failed for event {EventId}", id);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Weather service is currently unavailable");
            }
        }
    }
}

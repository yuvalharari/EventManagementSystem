using EventManagementApi.DTOs;
using EventManagementApi.Models;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace EventManagementApi.Controllers
{
    [ApiController]
    [Route("api/event")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IWeatherService _weatherService;


        public EventController(IEventService eventService, IWeatherService weatherService)
        {
            _eventService = eventService;
            _weatherService = weatherService;
        }


        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventCreateDto dto)
        {
            var created = await _eventService.CreateEventAsync(dto);
            return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventById(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound();

            return ev;
        }

        [HttpPost("{id}/registration")]
        public async Task<ActionResult<string>> RegisterUserToEvent(int id, [FromBody] UserRegisterDto dto)
        {
            var result = await _eventService.RegisterUserToEventAsync(id, dto);

            if (result == "Event not found")
                return NotFound(result);

            if (result == "User already registered to this event")
                return Conflict(result);

            return Ok(result);
        }

        [HttpGet("{id}/registration")]
        public async Task<ActionResult<IEnumerable<User>>> GetEventUsers(int id)
        {
            var users = await _eventService.GetUsersForEventAsync(id);
            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
        {
            var updated = await _eventService.UpdateEventAsync(id, dto);
            if (!updated)
                return NotFound("Event not found");

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var deleted = await _eventService.DeleteEventAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("/schedule")]
        public async Task<ActionResult<IEnumerable<Event>>> GetSchedule()
        {
            var events = await _eventService.GetEventsScheduleAsync();
            return Ok(events);
        }




        [HttpGet("{id}/weather")]
        public async Task<IActionResult> GetEventWeather(int id)
        {
            var ev = await _eventService.GetEventByIdAsync(id);
            if (ev == null)
                return NotFound("Event not found");

            var weather = await _weatherService.GetWeatherAsync(ev.Location);
            return Ok(weather);
        }



    }
}

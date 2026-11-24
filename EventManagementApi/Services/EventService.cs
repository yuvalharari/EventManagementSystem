using EventManagementApi.DTOs;
using EventManagementApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Services
{
    public class EventService : IEventService
    {
        private readonly EventsContext _context;

        public EventService(EventsContext context)
        {
            _context = context;
        }

        public async Task<EventResponseDto> CreateEventAsync(EventCreateDto dto)
        {
            var newEvent = new Event
            {
                Name = dto.Name.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location.Trim(),
                MaxRegistrations = dto.MaxRegistrations
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return ToDto(newEvent, registeredCount: 0);
        }

        public async Task<EventResponseDto?> GetEventByIdAsync(int id)
        {
            return await _context.Events
                .Where(e => e.Id == id)
                .Select(e => new { Event = e, Count = e.EventUsers.Count })
                .Select(x => ToDto(x.Event, x.Count))
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<EventResponseDto>> GetEventsScheduleAsync()
        {
            return await _context.Events
                .OrderBy(e => e.StartDate)
                .Select(e => new { Event = e, Count = e.EventUsers.Count })
                .Select(x => ToDto(x.Event, x.Count))
                .ToListAsync();
        }

        public async Task<bool> UpdateEventAsync(int id, EventUpdateDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return false;

            ev.Name = dto.Name.Trim();
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.MaxRegistrations = dto.MaxRegistrations;
            ev.Location = dto.Location.Trim();

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteEventAsync(int eventId)
        {
            var ev = await _context.Events
                .Include(e => e.EventUsers)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                return false;

            _context.EventUsers.RemoveRange(ev.EventUsers);
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<RegistrationResult> RegisterUserToEventAsync(int eventId, UserRegisterDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return RegistrationResult.EventNotFound;

            // A user is identified by name + date of birth, so registering the same person
            // twice reuses the existing user instead of creating a duplicate
            var name = dto.Name.Trim();
            var dateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == name && u.DateOfBirth == dateOfBirth);

            if (user != null)
            {
                bool alreadyRegistered = await _context.EventUsers
                    .AnyAsync(eu => eu.EventRef == eventId && eu.UserRef == user.Id);

                if (alreadyRegistered)
                    return RegistrationResult.AlreadyRegistered;
            }

            int registeredCount = await _context.EventUsers.CountAsync(eu => eu.EventRef == eventId);
            if (registeredCount >= ev.MaxRegistrations)
                return RegistrationResult.EventFull;

            if (user == null)
            {
                user = new User { Name = name, DateOfBirth = dateOfBirth };
                _context.Users.Add(user);
            }

            _context.EventUsers.Add(new EventUser
            {
                EventRefNavigation = ev,
                UserRefNavigation = user,
                Creation = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return RegistrationResult.Registered;
        }

        public async Task<IEnumerable<UserResponseDto>?> GetUsersForEventAsync(int eventId)
        {
            if (!await _context.Events.AnyAsync(e => e.Id == eventId))
                return null;

            return await _context.EventUsers
                .Where(eu => eu.EventRef == eventId)
                .OrderBy(eu => eu.Creation)
                .Select(eu => new UserResponseDto(
                    eu.UserRefNavigation.Id,
                    eu.UserRefNavigation.Name,
                    eu.UserRefNavigation.DateOfBirth,
                    eu.Creation))
                .ToListAsync();
        }

        private static EventResponseDto ToDto(Event ev, int registeredCount) => new(
            ev.Id,
            ev.Name,
            ev.StartDate,
            ev.EndDate,
            ev.Location,
            ev.MaxRegistrations,
            registeredCount,
            $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(ev.Location)}");
    }
}

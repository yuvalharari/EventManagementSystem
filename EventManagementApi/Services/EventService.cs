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

        public async Task<Event> CreateEventAsync(EventCreateDto dto)
        {
            var newEvent = new Event
            {
                Name = dto.Name,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                MaxRegistrations = dto.MaxRegistrations
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return newEvent;
        }

        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<string> RegisterUserToEventAsync(int eventId, UserRegisterDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return "Event not found";

            var user = new User
            {
                Name = dto.Name,
                DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            bool alreadyRegistered = await _context.EventUsers
                .AnyAsync(eu => eu.EventRef == eventId && eu.UserRef == user.Id);

            if (alreadyRegistered)
                return "User already registered";

            var registration = new EventUser
            {
                EventRef = eventId,
                UserRef = user.Id
            };

            _context.EventUsers.Add(registration);
            await _context.SaveChangesAsync();

            return "User registered successfully";
        }

        public async Task<List<User>> GetEventUsersAsync(int eventId)
        {
            var users = await _context.EventUsers
                .Where(eu => eu.EventRef == eventId)
                .Select(eu => eu.UserRefNavigation)
                .ToListAsync();

            return users;
        }

        public async Task<IEnumerable<User>> GetUsersForEventAsync(int eventId)
        {
            var users = await _context.EventUsers
                .Where(eu => eu.EventRef == eventId)
                .Select(eu => eu.UserRefNavigation)
                .ToListAsync();

            return users;
        }


        public async Task<bool> UpdateEventAsync(int id, EventUpdateDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return false;

            ev.Name = dto.Name;
            ev.StartDate = dto.StartDate;
            ev.EndDate = dto.EndDate;
            ev.MaxRegistrations = dto.MaxRegistrations;
            ev.Location = dto.Location;

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

        public async Task<IEnumerable<Event>> GetEventsScheduleAsync()
        {
            return await _context.Events.ToListAsync();
        }

       
    }
}

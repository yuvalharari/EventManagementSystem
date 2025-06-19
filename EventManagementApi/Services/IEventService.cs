using EventManagementApi.DTOs;
using EventManagementApi.Models;

namespace EventManagementApi.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(EventCreateDto dto);
        Task<Event?> GetEventByIdAsync(int id);
        Task<string> RegisterUserToEventAsync(int eventId, UserRegisterDto dto);
        Task<IEnumerable<User>> GetUsersForEventAsync(int eventId);

        Task<bool> UpdateEventAsync(int id, EventUpdateDto dto);
        Task<bool> DeleteEventAsync(int eventId);

        Task<IEnumerable<Event>> GetEventsScheduleAsync();

        



    }
}

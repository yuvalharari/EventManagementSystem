using EventManagementApi.DTOs;

namespace EventManagementApi.Services
{
    public enum RegistrationResult
    {
        Registered,
        EventNotFound,
        AlreadyRegistered,
        EventFull
    }

    public interface IEventService
    {
        Task<EventResponseDto> CreateEventAsync(EventCreateDto dto);
        Task<EventResponseDto?> GetEventByIdAsync(int id);
        Task<IEnumerable<EventResponseDto>> GetEventsScheduleAsync();
        Task<bool> UpdateEventAsync(int id, EventUpdateDto dto);
        Task<bool> DeleteEventAsync(int eventId);

        Task<RegistrationResult> RegisterUserToEventAsync(int eventId, UserRegisterDto dto);
        Task<IEnumerable<UserResponseDto>?> GetUsersForEventAsync(int eventId);
    }
}

using EventManagementApi.DTOs;

namespace EventManagementApi.Services
{
    public interface IWeatherService
    {
        Task<WeatherResponseDto> GetWeatherAsync(string location, DateOnly date);
    }
}

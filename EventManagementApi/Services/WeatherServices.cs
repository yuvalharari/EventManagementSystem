using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventManagementApi.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private static readonly string[] WeatherOptions = new[]
        {
            "Sunny", "Cloudy", "Rainy", "Windy", "Stormy", "Snowy", "Foggy", "Hot", "Cold"
        };

        public WeatherService(IMemoryCache cache, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
        }

        public async Task<string> GetWeatherAsync(string location)
        {
            if (_cache.TryGetValue(location, out string cachedWeather))
                return cachedWeather;

          
            var random = new Random();
            string condition = WeatherOptions[random.Next(WeatherOptions.Length)];
            string weather = $"{condition} in {location}";

            
            _cache.Set(location, weather, TimeSpan.FromDays(7));

            return weather;
        }
    }
}

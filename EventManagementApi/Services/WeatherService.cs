using System.Globalization;
using System.Text.Json;
using EventManagementApi.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace EventManagementApi.Services
{
    // Weather data from Open-Meteo (https://open-meteo.com): free and requires no API key.
    // Forecast API: about 3 months back up to 16 days ahead. Archive API: older dates.
    public class WeatherService : IWeatherService
    {
        private const string GeocodingUrl = "https://geocoding-api.open-meteo.com/v1/search";
        private const string ForecastUrl = "https://api.open-meteo.com/v1/forecast";
        private const string ArchiveUrl = "https://archive-api.open-meteo.com/v1/archive";

        private const int ForecastDaysAhead = 15;
        private const int ForecastDaysBack = 90;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;

        public WeatherService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<WeatherResponseDto> GetWeatherAsync(string location, DateOnly date)
        {
            string cacheKey = $"weather:{location.ToLowerInvariant()}:{date:yyyy-MM-dd}";
            if (_cache.TryGetValue(cacheKey, out WeatherResponseDto? cached) && cached != null)
                return cached;

            var result = await FetchWeatherAsync(location, date);
            _cache.Set(cacheKey, result, CacheDuration);
            return result;
        }

        private async Task<WeatherResponseDto> FetchWeatherAsync(string location, DateOnly date)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (date > today.AddDays(ForecastDaysAhead))
                return Unavailable(location, date, $"A forecast is available up to {ForecastDaysAhead + 1} days before the event.");

            var coordinates = await GeocodeAsync(location);
            if (coordinates == null)
                return Unavailable(location, date, "Location not found.");

            var (latitude, longitude, resolvedName) = coordinates.Value;
            bool useForecast = date >= today.AddDays(-ForecastDaysBack);

            string daily = useForecast
                ? "weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max"
                : "weather_code,temperature_2m_max,temperature_2m_min";
            string url = $"{(useForecast ? ForecastUrl : ArchiveUrl)}" +
                         $"?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
                         $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
                         $"&daily={daily}&timezone=auto&start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}";

            using var doc = JsonDocument.Parse(await _httpClient.GetStringAsync(url));
            if (!doc.RootElement.TryGetProperty("daily", out var dailyData))
                return Unavailable(resolvedName, date, "Weather data is not available for this date.");

            int? code = FirstInt(dailyData, "weather_code");
            return new WeatherResponseDto(
                resolvedName,
                date,
                ForecastAvailable: code != null,
                Summary: code != null ? DescribeWeatherCode(code.Value) : "Weather data is not available for this date.",
                MaxTemperatureC: FirstDouble(dailyData, "temperature_2m_max"),
                MinTemperatureC: FirstDouble(dailyData, "temperature_2m_min"),
                PrecipitationProbability: FirstInt(dailyData, "precipitation_probability_max"));
        }

        private async Task<(double Latitude, double Longitude, string Name)?> GeocodeAsync(string location)
        {
            string url = $"{GeocodingUrl}?name={Uri.EscapeDataString(location)}&count=1&language=en&format=json";
            using var doc = JsonDocument.Parse(await _httpClient.GetStringAsync(url));

            if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                return null;

            var place = results[0];
            string name = place.GetProperty("name").GetString() ?? location;
            if (place.TryGetProperty("country", out var country))
                name += $", {country.GetString()}";

            return (place.GetProperty("latitude").GetDouble(), place.GetProperty("longitude").GetDouble(), name);
        }

        private static WeatherResponseDto Unavailable(string location, DateOnly date, string reason) =>
            new(location, date, false, reason, null, null, null);

        private static double? FirstDouble(JsonElement daily, string field) =>
            daily.TryGetProperty(field, out var values) && values.GetArrayLength() > 0 &&
            values[0].ValueKind == JsonValueKind.Number ? values[0].GetDouble() : null;

        private static int? FirstInt(JsonElement daily, string field) =>
            FirstDouble(daily, field) is double value ? (int)value : null;

        // WMO weather interpretation codes: https://open-meteo.com/en/docs
        private static string DescribeWeatherCode(int code) => code switch
        {
            0 => "Clear sky",
            1 => "Mainly clear",
            2 => "Partly cloudy",
            3 => "Overcast",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            56 or 57 => "Freezing drizzle",
            61 => "Light rain",
            63 => "Rain",
            65 => "Heavy rain",
            66 or 67 => "Freezing rain",
            71 or 73 or 75 or 77 => "Snow",
            80 or 81 or 82 => "Rain showers",
            85 or 86 => "Snow showers",
            95 => "Thunderstorm",
            96 or 99 => "Thunderstorm with hail",
            _ => "Unknown"
        };
    }
}

namespace EventManagementApi.DTOs
{
    // Shapes returned by the API, so database entities are never exposed directly

    public record EventResponseDto(
        int Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        string Location,
        int MaxRegistrations,
        int RegisteredCount,
        string GoogleMapsLink);

    public record UserResponseDto(
        int Id,
        string Name,
        DateOnly DateOfBirth,
        DateTime RegisteredAt);

    public record WeatherResponseDto(
        string Location,
        DateOnly Date,
        bool ForecastAvailable,
        string Summary,
        double? MaxTemperatureC,
        double? MinTemperatureC,
        int? PrecipitationProbability);
}

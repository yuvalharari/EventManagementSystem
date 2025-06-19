using System.Threading.Tasks;

namespace EventManagementApi.Services
{
    public interface IWeatherService
    {
        Task<string> GetWeatherAsync(string location);
    }
}

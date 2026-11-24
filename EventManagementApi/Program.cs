using EventManagementApi.Models;
using EventManagementApi.Services;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<EventsContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EventsConnection")));

            builder.Services.AddMemoryCache();
            builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
            builder.Services.AddScoped<IEventService, EventService>();

            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ClientPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("ClientPolicy");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

using EventManagementApi.Models;
using Microsoft.EntityFrameworkCore;
using EventManagementApi.Services;

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
            builder.Services.AddHttpClient();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("corsapp", policy =>
                {
                    policy.WithOrigins("*")
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            builder.Services.AddScoped<IWeatherService, WeatherService>();


            builder.Services.AddControllers();
            builder.Services.AddScoped<IEventService, EventService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("corsapp");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

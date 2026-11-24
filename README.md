# Event Management System

A full-stack event management system: an **ASP.NET Core 8 Web API** backed by **SQL Server**, and a lightweight **HTML/JavaScript client**. Users can create and manage events, register participants, and see a real weather forecast for each event's location and date.

## Features

- **Event management:** Create, view, update, and delete events
- **Participant registration:**
  - Users are identified by name + date of birth, so the same person is never stored twice
  - Registering the same user to the same event twice returns `409 Conflict`
  - The event's maximum number of registrations is enforced
- **Real weather forecast:** Integrates with the [Open-Meteo](https://open-meteo.com) API (free, no API key needed). The event's location is geocoded and the forecast for the event's start date is returned: conditions, min/max temperature, and chance of rain. Results are cached in memory for one hour.
- **Validation:** Required fields, maximum lengths, positive capacity, end date on or after start date, and no future birth dates. Invalid input returns `400 Bad Request` with clear messages.
- **DTOs:** The API never exposes database entities directly
- **Google Maps link** for each event's location
- **Secure client:** All user-provided text is rendered with `textContent`, which prevents XSS

## Tech Stack

| Layer | Technologies |
|---|---|
| Backend | C#, ASP.NET Core 8 Web API, Entity Framework Core 9, SQL Server (LocalDB) |
| Client | HTML, CSS, vanilla JavaScript (`fetch`), hosted by a minimal ASP.NET Core static-file app |
| Integrations | Open-Meteo Geocoding, Forecast, and Archive APIs |
| Tooling | Swagger / OpenAPI, Postman collection, `.http` file |

## Architecture

```
Client (static HTML/JS)  ──HTTP──►  EventController  ──►  EventService  ──►  EF Core  ──►  SQL Server
                                          │
                                          └──►  WeatherService  ──►  Open-Meteo API (+ in-memory cache)
```

- **Controllers** handle HTTP concerns and map results to status codes
- **Services** (`IEventService`, `IWeatherService`) contain the business logic and are injected through dependency injection
- **DTOs** separate the API contract from the database model

## API Endpoints

Base URL: `https://localhost:7172/api/event`

| Method | Route | Description | Responses |
|---|---|---|---|
| `GET` | `/schedule` | List all events (sorted by start date) | 200 |
| `GET` | `/{id}` | Get a single event | 200, 404 |
| `POST` | `/` | Create an event | 201, 400 |
| `PUT` | `/{id}` | Update an event | 204, 400, 404 |
| `DELETE` | `/{id}` | Delete an event and its registrations | 204, 404 |
| `POST` | `/{id}/registration` | Register a user to an event | 200, 400, 404, 409 |
| `GET` | `/{id}/registration` | List the event's participants | 200, 404 |
| `GET` | `/{id}/weather` | Weather forecast for the event's location and date | 200, 404, 503 |

<details>
<summary>Example requests and responses</summary>

**Create an event:** `POST /api/event`
```json
{
  "name": "Tech Conference",
  "startDate": "2026-12-01T09:00:00",
  "endDate": "2026-12-01T17:00:00",
  "maxRegistrations": 100,
  "location": "Tel Aviv"
}
```

**Register a user:** `POST /api/event/1/registration`
```json
{ "name": "Alice", "dateOfBirth": "1990-01-01" }
```

**Weather:** `GET /api/event/1/weather`
```json
{
  "location": "Tel Aviv, Israel",
  "date": "2026-12-01",
  "forecastAvailable": true,
  "summary": "Partly cloudy",
  "maxTemperatureC": 22.4,
  "minTemperatureC": 14.1,
  "precipitationProbability": 10
}
```
A forecast is available up to 16 days before the event. For events further away, `forecastAvailable` is `false` and `summary` explains why.

</details>

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB (included with Visual Studio) or any SQL Server instance

### 1. Create the database
Run [`database/schema.sql`](database/schema.sql) in SSMS or Visual Studio, or from the command line:
```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -i database/schema.sql
```
To use a different SQL Server instance, update `ConnectionStrings:EventsConnection` in `EventManagementApi/appsettings.json`.

### 2. Trust the HTTPS development certificate (first time only)
```bash
dotnet dev-certs https --trust
```

### 3. Run the API and the client
**Visual Studio:** Open `EventManagementApi/EventManagementApi.sln`, set both `EventManagementApi` and `Client` as startup projects, and press F5.

**Command line** (two terminals):
```bash
dotnet run --project EventManagementApi --launch-profile https
```
```bash
dotnet run --project Client --launch-profile https
```

| App | URL |
|---|---|
| API + Swagger UI | https://localhost:7172/swagger |
| Client | https://localhost:7085 |

The client's API address is configured in [`Client/wwwroot/config.js`](Client/wwwroot/config.js), and the API's allowed client origins (CORS) are in `appsettings.json` under `Cors:AllowedOrigins`.

### Testing with Postman
Import [`EventManagementApi.postman_collection.json`](EventManagementApi.postman_collection.json). It uses the variables `baseUrl` and `eventId`. Alternatively, use [`EventManagementApi.http`](EventManagementApi/EventManagementApi.http) directly in Visual Studio or VS Code.

## Project Structure

```
├── EventManagementApi/            # ASP.NET Core Web API
│   ├── Controllers/               # EventController: HTTP endpoints
│   ├── Services/                  # EventService, WeatherService (+ interfaces)
│   ├── DTOs/                      # Request / response contracts with validation
│   ├── Models/                    # EF Core entities and DbContext
│   └── EventManagementApi.sln     # Solution (API + Client)
├── Client/                        # Static web client
│   └── wwwroot/                   # index.html, app.js, config.js, style.css
├── database/
│   └── schema.sql                 # Database creation script
└── EventManagementApi.postman_collection.json
```

## Database Schema

- **Events:** `ID`, `Name`, `StartDate`, `EndDate`, `MaxRegistrations`, `Location`
- **Users:** `ID`, `Name`, `DateOfBirth`
- **EventUser:** a many-to-many link between events and users (`EventRef`, `UserRef`, `Creation`), with a composite primary key that prevents duplicate registrations at the database level

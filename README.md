# Community Events & Registrations API (POC)

This repository is a Proof of Concept (POC) for a Community Events and Registrations platform built with ASP.NET Core Web API and Clean Architecture.

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core (SQL Server)
- xUnit for unit tests
- Swagger/OpenAPI

## Solution Structure

- src/Api: API layer (controllers, DTOs, startup configuration)
- src/Application: use cases, service logic, interfaces
- src/Domain: core domain entities and enums
- src/Infrastructure: EF Core DbContext, repositories, migrations
- tests/Unit: unit tests
- tests/Integration: integration test project scaffold

## Prerequisites

- .NET SDK 8.0+
- SQL Server instance (local or remote)

## Configuration

Update connection and JWT settings in src/Api/appsettings.json.

Example values:

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Server=.;Database=EventsDb;Trusted_Connection=True;TrustServerCertificate=True;"
	},
	"Jwt": {
		"Key": "ReplaceWithSecureKeyInProduction",
		"Issuer": "SpecKitDev",
		"Audience": "SpecKitClients"
	}
}
```

## Running Locally

1. Restore dependencies:

```powershell
dotnet restore
```

2. Apply database migrations:

```powershell
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

3. Run the API:

```powershell
dotnet run --project src/Api
```

4. Open Swagger UI:

- https://localhost:5001/swagger
- http://localhost:5000/swagger

## Current API Endpoints (POC)

Events:

- POST /api/events
- GET /api/events/{id}
- POST /api/events/{id}/cancel

Registrations:

- POST /api/events/{eventId}/registrations
- DELETE /api/events/{eventId}/registrations/{registrationId}

## Example Request Payloads

Create event:

```json
{
	"title": "Health Camp",
	"description": "Free annual checkup event",
	"location": "Community Hall",
	"startTime": "2026-05-10T10:00:00+05:30",
	"endTime": "2026-05-10T13:00:00+05:30",
	"capacity": 100,
	"registrationOpen": "2026-04-20T00:00:00+05:30",
	"registrationClose": "2026-05-09T23:59:00+05:30",
	"timezone": "Asia/Kolkata"
}
```

Create registration:

```json
{
	"residentId": "00000000-0000-0000-0000-000000000001"
}
```

## Run Tests

```powershell
dotnet test
```

## Notes

- This is a POC focused on demonstrating architecture and workflows.
- Error handling and validations exist for major paths but can be expanded further.
- Security settings (JWT key and policies) should be hardened for production.

# Community Events & Registrations (scaffold)

This workspace contains a scaffold for an ASP.NET Core Web API following Clean Architecture.

Structure:

- `src/Api` - ASP.NET Core Web API project
- `src/Application` - Application services and use cases
- `src/Domain` - Domain entities and business logic
- `src/Infrastructure` - EF Core, email, queue adapters
- `tests/Unit` - Unit tests
- `tests/Integration` - Integration tests

Quickstart (local):

1. Start local SQL Server (docker-compose):

```powershell
docker-compose up -d
```

2. Build and run Api:

```powershell
dotnet build src/Api
dotnet run --project src/Api
```

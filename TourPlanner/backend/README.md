# TourPlanner Backend

TourPlanner is split into layered ASP.NET Core backend projects:

- `TourPlanner.Api`: controllers, HTTP status codes, Swagger, JWT authentication and dependency injection.
- `TourPlanner.Business`: services, DTO mapping and business validation.
- `TourPlanner.Data`: Entity Framework Core context and repositories.
- `TourPlanner.Models`: entities, enums and DTOs.
- `TourPlanner.Tests`: test project placeholder.

Current data flow:

```text
API Layer -> Business Layer -> Repository Layer -> Entity Framework Core -> PostgreSQL
```

## PostgreSQL Storage

Temporary in-memory storage has been replaced with PostgreSQL through Entity Framework Core. EF Core maps the existing `User`, `Tour` and `TourLog` entities to database tables and handles migrations.

The repository pattern keeps database access in `TourPlanner.Data`, while business validation stays in `TourPlanner.Business`.

Restarting the API no longer deletes stored data. Data remains in PostgreSQL until the database is changed or deleted.

## Local Configuration

Development uses this placeholder connection string in `TourPlanner.Api/appsettings.Development.json`:

```text
Host=localhost;Port=5432;Database=tourplanner;Username=postgres;Password=postgres
```

These credentials are local development placeholders only. Production database credentials and JWT secrets must be stored in environment variables or secret storage. Real production secrets must not be committed to Git.

The JWT key in `appsettings.Development.json` is also only for local development.

## Start Backend

```powershell
dotnet run --project TourPlanner.Api
```

Swagger is available in development at:

```text
https://localhost:7115/swagger
http://localhost:5161/swagger
```

The exact ports come from `TourPlanner.Api/Properties/launchSettings.json`.

## Migrations

Create the initial migration:

```powershell
dotnet ef migrations add InitialCreate --project TourPlanner.Data --startup-project TourPlanner.Api
```

Apply migrations to PostgreSQL:

```powershell
dotnet ef database update --project TourPlanner.Data --startup-project TourPlanner.Api
```

PostgreSQL must be running locally for `database update` and runtime API testing.

## Endpoints

- `GET /api/health`
- `GET /api/tours`
- `GET /api/tours/{id}`
- `POST /api/tours`
- `PUT /api/tours/{id}`
- `DELETE /api/tours/{id}`
- `GET /api/tours/{tourId}/logs`
- `GET /api/tours/{tourId}/logs/{logId}`
- `POST /api/tours/{tourId}/logs`
- `PUT /api/tours/{tourId}/logs/{logId}`
- `DELETE /api/tours/{tourId}/logs/{logId}`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/auth/me`

## Registration And Login

Users can register with a username, email and password. Passwords are hashed with `PasswordHasher<User>` before they are stored in PostgreSQL.

Login verifies the stored password hash and returns an `AuthResponseDto` with a JWT token. Password hashes are never returned through API responses.

`GET /api/auth/me` requires a valid Bearer token. Tour endpoints are intentionally not protected yet because tour ownership and user-specific filtering will be added later.

## Manual Swagger Test

1. Start PostgreSQL locally.
2. Apply migrations with `dotnet ef database update --project TourPlanner.Data --startup-project TourPlanner.Api`.
3. Start the backend.
4. Open Swagger.
5. Register a user with `POST /api/auth/register`.
6. Log in with `POST /api/auth/login`.
7. Copy the JWT token from the response.
8. Click Swagger `Authorize` and enter the token.
9. Call `GET /api/auth/me`.
10. Create a tour with `POST /api/tours`.
11. Create a tour log with `POST /api/tours/{tourId}/logs`.
12. Retrieve tour logs with `GET /api/tours/{tourId}/logs`.
13. Update and delete the tour log with `PUT` and `DELETE`.
14. Restart the API and verify that the database data still exists.

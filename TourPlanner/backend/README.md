# TourPlanner Backend

TourPlanner is currently split into layered ASP.NET Core backend projects:

- `TourPlanner.Api`: controllers, HTTP status codes, Swagger and authentication setup.
- `TourPlanner.Business`: services, DTO mapping and business validation.
- `TourPlanner.Data`: temporary storage abstractions.
- `TourPlanner.Models`: entities, enums and DTOs.
- `TourPlanner.Tests`: test project placeholder.

## Temporary Storage

The backend currently uses a singleton `InMemoryDataStore` with lists for tours and users. Tour logs are stored inside their parent tour. This keeps the implementation simple until PostgreSQL and Entity Framework are added later.

Restarting the backend deletes all temporary tours, tour logs and users.

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

Users can register with a username, email and password. Passwords are hashed with `PasswordHasher<User>` before they are stored. Login verifies the password hash and returns an `AuthResponseDto` with a JWT token.

`GET /api/auth/me` requires a valid Bearer token. Tour endpoints are intentionally not protected yet because tour ownership and user-specific filtering will be added later.

The JWT key in `appsettings.Development.json` is only for local development. Production JWT secrets must not be committed to source control.

## Manual Swagger Test

1. Start the backend.
2. Open Swagger.
3. Register a user with `POST /api/auth/register`.
4. Copy the JWT token from the response.
5. Click Swagger `Authorize` and enter the token.
6. Call `GET /api/auth/me`.
7. Create a tour with `POST /api/tours`.
8. Create a tour log with `POST /api/tours/{tourId}/logs`.
9. Retrieve tour logs with `GET /api/tours/{tourId}/logs`.
10. Update and delete the tour log with `PUT` and `DELETE`.

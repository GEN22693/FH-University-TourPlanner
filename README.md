# TourPlanner

TourPlanner is a full-stack application for planning and managing personal tours.

The project contains:

- An ASP.NET Core backend with Entity Framework Core, PostgreSQL and JWT authentication
- An Angular frontend with TypeScript, Tailwind CSS, Angular Router and Leaflet
- A Docker Compose setup for the local PostgreSQL development database only

The backend and frontend still run locally during development. Only PostgreSQL runs in Docker, so PostgreSQL does not need to be installed on the developer machine.

## Project Structure

```text
TourPlanner/
+-- backend/
|   +-- TourPlanner.Api/
|   +-- TourPlanner.Business/
|   +-- TourPlanner.Data/
|   +-- TourPlanner.Models/
|   +-- TourPlanner.Tests/
|   +-- TourPlanner.sln
+-- frontend/
|   +-- tour-planner-ui/
+-- docs/
+-- docker-compose.yml
+-- README.md
```

## Technologies

Backend:

- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT authentication
- Layered architecture

Frontend:

- Angular
- TypeScript
- Tailwind CSS
- Angular Signals
- Angular Standalone Components
- Angular Router
- Leaflet

Development database:

- Docker Compose
- PostgreSQL 17

## Backend Architecture

The backend is split into layered ASP.NET Core projects:

- `TourPlanner.Api`: controllers, HTTP status codes, Swagger, JWT authentication and dependency injection
- `TourPlanner.Business`: services, DTO mapping and business validation
- `TourPlanner.Data`: Entity Framework Core context and repositories
- `TourPlanner.Models`: entities, enums and DTOs
- `TourPlanner.Tests`: test project placeholder

Current data flow:

```text
API Layer -> Business Layer -> Repository Layer -> Entity Framework Core -> PostgreSQL
```

## Development PostgreSQL

Start PostgreSQL from the repository root:

```powershell
docker compose up -d
```

Check database status:

```powershell
docker compose ps
```

The development connection string in `backend/TourPlanner.Api/appsettings.Development.json` is:

```text
Host=localhost;Port=5432;Database=tourplanner;Username=postgres;Password=postgres
```

These credentials are local development placeholders only. Production database credentials and JWT secrets must be stored in environment variables or secret storage.

Apply Entity Framework Core migrations from the `backend` directory:

```powershell
cd backend
dotnet ef database update --project TourPlanner.Data --startup-project TourPlanner.Api
```

Stop PostgreSQL while keeping stored data:

```powershell
docker compose down
```

Delete PostgreSQL including stored data:

```powershell
docker compose down -v
```

## Start Backend

Open a terminal in the `backend` directory:

```powershell
cd backend
dotnet run --project TourPlanner.Api
```

Swagger is available in development at:

```text
https://localhost:7115/swagger
http://localhost:5161/swagger
```

The exact ports come from `backend/TourPlanner.Api/Properties/launchSettings.json`.

## Run Backend Tests

```powershell
cd backend
dotnet test
```

## Start Frontend

Open a terminal in the Angular project folder:

```powershell
cd frontend/tour-planner-ui
```

Install dependencies:

```powershell
npm install
```

Start the Angular development server:

```powershell
npm start
```

Open the application in the browser:

```text
http://localhost:4200
```

## Main Features

- Register a user
- Login and logout
- Create tours
- View all tours in a list
- Search tours
- Open tour details
- Edit tours
- Delete tours
- Show an interactive Leaflet map on the tour detail page
- Create tour logs
- Edit tour logs
- Delete tour logs
- Input validation for forms
- Responsive design for desktop and smaller screens

## Backend Endpoints

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

## Frontend Routes

| Route        | Description                             |
| ------------ | --------------------------------------- |
| `/login`     | Login page                              |
| `/register`  | Register page                           |
| `/tours`     | Tour overview page                      |
| `/tours/:id` | Tour detail page with map and tour logs |

## Authentication

Users can register with a username, email and password. Passwords are hashed with `PasswordHasher<User>` before they are stored in PostgreSQL.

Login verifies the stored password hash and returns an `AuthResponseDto` with a JWT token. Password hashes are never returned through API responses.

`GET /api/auth/me` requires a valid Bearer token. Tour and tour log endpoints also require a valid Bearer token, and tours are scoped to the authenticated user: a user only sees, edits and deletes their own tours.

## OpenRouteService Setup

Creating or updating a tour calculates the real distance, estimated travel time and route information between `From` and `To` using the [OpenRouteService](https://openrouteservice.org/) API.

1. Create a free account at <https://openrouteservice.org/dev/#/signup>.
2. Request a token in the dashboard (free tier: 2,000 requests/day).
3. Store the key locally with .NET user-secrets so it never ends up in the repository:

```powershell
cd backend/TourPlanner.Api
dotnet user-secrets init
dotnet user-secrets set "OpenRouteService:ApiKey" "<your-api-key>"
```

`appsettings.json` only contains an empty placeholder for `OpenRouteService:ApiKey`. Without a configured key, creating or updating a tour fails with a clear error message instead of a silent crash.

## Manual Test Flow

1. Start PostgreSQL with `docker compose up -d`.
2. Apply migrations from `backend`.
3. Start the backend.
4. Start the frontend.
5. Open `http://localhost:4200`.
6. Register a user.
7. Log in with the created user.
8. Create a tour.
9. Search for the tour.
10. Open the tour details.
11. Check the Leaflet map.
12. Add, edit and delete a tour log.
13. Edit and delete the tour.
14. Restart the API and verify that database data still exists.

## Database Access With DBeaver

Use these connection settings:

```text
Database type: PostgreSQL
Host: localhost
Port: 5432
Database: tourplanner
Username: postgres
Password: postgres
```

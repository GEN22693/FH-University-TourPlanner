# Tour Planner

Tour Planner is a semester project for planning and managing tours. The current intermediate backend uses a simple ASP.NET Core Web API with layered projects and in-memory tour data.

## Folder Structure

- `backend/` contains the ASP.NET Core Web API solution and projects.
- `backend/TourPlanner.Api/` contains the API entry point and controllers.
- `backend/TourPlanner.Business/` contains business interfaces and services.
- `backend/TourPlanner.Data/` is reserved for data access code.
- `backend/TourPlanner.Models/` contains shared model classes.
- `backend/TourPlanner.Tests/` contains NUnit tests.
- `frontend/tour-planner-ui/` contains the Angular application.
- `docs/uml/` is reserved for UML diagrams.
- `docs/wireframes/` is reserved for UI wireframes.

## Start the Backend

```powershell
cd TourPlanner/backend
dotnet run --project TourPlanner.Api
```

The backend starts with the URLs printed by `dotnet run`. The current HTTP launch profile uses:

```text
http://localhost:5161
```

## Test the Health Endpoint

```powershell
curl http://localhost:5161/api/health
```

Expected response:

```json
{
  "status": "ok",
  "message": "TourPlanner API is running"
}
```

## Test the Tour Endpoints Manually

List all tours:

```powershell
curl http://localhost:5161/api/tours
```

Create a tour:

```powershell
curl -X POST http://localhost:5161/api/tours `
  -H "Content-Type: application/json" `
  -d "{\"name\":\"Morning Ride\",\"description\":\"Short city tour\",\"from\":\"Vienna\",\"to\":\"Klosterneuburg\",\"transportType\":\"Bike\"}"
```

Get one tour:

```text
GET http://localhost:5161/api/tours/1
```

Update a tour:

```powershell
curl -X PUT http://localhost:5161/api/tours/1 `
  -H "Content-Type: application/json" `
  -d "{\"name\":\"Updated Ride\",\"description\":\"Updated city tour\",\"from\":\"Vienna\",\"to\":\"Tulln\",\"transportType\":\"Bike\"}"
```

Delete a tour:

```powershell
curl -X DELETE http://localhost:5161/api/tours/1
```

Validation rules for creating and updating tours:

- `name` must not be empty.
- `from` must not be empty.
- `to` must not be empty.

## Start the Frontend

```powershell
cd TourPlanner/frontend/tour-planner-ui
npm install
npm start
```

## Initial Setup

This setup contains:

- An ASP.NET Core Web API project.
- Layered class library projects for business, data, and models.
- An NUnit test project.
- Basic domain entities and DTOs.
- A simple API health controller.
- Basic tour CRUD endpoints.
- An in-memory tour service in the business layer.
- An Angular project with routing and SCSS.
- Documentation folders, README, and `.gitignore`.

## Current Backend Architecture

- `TourPlanner.Api` contains controllers and HTTP endpoint configuration.
- `TourPlanner.Business` contains service interfaces and business logic.
- `TourPlanner.Models` contains domain entities, DTOs, and enums shared by the backend projects.
- `TourPlanner.Data` is reserved for future database access.
- `TourPlanner.Tests` is reserved for future automated tests.

The tour data is currently stored in an in-memory list inside `TourService`. Data is lost when the backend stops.

No PostgreSQL, Entity Framework, authentication, OpenRouteService integration, TourLog endpoints, or unit tests have been added yet.

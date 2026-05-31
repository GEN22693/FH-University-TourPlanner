# Tour Planner Frontend

Tour Planner is an Angular frontend application for planning and managing personal tours.
This version is prepared for the intermediate submission and focuses on the frontend part of the project.

The application allows users to register, log in, create tours, view tour details, manage tour logs and display an interactive map using Leaflet.

---

## Tech Stack

- Angular
- TypeScript
- Tailwind CSS
- Angular Signals
- Angular Standalone Components
- Angular Router
- Leaflet
- OpenStreetMap tiles
- LocalStorage

---

## Project Status

This is the frontend version for the intermediate submission.

There is currently no backend connection.
All data is stored locally in the browser using `localStorage`.

This includes:

- registered users
- current logged-in user
- tours
- tour logs

The project is structured so that the local storage logic is separated into services.
This makes it easier to replace the local storage implementation with real backend API calls later.

---

## How to Start the Project

Open a terminal in the frontend project folder:

```powershell
cd TourPlanner/frontend/tour-planner-ui

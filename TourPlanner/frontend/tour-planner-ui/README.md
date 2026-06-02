# Tour Planner Frontend

Tour Planner is an Angular frontend application for planning and managing personal tours.

This version is prepared for the intermediate submission. The focus is on the frontend implementation. There is no backend connection yet. All data is stored locally in the browser with `localStorage`.

---

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

---

## Technologies Used

- Angular
- TypeScript
- Tailwind CSS
- Angular Signals
- Angular Standalone Components
- Angular Router
- Leaflet
- LocalStorage

---

## Project Structure

```text
src/app/
├─ core/
│  └─ services/
│     ├─ auth.service.ts
│     ├─ tour.service.ts
│     └─ map-facade.service.ts
│
├─ models/
│  ├─ auth.model.ts
│  ├─ tour.model.ts
│  └─ tour-log.model.ts
│
├─ pages/
│  ├─ login/
│  ├─ register/
│  ├─ tourlist/
│  └─ tour-detail/
│
├─ shared/
│  └─ components/
│     ├─ navbar/
│     └─ map-placeholder/
│
├─ app.routes.ts
├─ app.config.ts
└─ app.ts
```

---

## How to Start the Project

Open a terminal in the frontend project folder:

```powershell
cd TourPlanner/frontend/tour-planner-ui
```

Install the dependencies:

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

---

## Available Routes

| Route        | Description                             |
| ------------ | --------------------------------------- |
| `/login`     | Login page                              |
| `/register`  | Register page                           |
| `/tours`     | Tour overview page                      |
| `/tours/:id` | Tour detail page with map and tour logs |

---

## LocalStorage

The application stores all data locally in the browser.

Stored data includes:

- users
- current logged-in user
- tours
- tour logs

To reset the application data, clear LocalStorage for:

```text
localhost:4200
```

In Chrome:

```text
DevTools → Application → Local Storage → localhost:4200 → Clear
```

---

## Test Flow

To test the main functionality:

1. Open `/register`
2. Create a new user
3. Login with the created user
4. Create a new tour
5. Search for the tour
6. Open the tour details
7. Check the Leaflet map
8. Edit the tour
9. Add a tour log
10. Edit the tour log
11. Delete the tour log
12. Go back to the tour overview
13. Delete the tour
14. Logout

---

## Notes

This is a frontend-only version for the intermediate submission.

The backend can be connected later by replacing the LocalStorage logic inside the services with real HTTP API calls.

The current implementation already separates the application into:

- models
- views/templates
- component logic
- services

# TimePE

TimePE is a small, single-user time-tracking application built on .NET 8 and SQLite using DevExpress XPO. It captures daily work sessions (date, start time, end time), associates time with projects, and preserves historical pay-rate information. The design favors immutability of historic records (soft deletes via XPO and explicit audit fields) so past data remains reliable for reporting and payroll.

This README provides a concise overview, quick setup steps, architecture notes, and a development roadmap to help contributors and maintainers get started.

## Key goals

- Track daily work sessions with project allocation.
- Maintain historical pay-rate information so previously-recorded time retains the pay rate that was in effect when the time was logged.
- Use soft deletes (XPO's built-in IsDeleted) and cascading soft-deletes for related data; nothing is physically deleted.
- Provide weekly report generation suitable for submitting to management.
- Show a running balance on the dashboard tracking owed vs. paid amounts.
- Allow entry of incidentals (one-off amounts owed to or by the user).
- Simple cookie-based authentication system for secure access.
- **Progressive Web App (PWA)** support - installable on desktop and mobile devices, works offline, provides native app-like experience.

## Tech stack

- .NET 8 (C#)
- SQLite (local file database, simple single-user deployment)
- DevExpress XPO (Object-Relational Mapping with built-in soft delete support)
- Dependency Injection using built-in Microsoft DI
- Serilog for structured logging
- PWA (Service Worker, Web App Manifest, offline support)

## Repository layout

Top-level folders you'll see in this repo:

- `src/TimePE.Core/` - Core domain models, services, and business logic using XPO.
- `src/TimePE.WebApp/` - Web application (UI/API) that consumes the Core library.
- `src/TimePE.Core/Database/` - Database initialization and XPO configuration.

There are also project/solution files at the repository root (`TimePE.sln`)

## Quick start (development)

Prerequisites:

- .NET 8 SDK installed (dotnet --version should show a 8.x SDK)
- Optional: a browser for the WebApp frontend

Build and run the entire solution from the repository root:

```bash
dotnet build
dotnet run --project src/TimePE.WebApp/TimePE.WebApp.csproj
```

If the WebApp is an ASP.NET project it will print the local URL (usually http://localhost:5176). Open that in your browser.

### Testing PWA Features

The application is a fully-featured Progressive Web App:

**Install as Desktop App:**
1. Open http://localhost:5176 in Chrome or Edge
2. Look for the install icon (⊕) in the address bar
3. Click "Install TimePE"
4. The app opens in a standalone window

**Install on Mobile:**
- **Android:** Chrome menu → "Install app" or "Add to Home screen"
- **iOS:** Safari Share button → "Add to Home Screen"

**Test Offline Support:**
1. Open the app and navigate a few pages
2. Open DevTools → Network tab → Set to "Offline"
3. Reload and navigate - static assets load from cache

For detailed PWA documentation, see [docs/PWA_IMPLEMENTATION.md](docs/PWA_IMPLEMENTATION.md).

## Authentication

The application uses cookie-based authentication to protect all pages. On first run, a default user account is automatically created:

- **Username:** `admin`
- **Password:** `admin123`

**Important:** Change the default password after first login for security.

All application pages require authentication except the login page. Sessions persist for:
- 30 days if "Remember me" is checked
- 8 hours for standard sessions

Notes about the database:

- The app uses SQLite with DevExpress XPO. The database file path and connection string live in the WebApp's `appsettings.json`. Look for a ConnectionStrings entry such as `DefaultConnection`.
- XPO handles schema creation automatically via `AutoCreateOption.DatabaseAndSchema`.

## Development notes

- Soft deletes: XPO provides built-in `IsDeleted` property on all persistent objects. Services use XPO's `Delete()` method which performs soft deletes.
- Pay rates: preserve historical pay-rate values on time entries (e.g., store the applied rate on each time record). Avoid mutating historical records when rates change.
- Reporting: implement a weekly report generator that aggregates time by project and applies the correct historic pay rates when computing amounts owed.
- Read associated [Code Standards](CodeStandards.md)

## Contributing

Contributions are welcome. A good starting flow:

1. Fork the repository and create a feature branch (git checkout -b feature/your-feature).
2. Open a draft PR with a short description of the change.
3. Keep changes focused and add tests for new logic where appropriate.

Coding conventions:

- Follow existing C# coding styles present in the codebase.
- Keep public APIs in `TimePE.Core` stable; prefer adding new services over changing existing contracts when possible.
- Use XPO best practices: Session for read-only operations, UnitOfWork for transactions.

## Roadmap / Next steps

### Completed
- ✅ Cookie-based authentication system
- ✅ User login/logout functionality
- ✅ Automatic default user creation
- ✅ Protected routes with authorization
- ✅ User profile management (change username/password)
- ✅ CSV import/export functionality
- ✅ Automatic data seeding (General project, default pay rate)
- ✅ Comprehensive Serilog logging system
- ✅ Soft delete implementation using XPO's built-in system
- ✅ Mobile-responsive design with touch-friendly controls
- ✅ Progressive Web App (PWA) support with offline capabilities

### Planned
- Enhanced reporting features with more filters
- Advanced dashboard analytics
- Time entry templates
- Project budgeting and tracking
- Multi-user support (future consideration)

## Where to look in the code

- `src/TimePE.Core/Models` - XPO persistent classes for time entries, projects, pay rates, incidentals, and users.
- `src/TimePE.Core/Services` - business logic: creating time entries, applying pay rates, computing balances, soft-delete handling, authentication.
- `src/TimePE.Core/Database` - XPO data layer initialization, schema setup, and user initialization.
- `src/TimePE.WebApp/Pages/Account` - Login and logout pages.
- `src/TimePE.WebApp/` - UI/API project, authentication configuration, and appsettings for connection strings.

## Support / Questions

If you need help, open an issue with a reproducible description of the problem and steps to reproduce. For small code questions, include the file/class and line numbers where the issue appears.

## License

Add a LICENSE file to this repository (MIT is recommended for small personal projects). If you already have a preferred license, include it at the repo root.

# TimePE

**Professional Time Tracking & Project Management**

TimePE is a feature-rich, single-user time-tracking Progressive Web App built on **.NET 9** and **SQLite** using **DevExpress XPO**. It captures daily work sessions (date, start time, end time), associates time with projects, and preserves historical pay-rate information. The design favors immutability of historic records (soft deletes via XPO's built-in system) so past data remains reliable for reporting and payroll.

**Latest Updates (December 2, 2025):**
- ✅ Migrated to .NET 9 with C# 13
- ✅ Comprehensive test suite: **142 tests passing**
- ✅ Modern Service Worker v2.0.0 with intelligent caching
- ✅ Zero build warnings
- ✅ Full PWA support with offline capabilities
 - ✅ Dockerized runtime and compose added (v1.0)

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

- **.NET 9** with **C# 13** (latest features)
- **SQLite** (local file database, simple single-user deployment)
- **DevExpress XPO 25.1.7** (Object-Relational Mapping with built-in soft delete support)
- **Dependency Injection** with keyed services (.NET 9 feature)
- **Serilog 4.3.0** for structured logging with async/await
- **xUnit 2.9.2** testing framework with **142 passing tests**
- **FluentAssertions 6.12.2** for readable test assertions
- **Progressive Web App** (Service Worker v2.0.0, multi-strategy caching, offline support)
- **HybridCache** (.NET 9 distributed caching)
- **Bootstrap 5** with custom dark theme

## Repository layout

Top-level folders you'll see in this repo:


There are also project/solution files at the repository root (`TimePE.sln`)

## Quick start (development)

Prerequisites:


Build and run the entire solution from the repository root:

```bash
# Build solution (0 warnings)
dotnet build

# Run tests (142 passing)
dotnet test

# Run application
dotnet run --project src/TimePE.WebApp/TimePE.WebApp.csproj
```

 - **[docs/QUICKSTART.md](docs/QUICKSTART.md)** - Quick start for dev and Docker

## Deployment (Docker)

Build and run using Docker:

```bash
# Build image (tag v1.0.0)
docker build -t timepe/web:1.0.0 -f src/TimePE.WebApp/Dockerfile .

# Run container (maps 5176 -> 8080)
docker run -d --name timepe-web \
  -p 5176:8080 \
  -v timepe_data:/data \
  -v timepe_logs:/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  timepe/web:1.0.0

# View logs
docker logs -f timepe-web
```

Or use `docker-compose`:

```bash
# Build and start services (web + optional Seq)
docker compose up -d --build

# Stop services
docker compose down
```

Notes:
- Default HTTP port is `5176` (host) mapped to `8080` (container).
- Persistent volumes: `timepe_data` for SQLite DB, `timepe_logs` for logs.
- To enable Seq, visit `http://localhost:5341` after `docker compose up`.
 - Health endpoint: `GET /health` (used by Docker/K8s readiness probes)

### Environment Variables
- `ASPNETCORE_ENVIRONMENT`: `Development` or `Production` (default in compose is `Production`).
- `ConnectionStrings__DefaultConnection`: override SQLite path (e.g., `XpoProvider=SQLite;Data Source=/data/timepe.db`).
- `Serilog:WriteTo:Seq:Args:serverUrl`: set Seq URL (e.g., `http://localhost:5341`).
- `Serilog:WriteTo:ApplicationInsights:Args:connectionString`: set AI connection string.

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

### Completed ✅
- **Core Features:**
  - Complete CRUD operations for all entities
  - Time tracking with project association
  - Pay rate management with historical preservation
  - Incidentals and payment tracking
  - Balance calculations and dashboard
  - CSV import/export with templates
  - Weekly/custom range reporting

- **Security & Authentication:**
  - Cookie-based authentication with session management
  - SHA256 password hashing
  - Protected routes with authorization
  - User profile management
  - Automatic default user creation

- **Technical Excellence:**
  - ✅ **.NET 9 with C# 13** (modern features)
  - ✅ **142 comprehensive tests** (xUnit + FluentAssertions)
  - ✅ **Zero build warnings**
  - ✅ **DevExpress XPO 25.1.7** with built-in soft deletes
  - ✅ **Serilog** structured logging
  - ✅ **HybridCache** for performance
  - ✅ **Keyed DI** services (.NET 9)

- **Progressive Web App:**
  - ✅ **Service Worker v2.0.0** with intelligent caching
  - ✅ **Multi-strategy caching** (static, dynamic, images)
  - ✅ **Offline support** with cache limits
  - ✅ **Installable** on desktop and mobile
  - ✅ **13 icon sizes** + 10 iOS splash screens
  - ✅ **Mobile-responsive** design
  - ✅ **Touch-friendly** controls (48x48px WCAG compliant)
  - ✅ **Dark theme** optimized for OLED

### Planned (post v1.0)
- Enhanced reporting features with more filters
- Advanced dashboard analytics
- Time entry templates
- Project budgeting and tracking
- Multi-user support (future consideration)

## Where to look in the code

- **`src/TimePE.Core/Models/`** - XPO persistent classes (7 models)
- **`src/TimePE.Core/Services/`** - Business logic services (8 services)
- **`src/TimePE.Core.Tests/Services/`** - Comprehensive test suite (142 tests)
- **`src/TimePE.Core/Database/`** - XPO initialization and migrations
- **`src/TimePE.WebApp/Pages/`** - Razor Pages UI (50+ pages)
- **`src/TimePE.WebApp/wwwroot/`** - Static assets, PWA files, service worker
- **`src/TimePE.WebApp/Program.cs`** - .NET 9 application configuration
- **`docs/`** - Comprehensive documentation

## Documentation

- **[PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)** - Complete architecture guide
- **[CodeStandards.md](CodeStandards.md)** - Coding conventions
- **[docs/NET9_CSHARP13_FEATURES.md](docs/NET9_CSHARP13_FEATURES.md)** - Modern features used
- **[docs/PWA_IMPLEMENTATION.md](docs/PWA_IMPLEMENTATION.md)** - PWA architecture
- **[docs/LOGGING.md](docs/LOGGING.md)** - Logging system

### Enabling Centralized Logging (Optional)
- **Seq:** Install Seq and set `serverUrl` in `src/TimePE.WebApp/appsettings.json` under `Serilog:WriteTo:Seq`.
- **Application Insights:** Replace `Serilog:WriteTo:ApplicationInsights:Args:connectionString` with your AI connection string.
- See `docs/LOGGING.md` for full configuration examples.
- **[docs/SOFT_DELETE_IMPLEMENTATION.md](docs/SOFT_DELETE_IMPLEMENTATION.md)** - Soft delete guide
- **[src/TimePE.Core.Tests/README.md](src/TimePE.Core.Tests/README.md)** - Testing guide

## Support / Questions

If you need help, open an issue with a reproducible description of the problem and steps to reproduce. For small code questions, include the file/class and line numbers where the issue appears.

## Troubleshooting

- **Ports:** Default HTTP `http://localhost:5176`, HTTPS `https://localhost:7176`. If different, check `Properties/launchSettings.json`.
- **Service Worker cache:** If UI seems stale, open DevTools → Application → Clear storage → Unregister service worker, then hard-reload.
- **SQLite path:** Check `src/TimePE.WebApp/appsettings.Development.json` for `ConnectionStrings`. Ensure the directory is writable.
- **Clean build:**
  ```bash
  dotnet clean
  dotnet restore
  dotnet build
  ```
- **Run tests with details:**
  ```bash
  dotnet test --verbosity normal
  ```
- **Reset PWA state:** Delete site data in browser settings for `localhost` and reload.

## License

This project is licensed under the terms in `LICENSE` at the repository root.

# TimePE Quick Start

This guide helps you get TimePE running quickly in development and production (Docker).

## Prerequisites
- .NET 9 SDK (`dotnet --version` → 9.0.x)
- Modern browser (Chrome/Edge/Firefox/Safari)
- Optional: Docker 24+ and Docker Compose

## Development

```bash
# Clone and enter
# git clone https://github.com/MrFrey75/TimePE.git
# cd TimePE

# Build solution
dotnet build

# Run tests (142 passing)
dotnet test

# Run web app
dotnet run --project src/TimePE.WebApp/TimePE.WebApp.csproj
```

Open `http://localhost:5176` (HTTPS: `https://localhost:7176`).

Default login:
- Username: `admin`
- Password: `admin123`

## Docker (Production-like)

```bash
# Build image
docker build -t timepe/web:1.0.0 -f src/TimePE.WebApp/Dockerfile .

# Run container
docker run -d --name timepe-web \
  -p 5176:8080 \
  -v timepe_data:/data \
  -v timepe_logs:/logs \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="XpoProvider=SQLite;Data Source=/data/timepe.db" \
  timepe/web:1.0.0

# Logs
docker logs -f timepe-web
```

Or with compose:

```bash
docker compose up -d --build
# Stop
docker compose down
```

Health check:
- Endpoint: `GET /health`
- Example: `curl -fsSL http://localhost:5176/health`
- Returns: `{ "status": "ok", "time": "2025-12-02T12:34:56.000Z" }`

## PWA Tips
- Install from the browser (⊕ icon) for desktop.
- Test offline: DevTools → Network → Offline → reload.
- Clear service worker cache: DevTools → Application → Clear storage → Unregister SW.

## Troubleshooting
- Ports: default `5176` (host) → `8080` (container).
- SQLite path: override via `ConnectionStrings__DefaultConnection` to `/data/timepe.db`.
- Clean build:
  ```bash
  dotnet clean && dotnet restore && dotnet build
  ```
- Verbose tests:
  ```bash
  dotnet test --verbosity normal
  ```

## References
- `README.md` for overview and tech stack
- `docs/LOGGING.md` for Serilog configuration
- `docs/PWA_IMPLEMENTATION.md` for service worker details

# TimePE Project Structure

## Overview
TimePE is a .NET 8 time-tracking application with SQLite database backend using DevExpress XPO for data access.

## Solution Structure

```
TimePE/
├── src/
│   ├── TimePE.Core/              # Core business logic library
│   │   ├── Models/               # XPO persistent domain models
│   │   │   ├── BaseEntity.cs     # Base class with soft delete support
│   │   │   ├── Project.cs
│   │   │   ├── PayRate.cs
│   │   │   ├── TimeEntry.cs
│   │   │   ├── Incidental.cs
│   │   │   ├── Payment.cs
│   │   │   └── User.cs
│   │   ├── DTOs/                 # Data Transfer Objects
│   │   │   └── ProjectSummaryDto.cs
│   │   ├── Services/             # Business logic services
│   │   │   ├── ProjectService.cs
│   │   │   ├── PayRateService.cs
│   │   │   ├── TimeEntryService.cs
│   │   │   ├── IncidentalService.cs
│   │   │   ├── PaymentService.cs
│   │   │   └── DashboardService.cs
│   │   └── Database/
│   │       ├── DatabaseContext.cs
│   │       └── Migrations/
│   │           └── DatabaseMigrator.cs
│   │
│   └── TimePE.WebApp/            # ASP.NET Core web application
│       ├── Pages/                # Razor Pages
│       │   ├── Account/          # Login/Logout/Profile pages
│       │   ├── Projects/         # Project CRUD pages
│       │   ├── PayRates/         # Pay rate management
│       │   ├── TimeEntries/      # Time entry management
│       │   ├── Incidentals/      # Incidentals tracking
│       │   ├── Payments/         # Payment records
│       │   ├── Reports/          # Report generation
│       │   └── Shared/           # Layout and shared components
│       ├── wwwroot/              # Static files
│       │   ├── css/              
│       │   │   └── site.css      # Mobile-responsive styles
│       │   ├── js/
│       │   │   └── site.js       # PWA and mobile enhancements
│       │   ├── lib/              # Third-party libraries (Bootstrap, jQuery)
│       │   ├── icons/            # PWA icons (13 sizes)
│       │   ├── splash/           # iOS splash screens (10 sizes)
│       │   ├── manifest.json     # PWA manifest
│       │   ├── sw.js             # Service worker
│       │   └── browserconfig.xml # Microsoft tiles config
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
├── docs/                         # Documentation
│   ├── PWA_IMPLEMENTATION.md     # PWA architecture guide
│   ├── MOBILE_PWA_TESTING.md     # Testing checklist
│   ├── LOGGING.md                # Logging system documentation
│   ├── SOFT_DELETE_IMPLEMENTATION.md # Soft delete guide
│   ├── generate-icons.sh         # Bash icon generator (ImageMagick)
│   ├── generate-icons.py         # Python icon generator (Pillow)
│   └── archive/                  # Archived documentation
│
├── TimePE.sln                    # Solution file
├── README.md                     # Main documentation
├── PROJECT_STRUCTURE.md          # This file
├── CodeStandards.md              # Coding standards
└── LICENSE                       # MIT License

## Technology Stack

- **.NET 8** - Framework
- **ASP.NET Core Razor Pages** - Web UI
- **SQLite** - Database
- **DevExpress XPO 24.1.6** - Object-Relational Mapping framework with built-in soft delete
- **Serilog** - Structured logging (file rotation, dual outputs)
- **Bootstrap 5** - UI framework with custom dark theme
- **Progressive Web App (PWA)** - Service Worker, Web App Manifest, offline support
- **Cookie Authentication** - SHA256 password hashing, session management

## DevExpress XPO Architecture

### Data Access Pattern
XPO uses two main session types:
- **Session** - For read-only queries and data retrieval
- **UnitOfWork** - For transactional operations (create, update, delete)

### Persistent Objects
All domain models inherit from `XPObject` through `BaseEntity`:
- Automatic Id (Oid) generation
- Built-in soft-delete support via `IsDeleted` property
- Change tracking and lazy loading
- Associations and collections

### Key Features Used
- **Auto Schema Creation** - Database tables created automatically from models
- **Soft Deletes** - XPO's built-in `[DeferredDeletion(true)]` with GCRecord system
- **LINQ Support** - Query data using `XPQuery<T>`
- **Associations** - One-to-many relationships (e.g., Project → TimeEntries)
- **Persistent Aliases** - Calculated properties (e.g., Duration, AmountOwed)
- **View Models** - For delete operations to avoid ObjectDisposedException

## Database Schema

### Tables (Auto-created by XPO):
- **User** - User accounts for authentication
- **Project** - Work projects to track time against
- **PayRate** - Historical pay rate records
- **TimeEntry** - Daily work sessions with project association
- **Incidental** - One-off amounts owed/by
- **Payment** - Payment records

All tables include:
- `OID` (Primary Key, auto-increment)
- `CreatedAt` - Timestamp when record was created
- `UpdatedAt` - Last modification timestamp
- `GCRecord` - Soft delete tracking (built-in XPO feature via `[DeferredDeletion(true)]`)
- `OptimisticLockField` - Concurrency control (built-in XPO feature)

## Getting Started

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Run the web application:**
   ```bash
   dotnet run --project src/TimePE.WebApp/TimePE.WebApp.csproj
   ```

## XPO Connection String

Connection string format in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "XpoProvider=SQLite;Data Source=timepe.db"
}
```

XPO supports multiple providers (SQLite, SQL Server, MySQL, PostgreSQL, etc.) by changing the `XpoProvider` value.

## Service Layer Pattern

Services use dependency injection and async/await pattern:

```csharp
// Read operations - use Session
using var session = new Session(XpoDefault.DataLayer);
var project = session.GetObjectByKey<Project>(id);

// Write operations - use UnitOfWork
using var uow = new UnitOfWork(XpoDefault.DataLayer);
var project = new Project(uow) { Name = "New Project" };
uow.CommitChanges();
```

## Implemented Features

### ✅ Core Services
- Project management (CRUD operations with soft delete)
- Pay rate tracking with historical preservation
- Time entry tracking with automatic pay rate application
- Incidentals management (amounts owed/by)
- Payment tracking
- Dashboard with balance calculations and project summaries
- CSV import/export for all entities with sample templates

### ✅ Web UI
- Complete CRUD interfaces for all entities
- Dashboard with balance display, recent activity, and project summaries
- Time entry forms with project association and validation
- Project management pages with relationship tracking
- Pay rate management with effective date handling
- Incidentals tracking with type classification
- Payment records with date tracking
- Weekly/custom date range reporting with PDF-ready layouts
- Responsive design optimized for mobile, tablet, and desktop
- Touch-friendly controls (48x48px WCAG-compliant touch targets)
- Dark theme with high contrast colors (#00d4ff on #0a0e17)

### ✅ Security & Authentication
- Cookie-based authentication ("CookieAuth" scheme)
- Automatic default user creation on first run (admin/admin123)
- SHA256 password hashing with salt
- Protected routes with authorization middleware
- Session management (8 hours / 30 days with "Remember me")
- User profile management (change username/password)
- Secure logout with cookie clearing

### ✅ Progressive Web App (PWA)
- **Installable** - Can be installed as native-like app on desktop and mobile
- **Offline Support** - Service worker with cache-first strategy
- **App Icons** - 13 sizes (72x72 to 512x512) for all platforms
- **Splash Screens** - 10 iOS launch screens for all device sizes
- **Web App Manifest** - Full PWA configuration with shortcuts and share target
- **Mobile Optimizations** - Touch feedback, haptic vibration, pull-to-refresh
- **Network Detection** - Offline banner, auto-reconnect
- **Form Auto-Save** - LocalStorage backup for unsaved changes
- **Web Share API** - Share data to other apps on supported devices

### ✅ Logging & Monitoring
- **Serilog** structured logging with dual outputs
- Console logging for development debugging
- File logging with daily rotation (logs/timepe-YYYYMMDD.log)
- Retention policies (30 days general, 90 days errors)
- HTTP request/response logging with timing
- Service operation logging (CRUD, authentication)
- Application lifecycle events (startup, shutdown)

### ✅ Data Management
- **Soft Delete** - XPO's built-in `[DeferredDeletion(true)]` system
- **Data Seeding** - Automatic creation of "General" project and default pay rate
- **CSV Import** - Bulk data import with validation and error handling
- **CSV Export** - Full data export with proper formatting
- **Sample Templates** - Downloadable CSV templates for each entity
- **Cascade Delete** - Related entities soft-deleted together (via XPO associations)

## Potential Future Enhancements

1. **Advanced Reporting**
   - PDF generation for reports
   - Email report delivery
   - Custom report builder
   - Data visualization (charts/graphs)
   - Export to Excel format

2. **Enhanced PWA Features**
   - Background sync for offline time entries
   - Push notifications for reminders
   - IndexedDB for offline data storage
   - Sync conflict resolution
   - App Store deployment (TWA for Android, wrapper for iOS)

3. **User Management**
   - Password strength requirements and validation
   - Password recovery mechanism (email-based)
   - Two-factor authentication
   - User roles and permissions (future multi-user support)

4. **Performance & Scalability**
   - Caching strategies (in-memory, distributed)
   - Query optimization and profiling
   - Pagination for large datasets
   - Database indexing optimization
   - CDN integration for static assets

5. **Testing**
   - Unit tests for services
   - Integration tests for authentication
   - UI tests for critical workflows
   - PWA functionality tests
   - Mobile device testing automation

6. **Advanced Features**
   - Time entry templates for recurring tasks
   - Project budgeting and tracking
   - Expense tracking integration
   - Invoice generation
   - Multiple time zones support
   - Bulk operations and batch processing
   - Advanced search and filtering
   - Audit trail and history tracking

## XPO Best Practices

- Always dispose Sessions and UnitOfWorks (use `using` statements)
- Use Session for read-only operations
- Use UnitOfWork for transactions
- Query with `XPQuery<T>` for LINQ support
- Let XPO manage the database schema with `AutoCreateOption.DatabaseAndSchema`
- Use built-in soft deletes via `[DeferredDeletion(true)]` and `.Delete()` method
- Leverage associations for navigation properties
- Load related data before session disposal to avoid `ObjectDisposedException`
- Use view models for delete confirmations with lazy-loaded collections
- XPO automatically filters out soft-deleted records via GCRecord
- See `docs/SOFT_DELETE_IMPLEMENTATION.md` for detailed guidance

## Configuration

### Database Connection
Database connection string is in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "XpoProvider=SQLite;Data Source=timepe.db"
}
```

### Logging Configuration
Serilog is configured in `Program.cs` with:
- **Console Sink** - Development debugging with colored output
- **File Sink** - Daily rotating logs in `logs/timepe-{Date}.log`
- **Retention** - 30 days for general logs, 90 days for errors
- **Format** - Structured JSON-style logging with timestamps

### Authentication
Cookie authentication settings:
- **Scheme** - "CookieAuth"
- **Login Path** - `/Account/Login`
- **Logout Path** - `/Account/Logout`
- **Session Duration** - 8 hours (standard), 30 days (persistent)

### PWA Configuration
PWA settings in `wwwroot/manifest.json`:
- **Theme Color** - `#0a0e17` (dark blue-black)
- **Background Color** - `#0a0e17`
- **Display Mode** - `standalone` (full-screen app)
- **Cache Strategy** - Cache-first with network fallback

### Default Seeded Data
On first run, the application automatically creates:
- **User** - admin / admin123 (change password after first login)
- **Project** - "General" project for miscellaneous work
- **PayRate** - $20/hour default rate

## License

MIT License - See LICENSE file for details.

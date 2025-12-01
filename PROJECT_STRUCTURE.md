# TimePE Project Structure

## Overview
TimePE is a .NET 8 time-tracking application with SQLite database backend using DevExpress XPO for data access.

## Solution Structure

```
TimePE/
├── src/
│   ├── TimePE.Core/              # Core business logic library
│   │   ├── Models/               # XPO persistent domain models
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Project.cs
│   │   │   ├── PayRate.cs
│   │   │   ├── TimeEntry.cs
│   │   │   ├── Incidental.cs
│   │   │   ├── Payment.cs
│   │   │   └── User.cs
│   │   ├── Services/             # Business logic services
│   │   │   ├── ProjectService.cs
│   │   │   ├── PayRateService.cs
│   │   │   ├── TimeEntryService.cs
│   │   │   ├── IncidentalService.cs
│   │   │   ├── PaymentService.cs
│   │   │   ├── DashboardService.cs
│   │   │   └── AuthService.cs
│   │   └── Database/
│   │       ├── DatabaseContext.cs
│   │       └── Migrations/
│   │           ├── DatabaseMigrator.cs
│   │           └── UserInitializer.cs
│   │
│   └── TimePE.WebApp/            # ASP.NET Core web application
│       ├── Pages/                # Razor Pages
│       │   ├── Account/          # Login/Logout pages
│       │   ├── Projects/         # Project CRUD pages
│       │   ├── PayRates/         # Pay rate management
│       │   ├── TimeEntries/      # Time entry management
│       │   ├── Incidentals/      # Incidentals tracking
│       │   ├── Payments/         # Payment records
│       │   ├── Reports/          # Report generation
│       │   └── Shared/           # Layout and shared components
│       ├── wwwroot/              # Static files
│       ├── appsettings.json
│       └── Program.cs
│
├── TimePE.sln                    # Solution file
├── READMD.md                     # Main documentation
├── CodeStandards.md              # Coding standards
└── LICENSE                       # MIT License

## Technology Stack

- **.NET 8** - Framework
- **SQLite** - Database
- **DevExpress XPO 24.1.6** - Object-Relational Mapping framework
- **Serilog** - Structured logging

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
- **Soft Deletes** - XPO's built-in `IsDeleted` property
- **LINQ Support** - Query data using `XPQuery<T>`
- **Associations** - One-to-many relationships (e.g., Project → TimeEntries)
- **Persistent Aliases** - Calculated properties (e.g., Duration, AmountOwed)

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
- `IsDeleted` - Soft delete flag (built-in XPO feature)
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
- Project management (CRUD operations)
- Pay rate tracking with historical preservation
- Time entry tracking with automatic pay rate application
- Incidentals management (amounts owed/by)
- Payment tracking
- Dashboard with balance calculations and summaries
- Authentication and user management

### ✅ Web UI
- Complete CRUD interfaces for all entities
- Dashboard with balance display and recent activity
- Time entry forms with project association
- Project management pages
- Pay rate management
- Incidentals tracking
- Payment records
- Weekly/custom date range reporting
- Login/logout functionality
- Protected routes with cookie authentication

### ✅ Security
- Cookie-based authentication
- Automatic default user creation on first run
- Password hashing (SHA256)
- Protected routes with [Authorize] attribute
- Session management (8 hours / 30 days with "Remember me")

## Next Steps

The following enhancements could be implemented:

1. **User Management**
   - Password change functionality
   - User profile editing
   - Password strength requirements
   - Password recovery mechanism

2. **Enhanced Features**
   - Data export (CSV, PDF)
   - Email report generation
   - Advanced filtering and search
   - Bulk operations

3. **Testing**
   - Unit tests for services
   - Integration tests for authentication
   - UI tests for critical workflows

4. **Performance**
   - Caching strategies
   - Query optimization
   - Pagination for large datasets

## XPO Best Practices

- Always dispose Sessions and UnitOfWorks (use `using` statements)
- Use Session for read-only operations
- Use UnitOfWork for transactions
- Query with `XPQuery<T>` for LINQ support
- Let XPO manage the database schema with `AutoCreateOption`
- Use built-in soft deletes via `.Delete()` method
- Leverage associations for navigation properties

## Configuration

Database connection string is in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "XpoProvider=SQLite;Data Source=timepe.db"
}
```

Logs are written to console and `logs/timepe-{Date}.log` files.

## License

MIT License - See LICENSE file for details.

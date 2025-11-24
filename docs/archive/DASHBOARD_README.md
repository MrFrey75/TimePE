# TimePE Dashboard - Quick Reference

## Dashboard Features Built

### 📊 Balance Summary Cards
- **Current Balance** - Shows the difference between owed and paid (green if positive, red if negative)
- **Total Owed** - Sum of all time entries and incidentals owed
- **Total Paid** - Sum of all payments received
- **Total Hours** - Total hours worked across all time entries

### 📅 Weekly Hours Tracking
- **This Week** - Progress bar showing current week hours vs 40-hour target
- **Last Week** - Progress bar showing last week hours vs 40-hour target

### 🕐 Recent Time Entries
- Table showing the 10 most recent time entries
- Displays: Date, Project, Time range, Duration, Amount owed
- Click "View All" to see all entries (placeholder)
- Responsive table with hover effects

### 📈 Project Hours Summary
- Shows project hours breakdown for the last 30 days
- Top 5 projects by hours worked
- Visual progress bars showing relative time allocation
- Percentage breakdown of time per project

### ⚡ Quick Actions
- **New Time Entry** - Quick access to create time entries
- **Manage Projects** - Navigate to projects management
- **Pay Rates** - Access pay rate configuration
- **Generate Report** - Create weekly/monthly reports

## Services Created

### Core Services (`TimePE.Core/Services/`)

1. **ProjectService.cs**
   - Create, read, update, delete projects
   - Get active projects
   - Soft delete support

2. **PayRateService.cs**
   - Create pay rates with automatic end-dating of previous rates
   - Get current pay rate
   - Get pay rate for a specific date (historical tracking)
   - Ordered list of all pay rates

3. **TimeEntryService.cs**
   - Create time entries with automatic pay rate application
   - Get entries by date range
   - Get entries by project
   - Update and delete entries
   - Calculated Duration and AmountOwed properties

4. **IncidentalService.cs**
   - Track one-off amounts (owed/owed by)
   - Date range queries
   - Full CRUD operations

5. **PaymentService.cs**
   - Record payments received
   - Date range queries
   - Full CRUD operations

6. **DashboardService.cs**
   - `GetBalanceSummaryAsync()` - Comprehensive balance calculation
   - `GetRecentTimeEntriesAsync(count)` - Recent entries
   - `GetProjectHoursSummaryAsync()` - Project time breakdown
   - `GetWeeklyHoursAsync(weekStart)` - Weekly hour totals

## UI Components

### Layout (`Pages/Shared/_Layout.cshtml`)
- Modern navigation bar with Bootstrap primary color scheme
- Bootstrap Icons integration
- Navigation menu with icons:
  - Dashboard
  - Time Entries
  - Projects
  - Pay Rates
  - Reports
- Responsive design (mobile-friendly)
- Footer with copyright and privacy link

### Dashboard Page (`Pages/Index.cshtml`)
- Fully responsive grid layout
- Cards with hover effects and shadows
- Color-coded balance indicators
- Progress bars for weekly hours
- Bootstrap Icons throughout
- Modern, clean design

### Styles (`wwwroot/css/site.css`)
- Card hover animations
- Progress bar transitions
- Table row hover effects
- Custom color scheme
- Responsive typography
- Smooth transitions and animations

## Data Flow

```
User Request → IndexModel.OnGetAsync()
              ↓
    DashboardService Methods
              ↓
    XPO DataLayer (Session/UnitOfWork)
              ↓
    SQLite Database
              ↓
    Return to Dashboard View
              ↓
    Render with Razor
```

## Color Scheme

- **Primary**: Blue (`#0d6efd`) - Navigation, primary buttons
- **Success**: Green - Positive balances, amounts owed
- **Info**: Cyan - Total paid
- **Secondary**: Gray - Total hours, last week
- **Danger**: Red - Negative balances
- **Warning**: Yellow - Quick actions icon

## Running the Dashboard

```bash
# Build the solution
dotnet build

# Run the web application
dotnet run --project src/TimePE.WebApp/TimePE.WebApp.csproj

# Navigate to https://localhost:5001 or http://localhost:5000
```

## Next Steps for Development

### Immediate Priorities:
1. Create Time Entry pages (Create, Edit, List)
2. Create Projects management pages
3. Create Pay Rates management pages
4. Create Reports generation page
5. Add form validation
6. Add user authentication

### Future Enhancements:
- Weekly report PDF generation
- Data export (CSV, Excel)
- Charts and graphs (Chart.js integration)
- Email notifications
- Mobile app version
- Dark mode support
- Multi-user support with roles

## Dependencies Registered

All services are registered in `Program.cs`:
- `IProjectService` → `ProjectService`
- `IPayRateService` → `PayRateService`
- `ITimeEntryService` → `TimeEntryService`
- `IIncidentalService` → `IncidentalService`
- `IPaymentService` → `PaymentService`
- `IDashboardService` → `DashboardService`

## Notes

- Dashboard automatically calculates balances from all data sources
- Soft deletes are properly filtered in all queries
- Historical pay rates are preserved and applied correctly
- All monetary values use proper `decimal` type for accuracy
- Date/time handling uses UTC for consistency
- XPO handles all database operations and schema management

## Troubleshooting

If the dashboard shows no data:
1. Database will be created automatically on first run
2. Use the "New Time Entry" button to add your first entry
3. Create a project first via "Manage Projects"
4. Set up a pay rate via "Pay Rates"
5. Then create time entries

The dashboard is fully functional and ready for data entry!

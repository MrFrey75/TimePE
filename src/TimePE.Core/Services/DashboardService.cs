using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public class BalanceSummary
{
    public decimal TotalOwed { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal CurrentBalance => TotalOwed - TotalPaid;
    public int TimeEntriesCount { get; set; }
    public decimal TotalHoursWorked { get; set; }
}

public interface IDashboardService
{
    Task<BalanceSummary> GetBalanceSummaryAsync();
    Task<IEnumerable<TimeEntry>> GetRecentTimeEntriesAsync(int count = 10);
    Task<Dictionary<string, decimal>> GetProjectHoursSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetWeeklyHoursAsync(DateTime weekStart);
}

public class DashboardService : IDashboardService
{
    private readonly string _connectionString;

    public DashboardService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<BalanceSummary> GetBalanceSummaryAsync()
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            
            var timeEntries = new XPQuery<TimeEntry>(session)
                .Where(te => te.DeletedAt == null)
                .ToList();

            var incidentals = new XPQuery<Incidental>(session)
                .Where(i => i.DeletedAt == null)
                .ToList();

            var payments = new XPQuery<Payment>(session)
                .Where(p => p.DeletedAt == null)
                .ToList();

            var timeOwed = timeEntries.Sum(te => te.AmountOwed);
            var incidentalsOwed = incidentals.Where(i => i.Type == IncidentalType.Owed).Sum(i => i.Amount);
            var incidentalsOwedBy = incidentals.Where(i => i.Type == IncidentalType.OwedBy).Sum(i => i.Amount);
            var totalPaid = payments.Sum(p => p.Amount);

            return new BalanceSummary
            {
                TotalOwed = timeOwed + incidentalsOwed - incidentalsOwedBy,
                TotalPaid = totalPaid,
                TimeEntriesCount = timeEntries.Count,
                TotalHoursWorked = (decimal)timeEntries.Sum(te => te.Duration.TotalHours)
            };
        });
    }

    public async Task<IEnumerable<TimeEntry>> GetRecentTimeEntriesAsync(int count = 10)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return new XPQuery<TimeEntry>(session)
                .Where(te => te.DeletedAt == null)
                .OrderByDescending(te => te.Date)
                .ThenByDescending(te => te.StartTime)
                .Take(count)
                .ToList();
        });
    }

    public async Task<Dictionary<string, decimal>> GetProjectHoursSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var query = new XPQuery<TimeEntry>(session)
                .Where(te => te.DeletedAt == null);

            if (startDate.HasValue)
                query = (XPQuery<TimeEntry>)query.Where(te => te.Date >= startDate.Value);

            if (endDate.HasValue)
                query = (XPQuery<TimeEntry>)query.Where(te => te.Date <= endDate.Value);

            return query.ToList()
                .GroupBy(te => te.Project?.Name ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => (decimal)g.Sum(te => te.Duration.TotalHours)
                );
        });
    }

    public async Task<decimal> GetWeeklyHoursAsync(DateTime weekStart)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var weekEnd = weekStart.AddDays(7);

            var timeEntries = new XPQuery<TimeEntry>(session)
                .Where(te => te.Date >= weekStart && te.Date < weekEnd && te.DeletedAt == null)
                .ToList();

            return (decimal)timeEntries.Sum(te => te.Duration.TotalHours);
        });
    }
}

using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface ITimeEntryService
{
    Task<TimeEntry> CreateTimeEntryAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int projectId, string? notes = null);
    Task<TimeEntry?> GetTimeEntryByIdAsync(int id);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeEntry>> GetTimeEntriesByProjectAsync(int projectId);
    Task UpdateTimeEntryAsync(TimeEntry timeEntry);
    Task DeleteTimeEntryAsync(int id);
}

public class TimeEntryService : ITimeEntryService
{
    private readonly string _connectionString;
    private readonly IPayRateService _payRateService;

    public TimeEntryService(string connectionString, IPayRateService payRateService)
    {
        _connectionString = connectionString;
        _payRateService = payRateService;
    }

    public async Task<TimeEntry> CreateTimeEntryAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, int projectId, string? notes = null)
    {
        var payRate = await _payRateService.GetPayRateForDateAsync(date);
        if (payRate == null)
        {
            throw new InvalidOperationException($"No pay rate found for date {date:yyyy-MM-dd}");
        }

        return await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var project = uow.GetObjectByKey<Project>(projectId);
            if (project == null || project.DeletedAt != null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found");
            }

            var timeEntry = new TimeEntry(uow)
            {
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                Project = project,
                AppliedPayRate = payRate.HourlyRate,
                Notes = notes
            };
            uow.CommitChanges();
            return timeEntry;
        });
    }

    public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            return session.GetObjectByKey<TimeEntry>(id);
        });
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = CriteriaOperator.Parse("Date >= ? And Date <= ? And DeletedAt Is Null", startDate, endDate);
            var collection = new XPCollection<TimeEntry>(session, criteria);
            collection.Sorting.Add(new SortProperty("Date", DevExpress.Xpo.DB.SortingDirection.Ascending));
            collection.Sorting.Add(new SortProperty("StartTime", DevExpress.Xpo.DB.SortingDirection.Ascending));
            return collection.ToList();
        });
    }

    public async Task<IEnumerable<TimeEntry>> GetTimeEntriesByProjectAsync(int projectId)
    {
        return await Task.Run(() =>
        {
            using var session = new Session(XpoDefault.DataLayer);
            var criteria = CriteriaOperator.Parse("Project.Oid = ? And DeletedAt Is Null", projectId);
            var collection = new XPCollection<TimeEntry>(session, criteria);
            collection.Sorting.Add(new SortProperty("Date", DevExpress.Xpo.DB.SortingDirection.Ascending));
            collection.Sorting.Add(new SortProperty("StartTime", DevExpress.Xpo.DB.SortingDirection.Ascending));
            return collection.ToList();
        });
    }

    public async Task UpdateTimeEntryAsync(TimeEntry timeEntry)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var existingEntry = uow.GetObjectByKey<TimeEntry>(timeEntry.Oid);
            if (existingEntry != null)
            {
                existingEntry.Date = timeEntry.Date;
                existingEntry.StartTime = timeEntry.StartTime;
                existingEntry.EndTime = timeEntry.EndTime;
                existingEntry.Notes = timeEntry.Notes;
                existingEntry.UpdatedAt = DateTime.UtcNow;
                uow.CommitChanges();
            }
        });
    }

    public async Task DeleteTimeEntryAsync(int id)
    {
        await Task.Run(() =>
        {
            using var uow = new UnitOfWork(XpoDefault.DataLayer);
            var timeEntry = uow.GetObjectByKey<TimeEntry>(id);
            if (timeEntry != null)
            {
                timeEntry.Delete();
            }
        });
    }
}

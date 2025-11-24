using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class TimeEntry : BaseEntity
{
    public TimeEntry(Session session) : base(session) { }

    DateTime _date;
    [Persistent]
    public DateTime Date
    {
        get => _date;
        set => SetPropertyValue(nameof(Date), ref _date, value);
    }

    TimeSpan _startTime;
    [Persistent]
    public TimeSpan StartTime
    {
        get => _startTime;
        set => SetPropertyValue(nameof(StartTime), ref _startTime, value);
    }

    TimeSpan _endTime;
    [Persistent]
    public TimeSpan EndTime
    {
        get => _endTime;
        set => SetPropertyValue(nameof(EndTime), ref _endTime, value);
    }

    Project? _project;
    [Persistent]
    [Association("Project-TimeEntries")]
    public Project? Project
    {
        get => _project;
        set => SetPropertyValue(nameof(Project), ref _project, value);
    }

    decimal _appliedPayRate;
    [Persistent]
    public decimal AppliedPayRate
    {
        get => _appliedPayRate;
        set => SetPropertyValue(nameof(AppliedPayRate), ref _appliedPayRate, value);
    }

    string? _notes;
    [Persistent]
    [Size(SizeAttribute.Unlimited)]
    public string? Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [PersistentAlias("EndTime - StartTime")]
    public TimeSpan Duration => EndTime - StartTime;

    [PersistentAlias("Duration.TotalHours * AppliedPayRate")]
    public decimal AmountOwed => (decimal)Duration.TotalHours * AppliedPayRate;
}

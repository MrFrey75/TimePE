using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class Project : BaseEntity
{
    public Project(Session session) : base(session) { }

    string _name = string.Empty;
    [Persistent]
    [Size(200)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    string? _description;
    [Persistent]
    [Size(SizeAttribute.Unlimited)]
    public string? Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    bool _isActive = true;
    [Persistent]
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    [Association("Project-TimeEntries")]
    public XPCollection<TimeEntry> TimeEntries => GetCollection<TimeEntry>(nameof(TimeEntries));
}

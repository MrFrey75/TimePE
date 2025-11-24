using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class Incidental : BaseEntity
{
    public Incidental(Session session) : base(session) { }

    DateTime _date;
    [Persistent]
    public DateTime Date
    {
        get => _date;
        set => SetPropertyValue(nameof(Date), ref _date, value);
    }

    decimal _amount;
    [Persistent]
    public decimal Amount
    {
        get => _amount;
        set => SetPropertyValue(nameof(Amount), ref _amount, value);
    }

    string _description = string.Empty;
    [Persistent]
    [Size(500)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    IncidentalType _type = IncidentalType.Owed;
    [Persistent]
    public IncidentalType Type
    {
        get => _type;
        set => SetPropertyValue(nameof(Type), ref _type, value);
    }
}

public enum IncidentalType
{
    Owed,
    OwedBy
}

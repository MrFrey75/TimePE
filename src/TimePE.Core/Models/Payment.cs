using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class Payment : BaseEntity
{
    public Payment(Session session) : base(session) { }

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

    string? _notes;
    [Persistent]
    [Size(SizeAttribute.Unlimited)]
    public string? Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
}

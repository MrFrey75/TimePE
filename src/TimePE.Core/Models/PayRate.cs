using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class PayRate : BaseEntity
{
    public PayRate(Session session) : base(session) { }

    decimal _hourlyRate;
    [Persistent]
    public decimal HourlyRate
    {
        get => _hourlyRate;
        set => SetPropertyValue(nameof(HourlyRate), ref _hourlyRate, value);
    }

    DateTime _effectiveDate;
    [Persistent]
    public DateTime EffectiveDate
    {
        get => _effectiveDate;
        set => SetPropertyValue(nameof(EffectiveDate), ref _effectiveDate, value);
    }

    DateTime? _endDate;
    [Persistent]
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetPropertyValue(nameof(EndDate), ref _endDate, value);
    }
}

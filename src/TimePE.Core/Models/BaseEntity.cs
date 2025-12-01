using DevExpress.Xpo;

namespace TimePE.Core.Models;

/// <summary>
/// Base entity with audit fields and XPO's built-in soft delete support.
/// When an entity is deleted, XPO sets GCRecord != null instead of physically removing it.
/// To query non-deleted entities, XPO automatically filters them unless you use Session.Delete() or query GCRecord explicitly.
/// </summary>
[DeferredDeletion(true)] // Enable XPO's built-in soft delete (GCRecord-based)
public abstract class BaseEntity : XPObject
{
    public BaseEntity(Session session) : base(session) { }

    DateTime _createdAt = DateTime.UtcNow;
    [Persistent]
    public DateTime CreatedAt
    {
        get => _createdAt;
        set => SetPropertyValue(nameof(CreatedAt), ref _createdAt, value);
    }

    DateTime? _updatedAt;
    [Persistent]
    public DateTime? UpdatedAt
    {
        get => _updatedAt;
        set => SetPropertyValue(nameof(UpdatedAt), ref _updatedAt, value);
    }
}

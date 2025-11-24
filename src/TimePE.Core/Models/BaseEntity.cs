using DevExpress.Xpo;

namespace TimePE.Core.Models;

[DeferredDeletion(false)]
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

    DateTime? _deletedAt;
    [Persistent]
    public DateTime? DeletedAt
    {
        get => _deletedAt;
        set => SetPropertyValue(nameof(DeletedAt), ref _deletedAt, value);
    }

    public new void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}

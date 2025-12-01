using DevExpress.Xpo;

namespace TimePE.Core.Models;

public class User : BaseEntity
{
    public User(Session session) : base(session) { }

    string _username = string.Empty;
    [Persistent]
    [Indexed(Unique = true)]
    [Size(100)]
    public string Username
    {
        get => _username;
        set => SetPropertyValue(nameof(Username), ref _username, value);
    }

    string _passwordHash = string.Empty;
    [Persistent]
    [Size(SizeAttribute.Unlimited)]
    public string PasswordHash
    {
        get => _passwordHash;
        set => SetPropertyValue(nameof(PasswordHash), ref _passwordHash, value);
    }

    bool _isActive = true;
    [Persistent]
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    DateTime? _lastLoginAt;
    [Persistent]
    public DateTime? LastLoginAt
    {
        get => _lastLoginAt;
        set => SetPropertyValue(nameof(LastLoginAt), ref _lastLoginAt, value);
    }
}

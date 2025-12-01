using System.Security.Cryptography;
using System.Text;
using DevExpress.Xpo;
using TimePE.Core.Models;

namespace TimePE.Core.Services;

public interface IAuthService
{
    Task<User?> ValidateUserAsync(string username, string password);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> CreateUserAsync(string username, string password);
    Task UpdateLastLoginAsync(int userId);
    Task<bool> UpdateUsernameAsync(int userId, string newUsername);
    Task<bool> UpdatePasswordAsync(int userId, string newPassword);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public class AuthService : IAuthService
{
    private readonly string _connectionString;

    public AuthService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> ValidateUserAsync(string username, string password)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        var user = await Task.Run(() => uow.Query<User>()
            .FirstOrDefault(u => u.Username == username && u.IsActive && u.DeletedAt == null));
        
        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash))
            return null;

        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;
        
        return await Task.Run(() => uow.Query<User>()
            .FirstOrDefault(u => u.Username == username && u.DeletedAt == null));
    }

    public async Task<bool> CreateUserAsync(string username, string password)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        var existingUser = await GetUserByUsernameAsync(username);
        if (existingUser != null)
            return false;

        var user = new User(uow)
        {
            Username = username,
            PasswordHash = HashPassword(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await uow.CommitChangesAsync();
        return true;
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        var user = await uow.GetObjectByKeyAsync<User>(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await uow.CommitChangesAsync();
        }
    }

    public async Task<bool> UpdateUsernameAsync(int userId, string newUsername)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        var user = await uow.GetObjectByKeyAsync<User>(userId);
        if (user == null)
            return false;

        user.Username = newUsername;
        user.UpdatedAt = DateTime.UtcNow;
        
        try
        {
            await uow.CommitChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
    {
        using var uow = new UnitOfWork();
        uow.ConnectionString = _connectionString;

        var user = await uow.GetObjectByKeyAsync<User>(userId);
        if (user == null)
            return false;

        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        
        try
        {
            await uow.CommitChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        var hash = HashPassword(password);
        return hash == passwordHash;
    }
}

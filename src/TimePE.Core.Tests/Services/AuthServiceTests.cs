using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using FluentAssertions;
using TimePE.Core.Models;
using TimePE.Core.Services;

namespace TimePE.Core.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AuthService _authService;
    private readonly IDataLayer _dataLayer;
    private readonly string _connectionString;

    public AuthServiceTests()
    {
        // Use unique connection string per test instance to avoid conflicts
        _connectionString = $"XpoProvider=InMemoryDataStore;DBName={Guid.NewGuid()}";
        _dataLayer = XpoDefault.GetDataLayer(_connectionString, AutoCreateOption.DatabaseAndSchema);
        XpoDefault.DataLayer = _dataLayer;
        _authService = new AuthService(_connectionString);
    }

    public void Dispose()
    {
        _dataLayer?.Dispose();
    }

    [Fact]
    public async Task CreateUserAsync_ShouldCreateNewUser_WithHashedPassword()
    {
        // Arrange
        var username = "testuser";
        var password = "TestPassword123";

        // Act
        var result = await _authService.CreateUserAsync(username, password);

        // Assert
        result.Should().BeTrue();
        
        var user = await _authService.GetUserByUsernameAsync(username);
        user.Should().NotBeNull();
        user!.Username.Should().Be(username);
        user.PasswordHash.Should().NotBe(password); // Password should be hashed
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFalse_WhenUserAlreadyExists()
    {
        // Arrange
        var username = "existinguser";
        await _authService.CreateUserAsync(username, "password1");

        // Act
        var result = await _authService.CreateUserAsync(username, "password2");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnUser_WhenCredentialsAreCorrect()
    {
        // Arrange
        var username = "validuser";
        var password = "ValidPassword123";
        await _authService.CreateUserAsync(username, password);

        // Act
        var user = await _authService.ValidateUserAsync(username, password);

        // Assert
        user.Should().NotBeNull();
        user!.Username.Should().Be(username);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenPasswordIsIncorrect()
    {
        // Arrange
        var username = "testuser";
        await _authService.CreateUserAsync(username, "CorrectPassword");

        // Act
        var user = await _authService.ValidateUserAsync(username, "WrongPassword");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var user = await _authService.ValidateUserAsync("nonexistent", "password");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task ValidateUserAsync_ShouldReturnNull_WhenUserIsInactive()
    {
        // Arrange
        var username = "inactiveuser";
        var password = "password";
        await _authService.CreateUserAsync(username, password);
        
        // Deactivate user
        using (var uow = new UnitOfWork(XpoDefault.DataLayer))
        {
            var user = uow.Query<User>().First(u => u.Username == username);
            user.IsActive = false;
            await uow.CommitChangesAsync();
        }

        // Act
        var result = await _authService.ValidateUserAsync(username, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var username = "findme";
        await _authService.CreateUserAsync(username, "password");

        // Act
        var user = await _authService.GetUserByUsernameAsync(username);

        // Assert
        user.Should().NotBeNull();
        user!.Username.Should().Be(username);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var user = await _authService.GetUserByUsernameAsync("nonexistent");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLastLoginAsync_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var username = "loginuser";
        await _authService.CreateUserAsync(username, "password");
        var user = await _authService.GetUserByUsernameAsync(username);
        var beforeLogin = DateTime.UtcNow;

        // Wait a bit to ensure time difference
        await Task.Delay(100);

        // Act
        await _authService.UpdateLastLoginAsync(user!.Oid);

        // Assert
        var updatedUser = await _authService.GetUserByUsernameAsync(username);
        updatedUser!.LastLoginAt.Should().NotBeNull();
        updatedUser.LastLoginAt.Should().BeAfter(beforeLogin);
        updatedUser.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateUsernameAsync_ShouldUpdateUsername_WhenUserExists()
    {
        // Arrange
        var oldUsername = "oldusername";
        var newUsername = "newusername";
        await _authService.CreateUserAsync(oldUsername, "password");
        var user = await _authService.GetUserByUsernameAsync(oldUsername);

        // Act
        var result = await _authService.UpdateUsernameAsync(user!.Oid, newUsername);

        // Assert
        result.Should().BeTrue();
        var updatedUser = await _authService.GetUserByUsernameAsync(newUsername);
        updatedUser.Should().NotBeNull();
        updatedUser!.Username.Should().Be(newUsername);
        updatedUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        var oldUser = await _authService.GetUserByUsernameAsync(oldUsername);
        oldUser.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUsernameAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Act
        var result = await _authService.UpdateUsernameAsync(99999, "newusername");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldUpdatePassword_WhenUserExists()
    {
        // Arrange
        var username = "changepassword";
        var oldPassword = "OldPassword123";
        var newPassword = "NewPassword456";
        await _authService.CreateUserAsync(username, oldPassword);
        var user = await _authService.GetUserByUsernameAsync(username);

        // Act
        var result = await _authService.UpdatePasswordAsync(user!.Oid, newPassword);

        // Assert
        result.Should().BeTrue();
        
        // Old password should no longer work
        var validationOld = await _authService.ValidateUserAsync(username, oldPassword);
        validationOld.Should().BeNull();
        
        // New password should work
        var validationNew = await _authService.ValidateUserAsync(username, newPassword);
        validationNew.Should().NotBeNull();
        validationNew!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdatePasswordAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Act
        var result = await _authService.UpdatePasswordAsync(99999, "newpassword");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashForDifferentPasswords()
    {
        // Arrange
        var password1 = "Password123";
        var password2 = "Password456";

        // Act
        var hash1 = _authService.HashPassword(password1);
        var hash2 = _authService.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
        hash1.Should().NotBeNullOrEmpty();
        hash2.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ShouldReturnSameHashForSamePassword()
    {
        // Arrange
        var password = "ConsistentPassword123";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatches()
    {
        // Arrange
        var password = "TestPassword123";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var password = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var hash = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_ShouldBeCaseSensitive()
    {
        // Arrange
        var password = "Password123";
        var hash = _authService.HashPassword(password);

        // Act
        var resultLower = _authService.VerifyPassword("password123", hash);
        var resultUpper = _authService.VerifyPassword("PASSWORD123", hash);

        // Assert
        resultLower.Should().BeFalse();
        resultUpper.Should().BeFalse();
    }
}

# TimePE.Core.Tests

Unit tests for the TimePE.Core library using xUnit, Moq, and FluentAssertions.

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~ProjectServiceTests"

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run from solution root
cd /home/fray/Projets/TimePE
dotnet test
```

### Visual Studio / VS Code
- Use Test Explorer to run individual tests or test classes
- Set breakpoints in test methods for debugging

## Test Structure

```
TimePE.Core.Tests/
├── Services/
│   ├── ProjectServiceTests.cs       # ✅ 6 tests
│   ├── AuthServiceTests.cs          # ✅ 18 tests
│   ├── CsvServiceTests.cs           # ✅ 15 tests
│   ├── IncidentalServiceTests.cs    # ✅ 18 tests
│   ├── PayRateServiceTests.cs       # ✅ 18 tests
│   ├── PaymentServiceTests.cs       # ✅ 18 tests
│   ├── TimeEntryServiceTests.cs     # ✅ 21 tests
│   ├── DashboardServiceTests.cs     # ✅ 28 tests
│   └── ...
├── xunit.runner.json                # Test execution configuration
└── README.md
```

## Testing Strategy

### Unit Tests
- **Services**: Test CRUD operations, business logic, validation
- **Models**: Test calculated properties, XPO associations, constraints
- **Database**: Use InMemoryDataStore for isolated, fast tests

### Test Naming Convention
```csharp
[Fact]
public async Task MethodName_Should_ExpectedBehavior_When_StateUnderTest()
{
    // Arrange
    // Act
    // Assert
}
```

### Example Patterns

#### Testing Service CRUD Operations
```csharp
[Fact]
public async Task CreateAsync_ShouldCreateNewProject()
{
    // Arrange
    var projectName = "Test Project";

    // Act
    var result = await _projectService.CreateAsync(projectName);

    // Assert
    result.Should().NotBeNull();
    result.Name.Should().Be(projectName);
}
```

#### Testing Soft Deletes
```csharp
[Fact]
public async Task DeleteAsync_ShouldSoftDeleteProject()
{
    // Arrange
    var project = await _projectService.CreateAsync("To Delete");

    // Act
    await _projectService.DeleteAsync(project.Oid);

    // Assert
    var allProjects = await _projectService.GetAllAsync();
    allProjects.Should().NotContain(p => p.Oid == project.Oid);
}
```

#### Testing Validation
```csharp
[Theory]
[InlineData("")]
[InlineData("   ")]
[InlineData(null)]
public async Task CreateAsync_ShouldThrowException_WhenNameIsInvalid(string? invalidName)
{
    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        async () => await _projectService.CreateAsync(invalidName!)
    );
}
```

## Libraries Used

### xUnit
- Primary testing framework
- Supports parallel test execution
- Excellent .NET integration

### FluentAssertions
- More readable assertions
- Better error messages
- Extensive assertion methods

```csharp
// Instead of:
Assert.NotNull(result);
Assert.Equal("Test", result.Name);

// Use:
result.Should().NotBeNull();
result!.Name.Should().Be("Test");
```

### Moq (for future mocking)
- Mock dependencies when needed
- Verify method calls
- Setup return values

```csharp
var mockDataLayer = new Mock<IDataLayer>();
mockDataLayer.Setup(x => x.GetSomething()).Returns(value);
```

### Coverlet
- Code coverage collection
- Integrates with dotnet test
- Generates coverage reports

## XPO Testing Considerations

### In-Memory Database
```csharp
public ProjectServiceTests()
{
    var connectionString = "XpoProvider=InMemoryDataStore";
    _dataLayer = XpoDefault.GetDataLayer(
        connectionString, 
        DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema
    );
    XpoDefault.DataLayer = _dataLayer;
}
```

### Test Isolation
- Each test class gets fresh in-memory database
- Tests run in parallel safely
- No persistent state between tests

### Cleanup
```csharp
public void Dispose()
{
    XpoDefault.DataLayer = null;
    GC.SuppressFinalize(this);
}
```

## Adding New Tests

### 1. Create Test Class
```csharp
public class NewServiceTests : IDisposable
{
    private readonly IDataLayer _dataLayer;
    private readonly NewService _service;

    public NewServiceTests()
    {
        // Setup in-memory database
        var connectionString = "XpoProvider=InMemoryDataStore";
        _dataLayer = XpoDefault.GetDataLayer(
            connectionString, 
            DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema
        );
        XpoDefault.DataLayer = _dataLayer;
        _service = new NewService();
    }

    public void Dispose()
    {
        XpoDefault.DataLayer = null;
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task YourTest()
    {
        // Arrange, Act, Assert
    }
}
```

### 2. Run Tests
```bash
dotnet test
```

### 3. Check Coverage
```bash
dotnet test /p:CollectCoverage=true
```

## Future Test Coverage

### Current Status (December 2, 2025)
- ✅ **ProjectService** - 6 tests passing
- ✅ **AuthService** - 18 tests passing
- ✅ **CsvService** - 15 tests passing
- ✅ **IncidentalService** - 18 tests passing
- ✅ **PayRateService** - 18 tests passing
- ✅ **PaymentService** - 18 tests passing
- ✅ **TimeEntryService** - 21 tests passing
- ✅ **DashboardService** - 28 tests passing
- 📋 **Total**: **142 tests passing** ✅

### Future Priority
- [ ] Model tests (validation, calculated properties)
- [ ] Integration tests with real SQLite database
- [ ] Performance tests for large datasets

## Continuous Integration

Consider adding to CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Run tests
  run: dotnet test --no-build --verbosity normal

- name: Generate coverage
  run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

- name: Upload coverage
  uses: codecov/codecov-action@v3
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [Moq Documentation](https://github.com/moq/moq4)
- [DevExpress XPO Testing](https://docs.devexpress.com/XPO/2123/connect-to-a-data-store/connection-string-parameters)

---

**Last Updated:** December 2, 2025
**Framework:** .NET 9 with C# 13
**Test Count:** 142 tests passing (0 errors, 0 warnings)
**Coverage:** All services (ProjectService, AuthService, CsvService, IncidentalService, PayRateService, PaymentService, TimeEntryService, DashboardService)
**Status:** ✅ Comprehensive test coverage complete

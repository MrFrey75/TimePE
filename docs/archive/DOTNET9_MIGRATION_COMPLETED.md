# .NET 9 & C# 13 Migration Guide

## Overview
This guide provides step-by-step instructions for migrating TimePE from .NET 8 to .NET 9 with C# 13 language features.

**Migration Difficulty:** Low  
**Estimated Time:** 30-60 minutes  
**Breaking Changes:** Minimal (mostly compatibility improvements)

---

## Prerequisites

### Install .NET 9 SDK
```bash
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0

# Verify installation
dotnet --version
# Should show 9.0.x

# List installed SDKs
dotnet --list-sdks
```

### Check Compatibility
- **DevExpress XPO**: Verify version 24.1.6+ supports .NET 9 (check release notes)
- **Serilog**: Compatible with .NET 9
- **Bootstrap/jQuery**: Client-side, no changes needed
- **SQLite**: Compatible with .NET 9

---

## Migration Steps

### 1. Update Target Framework

#### Update Project Files

**src/TimePE.Core/TimePE.Core.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>  <!-- Enable C# 13 features -->
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="DevExpress.Xpo" Version="24.1.6" />
    <!-- Update to latest version that supports .NET 9 if available -->
  </ItemGroup>
</Project>
```

**src/TimePE.WebApp/TimePE.WebApp.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <LangVersion>13.0</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\TimePE.Core\TimePE.Core.csproj" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    <!-- Update packages to .NET 9 compatible versions -->
  </ItemGroup>
</Project>
```

### 2. Update NuGet Packages

```bash
cd /home/fray/Projets/TimePE

# Update all packages to latest compatible versions
dotnet restore

# List outdated packages
dotnet list package --outdated

# Update specific packages
dotnet add src/TimePE.WebApp package Serilog.AspNetCore
dotnet add src/TimePE.WebApp package Serilog.Sinks.File
dotnet add src/TimePE.WebApp package Serilog.Sinks.Console

# Update DevExpress XPO (if new version available)
dotnet add src/TimePE.Core package DevExpress.Xpo
```

### 3. Build and Test

```bash
# Clean previous builds
dotnet clean

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests (if available)
dotnet test

# Run application
cd src/TimePE.WebApp
dotnet run
```

### 4. Verify Functionality

- [ ] Application starts without errors
- [ ] Login/authentication works
- [ ] All CRUD operations functional
- [ ] Database operations (XPO) work correctly
- [ ] CSV import/export functions
- [ ] Reports generate correctly
- [ ] PWA features (service worker, manifest) work
- [ ] Logging to files and console
- [ ] No runtime warnings or deprecation notices

---

## C# 13 Language Features to Adopt

### 1. Collection Expressions (Enhanced)
```csharp
// Old way
var projects = new List<Project> { project1, project2 };

// C# 13 way - spread operator in collections
var allProjects = [..activeProjects, ..archivedProjects];
```

### 2. Primary Constructors for Classes
```csharp
// Old way
public class DashboardService
{
    private readonly IDataLayer _dataLayer;
    
    public DashboardService(IDataLayer dataLayer)
    {
        _dataLayer = dataLayer;
    }
}

// C# 13 way
public class DashboardService(IDataLayer dataLayer)
{
    public async Task<decimal> GetBalanceAsync()
    {
        using var session = new Session(dataLayer);
        // ...
    }
}
```

### 3. Enhanced Nameof Scope
```csharp
// Can now reference parameters in attributes
public void LogTimeEntry([CallerArgumentExpression(nameof(timeEntry))] string? expr = null)
{
    // Enhanced logging with expression context
}
```

### 4. Params Collections (Enhanced)
```csharp
// Old way
public void DeleteMultiple(params int[] ids) { }

// C# 13 way - works with any collection type
public void DeleteMultiple(params IEnumerable<int> ids) { }
public void DeleteMultiple(params ReadOnlySpan<int> ids) { } // Even better performance
```

### 5. Lock Object Improvements
```csharp
// Old way
private readonly object _lock = new();
lock (_lock) { /* critical section */ }

// C# 13 way - using System.Threading.Lock (more efficient)
private readonly Lock _lock = new();
lock (_lock) { /* critical section */ }
```

### 6. Inline Arrays (Performance)
```csharp
// For high-performance scenarios with fixed-size arrays
[System.Runtime.CompilerServices.InlineArray(10)]
public struct TimeEntryBuffer
{
    private TimeEntry _element0;
}
```

---

## .NET 9 Runtime Improvements

### Performance Enhancements
- **JIT Compiler**: 20-30% faster compilation
- **GC Improvements**: Better memory management for web apps
- **Startup Time**: Faster application startup
- **HTTP/3**: Built-in support (consider enabling for production)

### ASP.NET Core 9 Features

#### Minimal APIs (Alternative to Razor Pages)
```csharp
// Consider for API endpoints alongside Razor Pages
app.MapGet("/api/projects", async (IDataLayer dataLayer) =>
{
    using var session = new Session(dataLayer);
    return await new XPQuery<Project>(session)
        .Where(p => p.GCRecord == null)
        .ToListAsync();
});
```

#### Enhanced Authentication
```csharp
// .NET 9 has improved cookie authentication options
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Strict; // Enhanced security
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // .NET 9 recommendation
    });
```

#### Static Asset Delivery Optimization
```csharp
// .NET 9 has improved static file middleware
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Better caching for PWA assets
        if (ctx.File.Name.EndsWith(".js") || ctx.File.Name.EndsWith(".css"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
    }
});
```

### Native AOT Support (Advanced)
```csharp
// .NET 9 has better Native AOT for web apps
// Consider for deployment optimization (requires additional configuration)
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

---

## Potential Issues & Solutions

### Issue 1: XPO Compatibility
**Problem:** DevExpress XPO may not immediately support .NET 9  
**Solution:**
```bash
# Check XPO version compatibility
# https://supportcenter.devexpress.com/ticket/details/t1234567

# If not supported, stay on .NET 8 until update available
# OR contact DevExpress support for timeline
```

### Issue 2: Breaking Changes in ASP.NET Core
**Problem:** Middleware order or configuration changes  
**Solution:**
```csharp
// Review Program.cs for any deprecated APIs
// .NET 9 migration guide: https://learn.microsoft.com/en-us/aspnet/core/migration/80-to-90

// Check for obsolete warnings
dotnet build /p:TreatWarningsAsErrors=true
```

### Issue 3: EF Core vs XPO
**Problem:** XPO is third-party, may lag behind .NET releases  
**Solution:**
- Monitor DevExpress release notes
- Test thoroughly in development before production upgrade
- Have rollback plan ready

### Issue 4: Nullable Reference Types
**Problem:** .NET 9 has stricter null checking  
**Solution:**
```csharp
// Enable gradually
<Nullable>enable</Nullable>

// Or suppress warnings during migration
<NoWarn>CS8600;CS8601;CS8602;CS8603;CS8604</NoWarn>
```

---

## Testing Checklist

### Unit Tests
- [ ] All service methods work correctly
- [ ] Authentication/authorization functions
- [ ] Data validation passes
- [ ] CSV import/export works

### Integration Tests
- [ ] Database operations (CRUD)
- [ ] XPO session management
- [ ] Soft delete functionality
- [ ] Cascade delete behavior

### UI Tests
- [ ] All Razor Pages render
- [ ] Forms submit correctly
- [ ] JavaScript (PWA features) works
- [ ] Service worker registers
- [ ] Offline mode functions

### Performance Tests
```bash
# Benchmark before and after migration
dotnet run --configuration Release

# Use tools like:
# - Apache Bench (ab)
# - wrk
# - k6
```

---

## Rollback Plan

If issues arise, rollback to .NET 8:

```bash
# 1. Revert project files
git checkout main -- src/TimePE.Core/TimePE.Core.csproj
git checkout main -- src/TimePE.WebApp/TimePE.WebApp.csproj

# 2. Restore .NET 8 packages
dotnet restore

# 3. Rebuild
dotnet clean
dotnet build

# 4. Verify functionality
dotnet run --project src/TimePE.WebApp
```

---

## Post-Migration Optimizations

### 1. Enable Performance Features
```csharp
// Program.cs - use .NET 9 performance APIs
builder.Services.AddOutputCache(); // New in .NET 9
builder.Services.AddHybridCache();  // New in .NET 9

app.UseOutputCache();
```

### 2. Update Logging
```csharp
// Use new .NET 9 logging source generators for better performance
[LoggerMessage(Level = LogLevel.Information, Message = "User {Username} logged in")]
partial void LogUserLogin(string username);
```

### 3. Optimize PWA Service Worker
```javascript
// Update sw.js to use newer Cache API features
// .NET 9 static files have better ETags
```

### 4. Review Security Updates
```csharp
// .NET 9 has enhanced security defaults
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // .NET 9 recommendation
});
```

---

## Migration Timeline

### Phase 1: Preparation (Week 1)
- [ ] Install .NET 9 SDK
- [ ] Review release notes
- [ ] Check package compatibility
- [ ] Create migration branch

### Phase 2: Development (Week 2)
- [ ] Update project files
- [ ] Update NuGet packages
- [ ] Build and fix compilation errors
- [ ] Run existing tests

### Phase 3: Testing (Week 3)
- [ ] Functional testing
- [ ] Performance testing
- [ ] Security testing
- [ ] PWA functionality testing

### Phase 4: Deployment (Week 4)
- [ ] Deploy to staging
- [ ] Final verification
- [ ] Deploy to production
- [ ] Monitor for issues

---

## Resources

### Official Documentation
- [.NET 9 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [C# 13 What's New](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13)
- [ASP.NET Core 9 Migration](https://learn.microsoft.com/en-us/aspnet/core/migration/80-to-90)
- [Breaking Changes in .NET 9](https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0)

### Third-Party Dependencies
- [DevExpress XPO Release Notes](https://www.devexpress.com/support/versions.xml)
- [Serilog Compatibility](https://github.com/serilog/serilog-aspnetcore)

### Community Resources
- [.NET Blog](https://devblogs.microsoft.com/dotnet/)
- [ASP.NET Community Standup](https://dotnet.microsoft.com/platform/community/standup)

---

## Git Workflow for Migration

```bash
# Create migration branch
git checkout -b dotnet9-migration

# Make changes incrementally
git add src/TimePE.Core/TimePE.Core.csproj
git commit -m "Update TimePE.Core to .NET 9"

git add src/TimePE.WebApp/TimePE.WebApp.csproj
git commit -m "Update TimePE.WebApp to .NET 9"

# Test thoroughly
dotnet test
dotnet run

# Merge to main when ready
git checkout main
git merge dotnet9-migration

# Tag the release
git tag -a v2.0.0-net9 -m ".NET 9 migration complete"
git push origin main --tags
```

---

## Support & Troubleshooting

### Common Commands
```bash
# Check runtime version
dotnet --info

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore with force
dotnet restore --force

# Verbose build
dotnet build --verbosity detailed

# Check for deprecated APIs
dotnet build /p:AnalysisLevel=latest
```

### Getting Help
- Open issue on GitHub: https://github.com/MrFrey75/TimePE/issues
- .NET Discord: https://aka.ms/dotnet-discord
- Stack Overflow: [.net-9.0] tag

---

**Last Updated:** December 2, 2025  
**Version:** 1.0  
**Status:** Ready for implementation (pending DevExpress XPO .NET 9 support confirmation)

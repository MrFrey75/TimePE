using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Serilog;
using Serilog.Events;
using TimePE.Core.Database;
using TimePE.Core.Database.Migrations;
using TimePE.Core.Services;

// Configure Serilog with modern builder pattern
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "TimePE")
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .Enrich.WithProperty("ProcessId", Environment.ProcessId)
    .CreateLogger();

try
{
    Log.Information("Starting TimePE application");
    Log.Debug("Environment: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production");
    
    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Get connection string with C# 13 null-coalescing throw expression
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // Initialize XPO DataLayer
    XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);

    // Database initialization
    var initializer = new DatabaseInitializer();
    initializer.Initialize(connectionString);

    // Seed data - using await at top level (C# 13 feature in Program.cs)
    await new UserInitializer(connectionString).InitializeDefaultUserAsync();
    await new ProjectInitializer(connectionString).InitializeDefaultProjectsAsync();
    await new PayRateInitializer(connectionString).InitializeDefaultPayRateAsync();

    // Register services using keyed services (.NET 9 feature)
    builder.Services.AddKeyedSingleton("ConnectionString", connectionString);
    
    // Register services with simplified lambda expressions (C# 13)
    builder.Services.AddScoped<IProjectService>(sp => 
        new ProjectService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<IPayRateService>(sp => 
        new PayRateService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<ITimeEntryService>(sp => 
        new TimeEntryService(
            sp.GetRequiredKeyedService<string>("ConnectionString"), 
            sp.GetRequiredService<IPayRateService>()));
    builder.Services.AddScoped<IIncidentalService>(sp => 
        new IncidentalService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<IPaymentService>(sp => 
        new PaymentService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<IDashboardService>(sp => 
        new DashboardService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<IAuthService>(sp => 
        new AuthService(sp.GetRequiredKeyedService<string>("ConnectionString")));
    builder.Services.AddScoped<ICsvService>(sp => 
        new CsvService(
            sp.GetRequiredService<ITimeEntryService>(),
            sp.GetRequiredService<IProjectService>(),
            sp.GetRequiredKeyedService<string>("ConnectionString")));

    // Configure authentication with modern options
    builder.Services.AddAuthentication("CookieAuth")
        .AddCookie("CookieAuth", options =>
        {
            options.Cookie.Name = "TimePE.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();

    // Add Razor Pages with enhanced options
    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/");
        options.Conventions.AllowAnonymousToPage("/Account/Login");
    });

    // .NET 9 caching features
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(builder => builder.Cache());
        options.AddPolicy("Static", builder => builder.Expire(TimeSpan.FromMinutes(60)));
    });
    
    builder.Services.AddHybridCache(options =>
    {
        options.MaximumPayloadBytes = 1024 * 1024; // 1 MB
        options.MaximumKeyLength = 512;
        options.DefaultEntryOptions = new()
        {
            Expiration = TimeSpan.FromMinutes(5),
            LocalCacheExpiration = TimeSpan.FromMinutes(5)
        };
    });

    var app = builder.Build();

    // Configure middleware pipeline
    app.UseOutputCache();

    // Configure request logging with structured logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex switch
        {
            not null => LogEventLevel.Error,
            _ => httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information
        };
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
            
            if (httpContext.User.Identity is { IsAuthenticated: true })
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            }
        };
    });

    // Environment-specific configuration
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    
    // Static files with caching
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Cache static files for 1 year
            ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000";
        }
    });

    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

     // Health check endpoint for Docker/K8s readiness
     app.MapGet("/health", () => Results.Json(new { status = "ok", time = DateTimeOffset.UtcNow }))
         .WithName("HealthCheck")
         .WithTags("System");

    // Startup logging
    Log.Information("TimePE application configured successfully");
    Log.Information("Application starting on: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Content root: {ContentRoot}", app.Environment.ContentRootPath);
    Log.Information("URLs: {Urls}", string.Join(", ", builder.WebHost.GetSetting("urls")?.Split(';') ?? ["Not configured"]));
    
    await app.RunAsync();
    
    Log.Information("TimePE application stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.Information("Shutting down TimePE application");
    await Log.CloseAndFlushAsync();
}

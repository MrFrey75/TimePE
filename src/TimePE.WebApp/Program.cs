using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Serilog;
using Serilog.Events;
using TimePE.Core.Database;
using TimePE.Core.Database.Migrations;
using TimePE.Core.Services;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
        .Build())
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

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    XpoDefault.DataLayer = XpoDefault.GetDataLayer(connectionString, AutoCreateOption.DatabaseAndSchema);

    var initializer = new DatabaseInitializer();
    initializer.Initialize(connectionString);

    var userInitializer = new UserInitializer(connectionString);
    await userInitializer.InitializeDefaultUserAsync();

    var projectInitializer = new ProjectInitializer(connectionString);
    await projectInitializer.InitializeDefaultProjectsAsync();

    var payRateInitializer = new PayRateInitializer(connectionString);
    await payRateInitializer.InitializeDefaultPayRateAsync();

    builder.Services.AddSingleton(connectionString);
    builder.Services.AddScoped<IProjectService, ProjectService>(sp => new ProjectService(connectionString));
    builder.Services.AddScoped<IPayRateService, PayRateService>(sp => new PayRateService(connectionString));
    builder.Services.AddScoped<ITimeEntryService, TimeEntryService>(sp => 
        new TimeEntryService(connectionString, sp.GetRequiredService<IPayRateService>()));
    builder.Services.AddScoped<IIncidentalService, IncidentalService>(sp => new IncidentalService(connectionString));
    builder.Services.AddScoped<IPaymentService, PaymentService>(sp => new PaymentService(connectionString));
    builder.Services.AddScoped<IDashboardService, DashboardService>(sp => new DashboardService(connectionString));
    builder.Services.AddScoped<IAuthService, AuthService>(sp => new AuthService(connectionString));
    builder.Services.AddScoped<ICsvService, CsvService>(sp => new CsvService(
        sp.GetRequiredService<ITimeEntryService>(),
        sp.GetRequiredService<IProjectService>(),
        connectionString));

    builder.Services.AddAuthentication("CookieAuth")
        .AddCookie("CookieAuth", options =>
        {
            options.Cookie.Name = "TimePE.Auth";
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();

    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Configure request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null
            ? LogEventLevel.Error
            : httpContext.Response.StatusCode > 499
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
            }
        };
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    Log.Information("TimePE application configured successfully");
    Log.Information("Application starting on: {Environment}", app.Environment.EnvironmentName);
    Log.Information("Content root: {ContentRoot}", app.Environment.ContentRootPath);
    
    app.Run();
    
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
    Log.CloseAndFlush();
}

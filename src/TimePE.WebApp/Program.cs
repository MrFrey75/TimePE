using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Serilog;
using TimePE.Core.Database;
using TimePE.Core.Database.Migrations;
using TimePE.Core.Services;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

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

    Log.Information("TimePE application starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

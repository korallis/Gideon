using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Windows;
using Gideon.WPF.Infrastructure.Data.Context;
using Gideon.WPF.Infrastructure.Data.Repositories;
using Gideon.WPF.Infrastructure.Services;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Presentation.ViewModels.Shared;
using Gideon.WPF.Infrastructure.Configuration;
using FluentValidation;

namespace Gideon.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Initialize logging first
        InitializeLogging();

        try
        {
            // Build and start the host
            _host = CreateHostBuilder(e.Args).Build();
            await _host.StartAsync();

            // Ensure database is created
            await EnsureDatabaseAsync();

            // Initialize Windows services
            var themeService = _host.Services.GetRequiredService<WindowsThemeService>();
            themeService.Initialize();
            themeService.ApplyCurrentTheme();

            var applicationModelService = _host.Services.GetRequiredService<WindowsApplicationModelService>();
            await applicationModelService.InitializeAsync();

            var notificationManager = _host.Services.GetRequiredService<NotificationManager>();
            await notificationManager.InitializeAsync();

            var systemTrayService = _host.Services.GetRequiredService<SystemTrayService>();
            await systemTrayService.InitializeAsync();

            var jumpListService = _host.Services.GetRequiredService<WindowsJumpListService>();
            await jumpListService.InitializeAsync();

            // Show main window
            var mainWindow = _host.Services.GetRequiredService<Presentation.Views.MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start");
            MessageBox.Show($"Application failed to start: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        finally
        {
            Log.CloseAndFlush();
        }

        base.OnExit(e);
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddUserSecrets<App>(optional: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
            })
            .UseSerilog();

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register configuration options
        services.Configure<ApplicationModelConfiguration>(configuration.GetSection(ApplicationModelConfiguration.SectionName));
        services.Configure<NotificationConfiguration>(configuration.GetSection(NotificationConfiguration.SectionName));
        services.Configure<SystemIntegrationConfiguration>(configuration.GetSection(SystemIntegrationConfiguration.SectionName));
        
        // Register Entity Framework
        services.AddDbContext<GideonDbContext>(options =>
        {
            var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon");
            Directory.CreateDirectory(dataFolder);
            var connectionString = $"Data Source={Path.Combine(dataFolder, "gideon.db")}";
            options.UseSqlite(connectionString);
        });

        // Register repositories and unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Register application services
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IShipFittingService, ShipFittingService>();
        
        // Register Windows integration services
        services.AddSingleton<WindowsThemeService>();
        services.AddSingleton<WindowsApplicationModelService>();
        services.AddSingleton<WindowsCredentialService>();
        services.AddSingleton<WindowsNotificationService>();
        services.AddSingleton<NotificationManager>();
        services.AddSingleton<SystemTrayService>();
        services.AddSingleton<WindowsJumpListService>();
        
        // Register HttpClient for API calls
        services.AddHttpClient<IAuthenticationService, AuthenticationService>();
        
        // Register FluentValidation
        services.AddValidatorsFromAssemblyContaining<App>();

        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();

        // Register Views
        services.AddTransient<Presentation.Views.MainWindow>();
    }

    private async Task EnsureDatabaseAsync()
    {
        if (_host == null) return;

        using var scope = _host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GideonDbContext>();
        
        try
        {
            await context.Database.EnsureCreatedAsync();
            Log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize database");
        }
    }

    private static void InitializeLogging()
    {
        var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gideon", "Logs");
        Directory.CreateDirectory(logPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.File(Path.Combine(logPath, "gideon-.log"),
                rollingInterval: Serilog.RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Gideon starting up...");
    }
}
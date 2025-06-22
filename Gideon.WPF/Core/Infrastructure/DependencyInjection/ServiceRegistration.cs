// ==========================================================================
// ServiceRegistration.cs - Dependency Injection Service Registration
// ==========================================================================
// Configuration for dependency injection container registration.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Interfaces;
using Gideon.WPF.Core.Infrastructure.Data;
using Gideon.WPF.Core.Infrastructure.Persistence;
using Gideon.WPF.Core.Infrastructure.Services;
using Gideon.WPF.Core.Infrastructure.Caching;

namespace Gideon.WPF.Core.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for service registration
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Register all application services
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Services
        services.AddDatabaseServices(configuration);
        
        // Repository Services
        services.AddRepositoryServices();
        
        // Application Services
        services.AddApplicationLayerServices();
        
        // Infrastructure Services
        services.AddInfrastructureServices(configuration);
        
        return services;
    }

    /// <summary>
    /// Register database services
    /// </summary>
    private static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=GideonDatabase.db";

        services.AddDbContext<GideonDbContext>(options =>
        {
            options.UseSqlite(connectionString);
            
            #if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
            options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            #endif
        });

        return services;
    }

    /// <summary>
    /// Register repository services
    /// </summary>
    private static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Specialized Repositories
        services.AddScoped<ICharacterRepository, CharacterRepository>();
        services.AddScoped<IShipFittingRepository, ShipFittingRepository>();
        services.AddScoped<IMarketDataRepository, MarketDataRepository>();

        // Generic Repositories (registered as needed)
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }

    /// <summary>
    /// Register application layer services
    /// </summary>
    private static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
    {
        // Application Services (IAuthenticationService now registered in Infrastructure section)
        services.AddScoped<IShipFittingService, ShipFittingService>();
        services.AddScoped<IShipFittingCalculationService, ShipFittingCalculationService>();
        services.AddScoped<ICharacterService, CharacterService>();
        services.AddScoped<IMarketAnalysisService, MarketAnalysisService>();
        services.AddScoped<ISkillPlanningService, SkillPlanningService>();
        
        // Background Services
        services.AddScoped<IDataSynchronizationService, DataSynchronizationService>();
        services.AddScoped<IMarketDataUpdateService, MarketDataUpdateService>();
        services.AddScoped<ICharacterDataUpdateService, CharacterDataUpdateService>();
        
        // Caching Services
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IMarketDataCacheService, MarketDataCacheService>();
        services.AddScoped<ICharacterDataCacheService, CharacterDataCacheService>();

        return services;
    }

    /// <summary>
    /// Register infrastructure services
    /// </summary>
    private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // ESI API Services (Real implementations from EsiApiServices.cs)
        services.AddScoped<IRateLimitService, RateLimitService>();
        services.AddScoped<IEsiApiService, Gideon.WPF.Core.Infrastructure.Services.EsiApiService>();
        services.AddScoped<IEsiCharacterService, Gideon.WPF.Core.Infrastructure.Services.EsiCharacterService>();
        services.AddScoped<IEsiMarketService, Gideon.WPF.Core.Infrastructure.Services.EsiMarketService>();
        services.AddScoped<IEsiUniverseService, Gideon.WPF.Core.Infrastructure.Services.EsiUniverseService>();
        services.AddScoped<IEsiSkillsService, Gideon.WPF.Core.Infrastructure.Services.EsiSkillsService>();

        // Authentication Services (Real implementations from EsiAuthenticationServices.cs)
        services.AddScoped<IOAuth2Service, Gideon.WPF.Core.Infrastructure.Services.OAuth2Service>();
        services.AddScoped<ITokenStorageService, Gideon.WPF.Core.Infrastructure.Services.TokenStorageService>();
        services.AddScoped<IAuthenticationService, Gideon.WPF.Core.Infrastructure.Services.AuthenticationService>();
        
        // Multi-Character Management Services (from MultiCharacterServices.cs)
        services.AddScoped<IMultiCharacterService, MultiCharacterService>();
        services.AddSingleton<ISessionPersistenceService, SessionPersistenceService>();
        
        // ESI Configuration Services (from EsiConfigurationServices.cs)
        services.AddSingleton<IEsiConfigurationService, EsiConfigurationService>();
        services.AddSingleton<IEsiServerStatusService, EsiServerStatusService>();
        
        // ESI Degradation Services (from EsiDegradationServices.cs)
        services.AddSingleton<IEsiDegradationService, EsiDegradationService>();
        services.AddScoped<IOfflineFunctionalityService, OfflineFunctionalityService>();
        
        // ESI Error Handling Services (from EsiErrorHandlingServices.cs)
        services.AddScoped<IEsiErrorHandlingService, EsiErrorHandlingService>();
        services.AddSingleton<IUserNotificationService, UserNotificationService>();
        
        // Character Data Services (from CharacterDataServices.cs)
        services.AddScoped<ICharacterSkillDataService, CharacterSkillDataService>();
        services.AddScoped<ICharacterAssetService, CharacterAssetService>();
        services.AddScoped<ICorporationAllianceDataService, CorporationAllianceDataService>();
        services.AddScoped<IJumpCloneImplantService, JumpCloneImplantService>();
        services.AddScoped<ICharacterWalletService, CharacterWalletService>();
        services.AddScoped<ICharacterLocationService, CharacterLocationService>();
        services.AddScoped<ICharacterSkillQueueService, CharacterSkillQueueService>();
        services.AddScoped<ICharacterContactsService, CharacterContactsService>();
        services.AddScoped<IMarketOrderService, MarketOrderService>();

        // Configuration Services
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IUserPreferencesService, UserPreferencesService>();

        // Validation Services
        services.AddScoped<IDataValidationService, DataValidationService>();
        services.AddScoped<IShipFittingValidationService, ShipFittingValidationService>();

        // Performance and Monitoring
        services.AddScoped<IPerformanceMetricsService, PerformanceMetricsService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();

        // Backup and Export Services
        services.AddScoped<IBackupService, Infrastructure.Services.BackupService>();
        services.AddScoped<IDataExportService, FittingImportExportService>();
        services.AddScoped<IDataImportService, FittingImportExportService>();

        return services;
    }

    /// <summary>
    /// Register background services
    /// </summary>
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<MarketDataBackgroundService>();
        services.AddHostedService<CharacterDataBackgroundService>();
        services.AddHostedService<DatabaseMaintenanceBackgroundService>();
        services.AddHostedService<CacheMaintenanceBackgroundService>();

        return services;
    }

    /// <summary>
    /// Register caching services
    /// </summary>
    public static IServiceCollection AddCachingServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Memory caching
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 100; // MB
            options.CompactionPercentage = 0.25;
        });

        // Distributed caching (SQLite-based for simplicity)
        services.AddDistributedSqliteCache(options =>
        {
            options.CachePath = configuration.GetValue<string>("Caching:CachePath") ?? "cache.db";
            options.DefaultSlidingExpiration = TimeSpan.FromHours(1);
        });

        return services;
    }

    /// <summary>
    /// Register HTTP clients for ESI API with Polly resilience patterns
    /// </summary>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        var esiBaseUrl = configuration.GetValue<string>("ESI:BaseUrl") ?? "https://esi.evetech.net";
        var userAgent = configuration.GetValue<string>("ESI:UserAgent") ?? "Gideon-EVE-Copilot/1.0 (contact@gideon-eve.com)";

        // Configure named HTTP client for ESI
        services.AddHttpClient("ESI", client =>
        {
            client.BaseAddress = new Uri(esiBaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Configure typed HTTP client for ESI API service with Polly resilience
        services.AddHttpClient<Gideon.WPF.Core.Infrastructure.Services.EsiApiService>(client =>
        {
            client.BaseAddress = new Uri(esiBaseUrl);
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(45); // Longer timeout for resilience pipeline
        });

        return services;
    }

    /// <summary>
    /// Register logging services
    /// </summary>
    public static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            
            #if DEBUG
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
            #else
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            #endif
        });

        return services;
    }

    /// <summary>
    /// Configure all services for the application
    /// </summary>
    public static IServiceCollection ConfigureGideonServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationServices(configuration)
            .AddBackgroundServices()
            .AddCachingServices(configuration)
            .AddHttpClients(configuration)
            .AddLoggingServices(configuration);
    }
}
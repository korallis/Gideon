// ==========================================================================
// ServiceLocator.cs - Service Locator for WPF Application
// ==========================================================================
// Service locator pattern implementation for WPF dependency injection.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Gideon.WPF.Core.Infrastructure.DependencyInjection;

/// <summary>
/// Service locator for WPF application dependency injection
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _serviceProvider;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the current service provider
    /// </summary>
    public static IServiceProvider ServiceProvider
    {
        get
        {
            if (_serviceProvider == null)
            {
                throw new InvalidOperationException("ServiceLocator not initialized. Call Initialize() first.");
            }
            return _serviceProvider;
        }
    }

    /// <summary>
    /// Gets whether the service locator is initialized
    /// </summary>
    public static bool IsInitialized => _serviceProvider != null;

    /// <summary>
    /// Initialize the service locator with dependency injection container
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_serviceProvider != null)
                return;

            var services = new ServiceCollection();
            var configuration = BuildConfiguration();

            // Register all application services
            services.ConfigureGideonServices(configuration);

            // Add configuration as singleton
            services.AddSingleton(configuration);

            _serviceProvider = services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// Initialize with custom service provider
    /// </summary>
    public static void Initialize(IServiceProvider serviceProvider)
    {
        lock (_lock)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }
    }

    /// <summary>
    /// Get service of type T
    /// </summary>
    public static T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Get service of type T (nullable)
    /// </summary>
    public static T? GetServiceOrDefault<T>()
    {
        return ServiceProvider.GetService<T>();
    }

    /// <summary>
    /// Get service by type
    /// </summary>
    public static object GetService(Type serviceType)
    {
        return ServiceProvider.GetRequiredService(serviceType);
    }

    /// <summary>
    /// Get service by type (nullable)
    /// </summary>
    public static object? GetServiceOrDefault(Type serviceType)
    {
        return ServiceProvider.GetService(serviceType);
    }

    /// <summary>
    /// Get all services of type T
    /// </summary>
    public static IEnumerable<T> GetServices<T>()
    {
        return ServiceProvider.GetServices<T>();
    }

    /// <summary>
    /// Create a new scope for scoped services
    /// </summary>
    public static IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }

    /// <summary>
    /// Reset the service locator (mainly for testing)
    /// </summary>
    public static void Reset()
    {
        lock (_lock)
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _serviceProvider = null;
        }
    }

    /// <summary>
    /// Build configuration from multiple sources
    /// </summary>
    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(GetApplicationDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables("GIDEON_")
            .AddUserSecrets<ServiceLocator>();

        return builder.Build();
    }

    /// <summary>
    /// Get the application directory
    /// </summary>
    private static string GetApplicationDirectory()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var location = assembly.Location;
        return Path.GetDirectoryName(location) ?? Directory.GetCurrentDirectory();
    }
}

/// <summary>
/// Extension methods for ViewModels to access services easily
/// </summary>
public static class ServiceLocatorExtensions
{
    /// <summary>
    /// Get service from ViewModels or other classes
    /// </summary>
    public static T GetService<T>(this object _) where T : notnull
    {
        return ServiceLocator.GetService<T>();
    }

    /// <summary>
    /// Get service or default from ViewModels or other classes
    /// </summary>
    public static T? GetServiceOrDefault<T>(this object _)
    {
        return ServiceLocator.GetServiceOrDefault<T>();
    }
}
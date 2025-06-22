// ==========================================================================
// MonitoringServices.cs - Performance Monitoring and Metrics Services
// ==========================================================================
// Comprehensive monitoring services for performance tracking, error logging, and audit trails.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Diagnostics;
using System.Text.Json;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Performance Monitoring Services

/// <summary>
/// Performance metrics service for tracking application performance
/// </summary>
public class PerformanceMetricsService : IPerformanceMetricsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PerformanceMetricsService> _logger;
    private readonly Dictionary<string, PerformanceCounter> _performanceCounters;
    private readonly object _countersLock = new object();

    public PerformanceMetricsService(IUnitOfWork unitOfWork, ILogger<PerformanceMetricsService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _performanceCounters = new Dictionary<string, PerformanceCounter>();
    }

    /// <summary>
    /// Record a performance metric
    /// </summary>
    public async Task RecordMetricAsync(string name, double value, string? category = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var metric = new PerformanceMetric
            {
                MetricName = name,
                Value = value,
                Category = category ?? "General",
                RecordedDate = DateTime.UtcNow,
                MachineName = Environment.MachineName,
                ProcessId = Environment.ProcessId
            };

            await _unitOfWork.PerformanceMetrics.AddAsync(metric, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update in-memory performance counters
            UpdatePerformanceCounter(name, value, category);

            _logger.LogDebug("Recorded performance metric: {Name} = {Value} ({Category})", name, value, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance metric: {Name}", name);
        }
    }

    /// <summary>
    /// Get performance metrics for analysis
    /// </summary>
    public async Task<IEnumerable<PerformanceMetric>> GetMetricsAsync(string category, DateTime? from = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddHours(-24); // Default to last 24 hours
            
            var metrics = await _unitOfWork.PerformanceMetrics.FindAsync(
                m => m.Category == category && m.RecordedDate >= fromDate,
                cancellationToken);

            return metrics.OrderBy(m => m.RecordedDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for category: {Category}", category);
            return Enumerable.Empty<PerformanceMetric>();
        }
    }

    /// <summary>
    /// Get performance statistics summary
    /// </summary>
    public async Task<PerformanceStatistics> GetPerformanceStatisticsAsync(string? category = null, TimeSpan? period = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = DateTime.UtcNow.Subtract(period ?? TimeSpan.FromDays(1));
            
            var query = _unitOfWork.PerformanceMetrics.AsQueryable()
                .Where(m => m.RecordedDate >= fromDate);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(m => m.Category == category);

            var metrics = await Task.FromResult(query.ToList());

            if (!metrics.Any())
            {
                return new PerformanceStatistics();
            }

            var statistics = new PerformanceStatistics
            {
                TotalMetrics = metrics.Count,
                AverageValue = metrics.Average(m => m.Value),
                MinValue = metrics.Min(m => m.Value),
                MaxValue = metrics.Max(m => m.Value),
                StandardDeviation = CalculateStandardDeviation(metrics.Select(m => m.Value)),
                Period = period ?? TimeSpan.FromDays(1),
                Category = category,
                MetricsByCategory = metrics.GroupBy(m => m.Category)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TopSlowOperations = metrics.Where(m => m.MetricName.Contains("time") || m.MetricName.Contains("duration"))
                    .OrderByDescending(m => m.Value)
                    .Take(10)
                    .ToList()
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance statistics");
            return new PerformanceStatistics();
        }
    }

    /// <summary>
    /// Record response time for an operation
    /// </summary>
    public async Task<IDisposable> StartTimingAsync(string operationName, string? category = null)
    {
        return new OperationTimer(this, operationName, category ?? "Operations");
    }

    /// <summary>
    /// Record memory usage metrics
    /// </summary>
    public async Task RecordMemoryUsageAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            
            await RecordMetricAsync("memory_working_set_mb", process.WorkingSet64 / 1024.0 / 1024.0, "Memory", cancellationToken);
            await RecordMetricAsync("memory_private_bytes_mb", process.PrivateMemorySize64 / 1024.0 / 1024.0, "Memory", cancellationToken);
            await RecordMetricAsync("memory_virtual_bytes_mb", process.VirtualMemorySize64 / 1024.0 / 1024.0, "Memory", cancellationToken);
            
            // GC metrics
            await RecordMetricAsync("gc_total_memory_mb", GC.GetTotalMemory(false) / 1024.0 / 1024.0, "Memory", cancellationToken);
            await RecordMetricAsync("gc_gen0_collections", GC.CollectionCount(0), "Memory", cancellationToken);
            await RecordMetricAsync("gc_gen1_collections", GC.CollectionCount(1), "Memory", cancellationToken);
            await RecordMetricAsync("gc_gen2_collections", GC.CollectionCount(2), "Memory", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording memory usage metrics");
        }
    }

    /// <summary>
    /// Record system performance metrics
    /// </summary>
    public async Task RecordSystemMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            
            await RecordMetricAsync("cpu_time_total_ms", process.TotalProcessorTime.TotalMilliseconds, "System", cancellationToken);
            await RecordMetricAsync("thread_count", process.Threads.Count, "System", cancellationToken);
            await RecordMetricAsync("handle_count", process.HandleCount, "System", cancellationToken);
            
            // Application uptime
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            await RecordMetricAsync("uptime_hours", uptime.TotalHours, "System", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording system metrics");
        }
    }

    #region Helper Methods

    private void UpdatePerformanceCounter(string name, double value, string? category)
    {
        lock (_countersLock)
        {
            var key = $"{category}:{name}";
            
            if (_performanceCounters.TryGetValue(key, out var counter))
            {
                counter.Update(value);
            }
            else
            {
                _performanceCounters[key] = new PerformanceCounter(name, category, value);
            }
        }
    }

    private static double CalculateStandardDeviation(IEnumerable<double> values)
    {
        var valuesList = values.ToList();
        if (valuesList.Count < 2) return 0;

        var average = valuesList.Average();
        var sumOfSquares = valuesList.Sum(val => (val - average) * (val - average));
        return Math.Sqrt(sumOfSquares / (valuesList.Count - 1));
    }

    #endregion

    #region Operation Timer

    private class OperationTimer : IDisposable
    {
        private readonly PerformanceMetricsService _metricsService;
        private readonly string _operationName;
        private readonly string _category;
        private readonly Stopwatch _stopwatch;
        private bool _disposed;

        public OperationTimer(PerformanceMetricsService metricsService, string operationName, string category)
        {
            _metricsService = metricsService;
            _operationName = operationName;
            _category = category;
            _stopwatch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _stopwatch.Stop();
                _ = Task.Run(async () =>
                {
                    await _metricsService.RecordMetricAsync(
                        $"{_operationName}_duration_ms",
                        _stopwatch.ElapsedMilliseconds,
                        _category);
                });
                _disposed = true;
            }
        }
    }

    #endregion

    #region Performance Counter

    private class PerformanceCounter
    {
        public string Name { get; }
        public string? Category { get; }
        public double CurrentValue { get; private set; }
        public double MinValue { get; private set; }
        public double MaxValue { get; private set; }
        public double AverageValue { get; private set; }
        public int SampleCount { get; private set; }
        public DateTime LastUpdated { get; private set; }

        public PerformanceCounter(string name, string? category, double initialValue)
        {
            Name = name;
            Category = category;
            CurrentValue = initialValue;
            MinValue = initialValue;
            MaxValue = initialValue;
            AverageValue = initialValue;
            SampleCount = 1;
            LastUpdated = DateTime.UtcNow;
        }

        public void Update(double value)
        {
            CurrentValue = value;
            MinValue = Math.Min(MinValue, value);
            MaxValue = Math.Max(MaxValue, value);
            AverageValue = ((AverageValue * SampleCount) + value) / (SampleCount + 1);
            SampleCount++;
            LastUpdated = DateTime.UtcNow;
        }
    }

    #endregion
}

#endregion

#region Audit Logging Services

/// <summary>
/// Audit logging service for tracking user actions and system events
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IUnitOfWork unitOfWork, ILogger<AuditLogService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Log an audit action
    /// </summary>
    public async Task LogActionAsync(string action, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                IPAddress = GetCurrentIPAddress(),
                UserAgent = GetCurrentUserAgent(),
                SessionId = GetCurrentSessionId(),
                AdditionalData = GetAdditionalContextData()
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit log recorded: {Action} on {EntityType}:{EntityId} by {UserName}",
                action, entityType, entityId, auditLog.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording audit log for action: {Action}", action);
        }
    }

    /// <summary>
    /// Log a detailed audit action with custom data
    /// </summary>
    public async Task LogDetailedActionAsync(string action, object? details, string? entityType = null, string? entityId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                IPAddress = GetCurrentIPAddress(),
                UserAgent = GetCurrentUserAgent(),
                SessionId = GetCurrentSessionId(),
                AdditionalData = details != null ? JsonSerializer.Serialize(details) : GetAdditionalContextData()
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Detailed audit log recorded: {Action} on {EntityType}:{EntityId}",
                action, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording detailed audit log for action: {Action}", action);
        }
    }

    /// <summary>
    /// Get audit logs with filtering
    /// </summary>
    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(DateTime? from = null, string? entityType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30); // Default to last 30 days
            
            var query = _unitOfWork.AuditLogs.AsQueryable()
                .Where(log => log.Timestamp >= fromDate);

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(log => log.EntityType == entityType);

            var logs = await Task.FromResult(query.OrderByDescending(log => log.Timestamp).ToList());
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return Enumerable.Empty<AuditLog>();
        }
    }

    /// <summary>
    /// Get audit statistics for reporting
    /// </summary>
    public async Task<AuditStatistics> GetAuditStatisticsAsync(DateTime? from = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7); // Default to last 7 days
            
            var logs = await _unitOfWork.AuditLogs.FindAsync(log => log.Timestamp >= fromDate, cancellationToken);
            
            var statistics = new AuditStatistics
            {
                TotalActions = logs.Count(),
                Period = DateTime.UtcNow - fromDate,
                ActionsByType = logs.GroupBy(l => l.Action)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActionsByUser = logs.GroupBy(l => l.UserName)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                ActionsByEntityType = logs.GroupBy(l => l.EntityType)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToDictionary(g => g.Key!, g => g.Count()),
                HourlyActivity = logs.GroupBy(l => l.Timestamp.Hour)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating audit statistics");
            return new AuditStatistics();
        }
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        // TODO: Implement current user ID retrieval from authentication context
        return Environment.UserName;
    }

    private string? GetCurrentUserName()
    {
        // TODO: Implement current user name retrieval from authentication context
        return Environment.UserName;
    }

    private string? GetCurrentIPAddress()
    {
        // TODO: Implement IP address retrieval from HTTP context
        return "127.0.0.1";
    }

    private string? GetCurrentUserAgent()
    {
        // TODO: Implement user agent retrieval from HTTP context
        return "Gideon/1.0";
    }

    private string? GetCurrentSessionId()
    {
        // TODO: Implement session ID retrieval from authentication context
        return Guid.NewGuid().ToString();
    }

    private string? GetAdditionalContextData()
    {
        var context = new
        {
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(context);
    }

    #endregion
}

#endregion

#region Error Logging Services

/// <summary>
/// Error logging service for tracking and managing application errors
/// </summary>
public class ErrorLogService : IErrorLogService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ErrorLogService> _logger;

    public ErrorLogService(IUnitOfWork unitOfWork, ILogger<ErrorLogService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Log an error with details
    /// </summary>
    public async Task LogErrorAsync(string message, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var errorLog = new ErrorLog
            {
                Message = message,
                ExceptionType = exception?.GetType().Name,
                ExceptionMessage = exception?.Message,
                StackTrace = exception?.StackTrace,
                Source = exception?.Source,
                LoggedDate = DateTime.UtcNow,
                Severity = DetermineErrorSeverity(exception),
                Category = DetermineErrorCategory(exception),
                UserId = GetCurrentUserId(),
                MachineName = Environment.MachineName,
                ApplicationVersion = GetApplicationVersion(),
                AdditionalData = GetErrorContextData(exception)
            };

            await _unitOfWork.ErrorLogs.AddAsync(errorLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(exception, "Error logged: {Message} - {ExceptionType}", message, exception?.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical error: Failed to log error to database");
        }
    }

    /// <summary>
    /// Get error logs with filtering
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetErrorLogsAsync(DateTime? from = null, bool? resolved = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-7); // Default to last 7 days
            
            var query = _unitOfWork.ErrorLogs.AsQueryable()
                .Where(log => log.LoggedDate >= fromDate);

            if (resolved.HasValue)
                query = query.Where(log => log.IsResolved == resolved.Value);

            var logs = await Task.FromResult(query.OrderByDescending(log => log.LoggedDate).ToList());
            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error logs");
            return Enumerable.Empty<ErrorLog>();
        }
    }

    /// <summary>
    /// Mark an error as resolved
    /// </summary>
    public async Task MarkErrorResolvedAsync(int errorId, string? resolution = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var errorLog = await _unitOfWork.ErrorLogs.GetByIdAsync(errorId, cancellationToken);
            if (errorLog != null)
            {
                errorLog.IsResolved = true;
                errorLog.ResolvedDate = DateTime.UtcNow;
                errorLog.Resolution = resolution;
                errorLog.ResolvedBy = GetCurrentUserId();

                await _unitOfWork.ErrorLogs.UpdateAsync(errorLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Error {ErrorId} marked as resolved by {User}", errorId, errorLog.ResolvedBy);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking error {ErrorId} as resolved", errorId);
        }
    }

    /// <summary>
    /// Get error statistics for monitoring
    /// </summary>
    public async Task<ErrorStatistics> GetErrorStatisticsAsync(DateTime? from = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-1); // Default to last 24 hours
            
            var errors = await _unitOfWork.ErrorLogs.FindAsync(log => log.LoggedDate >= fromDate, cancellationToken);
            
            var statistics = new ErrorStatistics
            {
                TotalErrors = errors.Count(),
                UnresolvedErrors = errors.Count(e => !e.IsResolved),
                ResolvedErrors = errors.Count(e => e.IsResolved),
                Period = DateTime.UtcNow - fromDate,
                ErrorsBySeverity = errors.GroupBy(e => e.Severity)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ErrorsByCategory = errors.GroupBy(e => e.Category)
                    .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                ErrorsByType = errors.GroupBy(e => e.ExceptionType)
                    .Where(g => !string.IsNullOrEmpty(g.Key))
                    .ToDictionary(g => g.Key!, g => g.Count()),
                MostFrequentErrors = errors.GroupBy(e => e.Message)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating error statistics");
            return new ErrorStatistics();
        }
    }

    #region Helper Methods

    private string DetermineErrorSeverity(Exception? exception)
    {
        return exception switch
        {
            ArgumentNullException => "High",
            InvalidOperationException => "Medium",
            NotImplementedException => "Low",
            OutOfMemoryException => "Critical",
            StackOverflowException => "Critical",
            AccessViolationException => "Critical",
            _ => "Medium"
        };
    }

    private string DetermineErrorCategory(Exception? exception)
    {
        if (exception == null) return "General";

        var typeName = exception.GetType().Name;
        return typeName switch
        {
            var name when name.Contains("Data") || name.Contains("Sql") => "Database",
            var name when name.Contains("Http") || name.Contains("Web") => "Network",
            var name when name.Contains("IO") || name.Contains("File") => "FileSystem",
            var name when name.Contains("Security") || name.Contains("Unauthorized") => "Security",
            var name when name.Contains("Argument") || name.Contains("Format") => "Validation",
            _ => "Application"
        };
    }

    private string? GetCurrentUserId()
    {
        // TODO: Implement current user ID retrieval from authentication context
        return Environment.UserName;
    }

    private string GetApplicationVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    private string? GetErrorContextData(Exception? exception)
    {
        var context = new
        {
            MachineName = Environment.MachineName,
            ProcessId = Environment.ProcessId,
            ThreadId = Environment.CurrentManagedThreadId,
            Timestamp = DateTime.UtcNow,
            InnerException = exception?.InnerException?.Message,
            Data = exception?.Data?.Count > 0 ? exception.Data : null
        };

        return JsonSerializer.Serialize(context);
    }

    #endregion
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Performance statistics summary
/// </summary>
public class PerformanceStatistics
{
    public int TotalMetrics { get; set; }
    public double AverageValue { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double StandardDeviation { get; set; }
    public TimeSpan Period { get; set; }
    public string? Category { get; set; }
    public Dictionary<string, int> MetricsByCategory { get; set; } = new();
    public List<PerformanceMetric> TopSlowOperations { get; set; } = new();
}

/// <summary>
/// Audit statistics summary
/// </summary>
public class AuditStatistics
{
    public int TotalActions { get; set; }
    public TimeSpan Period { get; set; }
    public Dictionary<string, int> ActionsByType { get; set; } = new();
    public Dictionary<string, int> ActionsByUser { get; set; } = new();
    public Dictionary<string, int> ActionsByEntityType { get; set; } = new();
    public Dictionary<int, int> HourlyActivity { get; set; } = new();
}

/// <summary>
/// Error statistics summary
/// </summary>
public class ErrorStatistics
{
    public int TotalErrors { get; set; }
    public int UnresolvedErrors { get; set; }
    public int ResolvedErrors { get; set; }
    public TimeSpan Period { get; set; }
    public Dictionary<string, int> ErrorsBySeverity { get; set; } = new();
    public Dictionary<string, int> ErrorsByCategory { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, int> MostFrequentErrors { get; set; } = new();
}

#endregion
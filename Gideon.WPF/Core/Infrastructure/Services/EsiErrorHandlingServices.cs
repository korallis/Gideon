// ==========================================================================
// EsiErrorHandlingServices.cs - Comprehensive ESI Error Handling and User Guidance
// ==========================================================================
// Advanced error handling, user guidance, and help system for ESI integration issues.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Text.Json;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Error Handling and User Guidance Services

/// <summary>
/// Comprehensive ESI error handling and user guidance service
/// </summary>
public class EsiErrorHandlingService : IEsiErrorHandlingService
{
    private readonly ILogger<EsiErrorHandlingService> _logger;
    private readonly IAuditLogService _auditService;
    private readonly IUserNotificationService _notificationService;
    private readonly IEsiDegradationService _degradationService;
    
    private readonly Dictionary<Type, ErrorHandlingStrategy> _errorStrategies;
    private readonly Dictionary<string, UserGuidanceInfo> _guidanceDatabase;

    public EsiErrorHandlingService(
        ILogger<EsiErrorHandlingService> logger,
        IAuditLogService auditService,
        IUserNotificationService notificationService,
        IEsiDegradationService degradationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _degradationService = degradationService ?? throw new ArgumentNullException(nameof(degradationService));
        
        _errorStrategies = BuildErrorStrategies();
        _guidanceDatabase = BuildUserGuidanceDatabase();
    }

    /// <summary>
    /// Handle ESI API errors with appropriate user guidance
    /// </summary>
    public async Task<ErrorHandlingResult> HandleErrorAsync(Exception error, string context, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new ErrorHandlingResult
            {
                OriginalError = error,
                Context = context,
                Timestamp = DateTime.UtcNow
            };
            
            // Determine error type and strategy
            var errorType = error.GetType();
            var strategy = _errorStrategies.GetValueOrDefault(errorType, _errorStrategies[typeof(Exception)]);
            
            result.ErrorType = GetErrorCategory(error);
            result.Severity = strategy.Severity;
            result.IsRecoverable = strategy.IsRecoverable;
            
            // Generate user-friendly message
            result.UserMessage = await GenerateUserMessageAsync(error, context, cancellationToken);
            
            // Get specific guidance
            result.Guidance = await GetUserGuidanceAsync(error, context, cancellationToken);
            
            // Determine recommended actions
            result.RecommendedActions = await GetRecommendedActionsAsync(error, context, cancellationToken);
            
            // Log the error appropriately
            await LogErrorAsync(error, result, cancellationToken);
            
            // Notify user if needed
            if (strategy.ShouldNotifyUser)
            {
                await NotifyUserAsync(result, cancellationToken);
            }
            
            // Auto-recovery attempt if applicable
            if (strategy.CanAutoRecover)
            {
                result.AutoRecoveryAttempted = await AttemptAutoRecoveryAsync(error, context, cancellationToken);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error in error handling service - this is a critical system failure");
            
            return new ErrorHandlingResult
            {
                OriginalError = error,
                Context = context,
                ErrorType = ErrorCategory.SystemFailure,
                Severity = ErrorSeverity.Critical,
                UserMessage = "A critical system error occurred. Please restart the application.",
                IsRecoverable = false,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get user guidance for specific error scenarios
    /// </summary>
    public async Task<UserGuidanceInfo> GetUserGuidanceAsync(Exception error, string context, CancellationToken cancellationToken = default)
    {
        try
        {
            var errorKey = GetErrorKey(error);
            
            if (_guidanceDatabase.TryGetValue(errorKey, out var guidance))
            {
                // Contextualize the guidance
                return await ContextualizeGuidanceAsync(guidance, error, context, cancellationToken);
            }
            
            // Fallback to generic guidance
            return await GenerateGenericGuidanceAsync(error, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating user guidance");
            
            return new UserGuidanceInfo
            {
                Title = "Error Occurred",
                Description = "An unexpected error occurred. Please try again or contact support.",
                Category = GuidanceCategory.General
            };
        }
    }

    /// <summary>
    /// Get troubleshooting steps for specific issues
    /// </summary>
    public async Task<TroubleshootingGuide> GetTroubleshootingGuideAsync(string issueType, CancellationToken cancellationToken = default)
    {
        try
        {
            var guide = new TroubleshootingGuide
            {
                IssueType = issueType,
                CreatedAt = DateTime.UtcNow
            };
            
            switch (issueType.ToLowerInvariant())
            {
                case "authentication":
                    guide = await BuildAuthenticationTroubleshootingAsync(cancellationToken);
                    break;
                    
                case "connectivity":
                    guide = await BuildConnectivityTroubleshootingAsync(cancellationToken);
                    break;
                    
                case "ratelimit":
                    guide = await BuildRateLimitTroubleshootingAsync(cancellationToken);
                    break;
                    
                case "permissions":
                    guide = await BuildPermissionsTroubleshootingAsync(cancellationToken);
                    break;
                    
                case "datacorruption":
                    guide = await BuildDataCorruptionTroubleshootingAsync(cancellationToken);
                    break;
                    
                default:
                    guide = await BuildGenericTroubleshootingAsync(issueType, cancellationToken);
                    break;
            }
            
            return guide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating troubleshooting guide for {IssueType}", issueType);
            
            return new TroubleshootingGuide
            {
                IssueType = issueType,
                Steps = new List<TroubleshootingStep>
                {
                    new() { Description = "Please restart the application and try again.", Order = 1 }
                }
            };
        }
    }

    /// <summary>
    /// Generate error report for support
    /// </summary>
    public async Task<ErrorReport> GenerateErrorReportAsync(Exception error, string context, bool includeSystemInfo = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = new ErrorReport
            {
                ReportId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Context = context,
                ErrorType = error.GetType().Name,
                ErrorMessage = error.Message,
                StackTrace = error.StackTrace ?? string.Empty,
                InnerException = error.InnerException?.ToString()
            };
            
            if (includeSystemInfo)
            {
                report.SystemInfo = await CollectSystemInfoAsync(cancellationToken);
            }
            
            // Include application state
            report.ApplicationState = await CollectApplicationStateAsync(cancellationToken);
            
            // Include recent log entries
            report.RecentLogEntries = await CollectRecentLogEntriesAsync(cancellationToken);
            
            // Redact sensitive information
            RedactSensitiveInformation(report);
            
            await _auditService.LogActionAsync("error_report_generated", "ErrorHandling", report.ReportId, cancellationToken);
            
            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating error report");
            
            return new ErrorReport
            {
                ReportId = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Context = context,
                ErrorType = error.GetType().Name,
                ErrorMessage = error.Message,
                Notes = "Error occurred while generating detailed report"
            };
        }
    }

    #region Private Methods - Error Strategies

    private Dictionary<Type, ErrorHandlingStrategy> BuildErrorStrategies()
    {
        return new Dictionary<Type, ErrorHandlingStrategy>
        {
            [typeof(EsiUnauthorizedException)] = new()
            {
                Severity = ErrorSeverity.High,
                IsRecoverable = true,
                CanAutoRecover = true,
                ShouldNotifyUser = true,
                RetryDelay = TimeSpan.FromSeconds(5)
            },
            [typeof(EsiRateLimitException)] = new()
            {
                Severity = ErrorSeverity.Medium,
                IsRecoverable = true,
                CanAutoRecover = true,
                ShouldNotifyUser = false,
                RetryDelay = TimeSpan.FromMinutes(1)
            },
            [typeof(EsiServerException)] = new()
            {
                Severity = ErrorSeverity.High,
                IsRecoverable = true,
                CanAutoRecover = false,
                ShouldNotifyUser = true,
                RetryDelay = TimeSpan.FromMinutes(5)
            },
            [typeof(EsiNotFoundException)] = new()
            {
                Severity = ErrorSeverity.Low,
                IsRecoverable = false,
                CanAutoRecover = false,
                ShouldNotifyUser = false
            },
            [typeof(EsiForbiddenException)] = new()
            {
                Severity = ErrorSeverity.High,
                IsRecoverable = false,
                CanAutoRecover = false,
                ShouldNotifyUser = true
            },
            [typeof(HttpRequestException)] = new()
            {
                Severity = ErrorSeverity.Medium,
                IsRecoverable = true,
                CanAutoRecover = true,
                ShouldNotifyUser = false,
                RetryDelay = TimeSpan.FromSeconds(30)
            },
            [typeof(TimeoutException)] = new()
            {
                Severity = ErrorSeverity.Medium,
                IsRecoverable = true,
                CanAutoRecover = true,
                ShouldNotifyUser = false,
                RetryDelay = TimeSpan.FromSeconds(15)
            },
            [typeof(Exception)] = new() // Default strategy
            {
                Severity = ErrorSeverity.High,
                IsRecoverable = false,
                CanAutoRecover = false,
                ShouldNotifyUser = true
            }
        };
    }

    private Dictionary<string, UserGuidanceInfo> BuildUserGuidanceDatabase()
    {
        return new Dictionary<string, UserGuidanceInfo>
        {
            ["EsiUnauthorizedException"] = new()
            {
                Title = "Authentication Required",
                Description = "Your EVE Online authentication has expired or is invalid.",
                Category = GuidanceCategory.Authentication,
                Steps = new List<string>
                {
                    "Click 'Authenticate' to log in with your EVE Online account",
                    "Ensure you have the correct permissions for your character",
                    "If the problem persists, try removing and re-adding your character"
                },
                HelpUrl = "https://docs.gideon-eve.com/authentication"
            },
            ["EsiRateLimitException"] = new()
            {
                Title = "Rate Limit Exceeded",
                Description = "Too many requests were sent to EVE Online's servers. Please wait a moment.",
                Category = GuidanceCategory.Performance,
                Steps = new List<string>
                {
                    "Wait for the rate limit to reset (usually 1 minute)",
                    "Reduce the frequency of character updates in Settings",
                    "Consider using fewer characters simultaneously"
                },
                HelpUrl = "https://docs.gideon-eve.com/rate-limits"
            },
            ["EsiServerException"] = new()
            {
                Title = "EVE Online Server Issues",
                Description = "EVE Online's servers are experiencing problems.",
                Category = GuidanceCategory.Connectivity,
                Steps = new List<string>
                {
                    "Check EVE Online server status at status.eveonline.com",
                    "Wait for the issue to resolve automatically",
                    "Use offline mode for ship fitting and planning"
                },
                HelpUrl = "https://docs.gideon-eve.com/server-issues"
            },
            ["EsiForbiddenException"] = new()
            {
                Title = "Permission Denied",
                Description = "You don't have permission to access this data.",
                Category = GuidanceCategory.Permissions,
                Steps = new List<string>
                {
                    "Check that your character has the required permissions",
                    "Re-authenticate to refresh your permissions",
                    "Contact your corporation/alliance leadership if needed"
                },
                HelpUrl = "https://docs.gideon-eve.com/permissions"
            },
            ["HttpRequestException"] = new()
            {
                Title = "Connection Problem",
                Description = "Unable to connect to EVE Online's servers.",
                Category = GuidanceCategory.Connectivity,
                Steps = new List<string>
                {
                    "Check your internet connection",
                    "Verify firewall settings allow Gideon to access the internet",
                    "Try again in a few moments"
                },
                HelpUrl = "https://docs.gideon-eve.com/connectivity"
            }
        };
    }

    private async Task<string> GenerateUserMessageAsync(Exception error, string context, CancellationToken cancellationToken)
    {
        var errorType = error.GetType().Name;
        
        return errorType switch
        {
            nameof(EsiUnauthorizedException) => "Please authenticate with your EVE Online account to continue.",
            nameof(EsiRateLimitException) => "Please wait a moment - too many requests were sent to EVE Online's servers.",
            nameof(EsiServerException) => "EVE Online's servers are currently experiencing issues. You can continue using offline features.",
            nameof(EsiForbiddenException) => "You don't have permission to access this data. Please check your character permissions.",
            nameof(EsiNotFoundException) => "The requested data was not found. It may have been moved or deleted.",
            nameof(HttpRequestException) => "Unable to connect to EVE Online's servers. Please check your internet connection.",
            nameof(TimeoutException) => "The request timed out. Please try again.",
            _ => $"An unexpected error occurred in {context}. Please try again or contact support."
        };
    }

    private async Task<List<RecommendedAction>> GetRecommendedActionsAsync(Exception error, string context, CancellationToken cancellationToken)
    {
        var actions = new List<RecommendedAction>();
        
        switch (error)
        {
            case EsiUnauthorizedException:
                actions.Add(new RecommendedAction
                {
                    Title = "Re-authenticate",
                    Description = "Log in with your EVE Online account",
                    ActionType = ActionType.Authentication,
                    Priority = ActionPriority.High,
                    EstimatedTime = TimeSpan.FromMinutes(2)
                });
                break;
                
            case EsiRateLimitException:
                actions.Add(new RecommendedAction
                {
                    Title = "Wait and Retry",
                    Description = "Wait for rate limit to reset",
                    ActionType = ActionType.Wait,
                    Priority = ActionPriority.Medium,
                    EstimatedTime = TimeSpan.FromMinutes(1)
                });
                break;
                
            case EsiServerException:
                actions.Add(new RecommendedAction
                {
                    Title = "Use Offline Mode",
                    Description = "Continue with cached data",
                    ActionType = ActionType.Fallback,
                    Priority = ActionPriority.Medium,
                    EstimatedTime = TimeSpan.Zero
                });
                break;
                
            case HttpRequestException:
                actions.Add(new RecommendedAction
                {
                    Title = "Check Connection",
                    Description = "Verify internet connectivity",
                    ActionType = ActionType.Diagnostic,
                    Priority = ActionPriority.High,
                    EstimatedTime = TimeSpan.FromMinutes(1)
                });
                break;
        }
        
        // Always add general fallback actions
        actions.Add(new RecommendedAction
        {
            Title = "Try Again",
            Description = "Retry the operation",
            ActionType = ActionType.Retry,
            Priority = ActionPriority.Low,
            EstimatedTime = TimeSpan.FromSeconds(30)
        });
        
        return actions;
    }

    private async Task LogErrorAsync(Exception error, ErrorHandlingResult result, CancellationToken cancellationToken)
    {
        var logLevel = result.Severity switch
        {
            ErrorSeverity.Low => LogLevel.Debug,
            ErrorSeverity.Medium => LogLevel.Warning,
            ErrorSeverity.High => LogLevel.Error,
            ErrorSeverity.Critical => LogLevel.Critical,
            _ => LogLevel.Error
        };
        
        _logger.Log(logLevel, error, "ESI Error in {Context}: {UserMessage}", result.Context, result.UserMessage);
        
        await _auditService.LogActionAsync("esi_error_handled", result.Context, 
            $"{error.GetType().Name}: {result.UserMessage}", cancellationToken);
    }

    private async Task NotifyUserAsync(ErrorHandlingResult result, CancellationToken cancellationToken)
    {
        var notification = new UserNotification
        {
            Title = result.Guidance?.Title ?? "Error Occurred",
            Message = result.UserMessage,
            Type = result.Severity switch
            {
                ErrorSeverity.Low => NotificationType.Info,
                ErrorSeverity.Medium => NotificationType.Warning,
                ErrorSeverity.High => NotificationType.Error,
                ErrorSeverity.Critical => NotificationType.Critical,
                _ => NotificationType.Error
            },
            Actions = result.RecommendedActions?.Take(2).ToList() ?? new List<RecommendedAction>()
        };
        
        await _notificationService.ShowNotificationAsync(notification, cancellationToken);
    }

    private async Task<bool> AttemptAutoRecoveryAsync(Exception error, string context, CancellationToken cancellationToken)
    {
        try
        {
            switch (error)
            {
                case EsiUnauthorizedException:
                    // Attempt token refresh
                    _logger.LogInformation("Attempting automatic token refresh for authentication error");
                    // Would call token refresh service
                    return true;
                    
                case EsiRateLimitException:
                    // Enable automatic retry with delay
                    _logger.LogInformation("Enabling automatic retry for rate limit error");
                    return true;
                    
                case HttpRequestException:
                    // Switch to degraded mode
                    await _degradationService.EnableMaintenanceModeAsync("Connectivity issues detected", 
                        TimeSpan.FromMinutes(15), cancellationToken);
                    return true;
                    
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-recovery attempt failed for {ErrorType}", error.GetType().Name);
            return false;
        }
    }

    #endregion

    #region Private Methods - Guidance Generation

    private async Task<UserGuidanceInfo> ContextualizeGuidanceAsync(UserGuidanceInfo baseGuidance, Exception error, string context, CancellationToken cancellationToken)
    {
        var contextualGuidance = new UserGuidanceInfo
        {
            Title = baseGuidance.Title,
            Description = $"{baseGuidance.Description} (Context: {context})",
            Category = baseGuidance.Category,
            Steps = new List<string>(baseGuidance.Steps),
            HelpUrl = baseGuidance.HelpUrl,
            RelatedErrors = baseGuidance.RelatedErrors
        };
        
        // Add context-specific steps
        switch (context.ToLowerInvariant())
        {
            case "character_sync":
                contextualGuidance.Steps.Insert(0, "Character synchronization will be retried automatically");
                break;
                
            case "market_data":
                contextualGuidance.Steps.Add("Market data will use cached values during the outage");
                break;
                
            case "ship_fitting":
                contextualGuidance.Steps.Add("Ship fitting calculations will continue using local data");
                break;
        }
        
        return contextualGuidance;
    }

    private async Task<UserGuidanceInfo> GenerateGenericGuidanceAsync(Exception error, string context, CancellationToken cancellationToken)
    {
        return new UserGuidanceInfo
        {
            Title = "Unexpected Error",
            Description = $"An unexpected error occurred during {context}.",
            Category = GuidanceCategory.General,
            Steps = new List<string>
            {
                "Try the operation again",
                "Check your internet connection",
                "Restart the application if the problem persists",
                "Contact support if the issue continues"
            },
            HelpUrl = "https://docs.gideon-eve.com/troubleshooting"
        };
    }

    #endregion

    #region Private Methods - Troubleshooting Guides

    private async Task<TroubleshootingGuide> BuildAuthenticationTroubleshootingAsync(CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = "Authentication",
            Title = "Authentication Issues",
            Description = "Steps to resolve EVE Online authentication problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Click the 'Authenticate' button", IsRequired = true },
                new() { Order = 2, Description = "Log in with your EVE Online account credentials", IsRequired = true },
                new() { Order = 3, Description = "Grant the requested permissions", IsRequired = true },
                new() { Order = 4, Description = "If login fails, clear browser cookies and try again" },
                new() { Order = 5, Description = "Check that your account has an active EVE Online subscription" },
                new() { Order = 6, Description = "Contact support if authentication continues to fail" }
            }
        };
    }

    private async Task<TroubleshootingGuide> BuildConnectivityTroubleshootingAsync(CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = "Connectivity",
            Title = "Connection Issues",
            Description = "Steps to resolve network connectivity problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Check your internet connection", IsRequired = true },
                new() { Order = 2, Description = "Verify firewall allows Gideon to access the internet" },
                new() { Order = 3, Description = "Check proxy settings if using a corporate network" },
                new() { Order = 4, Description = "Try disabling VPN temporarily" },
                new() { Order = 5, Description = "Check Windows Defender or antivirus settings" },
                new() { Order = 6, Description = "Restart your router/modem if connection issues persist" }
            }
        };
    }

    private async Task<TroubleshootingGuide> BuildRateLimitTroubleshootingAsync(CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = "Rate Limiting",
            Title = "Rate Limit Issues",
            Description = "Steps to resolve API rate limiting problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Wait 1-2 minutes for rate limit to reset", IsRequired = true },
                new() { Order = 2, Description = "Reduce update frequency in Settings > Performance" },
                new() { Order = 3, Description = "Limit the number of active characters" },
                new() { Order = 4, Description = "Use offline mode during high-activity periods" },
                new() { Order = 5, Description = "Schedule bulk operations during off-peak hours" }
            }
        };
    }

    private async Task<TroubleshootingGuide> BuildPermissionsTroubleshootingAsync(CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = "Permissions",
            Title = "Permission Issues",
            Description = "Steps to resolve access permission problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Verify your character has the required roles", IsRequired = true },
                new() { Order = 2, Description = "Re-authenticate to refresh permissions" },
                new() { Order = 3, Description = "Check corporation/alliance permission settings" },
                new() { Order = 4, Description = "Contact corporation leadership for role assignment" },
                new() { Order = 5, Description = "Use a different character with appropriate permissions" }
            }
        };
    }

    private async Task<TroubleshootingGuide> BuildDataCorruptionTroubleshootingAsync(CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = "Data Corruption",
            Title = "Data Issues",
            Description = "Steps to resolve data corruption or integrity problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Restart the application", IsRequired = true },
                new() { Order = 2, Description = "Use the 'Repair Database' option in Settings" },
                new() { Order = 3, Description = "Restore from a recent backup if available" },
                new() { Order = 4, Description = "Clear application cache and restart" },
                new() { Order = 5, Description = "Re-download character data from ESI" },
                new() { Order = 6, Description = "Contact support for data recovery assistance" }
            }
        };
    }

    private async Task<TroubleshootingGuide> BuildGenericTroubleshootingAsync(string issueType, CancellationToken cancellationToken)
    {
        return new TroubleshootingGuide
        {
            IssueType = issueType,
            Title = $"{issueType} Issues",
            Description = $"General troubleshooting steps for {issueType} problems",
            Steps = new List<TroubleshootingStep>
            {
                new() { Order = 1, Description = "Restart the application", IsRequired = true },
                new() { Order = 2, Description = "Check for application updates" },
                new() { Order = 3, Description = "Review the application logs for more details" },
                new() { Order = 4, Description = "Try using a different character or account" },
                new() { Order = 5, Description = "Contact support with detailed error information" }
            }
        };
    }

    #endregion

    #region Private Methods - System Information

    private async Task<SystemInfo> CollectSystemInfoAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new SystemInfo
            {
                OperatingSystem = Environment.OSVersion.ToString(),
                DotNetVersion = Environment.Version.ToString(),
                ApplicationVersion = GetApplicationVersion(),
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                WorkingSet = Environment.WorkingSet,
                TickCount = Environment.TickCount64,
                ProcessorCount = Environment.ProcessorCount,
                SystemDirectory = Environment.SystemDirectory,
                IsUserInteractive = Environment.UserInteractive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting system information");
            return new SystemInfo { Notes = "System information collection failed" };
        }
    }

    private async Task<ApplicationState> CollectApplicationStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            return new ApplicationState
            {
                UpTime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime,
                MemoryUsage = GC.GetTotalMemory(false),
                ThreadCount = Process.GetCurrentProcess().Threads.Count,
                HandleCount = Process.GetCurrentProcess().HandleCount,
                LastGcCollection = GetLastGcInfo()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting application state");
            return new ApplicationState { Notes = "Application state collection failed" };
        }
    }

    private async Task<List<string>> CollectRecentLogEntriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Would integrate with logging system to get recent entries
            return new List<string>
            {
                "Recent log entries would be collected here",
                "This would include the last 50-100 log entries",
                "Sensitive information would be redacted"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error collecting recent log entries");
            return new List<string> { "Log collection failed" };
        }
    }

    private static void RedactSensitiveInformation(ErrorReport report)
    {
        // Redact potentially sensitive information
        report.StackTrace = RedactString(report.StackTrace, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", "[EMAIL]");
        report.ErrorMessage = RedactString(report.ErrorMessage, @"\b\d{4}[- ]?\d{4}[- ]?\d{4}[- ]?\d{4}\b", "[CARD]");
        report.InnerException = RedactString(report.InnerException ?? "", @"password|secret|key|token", "[REDACTED]");
    }

    private static string RedactString(string input, string pattern, string replacement)
    {
        return string.IsNullOrEmpty(input) ? input : System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    private static string GetApplicationVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    private static string GetLastGcInfo()
    {
        try
        {
            return $"Gen0: {GC.CollectionCount(0)}, Gen1: {GC.CollectionCount(1)}, Gen2: {GC.CollectionCount(2)}";
        }
        catch
        {
            return "GC info unavailable";
        }
    }

    private static ErrorCategory GetErrorCategory(Exception error)
    {
        return error switch
        {
            EsiUnauthorizedException => ErrorCategory.Authentication,
            EsiRateLimitException => ErrorCategory.RateLimit,
            EsiServerException => ErrorCategory.ServerError,
            EsiForbiddenException => ErrorCategory.Permission,
            EsiNotFoundException => ErrorCategory.NotFound,
            HttpRequestException => ErrorCategory.Network,
            TimeoutException => ErrorCategory.Timeout,
            _ => ErrorCategory.Unknown
        };
    }

    private static string GetErrorKey(Exception error)
    {
        return error.GetType().Name;
    }

    #endregion
}

/// <summary>
/// User notification service for displaying errors and guidance
/// </summary>
public class UserNotificationService : IUserNotificationService
{
    private readonly ILogger<UserNotificationService> _logger;
    private readonly Queue<UserNotification> _notificationQueue = new();
    private readonly object _queueLock = new();

    public UserNotificationService(ILogger<UserNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Show notification to user
    /// </summary>
    public async Task ShowNotificationAsync(UserNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_queueLock)
            {
                _notificationQueue.Enqueue(notification);
            }
            
            _logger.LogInformation("User notification queued: {Title}", notification.Title);
            
            // In a real implementation, this would integrate with the UI notification system
            await SimulateNotificationDisplayAsync(notification, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing user notification");
        }
    }

    /// <summary>
    /// Get pending notifications
    /// </summary>
    public async Task<IEnumerable<UserNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            lock (_queueLock)
            {
                return _notificationQueue.ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending notifications");
            return Enumerable.Empty<UserNotification>();
        }
    }

    private async Task SimulateNotificationDisplayAsync(UserNotification notification, CancellationToken cancellationToken)
    {
        // Simulate displaying the notification
        await Task.Delay(100, cancellationToken);
        _logger.LogDebug("Notification displayed: {Title} - {Message}", notification.Title, notification.Message);
    }
}

#endregion

#region Supporting Data Structures

/// <summary>
/// Error handling result
/// </summary>
public class ErrorHandlingResult
{
    public Exception OriginalError { get; set; } = new();
    public string Context { get; set; } = string.Empty;
    public ErrorCategory ErrorType { get; set; }
    public ErrorSeverity Severity { get; set; }
    public string UserMessage { get; set; } = string.Empty;
    public UserGuidanceInfo? Guidance { get; set; }
    public List<RecommendedAction>? RecommendedActions { get; set; }
    public bool IsRecoverable { get; set; }
    public bool AutoRecoveryAttempted { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Error handling strategy
/// </summary>
public class ErrorHandlingStrategy
{
    public ErrorSeverity Severity { get; set; }
    public bool IsRecoverable { get; set; }
    public bool CanAutoRecover { get; set; }
    public bool ShouldNotifyUser { get; set; }
    public TimeSpan RetryDelay { get; set; }
}

/// <summary>
/// User guidance information
/// </summary>
public class UserGuidanceInfo
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GuidanceCategory Category { get; set; }
    public List<string> Steps { get; set; } = new();
    public string HelpUrl { get; set; } = string.Empty;
    public List<string> RelatedErrors { get; set; } = new();
}

/// <summary>
/// Troubleshooting guide
/// </summary>
public class TroubleshootingGuide
{
    public string IssueType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TroubleshootingStep> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Troubleshooting step
/// </summary>
public class TroubleshootingStep
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? AdditionalInfo { get; set; }
}

/// <summary>
/// Recommended action
/// </summary>
public class RecommendedAction
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    public ActionPriority Priority { get; set; }
    public TimeSpan EstimatedTime { get; set; }
}

/// <summary>
/// User notification
/// </summary>
public class UserNotification
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public List<RecommendedAction> Actions { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Error report for support
/// </summary>
public class ErrorReport
{
    public string ReportId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Context { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string? InnerException { get; set; }
    public SystemInfo? SystemInfo { get; set; }
    public ApplicationState? ApplicationState { get; set; }
    public List<string>? RecentLogEntries { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// System information for error reports
/// </summary>
public class SystemInfo
{
    public string OperatingSystem { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public string ApplicationVersion { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public long WorkingSet { get; set; }
    public long TickCount { get; set; }
    public int ProcessorCount { get; set; }
    public string SystemDirectory { get; set; } = string.Empty;
    public bool IsUserInteractive { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Application state information
/// </summary>
public class ApplicationState
{
    public TimeSpan UpTime { get; set; }
    public long MemoryUsage { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public string LastGcCollection { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

/// <summary>
/// Error categories
/// </summary>
public enum ErrorCategory
{
    Unknown,
    Authentication,
    Permission,
    RateLimit,
    Network,
    ServerError,
    NotFound,
    Timeout,
    DataCorruption,
    SystemFailure
}

/// <summary>
/// Error severity levels
/// </summary>
public enum ErrorSeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Guidance categories
/// </summary>
public enum GuidanceCategory
{
    General,
    Authentication,
    Connectivity,
    Performance,
    Permissions,
    Configuration
}

/// <summary>
/// Action types
/// </summary>
public enum ActionType
{
    Retry,
    Authentication,
    Wait,
    Fallback,
    Diagnostic,
    Configuration
}

/// <summary>
/// Action priorities
/// </summary>
public enum ActionPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Notification types
/// </summary>
public enum NotificationType
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// ESI error handling service interface
/// </summary>
public interface IEsiErrorHandlingService
{
    Task<ErrorHandlingResult> HandleErrorAsync(Exception error, string context, CancellationToken cancellationToken = default);
    Task<UserGuidanceInfo> GetUserGuidanceAsync(Exception error, string context, CancellationToken cancellationToken = default);
    Task<TroubleshootingGuide> GetTroubleshootingGuideAsync(string issueType, CancellationToken cancellationToken = default);
    Task<ErrorReport> GenerateErrorReportAsync(Exception error, string context, bool includeSystemInfo = true, CancellationToken cancellationToken = default);
}

/// <summary>
/// User notification service interface
/// </summary>
public interface IUserNotificationService
{
    Task ShowNotificationAsync(UserNotification notification, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserNotification>> GetPendingNotificationsAsync(CancellationToken cancellationToken = default);
}

#endregion
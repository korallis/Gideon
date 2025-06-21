using Gideon.WPF.Core.Domain.Entities;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Ship fitting service interface for managing fittings and calculations
/// </summary>
public interface IShipFittingService
{
    // Fitting management
    Task<ShipFitting> CreateFittingAsync(int shipId, string name, string? description = null, CancellationToken cancellationToken = default);
    
    Task<ShipFitting> SaveFittingAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<ShipFitting?> GetFittingAsync(Guid fittingId, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ShipFitting>> GetFittingsAsync(int? characterId = null, int? shipId = null, CancellationToken cancellationToken = default);
    
    Task<bool> DeleteFittingAsync(Guid fittingId, CancellationToken cancellationToken = default);
    
    Task<ShipFitting> CloneFittingAsync(Guid sourceFittingId, string newName, CancellationToken cancellationToken = default);
    
    // Module fitting
    Task<bool> FitModuleAsync(Guid fittingId, int moduleId, string slotType, int slotIndex, CancellationToken cancellationToken = default);
    
    Task<bool> UnfitModuleAsync(Guid fittingId, string slotType, int slotIndex, CancellationToken cancellationToken = default);
    
    Task<bool> SwapModulesAsync(Guid fittingId, string fromSlotType, int fromSlotIndex, string toSlotType, int toSlotIndex, CancellationToken cancellationToken = default);
    
    Task<bool> FitDroneAsync(Guid fittingId, int droneId, int quantity, CancellationToken cancellationToken = default);
    
    Task<bool> UnfitDroneAsync(Guid fittingId, int droneId, CancellationToken cancellationToken = default);
    
    // Fitting calculations
    Task<ShipFitting> CalculateFittingStatsAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<double> CalculateDpsAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<double> CalculateVolleyDamageAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double shield, double armor, double hull)> CalculateEhpAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double stability, double timeToEmpty)> CalculateCapacitorAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double maxVelocity, double alignTime, double warpSpeed)> CalculateSpeedAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double maxRange, double scanResolution, double signatureRadius)> CalculateTargetingAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    // Resource validation
    Task<bool> ValidateFittingAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(bool isValid, IEnumerable<string> errors)> ValidateFittingWithErrorsAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double used, double total)> GetPowerGridUsageAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double used, double total)> GetCpuUsageAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<(double used, double total)> GetCalibrationUsageAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    // Import/Export
    Task<ShipFitting> ImportFromEftAsync(string eftFormat, CancellationToken cancellationToken = default);
    
    Task<ShipFitting> ImportFromDnaAsync(string dnaFormat, CancellationToken cancellationToken = default);
    
    Task<string> ExportToEftAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<string> ExportToDnaAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<string> ExportToXmlAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    // Cost analysis
    Task<decimal> CalculateFittingCostAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    Task<Dictionary<int, decimal>> GetModuleCostsAsync(ShipFitting fitting, CancellationToken cancellationToken = default);
    
    // Optimization
    Task<IEnumerable<ShipFitting>> OptimizeFittingAsync(ShipFitting baseFitting, string optimizationGoal, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Module>> SuggestModulesAsync(int shipId, string slotType, string role, CancellationToken cancellationToken = default);
    
    // Comparison
    Task<Dictionary<string, object>> CompareFittingsAsync(IEnumerable<ShipFitting> fittings, CancellationToken cancellationToken = default);
    
    // Templates and presets
    Task<IEnumerable<ShipFitting>> GetFittingTemplatesAsync(int shipId, string? role = null, CancellationToken cancellationToken = default);
    
    Task<ShipFitting> SaveAsFittingTemplateAsync(ShipFitting fitting, bool isPublic = false, CancellationToken cancellationToken = default);
}
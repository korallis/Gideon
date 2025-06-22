// ==========================================================================
// SkillCalculationService.cs - Comprehensive EVE Online Skill Calculation Service
// ==========================================================================
// Implementation of accurate EVE Online skill bonuses and calculations matching
// server-side formulas for all character skills affecting ship fitting.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;

namespace Gideon.WPF.Core.Application.Services;

/// <summary>
/// Service for accurate EVE Online skill calculations and bonuses
/// </summary>
public class SkillCalculationService : ISkillCalculationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SkillCalculationService> _logger;
    
    // EVE skill constants for accurate calculations
    private const double SkillLevelMultiplier = 1.0;
    private const int MaxSkillLevel = 5;
    private const int MaxImplantBonus = 5; // +5 implants
    
    public SkillCalculationService(IUnitOfWork unitOfWork, ILogger<SkillCalculationService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Calculate comprehensive skill bonuses for a character
    /// </summary>
    public async Task<SkillBonusResult> CalculateComprehensiveSkillBonusesAsync(
        Character? character,
        SkillCategory category = SkillCategory.All,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Starting comprehensive skill bonus calculation for character {CharacterName}", character?.Name ?? "Unknown");

            var result = new SkillBonusResult();
            
            if (character == null)
            {
                _logger.LogDebug("No character provided, using base values with no skill bonuses");
                return GetBaseSkillBonuses();
            }

            // Calculate different skill categories based on request
            if (category == SkillCategory.All || category == SkillCategory.Gunnery)
            {
                result.GunneryBonuses = await CalculateGunnerySkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Missiles)
            {
                result.MissileBonuses = await CalculateMissileSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Drones)
            {
                result.DroneBonuses = await CalculateDroneSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Engineering)
            {
                result.EngineeringBonuses = await CalculateEngineeringSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Electronics)
            {
                result.ElectronicsBonuses = await CalculateElectronicsSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Navigation)
            {
                result.NavigationBonuses = await CalculateNavigationSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Armor)
            {
                result.ArmorBonuses = await CalculateArmorSkillBonusesAsync(character, cancellationToken);
            }
            
            if (category == SkillCategory.All || category == SkillCategory.Shields)
            {
                result.ShieldBonuses = await CalculateShieldSkillBonusesAsync(character, cancellationToken);
            }

            // Apply implant bonuses
            result = await ApplyImplantBonusesAsync(result, character, cancellationToken);

            var calculationTime = DateTime.UtcNow - startTime;
            result.CalculationTime = calculationTime;
            
            _logger.LogInformation("Skill bonus calculation completed in {Ms}ms for character {CharacterName}", 
                calculationTime.TotalMilliseconds, character.Name);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating skill bonuses for character {CharacterName}", character?.Name ?? "Unknown");
            throw;
        }
    }

    /// <summary>
    /// Calculate gunnery skill bonuses (turret weapons)
    /// </summary>
    private async Task<GunnerySkillBonuses> CalculateGunnerySkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new GunnerySkillBonuses();

        // Core gunnery skills
        var gunneryLevel = await GetSkillLevelAsync(character, SkillId.Gunnery, cancellationToken);
        var motionPredictionLevel = await GetSkillLevelAsync(character, SkillId.MotionPrediction, cancellationToken);
        var sharpshooterLevel = await GetSkillLevelAsync(character, SkillId.Sharpshooter, cancellationToken);
        var surgicalStrikeLevel = await GetSkillLevelAsync(character, SkillId.SurgicalStrike, cancellationToken);
        var rapidFiringLevel = await GetSkillLevelAsync(character, SkillId.RapidFiring, cancellationToken);
        var controlledBurstsLevel = await GetSkillLevelAsync(character, SkillId.ControlledBursts, cancellationToken);

        // Weapon specialization skills
        var smallEnergyTurretLevel = await GetSkillLevelAsync(character, SkillId.SmallEnergyTurret, cancellationToken);
        var mediumEnergyTurretLevel = await GetSkillLevelAsync(character, SkillId.MediumEnergyTurret, cancellationToken);
        var largeEnergyTurretLevel = await GetSkillLevelAsync(character, SkillId.LargeEnergyTurret, cancellationToken);
        var smallHybridTurretLevel = await GetSkillLevelAsync(character, SkillId.SmallHybridTurret, cancellationToken);
        var mediumHybridTurretLevel = await GetSkillLevelAsync(character, SkillId.MediumHybridTurret, cancellationToken);
        var largeHybridTurretLevel = await GetSkillLevelAsync(character, SkillId.LargeHybridTurret, cancellationToken);
        var smallProjectileTurretLevel = await GetSkillLevelAsync(character, SkillId.SmallProjectileTurret, cancellationToken);
        var mediumProjectileTurretLevel = await GetSkillLevelAsync(character, SkillId.MediumProjectileTurret, cancellationToken);
        var largeProjectileTurretLevel = await GetSkillLevelAsync(character, SkillId.LargeProjectileTurret, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.DamageMultiplier = 1.0 + (surgicalStrikeLevel * 0.03); // 3% per level
        bonuses.TrackingSpeedMultiplier = 1.0 + (motionPredictionLevel * 0.05); // 5% per level
        bonuses.OptimalRangeMultiplier = 1.0 + (sharpshooterLevel * 0.05); // 5% per level
        bonuses.RateOfFireMultiplier = 1.0 - (rapidFiringLevel * 0.04); // 4% reduction per level (faster = lower cycle time)
        bonuses.CapacitorUsageMultiplier = 1.0 - (controlledBurstsLevel * 0.05); // 5% reduction per level

        // Weapon size bonuses
        bonuses.SmallTurretBonuses = new WeaponSizeBonuses
        {
            EnergyWeaponBonus = 1.0 + (smallEnergyTurretLevel * 0.05),
            HybridWeaponBonus = 1.0 + (smallHybridTurretLevel * 0.05),
            ProjectileWeaponBonus = 1.0 + (smallProjectileTurretLevel * 0.05)
        };

        bonuses.MediumTurretBonuses = new WeaponSizeBonuses
        {
            EnergyWeaponBonus = 1.0 + (mediumEnergyTurretLevel * 0.05),
            HybridWeaponBonus = 1.0 + (mediumHybridTurretLevel * 0.05),
            ProjectileWeaponBonus = 1.0 + (mediumProjectileTurretLevel * 0.05)
        };

        bonuses.LargeTurretBonuses = new WeaponSizeBonuses
        {
            EnergyWeaponBonus = 1.0 + (largeEnergyTurretLevel * 0.05),
            HybridWeaponBonus = 1.0 + (largeHybridTurretLevel * 0.05),
            ProjectileWeaponBonus = 1.0 + (largeProjectileTurretLevel * 0.05)
        };

        _logger.LogDebug("Calculated gunnery bonuses: Damage={DamageMultiplier:F3}, Tracking={TrackingMultiplier:F3}, Range={RangeMultiplier:F3}",
            bonuses.DamageMultiplier, bonuses.TrackingSpeedMultiplier, bonuses.OptimalRangeMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate missile skill bonuses
    /// </summary>
    private async Task<MissileSkillBonuses> CalculateMissileSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new MissileSkillBonuses();

        // Core missile skills
        var missilesLevel = await GetSkillLevelAsync(character, SkillId.Missiles, cancellationToken);
        var missileLauncherOperationLevel = await GetSkillLevelAsync(character, SkillId.MissileLauncherOperation, cancellationToken);
        var missileBombardmentLevel = await GetSkillLevelAsync(character, SkillId.MissileBombardment, cancellationToken);
        var missileProjectionLevel = await GetSkillLevelAsync(character, SkillId.MissileProjection, cancellationToken);
        var rapidLaunchLevel = await GetSkillLevelAsync(character, SkillId.RapidLaunch, cancellationToken);
        var targetNavigationPredictionLevel = await GetSkillLevelAsync(character, SkillId.TargetNavigationPrediction, cancellationToken);
        var warheadUpgradesLevel = await GetSkillLevelAsync(character, SkillId.WarheadUpgrades, cancellationToken);

        // Launcher specialization skills
        var lightMissilesLevel = await GetSkillLevelAsync(character, SkillId.LightMissiles, cancellationToken);
        var heavyMissilesLevel = await GetSkillLevelAsync(character, SkillId.HeavyMissiles, cancellationToken);
        var cruiseMissilesLevel = await GetSkillLevelAsync(character, SkillId.CruiseMissiles, cancellationToken);
        var torpedoesLevel = await GetSkillLevelAsync(character, SkillId.Torpedoes, cancellationToken);
        var rocketsLevel = await GetSkillLevelAsync(character, SkillId.Rockets, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.DamageMultiplier = 1.0 + (warheadUpgradesLevel * 0.02); // 2% per level
        bonuses.ExplosionRadiusMultiplier = 1.0 - (missileBombardmentLevel * 0.05); // 5% reduction per level
        bonuses.ExplosionVelocityMultiplier = 1.0 + (targetNavigationPredictionLevel * 0.10); // 10% per level
        bonuses.FlightTimeMultiplier = 1.0 + (missileProjectionLevel * 0.10); // 10% per level
        bonuses.RateOfFireMultiplier = 1.0 - (rapidLaunchLevel * 0.03); // 3% reduction per level

        // Missile type bonuses
        bonuses.LightMissileBonus = 1.0 + (lightMissilesLevel * 0.05); // 5% per level
        bonuses.HeavyMissileBonus = 1.0 + (heavyMissilesLevel * 0.05);
        bonuses.CruiseMissileBonus = 1.0 + (cruiseMissilesLevel * 0.05);
        bonuses.TorpedoBonus = 1.0 + (torpedoesLevel * 0.05);
        bonuses.RocketBonus = 1.0 + (rocketsLevel * 0.05);

        _logger.LogDebug("Calculated missile bonuses: Damage={DamageMultiplier:F3}, ExplosionRadius={ExplosionRadiusMultiplier:F3}",
            bonuses.DamageMultiplier, bonuses.ExplosionRadiusMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate drone skill bonuses
    /// </summary>
    private async Task<DroneSkillBonuses> CalculateDroneSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new DroneSkillBonuses();

        // Core drone skills
        var dronesLevel = await GetSkillLevelAsync(character, SkillId.Drones, cancellationToken);
        var droneInterfacingLevel = await GetSkillLevelAsync(character, SkillId.DroneInterfacing, cancellationToken);
        var combatDroneOperationLevel = await GetSkillLevelAsync(character, SkillId.CombatDroneOperation, cancellationToken);
        var droneSharpshootingLevel = await GetSkillLevelAsync(character, SkillId.DroneSharpshootingLevel, cancellationToken);
        var droneNavigationLevel = await GetSkillLevelAsync(character, SkillId.DroneNavigation, cancellationToken);
        var droneDurabilityLevel = await GetSkillLevelAsync(character, SkillId.DroneDurability, cancellationToken);

        // Drone specialization skills
        var lightDroneOperationLevel = await GetSkillLevelAsync(character, SkillId.LightDroneOperation, cancellationToken);
        var mediumDroneOperationLevel = await GetSkillLevelAsync(character, SkillId.MediumDroneOperation, cancellationToken);
        var heavyDroneOperationLevel = await GetSkillLevelAsync(character, SkillId.HeavyDroneOperation, cancellationToken);
        var sentryDroneInterfacingLevel = await GetSkillLevelAsync(character, SkillId.SentryDroneInterfacing, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.DamageMultiplier = 1.0 + (droneInterfacingLevel * 0.20); // 20% per level
        bonuses.HitPointsMultiplier = 1.0 + (droneDurabilityLevel * 0.05); // 5% per level
        bonuses.MiningYieldMultiplier = 1.0 + (droneInterfacingLevel * 0.15); // 15% per level for mining drones
        bonuses.VelocityMultiplier = 1.0 + (droneNavigationLevel * 0.05); // 5% per level
        bonuses.TrackingSpeedMultiplier = 1.0 + (droneSharpshootingLevel * 0.05); // 5% per level
        bonuses.OptimalRangeMultiplier = 1.0 + (droneSharpshootingLevel * 0.05); // 5% per level

        // Max drones in space
        bonuses.MaxDronesInSpace = Math.Min(5, 1 + dronesLevel); // 1 drone per level, max 5

        // Drone size bonuses
        bonuses.LightDroneBonus = 1.0 + (lightDroneOperationLevel * 0.05);
        bonuses.MediumDroneBonus = 1.0 + (mediumDroneOperationLevel * 0.05);
        bonuses.HeavyDroneBonus = 1.0 + (heavyDroneOperationLevel * 0.05);
        bonuses.SentryDroneBonus = 1.0 + (sentryDroneInterfacingLevel * 0.05);

        _logger.LogDebug("Calculated drone bonuses: Damage={DamageMultiplier:F3}, MaxDrones={MaxDrones}",
            bonuses.DamageMultiplier, bonuses.MaxDronesInSpace);

        return bonuses;
    }

    /// <summary>
    /// Calculate engineering skill bonuses (CPU, PowerGrid, Capacitor)
    /// </summary>
    private async Task<EngineeringSkillBonuses> CalculateEngineeringSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new EngineeringSkillBonuses();

        // Core engineering skills
        var engineeringLevel = await GetSkillLevelAsync(character, SkillId.Engineering, cancellationToken);
        var electronicsLevel = await GetSkillLevelAsync(character, SkillId.Electronics, cancellationToken);
        var energyManagementLevel = await GetSkillLevelAsync(character, SkillId.EnergyManagement, cancellationToken);
        var energySystemsOperationLevel = await GetSkillLevelAsync(character, SkillId.EnergySystemsOperation, cancellationToken);
        var capacitorManagementLevel = await GetSkillLevelAsync(character, SkillId.CapacitorManagement, cancellationToken);
        var capacitorSystemsOperationLevel = await GetSkillLevelAsync(character, SkillId.CapacitorSystemsOperation, cancellationToken);
        var powerGridManagementLevel = await GetSkillLevelAsync(character, SkillId.PowerGridManagement, cancellationToken);
        var cpuManagementLevel = await GetSkillLevelAsync(character, SkillId.CpuManagement, cancellationToken);

        // Advanced engineering skills
        var advancedEnergyManagementLevel = await GetSkillLevelAsync(character, SkillId.AdvancedEnergyManagement, cancellationToken);
        var thermodynamicsLevel = await GetSkillLevelAsync(character, SkillId.Thermodynamics, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.PowerGridMultiplier = 1.0 + (powerGridManagementLevel * 0.05); // 5% per level
        bonuses.CpuMultiplier = 1.0 + (cpuManagementLevel * 0.05); // 5% per level
        bonuses.CapacitorCapacityMultiplier = 1.0 + (capacitorManagementLevel * 0.05); // 5% per level
        bonuses.CapacitorRechargeTimeMultiplier = 1.0 - (capacitorSystemsOperationLevel * 0.05); // 5% reduction per level
        bonuses.EnergyTransferEfficiencyMultiplier = 1.0 + (energyManagementLevel * 0.05); // 5% per level

        // Advanced bonuses
        bonuses.OverheatDamageReductionMultiplier = 1.0 - (thermodynamicsLevel * 0.05); // 5% damage reduction per level
        bonuses.CapacitorBoosterEfficiencyMultiplier = 1.0 + (energySystemsOperationLevel * 0.05); // 5% per level

        _logger.LogDebug("Calculated engineering bonuses: PowerGrid={PowerGridMultiplier:F3}, CPU={CpuMultiplier:F3}, Capacitor={CapacitorMultiplier:F3}",
            bonuses.PowerGridMultiplier, bonuses.CpuMultiplier, bonuses.CapacitorCapacityMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate electronics skill bonuses (Targeting, EWAR, Scanning)
    /// </summary>
    private async Task<ElectronicsSkillBonuses> CalculateElectronicsSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new ElectronicsSkillBonuses();

        // Core electronics skills
        var electronicsLevel = await GetSkillLevelAsync(character, SkillId.Electronics, cancellationToken);
        var targetingLevel = await GetSkillLevelAsync(character, SkillId.Targeting, cancellationToken);
        var multitaskingLevel = await GetSkillLevelAsync(character, SkillId.Multitasking, cancellationToken);
        var signatureAnalysisLevel = await GetSkillLevelAsync(character, SkillId.SignatureAnalysis, cancellationToken);
        var longRangeTargetingLevel = await GetSkillLevelAsync(character, SkillId.LongRangeTargeting, cancellationToken);

        // Electronic warfare skills
        var electronicWarfareLevel = await GetSkillLevelAsync(character, SkillId.ElectronicWarfare, cancellationToken);
        var propulsionJammingLevel = await GetSkillLevelAsync(character, SkillId.PropulsionJamming, cancellationToken);
        var weaponDisruptionLevel = await GetSkillLevelAsync(character, SkillId.WeaponDisruption, cancellationToken);
        var sensorLinkingLevel = await GetSkillLevelAsync(character, SkillId.SensorLinking, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.MaxTargetsBonus = Math.Min(5, targetingLevel); // 1 additional target per level, max +5
        bonuses.TargetRangeMultiplier = 1.0 + (longRangeTargetingLevel * 0.05); // 5% per level
        bonuses.ScanResolutionMultiplier = 1.0 + (signatureAnalysisLevel * 0.05); // 5% per level
        bonuses.SensorStrengthMultiplier = 1.0 + (sensorLinkingLevel * 0.05); // 5% per level

        // EWAR bonuses
        bonuses.EcmStrengthMultiplier = 1.0 + (electronicWarfareLevel * 0.05); // 5% per level
        bonuses.WebifierStrengthMultiplier = 1.0 + (propulsionJammingLevel * 0.05); // 5% per level
        bonuses.TrackingDisruptorStrengthMultiplier = 1.0 + (weaponDisruptionLevel * 0.05); // 5% per level

        _logger.LogDebug("Calculated electronics bonuses: MaxTargets={MaxTargetsBonus}, Range={TargetRangeMultiplier:F3}, ScanRes={ScanResolutionMultiplier:F3}",
            bonuses.MaxTargetsBonus, bonuses.TargetRangeMultiplier, bonuses.ScanResolutionMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate navigation skill bonuses (Speed, Agility, Warp)
    /// </summary>
    private async Task<NavigationSkillBonuses> CalculateNavigationSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new NavigationSkillBonuses();

        // Core navigation skills
        var navigationLevel = await GetSkillLevelAsync(character, SkillId.Navigation, cancellationToken);
        var spacecraftCommandLevel = await GetSkillLevelAsync(character, SkillId.SpacecraftCommand, cancellationToken);
        var evasiveManeuveringLevel = await GetSkillLevelAsync(character, SkillId.EvasiveManeuvering, cancellationToken);
        var warpDriveOperationLevel = await GetSkillLevelAsync(character, SkillId.WarpDriveOperation, cancellationToken);
        var accelerationControlLevel = await GetSkillLevelAsync(character, SkillId.AccelerationControl, cancellationToken);
        var highSpeedManeuveringLevel = await GetSkillLevelAsync(character, SkillId.HighSpeedManeuvering, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.VelocityMultiplier = 1.0 + (navigationLevel * 0.05); // 5% per level
        bonuses.AgilityMultiplier = 1.0 - (evasiveManeuveringLevel * 0.05); // 5% reduction per level (lower is better)
        bonuses.WarpSpeedMultiplier = 1.0 + (warpDriveOperationLevel * 0.10); // 10% per level
        bonuses.WarpCapacitorNeedMultiplier = 1.0 - (warpDriveOperationLevel * 0.10); // 10% reduction per level
        bonuses.AccelerationMultiplier = 1.0 + (accelerationControlLevel * 0.05); // 5% per level
        bonuses.MicrowarpdriveDurationMultiplier = 1.0 + (highSpeedManeuveringLevel * 0.05); // 5% per level

        _logger.LogDebug("Calculated navigation bonuses: Velocity={VelocityMultiplier:F3}, Agility={AgilityMultiplier:F3}, WarpSpeed={WarpSpeedMultiplier:F3}",
            bonuses.VelocityMultiplier, bonuses.AgilityMultiplier, bonuses.WarpSpeedMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate armor skill bonuses
    /// </summary>
    private async Task<ArmorSkillBonuses> CalculateArmorSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new ArmorSkillBonuses();

        // Core armor skills
        var mechanicsLevel = await GetSkillLevelAsync(character, SkillId.Mechanics, cancellationToken);
        var repairSystemsLevel = await GetSkillLevelAsync(character, SkillId.RepairSystems, cancellationToken);
        var hullUpgradesLevel = await GetSkillLevelAsync(character, SkillId.HullUpgrades, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.HullHitPointsMultiplier = 1.0 + (mechanicsLevel * 0.05); // 5% per level
        bonuses.ArmorHitPointsMultiplier = 1.0 + (hullUpgradesLevel * 0.05); // 5% per level
        bonuses.ArmorRepairAmountMultiplier = 1.0 + (repairSystemsLevel * 0.05); // 5% per level

        _logger.LogDebug("Calculated armor bonuses: Hull={HullMultiplier:F3}, Armor={ArmorMultiplier:F3}, Repair={RepairMultiplier:F3}",
            bonuses.HullHitPointsMultiplier, bonuses.ArmorHitPointsMultiplier, bonuses.ArmorRepairAmountMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Calculate shield skill bonuses
    /// </summary>
    private async Task<ShieldSkillBonuses> CalculateShieldSkillBonusesAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        var bonuses = new ShieldSkillBonuses();

        // Core shield skills
        var shieldsLevel = await GetSkillLevelAsync(character, SkillId.Shields, cancellationToken);
        var shieldManagementLevel = await GetSkillLevelAsync(character, SkillId.ShieldManagement, cancellationToken);
        var shieldOperationLevel = await GetSkillLevelAsync(character, SkillId.ShieldOperation, cancellationToken);
        var shieldUpgradesLevel = await GetSkillLevelAsync(character, SkillId.ShieldUpgrades, cancellationToken);
        var tacticalShieldManipulationLevel = await GetSkillLevelAsync(character, SkillId.TacticalShieldManipulation, cancellationToken);

        // Calculate bonuses using EVE formulas
        bonuses.ShieldCapacityMultiplier = 1.0 + (shieldManagementLevel * 0.05); // 5% per level
        bonuses.ShieldRechargeTimeMultiplier = 1.0 - (shieldOperationLevel * 0.05); // 5% reduction per level
        bonuses.ShieldBoostAmountMultiplier = 1.0 + (shieldUpgradesLevel * 0.05); // 5% per level
        bonuses.PassiveShieldRegenMultiplier = 1.0 + (shieldsLevel * 0.10); // 10% per level

        _logger.LogDebug("Calculated shield bonuses: Capacity={CapacityMultiplier:F3}, Recharge={RechargeMultiplier:F3}, Boost={BoostMultiplier:F3}",
            bonuses.ShieldCapacityMultiplier, bonuses.ShieldRechargeTimeMultiplier, bonuses.ShieldBoostAmountMultiplier);

        return bonuses;
    }

    /// <summary>
    /// Apply implant bonuses to calculated skill bonuses
    /// </summary>
    private async Task<SkillBonusResult> ApplyImplantBonusesAsync(
        SkillBonusResult skillBonuses,
        Character character,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get character's active implants
            var implants = await GetCharacterImplantsAsync(character, cancellationToken);
            
            foreach (var implant in implants)
            {
                await ApplyImplantToSkillBonusesAsync(skillBonuses, implant, cancellationToken);
            }

            _logger.LogDebug("Applied {ImplantCount} implant bonuses to skill calculations", implants.Count);
            
            return skillBonuses;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply implant bonuses, using base skill bonuses only");
            return skillBonuses;
        }
    }

    /// <summary>
    /// Get character skill level by skill ID
    /// </summary>
    private async Task<int> GetSkillLevelAsync(
        Character character, 
        int skillId, 
        CancellationToken cancellationToken)
    {
        try
        {
            // In a real implementation, this would query the character's skills from the database
            // For now, we'll simulate skill levels based on character data
            
            if (character.Skills?.Any() == true)
            {
                var skill = character.Skills.FirstOrDefault(s => s.SkillId == skillId);
                return skill?.TrainedSkillLevel ?? 0;
            }
            
            // Default to level 0 if skill not found
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get skill level for skill {SkillId}, defaulting to 0", skillId);
            return 0;
        }
    }

    /// <summary>
    /// Get base skill bonuses with no character (all 1.0 multipliers)
    /// </summary>
    private SkillBonusResult GetBaseSkillBonuses()
    {
        return new SkillBonusResult
        {
            GunneryBonuses = new GunnerySkillBonuses(),
            MissileBonuses = new MissileSkillBonuses(),
            DroneBonuses = new DroneSkillBonuses(),
            EngineeringBonuses = new EngineeringSkillBonuses(),
            ElectronicsBonuses = new ElectronicsSkillBonuses(),
            NavigationBonuses = new NavigationSkillBonuses(),
            ArmorBonuses = new ArmorSkillBonuses(),
            ShieldBonuses = new ShieldSkillBonuses(),
            CalculationTime = TimeSpan.Zero
        };
    }

    /// <summary>
    /// Get character's active implants
    /// </summary>
    private async Task<List<CharacterImplant>> GetCharacterImplantsAsync(
        Character character, 
        CancellationToken cancellationToken)
    {
        // In a real implementation, this would query the character's implants from the database
        // For now, return empty list
        return new List<CharacterImplant>();
    }

    /// <summary>
    /// Apply single implant bonus to skill bonuses
    /// </summary>
    private async Task ApplyImplantToSkillBonusesAsync(
        SkillBonusResult skillBonuses,
        CharacterImplant implant,
        CancellationToken cancellationToken)
    {
        // Implementation would apply specific implant bonuses based on implant type
        // This is a placeholder for the complex implant bonus system
    }
}

/// <summary>
/// Interface for skill calculation service
/// </summary>
public interface ISkillCalculationService
{
    Task<SkillBonusResult> CalculateComprehensiveSkillBonusesAsync(Character? character, SkillCategory category = SkillCategory.All, CancellationToken cancellationToken = default);
}

/// <summary>
/// Skill categories for focused calculations
/// </summary>
public enum SkillCategory
{
    All,
    Gunnery,
    Missiles,
    Drones,
    Engineering,
    Electronics,
    Navigation,
    Armor,
    Shields
}

/// <summary>
/// Comprehensive skill bonus calculation result
/// </summary>
public class SkillBonusResult
{
    public GunnerySkillBonuses GunneryBonuses { get; set; } = new();
    public MissileSkillBonuses MissileBonuses { get; set; } = new();
    public DroneSkillBonuses DroneBonuses { get; set; } = new();
    public EngineeringSkillBonuses EngineeringBonuses { get; set; } = new();
    public ElectronicsSkillBonuses ElectronicsBonuses { get; set; } = new();
    public NavigationSkillBonuses NavigationBonuses { get; set; } = new();
    public ArmorSkillBonuses ArmorBonuses { get; set; } = new();
    public ShieldSkillBonuses ShieldBonuses { get; set; } = new();
    public TimeSpan CalculationTime { get; set; }
}

/// <summary>
/// Gunnery skill bonuses
/// </summary>
public class GunnerySkillBonuses
{
    public double DamageMultiplier { get; set; } = 1.0;
    public double TrackingSpeedMultiplier { get; set; } = 1.0;
    public double OptimalRangeMultiplier { get; set; } = 1.0;
    public double FalloffRangeMultiplier { get; set; } = 1.0;
    public double RateOfFireMultiplier { get; set; } = 1.0;
    public double CapacitorUsageMultiplier { get; set; } = 1.0;
    
    public WeaponSizeBonuses SmallTurretBonuses { get; set; } = new();
    public WeaponSizeBonuses MediumTurretBonuses { get; set; } = new();
    public WeaponSizeBonuses LargeTurretBonuses { get; set; } = new();
}

/// <summary>
/// Weapon size-specific bonuses
/// </summary>
public class WeaponSizeBonuses
{
    public double EnergyWeaponBonus { get; set; } = 1.0;
    public double HybridWeaponBonus { get; set; } = 1.0;
    public double ProjectileWeaponBonus { get; set; } = 1.0;
}

/// <summary>
/// Missile skill bonuses
/// </summary>
public class MissileSkillBonuses
{
    public double DamageMultiplier { get; set; } = 1.0;
    public double ExplosionRadiusMultiplier { get; set; } = 1.0;
    public double ExplosionVelocityMultiplier { get; set; } = 1.0;
    public double FlightTimeMultiplier { get; set; } = 1.0;
    public double RateOfFireMultiplier { get; set; } = 1.0;
    
    public double LightMissileBonus { get; set; } = 1.0;
    public double HeavyMissileBonus { get; set; } = 1.0;
    public double CruiseMissileBonus { get; set; } = 1.0;
    public double TorpedoBonus { get; set; } = 1.0;
    public double RocketBonus { get; set; } = 1.0;
}

/// <summary>
/// Drone skill bonuses
/// </summary>
public class DroneSkillBonuses
{
    public double DamageMultiplier { get; set; } = 1.0;
    public double HitPointsMultiplier { get; set; } = 1.0;
    public double MiningYieldMultiplier { get; set; } = 1.0;
    public double VelocityMultiplier { get; set; } = 1.0;
    public double TrackingSpeedMultiplier { get; set; } = 1.0;
    public double OptimalRangeMultiplier { get; set; } = 1.0;
    public int MaxDronesInSpace { get; set; } = 1;
    
    public double LightDroneBonus { get; set; } = 1.0;
    public double MediumDroneBonus { get; set; } = 1.0;
    public double HeavyDroneBonus { get; set; } = 1.0;
    public double SentryDroneBonus { get; set; } = 1.0;
}

/// <summary>
/// Engineering skill bonuses
/// </summary>
public class EngineeringSkillBonuses
{
    public double PowerGridMultiplier { get; set; } = 1.0;
    public double CpuMultiplier { get; set; } = 1.0;
    public double CapacitorCapacityMultiplier { get; set; } = 1.0;
    public double CapacitorRechargeTimeMultiplier { get; set; } = 1.0;
    public double EnergyTransferEfficiencyMultiplier { get; set; } = 1.0;
    public double OverheatDamageReductionMultiplier { get; set; } = 1.0;
    public double CapacitorBoosterEfficiencyMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Electronics skill bonuses
/// </summary>
public class ElectronicsSkillBonuses
{
    public int MaxTargetsBonus { get; set; } = 0;
    public double TargetRangeMultiplier { get; set; } = 1.0;
    public double ScanResolutionMultiplier { get; set; } = 1.0;
    public double SensorStrengthMultiplier { get; set; } = 1.0;
    public double EcmStrengthMultiplier { get; set; } = 1.0;
    public double WebifierStrengthMultiplier { get; set; } = 1.0;
    public double TrackingDisruptorStrengthMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Navigation skill bonuses
/// </summary>
public class NavigationSkillBonuses
{
    public double VelocityMultiplier { get; set; } = 1.0;
    public double AgilityMultiplier { get; set; } = 1.0;
    public double WarpSpeedMultiplier { get; set; } = 1.0;
    public double WarpCapacitorNeedMultiplier { get; set; } = 1.0;
    public double AccelerationMultiplier { get; set; } = 1.0;
    public double MicrowarpdriveDurationMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Armor skill bonuses
/// </summary>
public class ArmorSkillBonuses
{
    public double HullHitPointsMultiplier { get; set; } = 1.0;
    public double ArmorHitPointsMultiplier { get; set; } = 1.0;
    public double ArmorRepairAmountMultiplier { get; set; } = 1.0;
}

/// <summary>
/// Shield skill bonuses
/// </summary>
public class ShieldSkillBonuses
{
    public double ShieldCapacityMultiplier { get; set; } = 1.0;
    public double ShieldRechargeTimeMultiplier { get; set; } = 1.0;
    public double ShieldBoostAmountMultiplier { get; set; } = 1.0;
    public double PassiveShieldRegenMultiplier { get; set; } = 1.0;
}

/// <summary>
/// EVE Online skill IDs for accurate lookups
/// </summary>
public static class SkillId
{
    // Gunnery Skills
    public const int Gunnery = 3300;
    public const int MotionPrediction = 3301;
    public const int Sharpshooter = 3302;
    public const int SurgicalStrike = 3312;
    public const int RapidFiring = 3305;
    public const int ControlledBursts = 3304;
    public const int SmallEnergyTurret = 3303;
    public const int MediumEnergyTurret = 3306;
    public const int LargeEnergyTurret = 3307;
    public const int SmallHybridTurret = 3308;
    public const int MediumHybridTurret = 3309;
    public const int LargeHybridTurret = 3310;
    public const int SmallProjectileTurret = 3311;
    public const int MediumProjectileTurret = 3315;
    public const int LargeProjectileTurret = 3316;

    // Missile Skills
    public const int Missiles = 3319;
    public const int MissileLauncherOperation = 3320;
    public const int MissileBombardment = 3321;
    public const int MissileProjection = 3322;
    public const int RapidLaunch = 3324;
    public const int TargetNavigationPrediction = 3325;
    public const int WarheadUpgrades = 3326;
    public const int LightMissiles = 3327;
    public const int HeavyMissiles = 3328;
    public const int CruiseMissiles = 3329;
    public const int Torpedoes = 3330;
    public const int Rockets = 12441;

    // Drone Skills
    public const int Drones = 3436;
    public const int DroneInterfacing = 3442;
    public const int CombatDroneOperation = 3440;
    public const int DroneSharpshootingLevel = 23606;
    public const int DroneNavigation = 12305;
    public const int DroneDurability = 3439;
    public const int LightDroneOperation = 3441;
    public const int MediumDroneOperation = 3444;
    public const int HeavyDroneOperation = 3445;
    public const int SentryDroneInterfacing = 23594;

    // Engineering Skills
    public const int Engineering = 3413;
    public const int Electronics = 3426;
    public const int EnergyManagement = 3429;
    public const int EnergySystemsOperation = 3431;
    public const int CapacitorManagement = 3423;
    public const int CapacitorSystemsOperation = 3432;
    public const int PowerGridManagement = 3416;
    public const int CpuManagement = 3424;
    public const int AdvancedEnergyManagement = 11207;
    public const int Thermodynamics = 28164;

    // Electronics Skills (Targeting/EWAR)
    public const int Targeting = 3428;
    public const int Multitasking = 3449;
    public const int SignatureAnalysis = 3427;
    public const int LongRangeTargeting = 19719;
    public const int ElectronicWarfare = 3434;
    public const int PropulsionJamming = 3435;
    public const int WeaponDisruption = 3433;
    public const int SensorLinking = 19760;

    // Navigation Skills
    public const int Navigation = 3449;
    public const int SpacecraftCommand = 3327;
    public const int EvasiveManeuvering = 3452;
    public const int WarpDriveOperation = 3456;
    public const int AccelerationControl = 3453;
    public const int HighSpeedManeuvering = 3465;

    // Armor Skills
    public const int Mechanics = 3392;
    public const int RepairSystems = 3393;
    public const int HullUpgrades = 3394;

    // Shield Skills
    public const int Shields = 3416;
    public const int ShieldManagement = 3422;
    public const int ShieldOperation = 3417;
    public const int ShieldUpgrades = 3419;
    public const int TacticalShieldManipulation = 3420;
}

/// <summary>
/// Character implant data
/// </summary>
public class CharacterImplant
{
    public int ImplantId { get; set; }
    public string ImplantName { get; set; } = string.Empty;
    public int Slot { get; set; }
    public Dictionary<string, double> AttributeBonuses { get; set; } = new();
}
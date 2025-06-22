// ==========================================================================
// ImportExportServices.cs - Ship Fitting Import/Export Service Implementations
// ==========================================================================
// Comprehensive import/export services for popular EVE Online fitting formats.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using Microsoft.Extensions.Logging;
using Gideon.WPF.Core.Application.Services;
using Gideon.WPF.Core.Domain.Entities;
using Gideon.WPF.Core.Domain.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Gideon.WPF.Core.Infrastructure.Services;

#region Import/Export Services

/// <summary>
/// Service for importing and exporting ship fittings in various formats
/// </summary>
public class FittingImportExportService : IDataImportService, IDataExportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FittingImportExportService> _logger;
    private readonly IShipFittingValidationService _validationService;

    public FittingImportExportService(
        IUnitOfWork unitOfWork,
        ILogger<FittingImportExportService> logger,
        IShipFittingValidationService validationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
    }

    #region Export Methods

    /// <summary>
    /// Export ship fitting data in specified format
    /// </summary>
    public async Task<string> ExportShipFittingAsync(Guid fittingId, string format, CancellationToken cancellationToken = default)
    {
        try
        {
            var fitting = await _unitOfWork.ShipFittings.GetByIdAsync(fittingId, cancellationToken);
            if (fitting == null)
            {
                throw new ArgumentException($"Fitting with ID {fittingId} not found");
            }

            _logger.LogInformation("Exporting fitting {FittingName} to {Format} format", fitting.Name, format);

            return format.ToUpperInvariant() switch
            {
                "EFT" => await ExportToEftFormatAsync(fitting, cancellationToken),
                "DNA" => await ExportToDnaFormatAsync(fitting, cancellationToken),
                "XML" => await ExportToXmlFormatAsync(fitting, cancellationToken),
                "JSON" => await ExportToJsonFormatAsync(fitting, cancellationToken),
                "KILLMAIL" => await ExportToKillmailFormatAsync(fitting, cancellationToken),
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting fitting {FittingId} to {Format}", fittingId, format);
            throw;
        }
    }

    /// <summary>
    /// Export character data in specified format
    /// </summary>
    public async Task<string> ExportCharacterDataAsync(int characterId, string format, CancellationToken cancellationToken = default)
    {
        try
        {
            var character = await _unitOfWork.Characters.GetByIdAsync(characterId, cancellationToken);
            if (character == null)
            {
                throw new ArgumentException($"Character with ID {characterId} not found");
            }

            _logger.LogInformation("Exporting character {CharacterName} to {Format} format", character.CharacterName, format);

            return format.ToUpperInvariant() switch
            {
                "JSON" => await ExportCharacterToJsonAsync(character, cancellationToken),
                "XML" => await ExportCharacterToXmlAsync(character, cancellationToken),
                "EVEMON" => await ExportCharacterToEvemonAsync(character, cancellationToken),
                _ => throw new ArgumentException($"Unsupported character export format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting character {CharacterId} to {Format}", characterId, format);
            throw;
        }
    }

    /// <summary>
    /// Export skill plan in specified format
    /// </summary>
    public async Task<string> ExportSkillPlanAsync(int skillPlanId, string format, CancellationToken cancellationToken = default)
    {
        try
        {
            var skillPlan = await _unitOfWork.SkillPlans.GetByIdAsync(skillPlanId, cancellationToken);
            if (skillPlan == null)
            {
                throw new ArgumentException($"Skill plan with ID {skillPlanId} not found");
            }

            _logger.LogInformation("Exporting skill plan {SkillPlanName} to {Format} format", skillPlan.Name, format);

            return format.ToUpperInvariant() switch
            {
                "JSON" => await ExportSkillPlanToJsonAsync(skillPlan, cancellationToken),
                "XML" => await ExportSkillPlanToXmlAsync(skillPlan, cancellationToken),
                "EVEMON" => await ExportSkillPlanToEvemonAsync(skillPlan, cancellationToken),
                "CSV" => await ExportSkillPlanToCsvAsync(skillPlan, cancellationToken),
                _ => throw new ArgumentException($"Unsupported skill plan export format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting skill plan {SkillPlanId} to {Format}", skillPlanId, format);
            throw;
        }
    }

    #endregion

    #region Import Methods

    /// <summary>
    /// Import ship fitting from data string
    /// </summary>
    public async Task<bool> ImportShipFittingAsync(string data, string format, int? characterId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Importing fitting from {Format} format", format);

            var fitting = format.ToUpperInvariant() switch
            {
                "EFT" => await ImportFromEftFormatAsync(data, characterId, cancellationToken),
                "DNA" => await ImportFromDnaFormatAsync(data, characterId, cancellationToken),
                "XML" => await ImportFromXmlFormatAsync(data, characterId, cancellationToken),
                "JSON" => await ImportFromJsonFormatAsync(data, characterId, cancellationToken),
                "KILLMAIL" => await ImportFromKillmailFormatAsync(data, characterId, cancellationToken),
                _ => throw new ArgumentException($"Unsupported import format: {format}")
            };

            if (fitting == null)
            {
                _logger.LogWarning("Failed to parse fitting from {Format} format", format);
                return false;
            }

            // Validate the imported fitting
            var validationErrors = await _validationService.GetValidationErrorsAsync(fitting, cancellationToken);
            if (validationErrors.Any())
            {
                _logger.LogWarning("Imported fitting has validation errors: {Errors}", string.Join(", ", validationErrors));
                // Still save but mark as invalid
                fitting.Notes = $"Import warnings: {string.Join("; ", validationErrors)}";
            }

            await _unitOfWork.ShipFittings.AddAsync(fitting, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully imported fitting {FittingName}", fitting.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing fitting from {Format} format", format);
            return false;
        }
    }

    /// <summary>
    /// Import skill plan from data string
    /// </summary>
    public async Task<bool> ImportSkillPlanAsync(string data, string format, int characterId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Importing skill plan from {Format} format for character {CharacterId}", format, characterId);

            var skillPlan = format.ToUpperInvariant() switch
            {
                "JSON" => await ImportSkillPlanFromJsonAsync(data, characterId, cancellationToken),
                "XML" => await ImportSkillPlanFromXmlAsync(data, characterId, cancellationToken),
                "EVEMON" => await ImportSkillPlanFromEvemonAsync(data, characterId, cancellationToken),
                "CSV" => await ImportSkillPlanFromCsvAsync(data, characterId, cancellationToken),
                _ => throw new ArgumentException($"Unsupported skill plan import format: {format}")
            };

            if (skillPlan == null)
            {
                _logger.LogWarning("Failed to parse skill plan from {Format} format", format);
                return false;
            }

            await _unitOfWork.SkillPlans.AddAsync(skillPlan, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully imported skill plan {SkillPlanName}", skillPlan.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing skill plan from {Format} format", format);
            return false;
        }
    }

    #endregion

    #region EFT Format (EVE Fitting Tool)

    /// <summary>
    /// Export fitting to EFT format (EVE Fitting Tool standard)
    /// </summary>
    private async Task<string> ExportToEftFormatAsync(ShipFitting fitting, CancellationToken cancellationToken)
    {
        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);
        var sb = new StringBuilder();

        // EFT Header: [ShipType, FittingName]
        sb.AppendLine($"[{shipType?.TypeName ?? "Unknown"}, {fitting.Name}]");

        // Group modules by slot type
        var modulesBySlot = fitting.Modules.GroupBy(m => m.Slot);

        foreach (var slotGroup in modulesBySlot.OrderBy(g => GetSlotOrder(g.Key)))
        {
            foreach (var module in slotGroup.OrderBy(m => m.TypeId))
            {
                var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(module.TypeId, cancellationToken);
                var moduleName = moduleType?.TypeName ?? $"Unknown Module {module.TypeId}";

                // Add quantity if more than 1
                if (module.Quantity > 1)
                {
                    sb.AppendLine($"{moduleName} x{module.Quantity}");
                }
                else
                {
                    sb.AppendLine(moduleName);
                }

                // Add charge if specified
                if (module.ChargeTypeId.HasValue)
                {
                    var chargeType = await _unitOfWork.EveTypes.GetByIdAsync(module.ChargeTypeId.Value, cancellationToken);
                    if (chargeType != null)
                    {
                        sb.AppendLine($"  {chargeType.TypeName}");
                    }
                }
            }
        }

        // Add drones if any
        var droneModules = fitting.Modules.Where(m => m.Slot.Equals("Drone", StringComparison.OrdinalIgnoreCase));
        if (droneModules.Any())
        {
            sb.AppendLine();
            foreach (var drone in droneModules)
            {
                var droneType = await _unitOfWork.EveTypes.GetByIdAsync(drone.TypeId, cancellationToken);
                sb.AppendLine($"{droneType?.TypeName ?? "Unknown Drone"} x{drone.Quantity}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Import fitting from EFT format
    /// </summary>
    private async Task<ShipFitting?> ImportFromEftFormatAsync(string data, int? characterId, CancellationToken cancellationToken)
    {
        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return null;

        // Parse header [ShipType, FittingName]
        var headerMatch = Regex.Match(lines[0], @"\[(.*?),\s*(.*?)\]");
        if (!headerMatch.Success) return null;

        var shipTypeName = headerMatch.Groups[1].Value.Trim();
        var fittingName = headerMatch.Groups[2].Value.Trim();

        // Find ship type
        var shipType = await _unitOfWork.EveTypes.FindFirstAsync(
            t => t.TypeName == shipTypeName, cancellationToken);
        if (shipType == null) return null;

        var fitting = new ShipFitting
        {
            Id = Guid.NewGuid(),
            Name = fittingName,
            ShipTypeId = shipType.TypeId,
            CharacterId = characterId,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Modules = new List<FittingModule>()
        };

        // Parse modules
        var currentSlot = "High"; // Default slot
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Check if line is indented (charge/ammo)
            if (line.StartsWith("  "))
            {
                // Handle charges - add to last module
                continue;
            }

            await ParseEftModuleLineAsync(line, fitting, currentSlot, cancellationToken);
        }

        return fitting;
    }

    #endregion

    #region DNA Format (EVE Client)

    /// <summary>
    /// Export fitting to DNA format (EVE client standard)
    /// </summary>
    private async Task<string> ExportToDnaFormatAsync(ShipFitting fitting, CancellationToken cancellationToken)
    {
        var dnaBuilder = new StringBuilder();
        dnaBuilder.Append($"{fitting.ShipTypeId}:");

        // Group modules by type and count
        var moduleGroups = fitting.Modules
            .GroupBy(m => m.TypeId)
            .OrderBy(g => g.Key);

        foreach (var group in moduleGroups)
        {
            var totalQuantity = group.Sum(m => m.Quantity);
            dnaBuilder.Append($"{group.Key};{totalQuantity}:");
        }

        // Add fitting name as suffix
        dnaBuilder.Append($"::{fitting.Name}");

        return dnaBuilder.ToString();
    }

    /// <summary>
    /// Import fitting from DNA format
    /// </summary>
    private async Task<ShipFitting?> ImportFromDnaFormatAsync(string data, int? characterId, CancellationToken cancellationToken)
    {
        var parts = data.Split(':');
        if (parts.Length < 2) return null;

        if (!int.TryParse(parts[0], out var shipTypeId)) return null;

        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(shipTypeId, cancellationToken);
        if (shipType == null) return null;

        var fitting = new ShipFitting
        {
            Id = Guid.NewGuid(),
            Name = parts.Length > 3 && !string.IsNullOrEmpty(parts[3]) ? parts[3] : $"{shipType.TypeName} Fitting",
            ShipTypeId = shipTypeId,
            CharacterId = characterId,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Modules = new List<FittingModule>()
        };

        // Parse modules
        for (int i = 1; i < parts.Length - 1; i++)
        {
            var modulePart = parts[i];
            if (string.IsNullOrEmpty(modulePart)) continue;

            var moduleData = modulePart.Split(';');
            if (moduleData.Length != 2) continue;

            if (int.TryParse(moduleData[0], out var moduleTypeId) && 
                int.TryParse(moduleData[1], out var quantity))
            {
                var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(moduleTypeId, cancellationToken);
                if (moduleType != null)
                {
                    fitting.Modules.Add(new FittingModule
                    {
                        TypeId = moduleTypeId,
                        Slot = DetermineSlotFromModuleType(moduleType),
                        Quantity = quantity,
                        IsOnline = true
                    });
                }
            }
        }

        return fitting;
    }

    #endregion

    #region XML Format

    /// <summary>
    /// Export fitting to XML format
    /// </summary>
    private async Task<string> ExportToXmlFormatAsync(ShipFitting fitting, CancellationToken cancellationToken)
    {
        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);

        var xml = new XElement("fitting",
            new XAttribute("name", fitting.Name),
            new XElement("shipType",
                new XAttribute("value", shipType?.TypeName ?? "Unknown")
            ),
            new XElement("description",
                new XAttribute("value", fitting.Description ?? "")
            )
        );

        // Add modules grouped by slot
        var slotGroups = fitting.Modules.GroupBy(m => m.Slot);
        foreach (var slotGroup in slotGroups)
        {
            foreach (var module in slotGroup)
            {
                var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(module.TypeId, cancellationToken);
                var moduleElement = new XElement("hardware",
                    new XAttribute("type", moduleType?.TypeName ?? "Unknown"),
                    new XAttribute("slot", module.Slot.ToLower())
                );

                if (module.Quantity > 1)
                {
                    moduleElement.Add(new XAttribute("qty", module.Quantity));
                }

                xml.Add(moduleElement);
            }
        }

        return xml.ToString();
    }

    /// <summary>
    /// Import fitting from XML format
    /// </summary>
    private async Task<ShipFitting?> ImportFromXmlFormatAsync(string data, int? characterId, CancellationToken cancellationToken)
    {
        try
        {
            var xml = XElement.Parse(data);
            var fittingName = xml.Attribute("name")?.Value ?? "Imported Fitting";
            var shipTypeName = xml.Element("shipType")?.Attribute("value")?.Value;

            if (string.IsNullOrEmpty(shipTypeName)) return null;

            var shipType = await _unitOfWork.EveTypes.FindFirstAsync(
                t => t.TypeName == shipTypeName, cancellationToken);
            if (shipType == null) return null;

            var fitting = new ShipFitting
            {
                Id = Guid.NewGuid(),
                Name = fittingName,
                ShipTypeId = shipType.TypeId,
                CharacterId = characterId,
                Description = xml.Element("description")?.Attribute("value")?.Value,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Modules = new List<FittingModule>()
            };

            // Parse modules
            foreach (var hardware in xml.Elements("hardware"))
            {
                var typeName = hardware.Attribute("type")?.Value;
                var slot = hardware.Attribute("slot")?.Value ?? "high";
                var qtyAttr = hardware.Attribute("qty")?.Value;
                var quantity = int.TryParse(qtyAttr, out var qty) ? qty : 1;

                if (!string.IsNullOrEmpty(typeName))
                {
                    var moduleType = await _unitOfWork.EveTypes.FindFirstAsync(
                        t => t.TypeName == typeName, cancellationToken);
                    if (moduleType != null)
                    {
                        fitting.Modules.Add(new FittingModule
                        {
                            TypeId = moduleType.TypeId,
                            Slot = CapitalizeSlot(slot),
                            Quantity = quantity,
                            IsOnline = true
                        });
                    }
                }
            }

            return fitting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing XML fitting data");
            return null;
        }
    }

    #endregion

    #region JSON Format

    /// <summary>
    /// Export fitting to JSON format
    /// </summary>
    private async Task<string> ExportToJsonFormatAsync(ShipFitting fitting, CancellationToken cancellationToken)
    {
        var shipType = await _unitOfWork.EveTypes.GetByIdAsync(fitting.ShipTypeId, cancellationToken);
        
        var exportData = new
        {
            name = fitting.Name,
            ship = new
            {
                typeId = fitting.ShipTypeId,
                typeName = shipType?.TypeName
            },
            description = fitting.Description,
            modules = await Task.WhenAll(fitting.Modules.Select(async m =>
            {
                var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(m.TypeId, cancellationToken);
                return new
                {
                    typeId = m.TypeId,
                    typeName = moduleType?.TypeName,
                    slot = m.Slot,
                    quantity = m.Quantity,
                    isOnline = m.IsOnline,
                    chargeTypeId = m.ChargeTypeId
                };
            })),
            createdDate = fitting.CreatedDate,
            modifiedDate = fitting.ModifiedDate
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    /// <summary>
    /// Import fitting from JSON format
    /// </summary>
    private async Task<ShipFitting?> ImportFromJsonFormatAsync(string data, int? characterId, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;

            var name = root.GetProperty("name").GetString() ?? "Imported Fitting";
            var shipTypeId = root.GetProperty("ship").GetProperty("typeId").GetInt32();

            var fitting = new ShipFitting
            {
                Id = Guid.NewGuid(),
                Name = name,
                ShipTypeId = shipTypeId,
                CharacterId = characterId,
                Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Modules = new List<FittingModule>()
            };

            if (root.TryGetProperty("modules", out var modulesArray))
            {
                foreach (var moduleElement in modulesArray.EnumerateArray())
                {
                    var module = new FittingModule
                    {
                        TypeId = moduleElement.GetProperty("typeId").GetInt32(),
                        Slot = moduleElement.GetProperty("slot").GetString() ?? "High",
                        Quantity = moduleElement.TryGetProperty("quantity", out var qty) ? qty.GetInt32() : 1,
                        IsOnline = moduleElement.TryGetProperty("isOnline", out var online) ? online.GetBoolean() : true
                    };

                    if (moduleElement.TryGetProperty("chargeTypeId", out var chargeType) && 
                        chargeType.ValueKind != JsonValueKind.Null)
                    {
                        module.ChargeTypeId = chargeType.GetInt32();
                    }

                    fitting.Modules.Add(module);
                }
            }

            return fitting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JSON fitting data");
            return null;
        }
    }

    #endregion

    #region Character Export/Import

    private async Task<string> ExportCharacterToJsonAsync(Character character, CancellationToken cancellationToken)
    {
        var exportData = new
        {
            character = new
            {
                characterId = character.CharacterId,
                characterName = character.CharacterName,
                corporationId = character.CorporationId,
                allianceId = character.AllianceId,
                securityStatus = character.SecurityStatus,
                totalSp = character.TotalSp
            },
            skills = character.Skills.Select(s => new
            {
                skillId = s.SkillId,
                skillLevel = s.SkillLevel,
                skillPoints = s.SkillPoints,
                isActive = s.IsActive
            }),
            exportedAt = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> ExportCharacterToXmlAsync(Character character, CancellationToken cancellationToken)
    {
        var xml = new XElement("character",
            new XAttribute("name", character.CharacterName),
            new XAttribute("characterId", character.CharacterId),
            new XElement("corporation", character.CorporationId ?? 0),
            new XElement("alliance", character.AllianceId ?? 0),
            new XElement("securityStatus", character.SecurityStatus),
            new XElement("totalSp", character.TotalSp),
            new XElement("skills",
                character.Skills.Select(s => new XElement("skill",
                    new XAttribute("typeId", s.SkillId),
                    new XAttribute("level", s.SkillLevel),
                    new XAttribute("skillpoints", s.SkillPoints)
                ))
            )
        );

        return xml.ToString();
    }

    private async Task<string> ExportCharacterToEvemonAsync(Character character, CancellationToken cancellationToken)
    {
        // EVEMon XML format
        var xml = new XElement("SerializableSettingsCharacter",
            new XElement("Name", character.CharacterName),
            new XElement("ID", character.CharacterId),
            new XElement("Skills",
                character.Skills.Select(s => new XElement("SerializableCharacterSkill",
                    new XElement("ID", s.SkillId),
                    new XElement("Level", s.SkillLevel),
                    new XElement("Skillpoints", s.SkillPoints)
                ))
            )
        );

        return xml.ToString();
    }

    #endregion

    #region Skill Plan Export/Import

    private async Task<string> ExportSkillPlanToJsonAsync(SkillPlan skillPlan, CancellationToken cancellationToken)
    {
        var exportData = new
        {
            name = skillPlan.Name,
            characterId = skillPlan.CharacterId,
            description = skillPlan.Description,
            status = skillPlan.Status.ToString(),
            estimatedTrainingTime = skillPlan.EstimatedTrainingTime,
            entries = skillPlan.Entries.Select(e => new
            {
                skillId = e.SkillId,
                targetLevel = e.TargetLevel,
                priority = e.Priority,
                estimatedTime = e.EstimatedTime
            }),
            createdDate = skillPlan.CreatedDate
        };

        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<string> ExportSkillPlanToXmlAsync(SkillPlan skillPlan, CancellationToken cancellationToken)
    {
        var xml = new XElement("skillPlan",
            new XAttribute("name", skillPlan.Name),
            new XElement("description", skillPlan.Description ?? ""),
            new XElement("skills",
                skillPlan.Entries.Select(e => new XElement("skill",
                    new XAttribute("typeId", e.SkillId),
                    new XAttribute("level", e.TargetLevel),
                    new XAttribute("priority", e.Priority)
                ))
            )
        );

        return xml.ToString();
    }

    private async Task<string> ExportSkillPlanToEvemonAsync(SkillPlan skillPlan, CancellationToken cancellationToken)
    {
        var xml = new XElement("plan",
            new XAttribute("name", skillPlan.Name),
            skillPlan.Entries.Select(e => new XElement("entry",
                new XAttribute("skillID", e.SkillId),
                new XAttribute("level", e.TargetLevel)
            ))
        );

        return xml.ToString();
    }

    private async Task<string> ExportSkillPlanToCsvAsync(SkillPlan skillPlan, CancellationToken cancellationToken)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Skill ID,Skill Name,Target Level,Priority,Estimated Time");

        foreach (var entry in skillPlan.Entries.OrderBy(e => e.Priority))
        {
            var skill = await _unitOfWork.EveTypes.GetByIdAsync(entry.SkillId, cancellationToken);
            csv.AppendLine($"{entry.SkillId},{skill?.TypeName ?? "Unknown"},{entry.TargetLevel},{entry.Priority},{entry.EstimatedTime}");
        }

        return csv.ToString();
    }

    private async Task<SkillPlan?> ImportSkillPlanFromJsonAsync(string data, int characterId, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;

            var skillPlan = new SkillPlan
            {
                Name = root.GetProperty("name").GetString() ?? "Imported Skill Plan",
                CharacterId = characterId,
                Description = root.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                Status = SkillPlanStatus.Active,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Entries = new List<SkillPlanEntry>()
            };

            if (root.TryGetProperty("entries", out var entriesArray))
            {
                foreach (var entryElement in entriesArray.EnumerateArray())
                {
                    skillPlan.Entries.Add(new SkillPlanEntry
                    {
                        SkillId = entryElement.GetProperty("skillId").GetInt32(),
                        TargetLevel = entryElement.GetProperty("targetLevel").GetInt32(),
                        Priority = entryElement.TryGetProperty("priority", out var priority) ? priority.GetInt32() : 1
                    });
                }
            }

            return skillPlan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing JSON skill plan data");
            return null;
        }
    }

    #endregion

    #region Killmail Format

    private async Task<string> ExportToKillmailFormatAsync(ShipFitting fitting, CancellationToken cancellationToken)
    {
        // EVE killmail format (simplified)
        var killmail = new
        {
            victim = new
            {
                ship_type_id = fitting.ShipTypeId
            },
            items = fitting.Modules.Select(m => new
            {
                item_type_id = m.TypeId,
                quantity_destroyed = m.Quantity,
                singleton = 0
            })
        };

        return JsonSerializer.Serialize(killmail, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<ShipFitting?> ImportFromKillmailFormatAsync(string data, int? characterId, CancellationToken cancellationToken)
    {
        try
        {
            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;

            var shipTypeId = root.GetProperty("victim").GetProperty("ship_type_id").GetInt32();
            var shipType = await _unitOfWork.EveTypes.GetByIdAsync(shipTypeId, cancellationToken);

            var fitting = new ShipFitting
            {
                Id = Guid.NewGuid(),
                Name = $"{shipType?.TypeName ?? "Unknown"} Killmail Fit",
                ShipTypeId = shipTypeId,
                CharacterId = characterId,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Modules = new List<FittingModule>()
            };

            if (root.TryGetProperty("items", out var itemsArray))
            {
                foreach (var item in itemsArray.EnumerateArray())
                {
                    var typeId = item.GetProperty("item_type_id").GetInt32();
                    var quantity = item.TryGetProperty("quantity_destroyed", out var qty) ? qty.GetInt32() : 1;

                    var moduleType = await _unitOfWork.EveTypes.GetByIdAsync(typeId, cancellationToken);
                    if (moduleType != null)
                    {
                        fitting.Modules.Add(new FittingModule
                        {
                            TypeId = typeId,
                            Slot = DetermineSlotFromModuleType(moduleType),
                            Quantity = quantity,
                            IsOnline = true
                        });
                    }
                }
            }

            return fitting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing killmail fitting data");
            return null;
        }
    }

    #endregion

    #region Helper Methods

    private async Task ParseEftModuleLineAsync(string line, ShipFitting fitting, string defaultSlot, CancellationToken cancellationToken)
    {
        // Parse quantity (e.g., "Module Name x2")
        var quantityMatch = Regex.Match(line, @"^(.*?)\s+x(\d+)$");
        var moduleName = quantityMatch.Success ? quantityMatch.Groups[1].Value.Trim() : line.Trim();
        var quantity = quantityMatch.Success ? int.Parse(quantityMatch.Groups[2].Value) : 1;

        // Find module type
        var moduleType = await _unitOfWork.EveTypes.FindFirstAsync(
            t => t.TypeName == moduleName, cancellationToken);

        if (moduleType != null)
        {
            fitting.Modules.Add(new FittingModule
            {
                TypeId = moduleType.TypeId,
                Slot = DetermineSlotFromModuleType(moduleType),
                Quantity = quantity,
                IsOnline = true
            });
        }
    }

    private static string DetermineSlotFromModuleType(EveType moduleType)
    {
        // Basic slot determination - would be enhanced with actual EVE data
        return moduleType.GroupId switch
        {
            // Weapons (High Slots)
            >= 1 and <= 20 => "High",
            // Electronics/Engineering (Mid Slots)  
            >= 21 and <= 60 => "Medium",
            // Hull/Armor (Low Slots)
            >= 61 and <= 100 => "Low",
            // Rigs
            >= 101 and <= 120 => "Rig",
            // Subsystems
            >= 121 and <= 140 => "Subsystem",
            _ => "High" // Default
        };
    }

    private static string CapitalizeSlot(string slot)
    {
        return slot.ToLowerInvariant() switch
        {
            "high" => "High",
            "med" or "medium" => "Medium", 
            "low" => "Low",
            "rig" => "Rig",
            "subsystem" => "Subsystem",
            "drone" => "Drone",
            _ => slot
        };
    }

    private static int GetSlotOrder(string slot)
    {
        return slot.ToLowerInvariant() switch
        {
            "high" => 1,
            "medium" => 2,
            "low" => 3,
            "rig" => 4,
            "subsystem" => 5,
            "drone" => 6,
            _ => 7
        };
    }

    private async Task<SkillPlan?> ImportSkillPlanFromXmlAsync(string data, int characterId, CancellationToken cancellationToken)
    {
        // Implementation for XML skill plan import
        return null; // Placeholder
    }

    private async Task<SkillPlan?> ImportSkillPlanFromEvemonAsync(string data, int characterId, CancellationToken cancellationToken)
    {
        // Implementation for EVEMon skill plan import
        return null; // Placeholder
    }

    private async Task<SkillPlan?> ImportSkillPlanFromCsvAsync(string data, int characterId, CancellationToken cancellationToken)
    {
        // Implementation for CSV skill plan import
        return null; // Placeholder
    }

    #endregion
}

#endregion
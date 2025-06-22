// ==========================================================================
// HoloShipMaterials.cs - Holographic Ship Materials with EVE Textures
// ==========================================================================
// Advanced material system featuring holographic ship materials, EVE-style
// textures, dynamic material switching, and performance-optimized rendering.
//
// Author: Gideon Development Team
// Created: June 22, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Gideon.WPF.Presentation.Controls;

/// <summary>
/// Holographic ship materials system with EVE textures and dynamic material switching
/// </summary>
public class HoloShipMaterials : IAnimationIntensityTarget, IAdaptiveControl
{
    #region Private Fields

    private readonly Dictionary<string, Material> _materialCache = new();
    private readonly Dictionary<string, BitmapImage> _textureCache = new();
    private readonly DispatcherTimer _animationTimer;
    private bool _isSimplifiedMode;
    private double _animationPhase;
    private EVEColorScheme _colorScheme = EVEColorScheme.ElectricBlue;
    private double _holographicIntensity = 1.0;

    #endregion

    #region Constructor

    public HoloShipMaterials()
    {
        InitializeAnimationTimer();
        LoadDefaultTextures();
    }

    #endregion

    #region Initialization

    private void InitializeAnimationTimer()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        _animationTimer.Tick += OnAnimationTimerTick;
    }

    private void LoadDefaultTextures()
    {
        // Load default EVE-style textures
        _ = Task.Run(async () =>
        {
            await LoadDefaultShipTexturesAsync();
            await LoadDefaultMaterialTexturesAsync();
        });
    }

    #endregion

    #region Material Creation

    public Material CreateHolographicShipMaterial(ShipMaterialType materialType, EVEColorScheme colorScheme, double intensity = 1.0)
    {
        var cacheKey = $"{materialType}_{colorScheme}_{intensity:F1}";
        
        if (_materialCache.TryGetValue(cacheKey, out var cachedMaterial))
        {
            return cachedMaterial;
        }

        var material = materialType switch
        {
            ShipMaterialType.Hull => CreateHullMaterial(colorScheme, intensity),
            ShipMaterialType.Armor => CreateArmorMaterial(colorScheme, intensity),
            ShipMaterialType.Shield => CreateShieldMaterial(colorScheme, intensity),
            ShipMaterialType.Engine => CreateEngineMaterial(colorScheme, intensity),
            ShipMaterialType.Weapon => CreateWeaponMaterial(colorScheme, intensity),
            ShipMaterialType.Electronics => CreateElectronicsMaterial(colorScheme, intensity),
            ShipMaterialType.Holographic => CreatePureHolographicMaterial(colorScheme, intensity),
            ShipMaterialType.Wireframe => CreateWireframeMaterial(colorScheme, intensity),
            _ => CreateDefaultMaterial(colorScheme, intensity)
        };

        _materialCache[cacheKey] = material;
        return material;
    }

    public Material CreateModuleMaterial(ModuleSlotType slotType, bool isHighlighted = false, double intensity = 1.0)
    {
        var baseColor = GetModuleSlotColor(slotType);
        var highlightMultiplier = isHighlighted ? 2.0 : 1.0;
        
        var materialGroup = new MaterialGroup();

        // Base diffuse material
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(120 * intensity),
            (byte)(baseColor.R * highlightMultiplier),
            (byte)(baseColor.G * highlightMultiplier),
            (byte)(baseColor.B * highlightMultiplier))));
        materialGroup.Children.Add(diffuseMaterial);

        // Emissive glow
        if (isHighlighted || intensity > 0.5)
        {
            var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
                (byte)(80 * intensity * highlightMultiplier),
                baseColor.R, baseColor.G, baseColor.B)));
            materialGroup.Children.Add(emissiveMaterial);
        }

        // Specular highlights
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Colors.White), 30 * intensity);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    public async Task<Material> CreateTexturedMaterialAsync(string texturePath, ShipMaterialType materialType, double intensity = 1.0)
    {
        var texture = await LoadTextureAsync(texturePath);
        if (texture == null)
        {
            return CreateHolographicShipMaterial(materialType, _colorScheme, intensity);
        }

        var materialGroup = new MaterialGroup();

        // Textured diffuse material
        var diffuseBrush = new ImageBrush(texture)
        {
            TileMode = TileMode.Tile,
            Opacity = intensity
        };
        var diffuseMaterial = new DiffuseMaterial(diffuseBrush);
        materialGroup.Children.Add(diffuseMaterial);

        // Add holographic overlay
        if (materialType == ShipMaterialType.Holographic)
        {
            var holographicOverlay = CreateHolographicOverlay(intensity);
            materialGroup.Children.Add(holographicOverlay);
        }

        // Add material-specific effects
        AddMaterialTypeEffects(materialGroup, materialType, intensity);

        return materialGroup;
    }

    public Material CreateAnimatedMaterial(ShipMaterialType materialType, EVEColorScheme colorScheme, double intensity = 1.0)
    {
        var baseMaterial = CreateHolographicShipMaterial(materialType, colorScheme, intensity);
        
        if (_isSimplifiedMode || intensity < 0.3)
        {
            return baseMaterial;
        }

        // Add animated properties
        if (baseMaterial is MaterialGroup materialGroup)
        {
            AddAnimatedEffects(materialGroup, materialType, intensity);
        }

        return baseMaterial;
    }

    #endregion

    #region Specific Material Creators

    private Material CreateHullMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Base metallic hull
        var hullColor = DarkenColor(colors.Primary, 0.3);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(200 * intensity), hullColor.R, hullColor.G, hullColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Metallic reflection
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(150 * intensity), 255, 255, 255)), 80 * intensity);
        materialGroup.Children.Add(specularMaterial);

        // Subtle emissive glow
        if (intensity > 0.5)
        {
            var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
                (byte)(30 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B)));
            materialGroup.Children.Add(emissiveMaterial);
        }

        return materialGroup;
    }

    private Material CreateArmorMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Armored plating
        var armorColor = BlendColors(colors.Primary, Color.FromRgb(80, 80, 80), 0.7);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(220 * intensity), armorColor.R, armorColor.G, armorColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Reduced specular for matte armor
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(100 * intensity), 200, 200, 200)), 40 * intensity);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    private Material CreateShieldMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Translucent shield effect
        var shieldColor = LightenColor(colors.Primary, 0.4);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(60 * intensity), shieldColor.R, shieldColor.G, shieldColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Strong emissive glow for energy shields
        var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(120 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B)));
        materialGroup.Children.Add(emissiveMaterial);

        // High specular for energy reflection
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(180 * intensity), 255, 255, 255)), 100 * intensity);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    private Material CreateEngineMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Engine glow
        var engineColor = BlendColors(colors.Primary, Color.FromRgb(255, 150, 0), 0.6);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(180 * intensity), engineColor.R, engineColor.G, engineColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Intense emissive for engine thrust
        var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(200 * intensity), engineColor.R, engineColor.G, engineColor.B)));
        materialGroup.Children.Add(emissiveMaterial);

        return materialGroup;
    }

    private Material CreateWeaponMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Weapon charge/energy
        var weaponColor = BlendColors(colors.Primary, Color.FromRgb(255, 0, 0), 0.3);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(160 * intensity), weaponColor.R, weaponColor.G, weaponColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Weapon charging glow
        var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(100 * intensity), weaponColor.R, weaponColor.G, weaponColor.B)));
        materialGroup.Children.Add(emissiveMaterial);

        // Sharp specular highlights
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Colors.White), 120 * intensity);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    private Material CreateElectronicsMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Electronic circuit glow
        var electronicsColor = LightenColor(colors.Primary, 0.2);
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(140 * intensity), electronicsColor.R, electronicsColor.G, electronicsColor.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Pulsing electronic glow
        var emissiveBrush = new SolidColorBrush(Color.FromArgb(
            (byte)(80 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B));
        
        if (!_isSimplifiedMode && intensity > 0.5)
        {
            var pulseAnimation = new ColorAnimation
            {
                From = Color.FromArgb((byte)(40 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B),
                To = Color.FromArgb((byte)(120 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B),
                Duration = TimeSpan.FromSeconds(1.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            emissiveBrush.BeginAnimation(SolidColorBrush.ColorProperty, pulseAnimation);
        }

        var emissiveMaterial = new EmissiveMaterial(emissiveBrush);
        materialGroup.Children.Add(emissiveMaterial);

        return materialGroup;
    }

    private Material CreatePureHolographicMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        var materialGroup = new MaterialGroup();

        // Transparent holographic base
        var diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(80 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B)));
        materialGroup.Children.Add(diffuseMaterial);

        // Strong holographic glow
        var emissiveMaterial = new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(150 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B)));
        materialGroup.Children.Add(emissiveMaterial);

        // Holographic shimmer
        var specularMaterial = new SpecularMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(200 * intensity), 255, 255, 255)), 200 * intensity);
        materialGroup.Children.Add(specularMaterial);

        return materialGroup;
    }

    private Material CreateWireframeMaterial(EVEColorScheme colorScheme, double intensity)
    {
        var colors = EVEColorSchemeHelper.GetColors(colorScheme);
        
        // Simple emissive wireframe
        return new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(
            (byte)(180 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B)));
    }

    private Material CreateDefaultMaterial(EVEColorScheme colorScheme, double intensity)
    {
        return CreateHullMaterial(colorScheme, intensity);
    }

    #endregion

    #region Material Effects

    private Material CreateHolographicOverlay(double intensity)
    {
        var overlayBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb((byte)(50 * intensity), 100, 200, 255), 0),
                new GradientStop(Color.FromArgb((byte)(20 * intensity), 100, 200, 255), 0.5),
                new GradientStop(Color.FromArgb((byte)(40 * intensity), 100, 200, 255), 1)
            }
        };

        return new EmissiveMaterial(overlayBrush);
    }

    private void AddMaterialTypeEffects(MaterialGroup materialGroup, ShipMaterialType materialType, double intensity)
    {
        switch (materialType)
        {
            case ShipMaterialType.Shield:
                AddShieldDistortion(materialGroup, intensity);
                break;
            case ShipMaterialType.Engine:
                AddEngineFlicker(materialGroup, intensity);
                break;
            case ShipMaterialType.Holographic:
                AddHolographicDistortion(materialGroup, intensity);
                break;
        }
    }

    private void AddAnimatedEffects(MaterialGroup materialGroup, ShipMaterialType materialType, double intensity)
    {
        if (_isSimplifiedMode) return;

        switch (materialType)
        {
            case ShipMaterialType.Electronics:
                AddCircuitPulse(materialGroup, intensity);
                break;
            case ShipMaterialType.Engine:
                AddEngineThrust(materialGroup, intensity);
                break;
            case ShipMaterialType.Shield:
                AddShieldFlicker(materialGroup, intensity);
                break;
            case ShipMaterialType.Holographic:
                AddHolographicShimmer(materialGroup, intensity);
                break;
        }
    }

    private void AddShieldDistortion(MaterialGroup materialGroup, double intensity)
    {
        // Add subtle distortion effect for shields
        var distortionBrush = new RadialGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb((byte)(30 * intensity), 255, 255, 255), 0),
                new GradientStop(Color.FromArgb((byte)(10 * intensity), 255, 255, 255), 0.7),
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 1)
            }
        };

        var distortionMaterial = new EmissiveMaterial(distortionBrush);
        materialGroup.Children.Add(distortionMaterial);
    }

    private void AddEngineFlicker(MaterialGroup materialGroup, double intensity)
    {
        // Add engine heat flicker
        var flickerBrush = new SolidColorBrush(Color.FromArgb(
            (byte)(60 * intensity), 255, 100, 0));
        
        if (!_isSimplifiedMode)
        {
            var flickerAnimation = new DoubleAnimation
            {
                From = 0.3 * intensity,
                To = 1.0 * intensity,
                Duration = TimeSpan.FromMilliseconds(200),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            flickerBrush.BeginAnimation(Brush.OpacityProperty, flickerAnimation);
        }

        var flickerMaterial = new EmissiveMaterial(flickerBrush);
        materialGroup.Children.Add(flickerMaterial);
    }

    private void AddHolographicDistortion(MaterialGroup materialGroup, double intensity)
    {
        // Add holographic scan lines effect
        var scanLineBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
            GradientStops = new GradientStopCollection()
        };

        // Create scan line pattern
        for (int i = 0; i < 10; i++)
        {
            var position = i / 9.0;
            var alpha = (i % 2 == 0) ? (byte)(40 * intensity) : (byte)(10 * intensity);
            scanLineBrush.GradientStops.Add(new GradientStop(
                Color.FromArgb(alpha, 100, 200, 255), position));
        }

        var scanLineMaterial = new EmissiveMaterial(scanLineBrush);
        materialGroup.Children.Add(scanLineMaterial);
    }

    private void AddCircuitPulse(MaterialGroup materialGroup, double intensity)
    {
        // Add pulsing circuit pattern
        var colors = EVEColorSchemeHelper.GetColors(_colorScheme);
        var pulseBrush = new SolidColorBrush(Color.FromArgb(
            (byte)(80 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B));

        var pulseAnimation = new DoubleAnimation
        {
            From = 0.2 * intensity,
            To = 1.0 * intensity,
            Duration = TimeSpan.FromSeconds(2),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };
        pulseBrush.BeginAnimation(Brush.OpacityProperty, pulseAnimation);

        var pulseMaterial = new EmissiveMaterial(pulseBrush);
        materialGroup.Children.Add(pulseMaterial);
    }

    private void AddEngineThrust(MaterialGroup materialGroup, double intensity)
    {
        // Add engine thrust glow
        var thrustBrush = new RadialGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb((byte)(200 * intensity), 255, 150, 0), 0),
                new GradientStop(Color.FromArgb((byte)(100 * intensity), 255, 50, 0), 0.5),
                new GradientStop(Color.FromArgb(0, 255, 0, 0), 1)
            }
        };

        var thrustAnimation = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(100),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };
        thrustBrush.BeginAnimation(RadialGradientBrush.RadiusXProperty, thrustAnimation);
        thrustBrush.BeginAnimation(RadialGradientBrush.RadiusYProperty, thrustAnimation);

        var thrustMaterial = new EmissiveMaterial(thrustBrush);
        materialGroup.Children.Add(thrustMaterial);
    }

    private void AddShieldFlicker(MaterialGroup materialGroup, double intensity)
    {
        // Add shield energy flicker
        var colors = EVEColorSchemeHelper.GetColors(_colorScheme);
        var flickerBrush = new SolidColorBrush(Color.FromArgb(
            (byte)(120 * intensity), colors.Primary.R, colors.Primary.G, colors.Primary.B));

        var flickerAnimation = new DoubleAnimation
        {
            From = 0.3 * intensity,
            To = 1.0 * intensity,
            Duration = TimeSpan.FromMilliseconds(300),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true
        };
        flickerBrush.BeginAnimation(Brush.OpacityProperty, flickerAnimation);

        var flickerMaterial = new EmissiveMaterial(flickerBrush);
        materialGroup.Children.Add(flickerMaterial);
    }

    private void AddHolographicShimmer(MaterialGroup materialGroup, double intensity)
    {
        // Add holographic shimmer effect
        var shimmerBrush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0),
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 0),
                new GradientStop(Color.FromArgb((byte)(100 * intensity), 255, 255, 255), 0.5),
                new GradientStop(Color.FromArgb(0, 255, 255, 255), 1)
            }
        };

        var shimmerAnimation = new PointAnimation
        {
            From = new Point(-0.5, 0),
            To = new Point(1.5, 0),
            Duration = TimeSpan.FromSeconds(3),
            RepeatBehavior = RepeatBehavior.Forever
        };
        shimmerBrush.BeginAnimation(LinearGradientBrush.StartPointProperty, shimmerAnimation);

        var shimmerEndAnimation = new PointAnimation
        {
            From = new Point(0.5, 0),
            To = new Point(2, 0),
            Duration = TimeSpan.FromSeconds(3),
            RepeatBehavior = RepeatBehavior.Forever
        };
        shimmerBrush.BeginAnimation(LinearGradientBrush.EndPointProperty, shimmerEndAnimation);

        var shimmerMaterial = new EmissiveMaterial(shimmerBrush);
        materialGroup.Children.Add(shimmerMaterial);
    }

    #endregion

    #region Texture Loading

    private async Task LoadDefaultShipTexturesAsync()
    {
        var defaultTextures = new[]
        {
            "ship_hull_metal.jpg",
            "ship_armor_plating.jpg",
            "ship_electronics.jpg",
            "ship_engine_glow.jpg"
        };

        foreach (var textureName in defaultTextures)
        {
            await LoadTextureAsync($"pack://application:,,,/Resources/Textures/{textureName}");
        }
    }

    private async Task LoadDefaultMaterialTexturesAsync()
    {
        var materialTextures = new[]
        {
            "hologram_scanlines.png",
            "circuit_pattern.png",
            "energy_field.png",
            "metal_normal.jpg"
        };

        foreach (var textureName in materialTextures)
        {
            await LoadTextureAsync($"pack://application:,,,/Resources/Materials/{textureName}");
        }
    }

    private async Task<BitmapImage> LoadTextureAsync(string texturePath)
    {
        if (_textureCache.TryGetValue(texturePath, out var cachedTexture))
        {
            return cachedTexture;
        }

        try
        {
            var texture = new BitmapImage();
            
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    texture.BeginInit();
                    texture.UriSource = new Uri(texturePath, UriKind.RelativeOrAbsolute);
                    texture.CacheOption = BitmapCacheOption.OnLoad;
                    texture.EndInit();
                    texture.Freeze(); // Make it thread-safe
                });
            });

            _textureCache[texturePath] = texture;
            return texture;
        }
        catch
        {
            // Return null if texture loading fails
            return null;
        }
    }

    #endregion

    #region Helper Methods

    private Color GetModuleSlotColor(ModuleSlotType slotType)
    {
        return slotType switch
        {
            ModuleSlotType.High => Color.FromRgb(255, 100, 100),
            ModuleSlotType.Medium => Color.FromRgb(255, 255, 100),
            ModuleSlotType.Low => Color.FromRgb(100, 255, 100),
            ModuleSlotType.Rig => Color.FromRgb(100, 100, 255),
            ModuleSlotType.Subsystem => Color.FromRgb(255, 100, 255),
            _ => Color.FromRgb(150, 150, 150)
        };
    }

    private Color DarkenColor(Color color, double factor)
    {
        return Color.FromArgb(color.A,
            (byte)(color.R * (1 - factor)),
            (byte)(color.G * (1 - factor)),
            (byte)(color.B * (1 - factor)));
    }

    private Color LightenColor(Color color, double factor)
    {
        return Color.FromArgb(color.A,
            (byte)(color.R + (255 - color.R) * factor),
            (byte)(color.G + (255 - color.G) * factor),
            (byte)(color.B + (255 - color.B) * factor));
    }

    private Color BlendColors(Color color1, Color color2, double ratio)
    {
        return Color.FromArgb(
            (byte)(color1.A * (1 - ratio) + color2.A * ratio),
            (byte)(color1.R * (1 - ratio) + color2.R * ratio),
            (byte)(color1.G * (1 - ratio) + color2.G * ratio),
            (byte)(color1.B * (1 - ratio) + color2.B * ratio));
    }

    #endregion

    #region Public Methods

    public void SetColorScheme(EVEColorScheme colorScheme)
    {
        _colorScheme = colorScheme;
        ClearMaterialCache();
    }

    public void SetHolographicIntensity(double intensity)
    {
        _holographicIntensity = Math.Max(0, Math.Min(1, intensity));
        ClearMaterialCache();
    }

    public void ClearMaterialCache()
    {
        _materialCache.Clear();
    }

    public void ClearTextureCache()
    {
        _textureCache.Clear();
    }

    public void StartAnimations()
    {
        if (!_isSimplifiedMode)
        {
            _animationTimer.Start();
        }
    }

    public void StopAnimations()
    {
        _animationTimer.Stop();
    }

    #endregion

    #region Event Handlers

    private void OnAnimationTimerTick(object sender, EventArgs e)
    {
        if (!_isSimplifiedMode)
        {
            _animationPhase += 0.1;
            if (_animationPhase > Math.PI * 2)
                _animationPhase = 0;

            // Update animated materials if needed
            UpdateAnimatedMaterials();
        }
    }

    private void UpdateAnimatedMaterials()
    {
        // Update any time-based material animations
        // This method can be used for complex material animations that require manual updates
    }

    #endregion

    #region IAdaptiveControl Implementation

    public bool IsInSimplifiedMode => _isSimplifiedMode;

    public void EnterSimplifiedMode()
    {
        _isSimplifiedMode = true;
        StopAnimations();
        ClearMaterialCache(); // Recreate materials without animations
    }

    public void ExitSimplifiedMode()
    {
        _isSimplifiedMode = false;
        ClearMaterialCache(); // Recreate materials with animations
        StartAnimations();
    }

    #endregion

    #region IAnimationIntensityTarget Implementation

    public bool IsValid => true;

    public void ApplyIntensitySettings(AnimationIntensitySettings settings)
    {
        if (settings == null) return;

        SetHolographicIntensity(settings.MasterIntensity * settings.GlowIntensity);
        
        if (!settings.EnabledFeatures.HasFlag(AnimationFeatures.MaterialAnimations))
        {
            EnterSimplifiedMode();
        }
        else
        {
            ExitSimplifiedMode();
        }
    }

    #endregion
}

#region Supporting Enums

public enum ShipMaterialType
{
    Hull,
    Armor,
    Shield,
    Engine,
    Weapon,
    Electronics,
    Holographic,
    Wireframe
}

#endregion
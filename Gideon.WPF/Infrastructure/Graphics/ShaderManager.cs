// ==========================================================================
// ShaderManager.cs - HLSL Shader Management System
// ==========================================================================
// Manages loading, compilation, and application of HLSL shaders for the
// holographic UI system. Provides high-level interface for shader effects.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media;
using Microsoft.Extensions.Logging;

namespace Gideon.WPF.Infrastructure.Graphics;

/// <summary>
/// Manages HLSL shaders for the holographic UI system
/// </summary>
public class ShaderManager : IDisposable
{
    private readonly ILogger<ShaderManager> _logger;
    private readonly Dictionary<string, Effect> _loadedShaders = new();
    private readonly Dictionary<string, Uri> _shaderUris = new();
    private bool _disposed;

    public ShaderManager(ILogger<ShaderManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        InitializeShaderUris();
    }

    /// <summary>
    /// Initialize the URIs for built-in shaders
    /// </summary>
    private void InitializeShaderUris()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name;

        _shaderUris = new Dictionary<string, Uri>
        {
            ["HolographicGlow"] = new Uri($"pack://application:,,,/{assemblyName};component/Resources/Shaders/Holographic/HolographicGlow.fx"),
            ["Glassmorphism"] = new Uri($"pack://application:,,,/{assemblyName};component/Resources/Shaders/Holographic/Glassmorphism.fx"),
            ["ParticleSystem"] = new Uri($"pack://application:,,,/{assemblyName};component/Resources/Shaders/Effects/ParticleSystem.fx")
        };

        _logger.LogInformation("Initialized {Count} shader URIs", _shaderUris.Count);
    }

    /// <summary>
    /// Load a shader by name
    /// </summary>
    public Effect? LoadShader(string shaderName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ShaderManager));

        if (string.IsNullOrEmpty(shaderName))
            throw new ArgumentException("Shader name cannot be null or empty", nameof(shaderName));

        // Check if already loaded
        if (_loadedShaders.TryGetValue(shaderName, out var cachedShader))
        {
            _logger.LogDebug("Retrieved cached shader: {ShaderName}", shaderName);
            return cachedShader;
        }

        try
        {
            Effect? shader = null;

            // Try to load from built-in URIs first
            if (_shaderUris.TryGetValue(shaderName, out var uri))
            {
                shader = LoadShaderFromUri(uri);
            }

            // Try to load from file system as fallback
            if (shader == null)
            {
                var shaderPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                    "Resources", "Shaders", $"{shaderName}.fx");

                if (File.Exists(shaderPath))
                {
                    shader = LoadShaderFromFile(shaderPath);
                }
            }

            if (shader != null)
            {
                _loadedShaders[shaderName] = shader;
                _logger.LogInformation("Successfully loaded shader: {ShaderName}", shaderName);
                return shader;
            }

            _logger.LogWarning("Shader not found: {ShaderName}", shaderName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load shader: {ShaderName}", shaderName);
            return null;
        }
    }

    /// <summary>
    /// Load shader from URI (embedded resource)
    /// </summary>
    private Effect? LoadShaderFromUri(Uri uri)
    {
        try
        {
            var resourceInfo = Application.GetResourceStream(uri);
            if (resourceInfo?.Stream == null)
            {
                _logger.LogWarning("Resource stream not found for URI: {Uri}", uri);
                return null;
            }

            using var stream = resourceInfo.Stream;
            return LoadShaderFromStream(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load shader from URI: {Uri}", uri);
            return null;
        }
    }

    /// <summary>
    /// Load shader from file path
    /// </summary>
    private Effect? LoadShaderFromFile(string filePath)
    {
        try
        {
            using var stream = File.OpenRead(filePath);
            return LoadShaderFromStream(stream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load shader from file: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// Load shader from stream
    /// </summary>
    private Effect? LoadShaderFromStream(Stream stream)
    {
        try
        {
            // For now, we'll create a basic shader effect
            // In a full implementation, this would compile the HLSL code
            // and create the appropriate WPF Effect subclass
            
            // This is a placeholder - real implementation would need
            // custom Effect classes that inherit from ShaderEffect
            return new BlurEffect(); // Temporary placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create shader effect from stream");
            return null;
        }
    }

    /// <summary>
    /// Create a holographic glow effect
    /// </summary>
    public HolographicGlowEffect? CreateHolographicGlow()
    {
        try
        {
            return new HolographicGlowEffect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create holographic glow effect");
            return null;
        }
    }

    /// <summary>
    /// Create a glassmorphism effect
    /// </summary>
    public GlassmorphismEffect? CreateGlassmorphism()
    {
        try
        {
            return new GlassmorphismEffect();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create glassmorphism effect");
            return null;
        }
    }

    /// <summary>
    /// Get all available shader names
    /// </summary>
    public IEnumerable<string> GetAvailableShaderNames()
    {
        return _shaderUris.Keys;
    }

    /// <summary>
    /// Clear the shader cache
    /// </summary>
    public void ClearCache()
    {
        foreach (var shader in _loadedShaders.Values)
        {
            if (shader is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _loadedShaders.Clear();
        _logger.LogInformation("Shader cache cleared");
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public (int LoadedCount, int AvailableCount) GetCacheStats()
    {
        return (_loadedShaders.Count, _shaderUris.Count);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            ClearCache();
            _disposed = true;
            _logger.LogInformation("ShaderManager disposed");
        }
    }
}

/// <summary>
/// Holographic glow shader effect
/// </summary>
public class HolographicGlowEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty =
        ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(HolographicGlowEffect), 0);

    public static readonly DependencyProperty GlowIntensityProperty =
        DependencyProperty.Register("GlowIntensity", typeof(double), typeof(HolographicGlowEffect),
            new PropertyMetadata(1.0, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty GlowRadiusProperty =
        DependencyProperty.Register("GlowRadius", typeof(double), typeof(HolographicGlowEffect),
            new PropertyMetadata(10.0, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty PulseFrequencyProperty =
        DependencyProperty.Register("PulseFrequency", typeof(double), typeof(HolographicGlowEffect),
            new PropertyMetadata(2.0, PixelShaderConstantCallback(3)));

    public static readonly DependencyProperty GlowColorProperty =
        DependencyProperty.Register("GlowColor", typeof(Color), typeof(HolographicGlowEffect),
            new PropertyMetadata(Color.FromArgb(255, 0, 212, 255), PixelShaderConstantCallback(5)));

    public HolographicGlowEffect()
    {
        // In a real implementation, this would load the compiled HLSL bytecode
        // PixelShader = new PixelShader { UriSource = new Uri("pack://application:,,,/Shaders/HolographicGlow.ps") };
        
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(GlowIntensityProperty);
        UpdateShaderValue(GlowRadiusProperty);
        UpdateShaderValue(PulseFrequencyProperty);
        UpdateShaderValue(GlowColorProperty);
    }

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public double GlowIntensity
    {
        get => (double)GetValue(GlowIntensityProperty);
        set => SetValue(GlowIntensityProperty, value);
    }

    public double GlowRadius
    {
        get => (double)GetValue(GlowRadiusProperty);
        set => SetValue(GlowRadiusProperty, value);
    }

    public double PulseFrequency
    {
        get => (double)GetValue(PulseFrequencyProperty);
        set => SetValue(PulseFrequencyProperty, value);
    }

    public Color GlowColor
    {
        get => (Color)GetValue(GlowColorProperty);
        set => SetValue(GlowColorProperty, value);
    }
}

/// <summary>
/// Glassmorphism shader effect
/// </summary>
public class GlassmorphismEffect : ShaderEffect
{
    public static readonly DependencyProperty InputProperty =
        ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GlassmorphismEffect), 0);

    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.Register("BlurRadius", typeof(double), typeof(GlassmorphismEffect),
            new PropertyMetadata(15.0, PixelShaderConstantCallback(1)));

    public static readonly DependencyProperty GlassThicknessProperty =
        DependencyProperty.Register("GlassThickness", typeof(double), typeof(GlassmorphismEffect),
            new PropertyMetadata(0.1, PixelShaderConstantCallback(2)));

    public static readonly DependencyProperty OpacityProperty =
        DependencyProperty.Register("Opacity", typeof(double), typeof(GlassmorphismEffect),
            new PropertyMetadata(0.8, PixelShaderConstantCallback(7)));

    public GlassmorphismEffect()
    {
        // In a real implementation, this would load the compiled HLSL bytecode
        // PixelShader = new PixelShader { UriSource = new Uri("pack://application:,,,/Shaders/Glassmorphism.ps") };
        
        UpdateShaderValue(InputProperty);
        UpdateShaderValue(BlurRadiusProperty);
        UpdateShaderValue(GlassThicknessProperty);
        UpdateShaderValue(OpacityProperty);
    }

    public Brush Input
    {
        get => (Brush)GetValue(InputProperty);
        set => SetValue(InputProperty, value);
    }

    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public double GlassThickness
    {
        get => (double)GetValue(GlassThicknessProperty);
        set => SetValue(GlassThicknessProperty, value);
    }

    public new double Opacity
    {
        get => (double)GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }
}
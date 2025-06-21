// ==========================================================================
// Glassmorphism.fx - Advanced Glassmorphism Effect Shader
// ==========================================================================
// Creates sophisticated glassmorphism effects with depth blur, refraction,
// and dynamic frost patterns for the holographic UI system.
//
// Features:
// - Multi-layer depth blur
// - Refraction simulation
// - Dynamic frost/crystal patterns
// - Color separation (chromatic aberration)
// - Ambient occlusion simulation
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

#if HLSL_VERSION >= 400
#define PS_VERSION ps_4_0
#define VS_VERSION vs_4_0
#else
#define PS_VERSION ps_3_0
#define VS_VERSION vs_3_0
#endif

// ==========================================================================
// EXTERNAL PARAMETERS
// ==========================================================================

// Time for animation
float Time : register(C0);

// Glass properties
float BlurRadius : register(C1) = 15.0;
float GlassThickness : register(C2) = 0.1;
float RefractionStrength : register(C3) = 0.02;
float FrostIntensity : register(C4) = 0.3;

// Color and transparency
float4 GlassColor : register(C5) = float4(1.0, 1.0, 1.0, 0.15);
float4 TintColor : register(C6) = float4(0.0, 0.83, 1.0, 0.1); // EVE Cyan tint
float Opacity : register(C7) = 0.8;

// Advanced effects
float ChromaticAberration : register(C8) = 0.005;
float DepthFactor : register(C9) = 1.0;
float NoiseScale : register(C10) = 2.0;

// Texture samplers
sampler2D BackgroundTexture : register(S0);  // Content behind glass
sampler2D NoiseTexture : register(S1);       // Noise for frost patterns
sampler2D NormalMap : register(S2);          // Surface normal variations

// ==========================================================================
// VERTEX SHADER STRUCTURES
// ==========================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float Depth : TEXCOORD3;
};

// ==========================================================================
// UTILITY FUNCTIONS
// ==========================================================================

// Multi-octave noise for complex patterns
float ComplexNoise(float2 uv, float scale, float time)
{
    float2 animatedUV = uv * scale + time * 0.1;
    
    float noise = tex2D(NoiseTexture, animatedUV).r;
    noise += tex2D(NoiseTexture, animatedUV * 2.0 + time * 0.05).r * 0.5;
    noise += tex2D(NoiseTexture, animatedUV * 4.0 + time * 0.02).r * 0.25;
    noise += tex2D(NoiseTexture, animatedUV * 8.0 + time * 0.01).r * 0.125;
    
    return noise / 1.875; // Normalize
}

// Frost pattern generation
float FrostPattern(float2 uv, float time)
{
    float noise = ComplexNoise(uv, NoiseScale, time);
    
    // Create crystalline patterns
    float crystal = sin(uv.x * 50.0 + noise * 10.0) * sin(uv.y * 50.0 + noise * 10.0);
    crystal = abs(crystal);
    crystal = pow(crystal, 0.5);
    
    // Add branching patterns
    float branches = sin(uv.x * 20.0 + time * 0.5) * cos(uv.y * 20.0 + time * 0.3);
    branches = abs(branches);
    
    return saturate(crystal * branches * noise * FrostIntensity);
}

// Depth-based blur calculation
float CalculateBlurRadius(float depth)
{
    return BlurRadius * (1.0 + depth * DepthFactor);
}

// Multi-sample blur for glassmorphism effect
float4 GlassmorphismBlur(sampler2D tex, float2 uv, float radius)
{
    float4 result = float4(0, 0, 0, 0);
    float totalWeight = 0.0;
    
    // Gaussian-like sampling pattern
    const int samples = 16;
    const float pi = 3.14159265359;
    
    for (int i = 0; i < samples; i++)
    {
        float angle = (float(i) / float(samples)) * 2.0 * pi;
        float2 offset = float2(cos(angle), sin(angle)) * radius * 0.01;
        
        // Sample with varying distances for depth effect
        for (int j = 1; j <= 3; j++)
        {
            float2 sampleUV = uv + offset * float(j) / 3.0;
            float weight = 1.0 / float(j);
            
            result += tex2D(tex, sampleUV) * weight;
            totalWeight += weight;
        }
    }
    
    return result / totalWeight;
}

// ==========================================================================
// VERTEX SHADER
// ==========================================================================

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    output.Position = input.Position;
    output.Color = input.Color;
    output.TextureCoordinate = input.TextureCoordinate;
    output.ScreenPosition = input.TextureCoordinate;
    output.WorldNormal = input.Normal;
    output.Depth = input.Position.z / input.Position.w;
    
    return output;
}

// ==========================================================================
// PIXEL SHADER - GLASSMORPHISM EFFECT
// ==========================================================================

float4 GlassmorphismPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float depth = input.Depth;
    
    // Sample surface normal for refraction
    float3 surfaceNormal = tex2D(NormalMap, uv).rgb * 2.0 - 1.0;
    
    // Calculate refraction offset
    float2 refractionOffset = surfaceNormal.xy * RefractionStrength * GlassThickness;
    
    // Add time-based distortion for dynamic effect
    float timeDistortion = sin(Time * 2.0 + uv.x * 10.0) * cos(Time * 1.5 + uv.y * 8.0) * 0.001;
    refractionOffset += timeDistortion;
    
    // Sample background with refraction
    float2 refractedUV = uv + refractionOffset;
    
    // Calculate blur radius based on depth
    float blurRadius = CalculateBlurRadius(depth);
    
    // Sample blurred background
    float4 blurredBackground = GlassmorphismBlur(BackgroundTexture, refractedUV, blurRadius);
    
    // Generate frost pattern
    float frost = FrostPattern(uv, Time);
    
    // Create chromatic aberration effect
    float4 redChannel = GlassmorphismBlur(BackgroundTexture, refractedUV + float2(ChromaticAberration, 0), blurRadius);
    float4 blueChannel = GlassmorphismBlur(BackgroundTexture, refractedUV - float2(ChromaticAberration, 0), blurRadius);
    
    // Combine chromatic channels
    blurredBackground.r = redChannel.r;
    blurredBackground.b = blueChannel.b;
    
    // Apply glass color tinting
    float4 tintedBackground = blurredBackground * (1.0 - GlassColor.a) + GlassColor * GlassColor.a;
    tintedBackground = tintedBackground * (1.0 - TintColor.a) + TintColor * TintColor.a;
    
    // Add frost overlay
    float4 frostColor = float4(1.0, 1.0, 1.0, frost);
    tintedBackground = lerp(tintedBackground, frostColor, frost * 0.3);
    
    // Calculate edge highlights for glass thickness simulation
    float edgeHighlight = 1.0 - saturate(length(surfaceNormal.xy));
    edgeHighlight = pow(edgeHighlight, 4.0);
    
    // Add edge lighting
    float4 edgeColor = TintColor * edgeHighlight * 2.0;
    tintedBackground += edgeColor;
    
    // Apply final opacity
    tintedBackground.a = Opacity;
    
    return tintedBackground;
}

// ==========================================================================
// PIXEL SHADER - DEPTH BLUR ONLY
// ==========================================================================

float4 DepthBlurPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float depth = input.Depth;
    
    // Calculate blur radius based on depth
    float blurRadius = CalculateBlurRadius(depth);
    
    // Apply blur
    float4 blurredBackground = GlassmorphismBlur(BackgroundTexture, uv, blurRadius);
    
    // Apply minimal tinting
    blurredBackground = lerp(blurredBackground, GlassColor, GlassColor.a * 0.5);
    
    return blurredBackground;
}

// ==========================================================================
// PIXEL SHADER - FROST OVERLAY
// ==========================================================================

float4 FrostOverlayPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    
    // Generate frost pattern
    float frost = FrostPattern(uv, Time);
    
    // Create frost color with transparency
    float4 frostColor = float4(1.0, 1.0, 1.0, frost * FrostIntensity);
    
    // Add subtle blue tint to frost
    frostColor.rgb = lerp(frostColor.rgb, TintColor.rgb, 0.2);
    
    return frostColor;
}

// ==========================================================================
// TECHNIQUES
// ==========================================================================

technique Glassmorphism
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION GlassmorphismPixelShader();
        
        // Enable alpha blending
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
    }
}

technique DepthBlur
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION DepthBlurPixelShader();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
    }
}

technique FrostOverlay
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION FrostOverlayPixelShader();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;
    }
}
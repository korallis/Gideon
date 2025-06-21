// ==========================================================================
// HolographicGlow.fx - Holographic Glow Effect Shader
// ==========================================================================
// Creates dynamic holographic glow effects with pulse animation and
// edge enhancement for the Westworld-EVE fusion UI aesthetic.
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

// Shader Model 4.0+ for advanced features
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

// Time-based animation
float Time : register(C0);

// Glow properties
float GlowIntensity : register(C1) = 1.0;
float GlowRadius : register(C2) = 10.0;
float PulseFrequency : register(C3) = 2.0;
float EdgeSharpness : register(C4) = 2.0;

// Color properties
float4 GlowColor : register(C5) = float4(0.0, 0.83, 1.0, 1.0); // EVE Cyan
float4 CoreColor : register(C6) = float4(1.0, 1.0, 1.0, 0.8);  // White core

// Animation properties
float AnimationSpeed : register(C7) = 1.0;
float NoiseScale : register(C8) = 0.5;

// Texture samplers
sampler2D InputTexture : register(S0);
sampler2D NoiseTexture : register(S1);

// ==========================================================================
// VERTEX SHADER STRUCTURES
// ==========================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 ScreenPosition : TEXCOORD1;
};

// ==========================================================================
// UTILITY FUNCTIONS
// ==========================================================================

// Smooth pulse function for organic-feeling animation
float SmoothPulse(float time, float frequency)
{
    float pulse = sin(time * frequency * 3.14159) * 0.5 + 0.5;
    return smoothstep(0.0, 1.0, pulse);
}

// Enhanced noise function for holographic distortion
float Noise(float2 uv, float scale)
{
    float2 noiseUV = uv * scale + Time * AnimationSpeed * 0.1;
    float noise = tex2D(NoiseTexture, noiseUV).r;
    
    // Layer multiple octaves for more complex noise
    noiseUV = uv * scale * 2.0 + Time * AnimationSpeed * 0.05;
    noise += tex2D(NoiseTexture, noiseUV).r * 0.5;
    
    noiseUV = uv * scale * 4.0 + Time * AnimationSpeed * 0.02;
    noise += tex2D(NoiseTexture, noiseUV).r * 0.25;
    
    return noise / 1.75; // Normalize
}

// Distance field function for smooth edge glow
float DistanceField(float2 uv, float2 center)
{
    float distance = length(uv - center);
    return 1.0 - smoothstep(0.0, GlowRadius * 0.01, distance);
}

// Holographic scanline effect
float Scanlines(float2 uv, float frequency)
{
    float scanline = sin(uv.y * frequency + Time * AnimationSpeed * 10.0) * 0.1 + 0.9;
    return scanline;
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
    
    return output;
}

// ==========================================================================
// PIXEL SHADER - HOLOGRAPHIC GLOW
// ==========================================================================

float4 HolographicGlowPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float4 originalColor = tex2D(InputTexture, uv);
    
    // Calculate distance from center for radial glow
    float2 center = float2(0.5, 0.5);
    float distanceFromCenter = length(uv - center);
    
    // Create pulsing animation
    float pulse = SmoothPulse(Time, PulseFrequency);
    float animatedIntensity = GlowIntensity * (0.7 + 0.3 * pulse);
    
    // Generate holographic noise for organic feel
    float noise = Noise(uv, NoiseScale);
    float noiseInfluence = 0.2 + 0.1 * noise;
    
    // Create edge glow effect
    float edgeGlow = 1.0 - smoothstep(0.0, GlowRadius * 0.01, distanceFromCenter);
    edgeGlow = pow(edgeGlow, EdgeSharpness);
    
    // Apply scanline effect for holographic feel
    float scanlines = Scanlines(uv, 200.0);
    
    // Combine glow effects
    float glowStrength = edgeGlow * animatedIntensity * noiseInfluence * scanlines;
    
    // Create gradient from core to edge
    float coreInfluence = 1.0 - smoothstep(0.0, 0.3, distanceFromCenter);
    float4 finalGlowColor = lerp(GlowColor, CoreColor, coreInfluence);
    
    // Calculate final glow
    float4 glowEffect = finalGlowColor * glowStrength;
    
    // Blend with original texture
    float4 result = originalColor + glowEffect;
    
    // Apply slight chromatic aberration for holographic effect
    float aberration = 0.002 * glowStrength;
    float4 redChannel = tex2D(InputTexture, uv + float2(aberration, 0));
    float4 blueChannel = tex2D(InputTexture, uv - float2(aberration, 0));
    
    result.r = lerp(result.r, redChannel.r, 0.3);
    result.b = lerp(result.b, blueChannel.b, 0.3);
    
    // Preserve alpha channel
    result.a = max(originalColor.a, glowEffect.a);
    
    return result;
}

// ==========================================================================
// PIXEL SHADER - EDGE ENHANCEMENT
// ==========================================================================

float4 EdgeEnhancementPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float4 originalColor = tex2D(InputTexture, uv);
    
    // Sample neighboring pixels for edge detection
    float texelSize = 1.0 / 512.0; // Assuming 512x512 texture
    
    float4 left = tex2D(InputTexture, uv + float2(-texelSize, 0));
    float4 right = tex2D(InputTexture, uv + float2(texelSize, 0));
    float4 up = tex2D(InputTexture, uv + float2(0, -texelSize));
    float4 down = tex2D(InputTexture, uv + float2(0, texelSize));
    
    // Calculate edge strength using Sobel operator
    float edgeX = abs(left.r - right.r) + abs(left.g - right.g) + abs(left.b - right.b);
    float edgeY = abs(up.r - down.r) + abs(up.g - down.g) + abs(up.b - down.b);
    float edgeStrength = sqrt(edgeX * edgeX + edgeY * edgeY);
    
    // Create animated edge glow
    float pulse = SmoothPulse(Time, PulseFrequency);
    float4 edgeColor = GlowColor * edgeStrength * GlowIntensity * (0.8 + 0.2 * pulse);
    
    // Blend edge enhancement with original
    float4 result = originalColor + edgeColor;
    result.a = originalColor.a;
    
    return result;
}

// ==========================================================================
// TECHNIQUES
// ==========================================================================

technique HolographicGlow
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION HolographicGlowPixelShader();
    }
}

technique EdgeEnhancement
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
        PixelShader = compile PS_VERSION EdgeEnhancementPixelShader();
    }
}
// ==========================================================================
// ParticleSystem.fx - Advanced Particle System Shader
// ==========================================================================
// Creates animated particle effects for data visualization in the 
// Westworld-EVE fusion interface including data streams, market flows,
// and holographic projections.
//
// Features:
// - GPU-based particle animation
// - Dynamic color gradients
// - Velocity-based effects
// - Data-driven particle properties
// - Multiple particle types (stream, burst, orbit)
//
// Author: Gideon Development Team
// Created: June 21, 2025
// ==========================================================================

#if HLSL_VERSION >= 400
#define PS_VERSION ps_4_0
#define VS_VERSION vs_4_0
#define GS_VERSION gs_4_0
#else
#define PS_VERSION ps_3_0
#define VS_VERSION vs_3_0
#endif

// ==========================================================================
// EXTERNAL PARAMETERS
// ==========================================================================

// Time and animation
float Time : register(C0);
float DeltaTime : register(C1);

// Particle system properties
float4x4 ViewProjectionMatrix : register(C2);
float4x4 WorldMatrix : register(C6);

// Particle appearance
float ParticleSize : register(C10) = 2.0;
float AlphaMultiplier : register(C11) = 1.0;
float VelocityInfluence : register(C12) = 0.5;

// Color properties
float4 StartColor : register(C13) = float4(0.0, 0.83, 1.0, 1.0);  // EVE Cyan
float4 EndColor : register(C14) = float4(1.0, 0.84, 0.0, 0.0);    // EVE Gold, fade to transparent
float4 HighlightColor : register(C15) = float4(1.0, 1.0, 1.0, 1.0); // White highlights

// Physics simulation
float3 Gravity : register(C16) = float3(0, -9.8, 0);
float3 WindForce : register(C17) = float3(0, 0, 0);
float Damping : register(C18) = 0.98;

// Data visualization properties
float DataValue : register(C19) = 0.5;      // 0-1 range for data-driven effects
float FlowDirection : register(C20) = 1.0;   // 1 or -1 for flow direction
float Turbulence : register(C21) = 0.1;      // Chaos factor

// Texture samplers
sampler2D ParticleTexture : register(S0);
sampler2D NoiseTexture : register(S1);
sampler2D GradientTexture : register(S2);

// ==========================================================================
// VERTEX SHADER STRUCTURES
// ==========================================================================

struct ParticleVertex
{
    float3 Position : POSITION0;
    float3 Velocity : TEXCOORD0;
    float4 Color : COLOR0;
    float2 Size : TEXCOORD1;          // x=current size, y=target size
    float2 Life : TEXCOORD2;          // x=current life, y=max life
    float2 Data : TEXCOORD3;          // x=data value, y=particle type
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 ParticleData : TEXCOORD1;  // x=life ratio, y=data value
    float3 Velocity : TEXCOORD2;
    float Size : PSIZE;
};

#if HLSL_VERSION >= 400
struct GeometryShaderOutput
{
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 ParticleData : TEXCOORD1;
    float3 Velocity : TEXCOORD2;
};
#endif

// ==========================================================================
// UTILITY FUNCTIONS
// ==========================================================================

// Smooth noise function for particle perturbation
float3 SmoothNoise3D(float3 position, float time)
{
    float3 animatedPos = position + time * 0.1;
    
    // Sample noise texture at different scales
    float noise1 = tex2Dlod(NoiseTexture, float4(animatedPos.xy * 0.1, 0, 0)).r;
    float noise2 = tex2Dlod(NoiseTexture, float4(animatedPos.yz * 0.2, 0, 0)).g;
    float noise3 = tex2Dlod(NoiseTexture, float4(animatedPos.xz * 0.15, 0, 0)).b;
    
    return float3(noise1, noise2, noise3) * 2.0 - 1.0;
}

// Color interpolation based on particle life and data
float4 CalculateParticleColor(float lifeRatio, float dataValue, float3 velocity)
{
    // Base color interpolation
    float4 baseColor = lerp(StartColor, EndColor, lifeRatio);
    
    // Data-driven color modification
    float4 dataColor = tex2D(GradientTexture, float2(dataValue, 0.5));
    baseColor = lerp(baseColor, dataColor, 0.3);
    
    // Velocity-based highlighting
    float velocityMagnitude = length(velocity) * 0.1;
    velocityMagnitude = saturate(velocityMagnitude);
    baseColor = lerp(baseColor, HighlightColor, velocityMagnitude * VelocityInfluence);
    
    // Apply alpha based on life
    baseColor.a *= (1.0 - lifeRatio) * AlphaMultiplier;
    
    return baseColor;
}

// Particle size calculation with data influence
float CalculateParticleSize(float baseSize, float lifeRatio, float dataValue)
{
    // Size based on life (grow, then shrink)
    float lifeCurve = sin(lifeRatio * 3.14159);
    
    // Data influence on size
    float dataInfluence = 0.5 + dataValue * 1.5;
    
    return baseSize * lifeCurve * dataInfluence * ParticleSize;
}

// ==========================================================================
// VERTEX SHADER
// ==========================================================================

VertexShaderOutput VertexShaderFunction(ParticleVertex input)
{
    VertexShaderOutput output;
    
    // Calculate life ratio
    float lifeRatio = input.Life.x / input.Life.y;
    float dataValue = input.Data.x;
    
    // Apply physics simulation
    float3 position = input.Position;
    float3 velocity = input.Velocity;
    
    // Add turbulence
    float3 noise = SmoothNoise3D(position, Time) * Turbulence;
    velocity += noise;
    
    // Apply forces
    velocity += Gravity * DeltaTime;
    velocity += WindForce * DeltaTime;
    velocity *= Damping;
    
    // Update position
    position += velocity * DeltaTime;
    
    // Transform to screen space
    float4 worldPosition = mul(float4(position, 1.0), WorldMatrix);
    output.Position = mul(worldPosition, ViewProjectionMatrix);
    
    // Calculate particle properties
    output.Color = CalculateParticleColor(lifeRatio, dataValue, velocity);
    output.Size = CalculateParticleSize(input.Size.x, lifeRatio, dataValue);
    output.TextureCoordinate = float2(0, 0); // Will be set in geometry shader
    output.ParticleData = float2(lifeRatio, dataValue);
    output.Velocity = velocity;
    
    return output;
}

// ==========================================================================
// GEOMETRY SHADER (Shader Model 4.0+)
// ==========================================================================

#if HLSL_VERSION >= 400
[maxvertexcount(4)]
void GeometryShaderFunction(point VertexShaderOutput input[1], inout TriangleStream<GeometryShaderOutput> outputStream)
{
    GeometryShaderOutput output;
    
    float size = input[0].Size;
    float4 center = input[0].Position;
    
    // Create billboard quad
    float2 offsets[4] = {
        float2(-1, -1),
        float2( 1, -1),
        float2(-1,  1),
        float2( 1,  1)
    };
    
    float2 texCoords[4] = {
        float2(0, 1),
        float2(1, 1),
        float2(0, 0),
        float2(1, 0)
    };
    
    [unroll]
    for (int i = 0; i < 4; i++)
    {
        output.Position = center + float4(offsets[i] * size, 0, 0);
        output.Color = input[0].Color;
        output.TextureCoordinate = texCoords[i];
        output.ParticleData = input[0].ParticleData;
        output.Velocity = input[0].Velocity;
        
        outputStream.Append(output);
    }
}
#endif

// ==========================================================================
// PIXEL SHADER - STANDARD PARTICLES
// ==========================================================================

float4 StandardParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float lifeRatio = input.ParticleData.x;
    float dataValue = input.ParticleData.y;
    
    // Sample particle texture
    float4 particleTexture = tex2D(ParticleTexture, uv);
    
    // Apply color
    float4 finalColor = particleTexture * input.Color;
    
    // Add soft edge falloff
    float2 center = uv - 0.5;
    float distance = length(center);
    float softness = 1.0 - smoothstep(0.3, 0.5, distance);
    
    finalColor.a *= softness;
    
    return finalColor;
}

// ==========================================================================
// PIXEL SHADER - DATA STREAM PARTICLES
// ==========================================================================

float4 DataStreamPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float lifeRatio = input.ParticleData.x;
    float dataValue = input.ParticleData.y;
    
    // Create streaming effect
    float stream = sin(uv.x * 10.0 + Time * 20.0 * FlowDirection) * 0.5 + 0.5;
    stream *= sin(uv.y * 5.0 + Time * 15.0) * 0.5 + 0.5;
    
    // Data-driven intensity
    stream *= dataValue;
    
    // Create core and edge
    float2 center = uv - 0.5;
    float distance = length(center);
    float core = 1.0 - smoothstep(0.1, 0.3, distance);
    float edge = 1.0 - smoothstep(0.3, 0.5, distance);
    
    // Combine effects
    float4 finalColor = input.Color;
    finalColor.a *= (core + edge * 0.3) * stream;
    
    // Add velocity-based streaking
    float3 velocity = normalize(input.Velocity);
    float velocityDot = abs(dot(velocity.xy, center));
    finalColor.a *= 1.0 + velocityDot * 2.0;
    
    return finalColor;
}

// ==========================================================================
// PIXEL SHADER - ENERGY BURST PARTICLES
// ==========================================================================

float4 EnergyBurstPixelShader(VertexShaderOutput input) : COLOR0
{
    float2 uv = input.TextureCoordinate;
    float lifeRatio = input.ParticleData.x;
    float dataValue = input.ParticleData.y;
    
    // Create burst pattern
    float2 center = uv - 0.5;
    float angle = atan2(center.y, center.x);
    float distance = length(center);
    
    // Radial burst lines
    float burst = sin(angle * 8.0 + Time * 10.0) * 0.5 + 0.5;
    burst *= 1.0 - smoothstep(0.0, 0.4, distance);
    
    // Pulsing core
    float pulse = sin(Time * 15.0 + dataValue * 10.0) * 0.3 + 0.7;
    float core = 1.0 - smoothstep(0.0, 0.2, distance);
    
    // Combine effects
    float4 finalColor = input.Color;
    finalColor.a *= (burst + core * pulse) * (1.0 - lifeRatio * 0.5);
    
    return finalColor;
}

// ==========================================================================
// TECHNIQUES
// ==========================================================================

technique StandardParticles
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
#if HLSL_VERSION >= 400
        GeometryShader = compile GS_VERSION GeometryShaderFunction();
#endif
        PixelShader = compile PS_VERSION StandardParticlePixelShader();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = One;  // Additive blending for glow
        ZWriteEnable = false;
    }
}

technique DataStreamParticles
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
#if HLSL_VERSION >= 400
        GeometryShader = compile GS_VERSION GeometryShaderFunction();
#endif
        PixelShader = compile PS_VERSION DataStreamPixelShader();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = One;
        ZWriteEnable = false;
    }
}

technique EnergyBurstParticles
{
    pass Pass1
    {
        VertexShader = compile VS_VERSION VertexShaderFunction();
#if HLSL_VERSION >= 400
        GeometryShader = compile GS_VERSION GeometryShaderFunction();
#endif
        PixelShader = compile PS_VERSION EnergyBurstPixelShader();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = One;
        ZWriteEnable = false;
    }
}
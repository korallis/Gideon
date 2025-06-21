/**
 * PBR Material System
 * Advanced material management for EVE Online ship visualization
 */

import {
  Scene,
  PBRMaterial,
  StandardMaterial,
  Material,
  Texture,
  CubeTexture,
  Color3,
  Vector3,
  AbstractMesh,
  NodeMaterial,
  BaseTexture,
} from '@babylonjs/core';

export interface MaterialPreset {
  name: string;
  description: string;
  baseColor: Color3;
  metallicFactor: number;
  roughnessFactor: number;
  emissiveColor: Color3;
  emissiveIntensity: number;
  specularColor?: Color3;
  specularPower?: number;
  bumpIntensity?: number;
  environmentIntensity?: number;
}

export interface MaterialConfig {
  preset?: string;
  baseColor?: Color3;
  metallicFactor?: number;
  roughnessFactor?: number;
  emissiveColor?: Color3;
  emissiveIntensity?: number;
  enableEnvironmentMap?: boolean;
  enableNormalMap?: boolean;
  enableEmissiveMap?: boolean;
  enableMetallicRoughnessMap?: boolean;
}

export interface TextureSet {
  albedo?: Texture;
  normal?: Texture;
  metallic?: Texture;
  roughness?: Texture;
  metallicRoughness?: Texture;
  emissive?: Texture;
  occlusion?: Texture;
}

export class MaterialSystem {
  private scene: Scene;
  private environmentTexture: CubeTexture | null = null;
  private materials = new Map<string, Material>();
  private presets = new Map<string, MaterialPreset>();

  constructor(scene: Scene) {
    this.scene = scene;
    this.initializePresets();
  }

  /**
   * Initialize EVE Online material presets
   */
  private initializePresets(): void {
    // Amarr - Golden, religious aesthetic
    this.presets.set('amarr_hull', {
      name: 'Amarr Hull',
      description: 'Golden religious hull material',
      baseColor: new Color3(0.8, 0.7, 0.4),
      metallicFactor: 0.9,
      roughnessFactor: 0.2,
      emissiveColor: new Color3(0.4, 0.3, 0.1),
      emissiveIntensity: 0.3,
      environmentIntensity: 1.2,
    });

    // Caldari - Corporate, sleek technology
    this.presets.set('caldari_hull', {
      name: 'Caldari Hull',
      description: 'Sleek corporate technology',
      baseColor: new Color3(0.6, 0.7, 0.8),
      metallicFactor: 0.8,
      roughnessFactor: 0.3,
      emissiveColor: new Color3(0.1, 0.2, 0.4),
      emissiveIntensity: 0.2,
      environmentIntensity: 1.0,
    });

    // Gallente - Organic, curved designs
    this.presets.set('gallente_hull', {
      name: 'Gallente Hull',
      description: 'Organic curved hull design',
      baseColor: new Color3(0.4, 0.6, 0.4),
      metallicFactor: 0.7,
      roughnessFactor: 0.4,
      emissiveColor: new Color3(0.1, 0.3, 0.1),
      emissiveIntensity: 0.2,
      environmentIntensity: 0.9,
    });

    // Minmatar - Rust, industrial aesthetic
    this.presets.set('minmatar_hull', {
      name: 'Minmatar Hull',
      description: 'Industrial rust and metal',
      baseColor: new Color3(0.6, 0.4, 0.3),
      metallicFactor: 0.6,
      roughnessFactor: 0.7,
      emissiveColor: new Color3(0.2, 0.1, 0.0),
      emissiveIntensity: 0.1,
      environmentIntensity: 0.8,
    });

    // Energy/Shield materials
    this.presets.set('shield_energy', {
      name: 'Shield Energy',
      description: 'Shield bubble effect',
      baseColor: new Color3(0.2, 0.4, 1.0),
      metallicFactor: 0.0,
      roughnessFactor: 0.1,
      emissiveColor: new Color3(0.2, 0.6, 1.0),
      emissiveIntensity: 0.8,
      environmentIntensity: 0.3,
    });

    // Engine effects
    this.presets.set('engine_glow', {
      name: 'Engine Glow',
      description: 'Engine thrust glow',
      baseColor: new Color3(1.0, 0.6, 0.2),
      metallicFactor: 0.0,
      roughnessFactor: 0.0,
      emissiveColor: new Color3(1.0, 0.8, 0.4),
      emissiveIntensity: 2.0,
      environmentIntensity: 0.1,
    });

    // Module materials
    this.presets.set('module_standard', {
      name: 'Standard Module',
      description: 'Standard ship module',
      baseColor: new Color3(0.5, 0.5, 0.6),
      metallicFactor: 0.8,
      roughnessFactor: 0.4,
      emissiveColor: new Color3(0.1, 0.1, 0.2),
      emissiveIntensity: 0.1,
      environmentIntensity: 1.0,
    });

    // Weapon hardpoints
    this.presets.set('weapon_hardpoint', {
      name: 'Weapon Hardpoint',
      description: 'Weapon mounting point',
      baseColor: new Color3(0.3, 0.3, 0.4),
      metallicFactor: 0.9,
      roughnessFactor: 0.2,
      emissiveColor: new Color3(0.4, 0.1, 0.1),
      emissiveIntensity: 0.3,
      environmentIntensity: 1.1,
    });
  }

  /**
   * Set up environment mapping for realistic reflections
   */
  async setupEnvironmentMapping(): Promise<void> {
    try {
      // Create space environment cube texture
      this.environmentTexture = CubeTexture.CreateFromPrefilteredData(
        this.generateSpaceEnvironmentData(),
        this.scene
      );
      
      this.environmentTexture.name = 'spaceEnvironment';
      this.environmentTexture.gammaSpace = false;
      
      // Set as scene environment
      this.scene.environmentTexture = this.environmentTexture;
      this.scene.environmentIntensity = 0.8;

      console.log('Environment mapping set up successfully');
    } catch (error) {
      console.warn('Failed to set up environment mapping:', error);
      // Fallback to procedural environment
      this.createProceduralEnvironment();
    }
  }

  /**
   * Generate space environment data
   */
  private generateSpaceEnvironmentData(): string {
    // In production, this would load actual space environment textures
    // For now, return empty data to trigger fallback
    return '';
  }

  /**
   * Create procedural space environment as fallback
   */
  private createProceduralEnvironment(): void {
    // Create a simple procedural space environment
    const size = 512;
    const environmentTexture = new CubeTexture.CreateFromImages([
      this.generateSpaceFace(size), // +X
      this.generateSpaceFace(size), // -X  
      this.generateSpaceFace(size), // +Y
      this.generateSpaceFace(size), // -Y
      this.generateSpaceFace(size), // +Z
      this.generateSpaceFace(size), // -Z
    ], this.scene);

    this.environmentTexture = environmentTexture;
    this.scene.environmentTexture = environmentTexture;
  }

  /**
   * Generate a single face of space environment
   */
  private generateSpaceFace(size: number): string {
    const canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    const ctx = canvas.getContext('2d')!;

    // Create space background
    const gradient = ctx.createRadialGradient(size/2, size/2, 0, size/2, size/2, size/2);
    gradient.addColorStop(0, '#001122');
    gradient.addColorStop(1, '#000511');
    
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, size, size);

    // Add stars
    for (let i = 0; i < 200; i++) {
      const x = Math.random() * size;
      const y = Math.random() * size;
      const brightness = Math.random();
      
      ctx.fillStyle = `rgba(255, 255, 255, ${brightness})`;
      ctx.fillRect(x, y, 1, 1);
    }

    return canvas.toDataURL();
  }

  /**
   * Create PBR material with EVE aesthetic
   */
  createPBRMaterial(
    name: string,
    config: MaterialConfig = {},
    textures?: TextureSet
  ): PBRMaterial {
    const material = new PBRMaterial(name, this.scene);

    // Apply preset if specified
    if (config.preset && this.presets.has(config.preset)) {
      this.applyPreset(material, config.preset);
    }

    // Override with custom config
    if (config.baseColor) material.baseColor = config.baseColor;
    if (config.metallicFactor !== undefined) material.metallicFactor = config.metallicFactor;
    if (config.roughnessFactor !== undefined) material.roughnessFactor = config.roughnessFactor;
    if (config.emissiveColor) material.emissiveColor = config.emissiveColor;
    if (config.emissiveIntensity !== undefined) material.emissiveIntensity = config.emissiveIntensity;

    // Apply textures
    if (textures) {
      this.applyTextures(material, textures);
    }

    // Environment mapping
    if (config.enableEnvironmentMap !== false && this.environmentTexture) {
      material.environmentTexture = this.environmentTexture;
      material.environmentIntensity = 0.8;
    }

    // EVE-specific enhancements
    this.enhanceForEVE(material);

    // Store material
    this.materials.set(name, material);

    return material;
  }

  /**
   * Create standard material as fallback
   */
  createStandardMaterial(
    name: string,
    config: MaterialConfig = {},
    textures?: TextureSet
  ): StandardMaterial {
    const material = new StandardMaterial(name, this.scene);

    // Apply preset if specified
    if (config.preset && this.presets.has(config.preset)) {
      const preset = this.presets.get(config.preset)!;
      material.diffuseColor = config.baseColor || preset.baseColor;
      material.emissiveColor = config.emissiveColor || preset.emissiveColor;
      material.specularColor = preset.specularColor || new Color3(1, 1, 1);
      material.specularPower = preset.specularPower || 64;
    }

    // Apply textures (convert for StandardMaterial)
    if (textures) {
      if (textures.albedo) material.diffuseTexture = textures.albedo;
      if (textures.normal) material.bumpTexture = textures.normal;
      if (textures.emissive) material.emissiveTexture = textures.emissive;
    }

    // Environment reflection
    if (this.environmentTexture) {
      material.reflectionTexture = this.environmentTexture;
      material.reflectionTexture.coordinatesMode = Texture.SKYBOX_MODE;
      material.reflectionFresnelParameters = {
        bias: 0.1,
        power: 0.5,
        leftColor: Color3.White(),
        rightColor: Color3.Black()
      };
    }

    // Store material
    this.materials.set(name, material);

    return material;
  }

  /**
   * Apply material preset
   */
  private applyPreset(material: PBRMaterial, presetName: string): void {
    const preset = this.presets.get(presetName);
    if (!preset) return;

    material.baseColor = preset.baseColor;
    material.metallicFactor = preset.metallicFactor;
    material.roughnessFactor = preset.roughnessFactor;
    material.emissiveColor = preset.emissiveColor;
    material.emissiveIntensity = preset.emissiveIntensity;
    
    if (preset.environmentIntensity !== undefined) {
      material.environmentIntensity = preset.environmentIntensity;
    }
  }

  /**
   * Apply texture set to material
   */
  private applyTextures(material: PBRMaterial, textures: TextureSet): void {
    if (textures.albedo) {
      material.baseTexture = textures.albedo;
    }

    if (textures.normal) {
      material.bumpTexture = textures.normal;
      material.invertNormalMapX = false;
      material.invertNormalMapY = false;
    }

    if (textures.metallicRoughness) {
      material.metallicRoughnessTexture = textures.metallicRoughness;
    } else {
      if (textures.metallic) {
        material.metallicTexture = textures.metallic;
      }
      if (textures.roughness) {
        material.roughnessTexture = textures.roughness;
      }
    }

    if (textures.emissive) {
      material.emissiveTexture = textures.emissive;
    }

    if (textures.occlusion) {
      material.occlusionTexture = textures.occlusion;
      material.occlusionStrength = 1.0;
    }
  }

  /**
   * Enhance material for EVE Online aesthetic
   */
  private enhanceForEVE(material: PBRMaterial): void {
    // Enhanced metallic workflow
    material.useMetallnessFromMetallicTextureBlue = true;
    material.useRoughnessFromMetallicTextureGreen = true;
    material.useRoughnessFromMetallicTextureAlpha = false;

    // Improved lighting response
    material.enableSpecularAntiAliasing = true;
    
    // Real-time reflections
    material.realTimeFiltering = true;
    material.realTimeFilteringQuality = 8;

    // Subsurface scattering for organic parts
    if (material.baseColor.g > material.baseColor.r && material.baseColor.g > material.baseColor.b) {
      // Greenish materials (Gallente) get subsurface
      material.subSurface.isScatteringEnabled = true;
      material.subSurface.scatteringStrength = 0.3;
    }

    // Enhanced clearcoat for Amarr materials
    if (material.baseColor.r > 0.6 && material.metallicFactor > 0.8) {
      material.clearCoat.isEnabled = true;
      material.clearCoat.intensity = 0.5;
      material.clearCoat.roughness = 0.1;
    }

    // Sheen for fabric-like surfaces
    if (material.roughnessFactor > 0.6) {
      material.sheen.isEnabled = true;
      material.sheen.intensity = 0.3;
      material.sheen.color = new Color3(1, 1, 1);
    }
  }

  /**
   * Apply material to mesh with automatic variant selection
   */
  applyMaterialToMesh(mesh: AbstractMesh, materialType: string, variant?: string): void {
    const materialName = variant ? `${materialType}_${variant}` : materialType;
    
    let material = this.materials.get(materialName);
    
    if (!material) {
      // Create material based on mesh properties
      const config: MaterialConfig = { preset: materialType };
      material = this.createPBRMaterial(materialName, config);
    }

    mesh.material = material;
  }

  /**
   * Create material variants for different ship conditions
   */
  createMaterialVariants(baseMaterialName: string): void {
    const baseMaterial = this.materials.get(baseMaterialName);
    if (!baseMaterial || !(baseMaterial instanceof PBRMaterial)) return;

    // Damaged variant
    const damagedMaterial = baseMaterial.clone(`${baseMaterialName}_damaged`);
    damagedMaterial.roughnessFactor = Math.min(damagedMaterial.roughnessFactor + 0.3, 1.0);
    damagedMaterial.metallicFactor = Math.max(damagedMaterial.metallicFactor - 0.2, 0.0);
    damagedMaterial.emissiveIntensity *= 0.5;
    this.materials.set(`${baseMaterialName}_damaged`, damagedMaterial);

    // Powered variant (enhanced emissions)
    const poweredMaterial = baseMaterial.clone(`${baseMaterialName}_powered`);
    poweredMaterial.emissiveIntensity *= 1.5;
    poweredMaterial.emissiveColor.scaleInPlace(1.2);
    this.materials.set(`${baseMaterialName}_powered`, poweredMaterial);

    // Unpowered variant
    const unpoweredMaterial = baseMaterial.clone(`${baseMaterialName}_unpowered`);
    unpoweredMaterial.emissiveIntensity = 0;
    unpoweredMaterial.emissiveColor = Color3.Black();
    this.materials.set(`${baseMaterialName}_unpowered`, unpoweredMaterial);
  }

  /**
   * Update material properties in real-time
   */
  updateMaterialProperty(materialName: string, property: string, value: any): void {
    const material = this.materials.get(materialName);
    if (!material) return;

    if (material instanceof PBRMaterial) {
      switch (property) {
        case 'emissiveIntensity':
          material.emissiveIntensity = value;
          break;
        case 'metallicFactor':
          material.metallicFactor = value;
          break;
        case 'roughnessFactor':
          material.roughnessFactor = value;
          break;
        case 'baseColor':
          material.baseColor = value;
          break;
        case 'emissiveColor':
          material.emissiveColor = value;
          break;
      }
    }
  }

  /**
   * Get available presets
   */
  getPresets(): Map<string, MaterialPreset> {
    return new Map(this.presets);
  }

  /**
   * Get loaded materials
   */
  getMaterials(): Map<string, Material> {
    return new Map(this.materials);
  }

  /**
   * Dispose of all materials
   */
  dispose(): void {
    this.materials.forEach(material => material.dispose());
    this.materials.clear();

    if (this.environmentTexture) {
      this.environmentTexture.dispose();
      this.environmentTexture = null;
    }

    console.log('MaterialSystem disposed');
  }
}
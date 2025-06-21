/**
 * Advanced Lighting System
 * Manages dynamic lighting for EVE Online ship visualization
 */

import {
  Scene,
  Light,
  HemisphericLight,
  DirectionalLight,
  SpotLight,
  PointLight,
  Vector3,
  Color3,
  ShadowGenerator,
  CascadedShadowGenerator,
  AbstractMesh,
  Animation,
  CircleEase,
  EasingFunction,
} from '@babylonjs/core';

export interface LightConfig {
  intensity?: number;
  color?: Color3;
  position?: Vector3;
  direction?: Vector3;
  range?: number;
  angle?: number;
  exponent?: number;
  enableShadows?: boolean;
  shadowMapSize?: number;
}

export interface LightingPreset {
  name: string;
  description: string;
  ambientIntensity: number;
  ambientColor: Color3;
  mainLightIntensity: number;
  mainLightColor: Color3;
  mainLightDirection: Vector3;
  fillLightIntensity: number;
  fillLightColor: Color3;
  rimLightIntensity?: number;
  rimLightColor?: Color3;
  enableShadows: boolean;
  enableVolumetrics: boolean;
}

export interface DynamicLightingState {
  timeOfDay: number; // 0-1, represents position in lighting cycle
  intensity: number; // Overall lighting intensity multiplier
  temperature: number; // Color temperature (warm to cool)
  contrast: number; // Light/shadow contrast
  enableDynamicShadows: boolean;
  enableVolumetricLighting: boolean;
}

export class LightingSystem {
  private scene: Scene;
  private lights = new Map<string, Light>();
  private shadowGenerators = new Map<string, ShadowGenerator>();
  private lightingPresets = new Map<string, LightingPreset>();
  private dynamicState: DynamicLightingState;
  private animationRunning = false;

  constructor(scene: Scene) {
    this.scene = scene;
    this.initializePresets();
    this.dynamicState = {
      timeOfDay: 0.5,
      intensity: 1.0,
      temperature: 0.5,
      contrast: 1.0,
      enableDynamicShadows: true,
      enableVolumetricLighting: false,
    };
  }

  /**
   * Initialize lighting presets for different scenarios
   */
  private initializePresets(): void {
    // Default space lighting
    this.lightingPresets.set('space_default', {
      name: 'Space Default',
      description: 'Standard deep space lighting',
      ambientIntensity: 0.3,
      ambientColor: new Color3(0.1, 0.15, 0.3),
      mainLightIntensity: 1.2,
      mainLightColor: new Color3(1.0, 0.95, 0.8),
      mainLightDirection: new Vector3(-1, -0.5, -1),
      fillLightIntensity: 0.4,
      fillLightColor: new Color3(0.6, 0.7, 1.0),
      enableShadows: true,
      enableVolumetrics: false,
    });

    // Station/hangar lighting
    this.lightingPresets.set('hangar', {
      name: 'Station Hangar',
      description: 'Indoor station hangar lighting',
      ambientIntensity: 0.8,
      ambientColor: new Color3(0.9, 0.9, 1.0),
      mainLightIntensity: 1.5,
      mainLightColor: new Color3(1.0, 1.0, 1.0),
      mainLightDirection: new Vector3(0, -1, -0.3),
      fillLightIntensity: 0.6,
      fillLightColor: new Color3(0.8, 0.9, 1.0),
      enableShadows: true,
      enableVolumetrics: true,
    });

    // Nebula lighting
    this.lightingPresets.set('nebula', {
      name: 'Nebula',
      description: 'Colorful nebula environment',
      ambientIntensity: 0.6,
      ambientColor: new Color3(0.3, 0.2, 0.5),
      mainLightIntensity: 0.8,
      mainLightColor: new Color3(1.0, 0.7, 0.9),
      mainLightDirection: new Vector3(-0.5, -0.3, -1),
      fillLightIntensity: 0.8,
      fillLightColor: new Color3(0.4, 0.8, 1.0),
      rimLightIntensity: 0.5,
      rimLightColor: new Color3(0.8, 0.3, 1.0),
      enableShadows: false,
      enableVolumetrics: true,
    });

    // Combat/danger lighting
    this.lightingPresets.set('combat', {
      name: 'Combat Alert',
      description: 'High-contrast combat lighting',
      ambientIntensity: 0.2,
      ambientColor: new Color3(0.3, 0.1, 0.1),
      mainLightIntensity: 1.8,
      mainLightColor: new Color3(1.0, 0.8, 0.6),
      mainLightDirection: new Vector3(-1, -1, 0),
      fillLightIntensity: 0.3,
      fillLightColor: new Color3(1.0, 0.3, 0.3),
      enableShadows: true,
      enableVolumetrics: false,
    });

    // Minimalist/technical
    this.lightingPresets.set('technical', {
      name: 'Technical View',
      description: 'Clean technical inspection lighting',
      ambientIntensity: 0.9,
      ambientColor: new Color3(0.95, 0.95, 1.0),
      mainLightIntensity: 1.0,
      mainLightColor: new Color3(1.0, 1.0, 1.0),
      mainLightDirection: new Vector3(-0.5, -1, -0.5),
      fillLightIntensity: 0.8,
      fillLightColor: new Color3(1.0, 1.0, 1.0),
      enableShadows: false,
      enableVolumetrics: false,
    });
  }

  /**
   * Set up complete lighting system
   */
  setupLighting(presetName: string = 'space_default'): void {
    this.clearAllLights();
    
    const preset = this.lightingPresets.get(presetName);
    if (!preset) {
      console.error(`Lighting preset not found: ${presetName}`);
      return;
    }

    // Ambient lighting
    this.createAmbientLight(preset);
    
    // Main directional light (star/sun)
    this.createMainDirectionalLight(preset);
    
    // Fill light for shadow details
    this.createFillLight(preset);
    
    // Optional rim light for silhouette
    if (preset.rimLightIntensity) {
      this.createRimLight(preset);
    }

    // Set up shadows if enabled
    if (preset.enableShadows) {
      this.setupShadows();
    }

    console.log(`Lighting preset applied: ${preset.name}`);
  }

  /**
   * Create ambient hemispheric lighting
   */
  private createAmbientLight(preset: LightingPreset): void {
    const ambientLight = new HemisphericLight(
      'ambientLight',
      new Vector3(0, 1, 0),
      this.scene
    );
    
    ambientLight.intensity = preset.ambientIntensity;
    ambientLight.diffuse = preset.ambientColor;
    ambientLight.specular = preset.ambientColor.scale(0.5);
    ambientLight.groundColor = preset.ambientColor.scale(0.3);

    this.lights.set('ambient', ambientLight);
  }

  /**
   * Create main directional light
   */
  private createMainDirectionalLight(preset: LightingPreset): void {
    const mainLight = new DirectionalLight(
      'mainLight',
      preset.mainLightDirection.normalize(),
      this.scene
    );

    mainLight.intensity = preset.mainLightIntensity;
    mainLight.diffuse = preset.mainLightColor;
    mainLight.specular = preset.mainLightColor;
    
    // Position the light far away in the opposite direction
    mainLight.position = preset.mainLightDirection.scale(-50);

    this.lights.set('main', mainLight);
  }

  /**
   * Create fill light for shadow detail
   */
  private createFillLight(preset: LightingPreset): void {
    // Calculate fill light direction (opposite side of main light)
    const fillDirection = preset.mainLightDirection.scale(-1);
    fillDirection.y = Math.abs(fillDirection.y); // Keep above horizon
    
    const fillLight = new DirectionalLight(
      'fillLight',
      fillDirection.normalize(),
      this.scene
    );

    fillLight.intensity = preset.fillLightIntensity;
    fillLight.diffuse = preset.fillLightColor;
    fillLight.specular = preset.fillLightColor.scale(0.5);
    
    fillLight.position = fillDirection.scale(-30);

    this.lights.set('fill', fillLight);
  }

  /**
   * Create rim light for silhouette enhancement
   */
  private createRimLight(preset: LightingPreset): void {
    if (!preset.rimLightIntensity || !preset.rimLightColor) return;

    // Rim light from behind/side
    const rimDirection = new Vector3(1, 0.5, 1).normalize();
    
    const rimLight = new DirectionalLight(
      'rimLight',
      rimDirection,
      this.scene
    );

    rimLight.intensity = preset.rimLightIntensity;
    rimLight.diffuse = preset.rimLightColor;
    rimLight.specular = preset.rimLightColor;
    
    rimLight.position = rimDirection.scale(-40);

    this.lights.set('rim', rimLight);
  }

  /**
   * Set up shadow generation
   */
  private setupShadows(): void {
    const mainLight = this.lights.get('main') as DirectionalLight;
    if (!mainLight) return;

    // Use cascaded shadow generator for better quality
    const shadowGenerator = new CascadedShadowGenerator(2048, mainLight);
    shadowGenerator.lambda = 0.9;
    shadowGenerator.cascadeBlendPercentage = 0.1;
    shadowGenerator.depthClamp = false;
    shadowGenerator.autoCalcDepthBounds = true;
    
    // Enhanced shadow quality
    shadowGenerator.useExponentialShadowMap = true;
    shadowGenerator.useKernelBlur = true;
    shadowGenerator.blurKernel = 32;
    shadowGenerator.depthScale = 2;

    // Shadow filtering
    shadowGenerator.filteringQuality = ShadowGenerator.QUALITY_HIGH;
    shadowGenerator.contactHardeningLightSizeUVRatio = 0.075;

    this.shadowGenerators.set('main', shadowGenerator);
  }

  /**
   * Add mesh as shadow caster
   */
  addShadowCaster(mesh: AbstractMesh, lightName: string = 'main'): void {
    const shadowGenerator = this.shadowGenerators.get(lightName);
    if (shadowGenerator) {
      shadowGenerator.addShadowCaster(mesh);
    }
  }

  /**
   * Add mesh as shadow receiver
   */
  addShadowReceiver(mesh: AbstractMesh): void {
    mesh.receiveShadows = true;
  }

  /**
   * Create dynamic point lights for effects
   */
  createEffectLight(
    name: string,
    position: Vector3,
    config: LightConfig
  ): PointLight {
    const light = new PointLight(name, position, this.scene);
    
    light.intensity = config.intensity || 1.0;
    light.diffuse = config.color || Color3.White();
    light.specular = config.color || Color3.White();
    light.range = config.range || 50;

    this.lights.set(name, light);
    return light;
  }

  /**
   * Create spot light for focused illumination
   */
  createSpotLight(
    name: string,
    position: Vector3,
    direction: Vector3,
    config: LightConfig
  ): SpotLight {
    const light = new SpotLight(
      name,
      position,
      direction.normalize(),
      config.angle || Math.PI / 3,
      config.exponent || 2,
      this.scene
    );

    light.intensity = config.intensity || 1.0;
    light.diffuse = config.color || Color3.White();
    light.specular = config.color || Color3.White();
    light.range = config.range || 100;

    if (config.enableShadows) {
      const shadowGenerator = new ShadowGenerator(
        config.shadowMapSize || 1024,
        light
      );
      shadowGenerator.useExponentialShadowMap = true;
      this.shadowGenerators.set(name, shadowGenerator);
    }

    this.lights.set(name, light);
    return light;
  }

  /**
   * Update lighting dynamically based on state
   */
  updateDynamicLighting(newState: Partial<DynamicLightingState>): void {
    Object.assign(this.dynamicState, newState);

    // Update light intensities
    this.lights.forEach((light, name) => {
      if (name !== 'ambient') {
        light.intensity *= this.dynamicState.intensity;
      }
    });

    // Update color temperature
    if (this.dynamicState.temperature !== 0.5) {
      this.applyColorTemperature();
    }

    // Update contrast
    if (this.dynamicState.contrast !== 1.0) {
      this.applyContrast();
    }
  }

  /**
   * Apply color temperature changes
   */
  private applyColorTemperature(): void {
    const temperature = this.dynamicState.temperature;
    
    this.lights.forEach((light, name) => {
      if (name === 'main' || name === 'fill') {
        const warmColor = new Color3(1.0, 0.8, 0.6);
        const coolColor = new Color3(0.8, 0.9, 1.0);
        
        light.diffuse = Color3.Lerp(coolColor, warmColor, temperature);
      }
    });
  }

  /**
   * Apply contrast adjustments
   */
  private applyContrast(): void {
    const contrast = this.dynamicState.contrast;
    
    // Adjust main light intensity for contrast
    const mainLight = this.lights.get('main');
    if (mainLight) {
      mainLight.intensity *= contrast;
    }

    // Inversely adjust fill light
    const fillLight = this.lights.get('fill');
    if (fillLight) {
      fillLight.intensity *= (2 - contrast);
    }
  }

  /**
   * Start animated lighting cycle
   */
  startLightingAnimation(duration: number = 30000): void {
    if (this.animationRunning) return;

    this.animationRunning = true;
    
    const mainLight = this.lights.get('main');
    if (!mainLight) return;

    // Create rotation animation for main light
    const animationRotation = Animation.CreateAndStartAnimation(
      'lightRotation',
      mainLight,
      'direction',
      30,
      duration,
      mainLight.direction,
      mainLight.direction,
      Animation.ANIMATIONLOOPMODE_CYCLE
    );

    // Add subtle intensity pulsing
    const animationIntensity = Animation.CreateAndStartAnimation(
      'lightPulse',
      mainLight,
      'intensity',
      60,
      5000,
      mainLight.intensity,
      mainLight.intensity * 1.1,
      Animation.ANIMATIONLOOPMODE_YOYO
    );

    if (animationIntensity) {
      animationIntensity.setEasingFunction(new CircleEase());
    }
  }

  /**
   * Stop lighting animations
   */
  stopLightingAnimation(): void {
    this.animationRunning = false;
    this.scene.stopAnimation();
  }

  /**
   * Get current lighting state
   */
  getLightingState(): DynamicLightingState {
    return { ...this.dynamicState };
  }

  /**
   * Get available presets
   */
  getPresets(): Map<string, LightingPreset> {
    return new Map(this.lightingPresets);
  }

  /**
   * Get active lights
   */
  getLights(): Map<string, Light> {
    return new Map(this.lights);
  }

  /**
   * Remove specific light
   */
  removeLight(name: string): void {
    const light = this.lights.get(name);
    if (light) {
      light.dispose();
      this.lights.delete(name);
    }

    const shadowGenerator = this.shadowGenerators.get(name);
    if (shadowGenerator) {
      shadowGenerator.dispose();
      this.shadowGenerators.delete(name);
    }
  }

  /**
   * Clear all lights
   */
  private clearAllLights(): void {
    this.lights.forEach(light => light.dispose());
    this.lights.clear();

    this.shadowGenerators.forEach(generator => generator.dispose());
    this.shadowGenerators.clear();
  }

  /**
   * Dispose of lighting system
   */
  dispose(): void {
    this.stopLightingAnimation();
    this.clearAllLights();
    console.log('LightingSystem disposed');
  }
}
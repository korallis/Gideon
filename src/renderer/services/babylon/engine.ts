/**
 * Babylon.js Engine Service
 * Manages 3D engine initialization and configuration for ship visualization
 */

import {
  Engine,
  Scene,
  ArcRotateCamera,
  HemisphericLight,
  DirectionalLight,
  Vector3,
  Color3,
  ShadowGenerator,
  UniversalCamera,
  FreeCamera,
} from '@babylonjs/core';
import { SceneLoader } from '@babylonjs/loaders';

export interface BabylonEngineConfig {
  antialias?: boolean;
  adaptToDeviceRatio?: boolean;
  preserveDrawingBuffer?: boolean;
  stencil?: boolean;
  disableWebGL2Support?: boolean;
  audioEngine?: boolean;
}

export interface SceneConfig {
  clearColor?: Color3;
  ambientColor?: Color3;
  usePhysics?: boolean;
  enableShadows?: boolean;
}

export class BabylonEngine {
  private engine: Engine | null = null;
  private scene: Scene | null = null;
  private canvas: HTMLCanvasElement | null = null;
  private camera: ArcRotateCamera | null = null;
  private shadowGenerator: ShadowGenerator | null = null;

  constructor() {
    this.handleResize = this.handleResize.bind(this);
  }

  /**
   * Initialize the Babylon.js engine with the provided canvas
   */
  async initialize(
    canvas: HTMLCanvasElement,
    engineConfig: BabylonEngineConfig = {},
    sceneConfig: SceneConfig = {}
  ): Promise<boolean> {
    try {
      this.canvas = canvas;

      // Create engine with EVE Online optimized settings
      this.engine = new Engine(canvas, true, {
        antialias: engineConfig.antialias ?? true,
        adaptToDeviceRatio: engineConfig.adaptToDeviceRatio ?? true,
        preserveDrawingBuffer: engineConfig.preserveDrawingBuffer ?? true,
        stencil: engineConfig.stencil ?? true,
        disableWebGL2Support: engineConfig.disableWebGL2Support ?? false,
        audioEngine: engineConfig.audioEngine ?? false,
      });

      // Handle resize events
      window.addEventListener('resize', this.handleResize);

      // Create scene
      this.scene = new Scene(this.engine);
      
      // Configure scene for EVE Online aesthetic
      this.scene.clearColor = sceneConfig.clearColor ?? new Color3(0.02, 0.05, 0.1); // Deep space blue
      this.scene.ambientColor = sceneConfig.ambientColor ?? new Color3(0.1, 0.1, 0.2);

      // Set up lighting for ship visualization
      await this.setupLighting();

      // Set up camera for ship inspection
      this.setupCamera();

      // Enable shadows if requested
      if (sceneConfig.enableShadows) {
        this.setupShadows();
      }

      // Start render loop
      this.startRenderLoop();

      console.log('Babylon.js engine initialized successfully');
      return true;
    } catch (error) {
      console.error('Failed to initialize Babylon.js engine:', error);
      return false;
    }
  }

  /**
   * Set up lighting system optimized for EVE ship visualization
   */
  private async setupLighting(): Promise<void> {
    if (!this.scene) return;

    // Ambient lighting for general illumination
    const hemisphericLight = new HemisphericLight(
      'ambientLight',
      new Vector3(0, 1, 0),
      this.scene
    );
    hemisphericLight.intensity = 0.3;
    hemisphericLight.diffuse = new Color3(0.8, 0.9, 1.0); // Cool space lighting

    // Main directional light (simulating distant star)
    const directionalLight = new DirectionalLight(
      'mainLight',
      new Vector3(-1, -1, -1),
      this.scene
    );
    directionalLight.intensity = 1.2;
    directionalLight.diffuse = new Color3(1.0, 0.95, 0.8); // Warm starlight
    directionalLight.specular = new Color3(1.0, 1.0, 1.0);

    // Secondary fill light for ship details
    const fillLight = new DirectionalLight(
      'fillLight',
      new Vector3(1, 0.5, 1),
      this.scene
    );
    fillLight.intensity = 0.5;
    fillLight.diffuse = new Color3(0.6, 0.7, 1.0); // Cool fill light
  }

  /**
   * Set up camera for ship inspection
   */
  private setupCamera(): void {
    if (!this.scene) return;

    // ArcRotate camera for 360-degree ship inspection
    this.camera = new ArcRotateCamera(
      'shipCamera',
      -Math.PI / 2, // Alpha (horizontal rotation)
      Math.PI / 3,  // Beta (vertical rotation)
      50,           // Radius (distance from target)
      Vector3.Zero(), // Target position
      this.scene
    );

    // Configure camera controls for ship inspection
    this.camera.setTarget(Vector3.Zero());
    this.camera.wheelPrecision = 50;
    this.camera.pinchPrecision = 200;
    this.camera.panningSensibility = 1000;
    this.camera.angularSensibilityX = 1000;
    this.camera.angularSensibilityY = 1000;
    
    // Set camera limits
    this.camera.lowerRadiusLimit = 10;
    this.camera.upperRadiusLimit = 200;
    this.camera.lowerBetaLimit = 0.1;
    this.camera.upperBetaLimit = Math.PI - 0.1;

    // Attach camera controls to canvas
    if (this.canvas) {
      this.camera.attachToCanvas(this.canvas, true);
    }
  }

  /**
   * Set up shadow system for enhanced ship visualization
   */
  private setupShadows(): void {
    if (!this.scene) return;

    const directionalLight = this.scene.getLightByName('mainLight') as DirectionalLight;
    if (directionalLight) {
      this.shadowGenerator = new ShadowGenerator(2048, directionalLight);
      this.shadowGenerator.useExponentialShadowMap = true;
      this.shadowGenerator.useKernelBlur = true;
      this.shadowGenerator.blurKernel = 64;
    }
  }

  /**
   * Start the render loop
   */
  private startRenderLoop(): void {
    if (!this.engine || !this.scene) return;

    this.engine.runRenderLoop(() => {
      if (this.scene) {
        this.scene.render();
      }
    });
  }

  /**
   * Handle window resize events
   */
  private handleResize(): void {
    if (this.engine) {
      this.engine.resize();
    }
  }

  /**
   * Focus camera on a specific target
   */
  focusOnTarget(target: Vector3, radius?: number): void {
    if (!this.camera) return;

    this.camera.setTarget(target);
    if (radius !== undefined) {
      this.camera.radius = radius;
    }
  }

  /**
   * Reset camera to default position
   */
  resetCamera(): void {
    if (!this.camera) return;

    this.camera.setTarget(Vector3.Zero());
    this.camera.alpha = -Math.PI / 2;
    this.camera.beta = Math.PI / 3;
    this.camera.radius = 50;
  }

  /**
   * Get the current scene
   */
  getScene(): Scene | null {
    return this.scene;
  }

  /**
   * Get the current engine
   */
  getEngine(): Engine | null {
    return this.engine;
  }

  /**
   * Get the current camera
   */
  getCamera(): ArcRotateCamera | null {
    return this.camera;
  }

  /**
   * Add shadow caster to the shadow generator
   */
  addShadowCaster(mesh: any): void {
    if (this.shadowGenerator) {
      this.shadowGenerator.addShadowCaster(mesh);
    }
  }

  /**
   * Clean up resources
   */
  dispose(): void {
    try {
      // Remove event listeners
      window.removeEventListener('resize', this.handleResize);

      // Dispose shadows
      if (this.shadowGenerator) {
        this.shadowGenerator.dispose();
        this.shadowGenerator = null;
      }

      // Dispose scene
      if (this.scene) {
        this.scene.dispose();
        this.scene = null;
      }

      // Dispose engine
      if (this.engine) {
        this.engine.dispose();
        this.engine = null;
      }

      this.canvas = null;
      this.camera = null;

      console.log('Babylon.js engine disposed successfully');
    } catch (error) {
      console.error('Error disposing Babylon.js engine:', error);
    }
  }
}

// Singleton instance for global access
export const babylonEngine = new BabylonEngine();
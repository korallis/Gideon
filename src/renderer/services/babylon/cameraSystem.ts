/**
 * Advanced Camera System
 * Provides smooth camera controls and scene navigation for ship inspection
 */

import {
  Scene,
  ArcRotateCamera,
  UniversalCamera,
  FreeCamera,
  Vector3,
  Animation,
  BezierCurveEase,
  CircleEase,
  EasingFunction,
  Tools,
  AbstractMesh,
  BoundingInfo,
} from '@babylonjs/core';

export interface CameraPreset {
  name: string;
  description: string;
  alpha: number;
  beta: number;
  radius: number;
  target: Vector3;
  fov?: number;
  animationDuration?: number;
}

export interface CameraControls {
  panSensibility: number;
  wheelPrecision: number;
  pinchPrecision: number;
  angularSensibilityX: number;
  angularSensibilityY: number;
  minZ: number;
  maxZ: number;
  lowerRadiusLimit: number;
  upperRadiusLimit: number;
  lowerBetaLimit: number;
  upperBetaLimit: number;
  enableAutoRotation: boolean;
  autoRotationSpeed: number;
}

export interface CameraState {
  position: Vector3;
  target: Vector3;
  alpha: number;
  beta: number;
  radius: number;
  fov: number;
  isAnimating: boolean;
  currentPreset: string | null;
}

export interface ViewportBookmark {
  id: string;
  name: string;
  description: string;
  cameraState: CameraState;
  timestamp: number;
}

export class CameraSystem {
  private scene: Scene;
  private camera: ArcRotateCamera;
  private presets = new Map<string, CameraPreset>();
  private bookmarks = new Map<string, ViewportBookmark>();
  private currentState: CameraState;
  private controls: CameraControls;
  private autoRotationAnimation: Animation | null = null;
  private isAutoRotating = false;

  constructor(scene: Scene) {
    this.scene = scene;
    this.initializeCamera();
    this.initializePresets();
    this.initializeControls();
    this.currentState = this.getCameraState();
  }

  /**
   * Initialize the arc rotate camera
   */
  private initializeCamera(): void {
    // Create arc rotate camera for ship inspection
    this.camera = new ArcRotateCamera(
      'shipInspectionCamera',
      -Math.PI / 2, // Alpha (horizontal rotation)
      Math.PI / 3,  // Beta (vertical rotation)
      50,           // Radius (distance from target)
      Vector3.Zero(), // Target position
      this.scene
    );

    // Set camera as active
    this.scene.activeCamera = this.camera;
  }

  /**
   * Initialize camera presets for common viewpoints
   */
  private initializePresets(): void {
    // Front view
    this.presets.set('front', {
      name: 'Front View',
      description: 'Front-facing ship view',
      alpha: 0,
      beta: Math.PI / 2,
      radius: 40,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Rear view
    this.presets.set('rear', {
      name: 'Rear View',
      description: 'Rear-facing ship view',
      alpha: Math.PI,
      beta: Math.PI / 2,
      radius: 40,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Top view
    this.presets.set('top', {
      name: 'Top View',
      description: 'Top-down ship view',
      alpha: 0,
      beta: 0.1,
      radius: 60,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Bottom view
    this.presets.set('bottom', {
      name: 'Bottom View',
      description: 'Bottom-up ship view',
      alpha: 0,
      beta: Math.PI - 0.1,
      radius: 60,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Left side view
    this.presets.set('left', {
      name: 'Left Side',
      description: 'Left side ship view',
      alpha: -Math.PI / 2,
      beta: Math.PI / 2,
      radius: 45,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Right side view
    this.presets.set('right', {
      name: 'Right Side',
      description: 'Right side ship view',
      alpha: Math.PI / 2,
      beta: Math.PI / 2,
      radius: 45,
      target: Vector3.Zero(),
      animationDuration: 1000,
    });

    // Isometric view
    this.presets.set('isometric', {
      name: 'Isometric',
      description: '3/4 perspective view',
      alpha: -Math.PI / 4,
      beta: Math.PI / 3,
      radius: 50,
      target: Vector3.Zero(),
      animationDuration: 1200,
    });

    // Close inspection
    this.presets.set('close', {
      name: 'Close Inspection',
      description: 'Close-up detail view',
      alpha: -Math.PI / 6,
      beta: Math.PI / 2.5,
      radius: 20,
      target: Vector3.Zero(),
      animationDuration: 800,
    });

    // Wide overview
    this.presets.set('overview', {
      name: 'Wide Overview',
      description: 'Wide-angle overview',
      alpha: -Math.PI / 3,
      beta: Math.PI / 4,
      radius: 100,
      target: Vector3.Zero(),
      fov: 0.8,
      animationDuration: 1500,
    });
  }

  /**
   * Initialize camera controls
   */
  private initializeControls(): void {
    this.controls = {
      panSensibility: 1000,
      wheelPrecision: 50,
      pinchPrecision: 200,
      angularSensibilityX: 1000,
      angularSensibilityY: 1000,
      minZ: 0.1,
      maxZ: 1000,
      lowerRadiusLimit: 5,
      upperRadiusLimit: 200,
      lowerBetaLimit: 0.1,
      upperBetaLimit: Math.PI - 0.1,
      enableAutoRotation: false,
      autoRotationSpeed: 0.5,
    };

    this.applyControls();
  }

  /**
   * Apply control settings to camera
   */
  private applyControls(): void {
    this.camera.panningSensibility = this.controls.panSensibility;
    this.camera.wheelPrecision = this.controls.wheelPrecision;
    this.camera.pinchPrecision = this.controls.pinchPrecision;
    this.camera.angularSensibilityX = this.controls.angularSensibilityX;
    this.camera.angularSensibilityY = this.controls.angularSensibilityY;
    
    this.camera.minZ = this.controls.minZ;
    this.camera.maxZ = this.controls.maxZ;
    this.camera.lowerRadiusLimit = this.controls.lowerRadiusLimit;
    this.camera.upperRadiusLimit = this.controls.upperRadiusLimit;
    this.camera.lowerBetaLimit = this.controls.lowerBetaLimit;
    this.camera.upperBetaLimit = this.controls.upperBetaLimit;

    // Enable/disable inputs
    this.camera.attachToCanvas(this.scene.getEngine().getRenderingCanvas()!, true);
  }

  /**
   * Apply camera preset with smooth animation
   */
  async applyPreset(presetName: string, customTarget?: Vector3): Promise<void> {
    const preset = this.presets.get(presetName);
    if (!preset) {
      console.error(`Camera preset not found: ${presetName}`);
      return;
    }

    const target = customTarget || preset.target;
    const duration = preset.animationDuration || 1000;

    // Stop any existing animations
    this.scene.stopAnimation(this.camera);

    // Create smooth animations
    const alphaAnimation = Animation.CreateAndStartAnimation(
      'cameraAlpha',
      this.camera,
      'alpha',
      60,
      duration,
      this.camera.alpha,
      preset.alpha,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    const betaAnimation = Animation.CreateAndStartAnimation(
      'cameraBeta',
      this.camera,
      'beta',
      60,
      duration,
      this.camera.beta,
      preset.beta,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    const radiusAnimation = Animation.CreateAndStartAnimation(
      'cameraRadius',
      this.camera,
      'radius',
      60,
      duration,
      this.camera.radius,
      preset.radius,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    // Target animation
    const targetAnimation = Animation.CreateAndStartAnimation(
      'cameraTarget',
      this.camera,
      'target',
      60,
      duration,
      this.camera.getTarget(),
      target,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    // FOV animation if specified
    if (preset.fov && this.camera.fov !== preset.fov) {
      Animation.CreateAndStartAnimation(
        'cameraFov',
        this.camera,
        'fov',
        60,
        duration,
        this.camera.fov,
        preset.fov,
        Animation.ANIMATIONLOOPMODE_CONSTANT
      );
    }

    // Apply easing for smooth motion
    const easing = new BezierCurveEase(0.25, 0.1, 0.25, 1);
    if (alphaAnimation) alphaAnimation.setEasingFunction(easing);
    if (betaAnimation) betaAnimation.setEasingFunction(easing);
    if (radiusAnimation) radiusAnimation.setEasingFunction(easing);
    if (targetAnimation) targetAnimation.setEasingFunction(easing);

    // Update current state
    this.currentState.currentPreset = presetName;
    this.currentState.isAnimating = true;

    // Wait for animation to complete
    return new Promise((resolve) => {
      setTimeout(() => {
        this.currentState.isAnimating = false;
        this.currentState = this.getCameraState();
        resolve();
      }, duration);
    });
  }

  /**
   * Focus camera on specific mesh with optimal framing
   */
  focusOnMesh(mesh: AbstractMesh, animationDuration: number = 1000): void {
    const boundingInfo = mesh.getBoundingInfo();
    const center = boundingInfo.boundingBox.center;
    const size = boundingInfo.boundingBox.maximum.subtract(boundingInfo.boundingBox.minimum);
    
    // Calculate optimal distance based on mesh size
    const maxDimension = Math.max(size.x, size.y, size.z);
    const optimalRadius = maxDimension * 2.5;

    // Create smooth transition
    this.animateToPosition(
      this.camera.alpha,
      this.camera.beta,
      Math.max(optimalRadius, this.controls.lowerRadiusLimit),
      center,
      animationDuration
    );
  }

  /**
   * Animate camera to specific position
   */
  private animateToPosition(
    alpha: number,
    beta: number,
    radius: number,
    target: Vector3,
    duration: number = 1000
  ): void {
    this.scene.stopAnimation(this.camera);

    const alphaAnim = Animation.CreateAndStartAnimation(
      'cameraAlpha',
      this.camera,
      'alpha',
      60,
      duration,
      this.camera.alpha,
      alpha,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    const betaAnim = Animation.CreateAndStartAnimation(
      'cameraBeta',
      this.camera,
      'beta',
      60,
      duration,
      this.camera.beta,
      beta,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    const radiusAnim = Animation.CreateAndStartAnimation(
      'cameraRadius',
      this.camera,
      'radius',
      60,
      duration,
      this.camera.radius,
      radius,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    const targetAnim = Animation.CreateAndStartAnimation(
      'cameraTarget',
      this.camera,
      'target',
      60,
      duration,
      this.camera.getTarget(),
      target,
      Animation.ANIMATIONLOOPMODE_CONSTANT
    );

    // Apply smooth easing
    const easing = new BezierCurveEase(0.25, 0.1, 0.25, 1);
    if (alphaAnim) alphaAnim.setEasingFunction(easing);
    if (betaAnim) betaAnim.setEasingFunction(easing);
    if (radiusAnim) radiusAnim.setEasingFunction(easing);
    if (targetAnim) targetAnim.setEasingFunction(easing);
  }

  /**
   * Start auto-rotation around target
   */
  startAutoRotation(speed: number = 0.5): void {
    if (this.isAutoRotating) {
      this.stopAutoRotation();
    }

    this.autoRotationAnimation = Animation.CreateAndStartAnimation(
      'autoRotation',
      this.camera,
      'alpha',
      60,
      3000 / speed,
      this.camera.alpha,
      this.camera.alpha + 2 * Math.PI,
      Animation.ANIMATIONLOOPMODE_CYCLE
    );

    if (this.autoRotationAnimation) {
      const easing = new CircleEase();
      easing.setEasingMode(EasingFunction.EASINGMODE_EASEINOUT);
      this.autoRotationAnimation.setEasingFunction(easing);
    }

    this.isAutoRotating = true;
  }

  /**
   * Stop auto-rotation
   */
  stopAutoRotation(): void {
    if (this.autoRotationAnimation) {
      this.scene.stopAnimation(this.camera);
      this.autoRotationAnimation = null;
    }
    this.isAutoRotating = false;
  }

  /**
   * Create viewport bookmark
   */
  createBookmark(id: string, name: string, description: string = ''): ViewportBookmark {
    const bookmark: ViewportBookmark = {
      id,
      name,
      description,
      cameraState: this.getCameraState(),
      timestamp: Date.now(),
    };

    this.bookmarks.set(id, bookmark);
    return bookmark;
  }

  /**
   * Apply viewport bookmark
   */
  async applyBookmark(id: string, animationDuration: number = 1000): Promise<void> {
    const bookmark = this.bookmarks.get(id);
    if (!bookmark) {
      console.error(`Bookmark not found: ${id}`);
      return;
    }

    const state = bookmark.cameraState;
    return this.animateToPosition(
      state.alpha,
      state.beta,
      state.radius,
      state.target,
      animationDuration
    );
  }

  /**
   * Update camera controls
   */
  updateControls(newControls: Partial<CameraControls>): void {
    Object.assign(this.controls, newControls);
    this.applyControls();

    if (newControls.enableAutoRotation !== undefined) {
      if (newControls.enableAutoRotation && !this.isAutoRotating) {
        this.startAutoRotation(this.controls.autoRotationSpeed);
      } else if (!newControls.enableAutoRotation && this.isAutoRotating) {
        this.stopAutoRotation();
      }
    }
  }

  /**
   * Reset camera to default position
   */
  reset(animationDuration: number = 1000): void {
    this.animateToPosition(
      -Math.PI / 2,
      Math.PI / 3,
      50,
      Vector3.Zero(),
      animationDuration
    );

    this.currentState.currentPreset = null;
  }

  /**
   * Get current camera state
   */
  getCameraState(): CameraState {
    return {
      position: this.camera.position.clone(),
      target: this.camera.getTarget().clone(),
      alpha: this.camera.alpha,
      beta: this.camera.beta,
      radius: this.camera.radius,
      fov: this.camera.fov,
      isAnimating: this.currentState?.isAnimating || false,
      currentPreset: this.currentState?.currentPreset || null,
    };
  }

  /**
   * Get available presets
   */
  getPresets(): Map<string, CameraPreset> {
    return new Map(this.presets);
  }

  /**
   * Get viewport bookmarks
   */
  getBookmarks(): Map<string, ViewportBookmark> {
    return new Map(this.bookmarks);
  }

  /**
   * Remove bookmark
   */
  removeBookmark(id: string): boolean {
    return this.bookmarks.delete(id);
  }

  /**
   * Get camera controls
   */
  getControls(): CameraControls {
    return { ...this.controls };
  }

  /**
   * Get the camera instance
   */
  getCamera(): ArcRotateCamera {
    return this.camera;
  }

  /**
   * Check if auto-rotating
   */
  isAutoRotating(): boolean {
    return this.isAutoRotating;
  }

  /**
   * Dispose of camera system
   */
  dispose(): void {
    this.stopAutoRotation();
    this.bookmarks.clear();
    this.presets.clear();
    
    if (this.camera) {
      this.camera.dispose();
    }

    console.log('CameraSystem disposed');
  }
}
/**
 * 3D Scene Management System
 * Manages ship models, effects, and scene interactions for EVE Online ship visualization
 */

import {
  Scene,
  Mesh,
  Vector3,
  Color3,
  Animation,
  TransformNode,
  BoundingInfo,
  AbstractMesh,
  Light,
  ParticleSystem,
  Texture,
  StandardMaterial,
  PBRMaterial,
  AnimationGroup,
  AssetContainer,
} from '@babylonjs/core';
import { SceneLoader } from '@babylonjs/loaders';
import { babylonEngine } from './engine';
import { ModelLoader, LoadedModel, ModelLoadOptions, ModelLoadProgress } from './modelLoader';
import { MaterialSystem, MaterialConfig } from './materialSystem';
import { LightingSystem, LightingPreset, DynamicLightingState } from './lightingSystem';

export interface ShipModel {
  id: string;
  name: string;
  mesh: AbstractMesh;
  boundingInfo: BoundingInfo;
  animations?: AnimationGroup[];
  materials: (StandardMaterial | PBRMaterial)[];
}

export interface SceneObject {
  id: string;
  type: 'ship' | 'module' | 'effect' | 'backdrop';
  node: TransformNode;
  visible: boolean;
  interactive: boolean;
}

export interface SceneState {
  activeShip: string | null;
  selectedObjects: string[];
  cameraTarget: Vector3;
  lighting: {
    intensity: number;
    color: Color3;
    shadows: boolean;
  };
  effects: {
    particles: boolean;
    glow: boolean;
    trails: boolean;
  };
}

export class SceneManager {
  private scene: Scene | null = null;
  private modelLoader: ModelLoader | null = null;
  private materialSystem: MaterialSystem | null = null;
  private lightingSystem: LightingSystem | null = null;
  private loadedModels = new Map<string, ShipModel>();
  private sceneObjects = new Map<string, SceneObject>();
  private assetContainers = new Map<string, AssetContainer>();
  private currentState: SceneState;
  private animationGroups = new Map<string, AnimationGroup>();

  constructor() {
    this.currentState = {
      activeShip: null,
      selectedObjects: [],
      cameraTarget: Vector3.Zero(),
      lighting: {
        intensity: 1.0,
        color: new Color3(1, 1, 1),
        shadows: true,
      },
      effects: {
        particles: true,
        glow: true,
        trails: false,
      },
    };
  }

  /**
   * Initialize scene manager with active Babylon.js scene
   */
  initialize(): boolean {
    this.scene = babylonEngine.getScene();
    if (!this.scene) {
      console.error('Cannot initialize SceneManager: No active Babylon.js scene');
      return false;
    }

    // Initialize subsystems
    this.modelLoader = new ModelLoader(this.scene);
    this.materialSystem = new MaterialSystem(this.scene);
    this.lightingSystem = new LightingSystem(this.scene);

    // Setup environment and lighting
    await this.setupEnvironment();
    this.setupEventHandlers();
    
    console.log('SceneManager initialized successfully');
    return true;
  }

  /**
   * Set up the space environment and backdrop
   */
  private async setupEnvironment(): Promise<void> {
    if (!this.scene || !this.materialSystem || !this.lightingSystem) return;

    // Create space backdrop
    this.createSpaceBackdrop();
    
    // Initialize material system with environment mapping
    await this.materialSystem.setupEnvironmentMapping();
    
    // Set up default lighting
    this.lightingSystem.setupLighting('space_default');
  }

  /**
   * Create space backdrop with stars and nebula
   */
  private createSpaceBackdrop(): void {
    if (!this.scene) return;

    // Create skybox for space environment
    const skybox = Mesh.CreateSphere('skybox', 100, 1000, this.scene);
    const skyboxMaterial = new StandardMaterial('skyboxMaterial', this.scene);
    
    skyboxMaterial.backFaceCulling = false;
    skyboxMaterial.diffuseColor = new Color3(0, 0, 0);
    skyboxMaterial.emissiveColor = new Color3(0.02, 0.05, 0.1);
    skybox.material = skyboxMaterial;
    skybox.infiniteDistance = true;

    // Register backdrop as scene object
    const backdropObject: SceneObject = {
      id: 'space-backdrop',
      type: 'backdrop',
      node: skybox,
      visible: true,
      interactive: false,
    };
    this.sceneObjects.set('space-backdrop', backdropObject);
  }

  /**
   * Set up scene event handlers for interactions
   */
  private setupEventHandlers(): void {
    if (!this.scene) return;

    // Handle picking/selection
    this.scene.onPointerObservable.add((pointerInfo) => {
      if (pointerInfo.pickInfo?.hit && pointerInfo.pickInfo.pickedMesh) {
        this.handleMeshSelection(pointerInfo.pickInfo.pickedMesh);
      }
    });

    // Handle scene ready events
    this.scene.onReadyObservable.add(() => {
      console.log('Scene ready for rendering');
    });
  }

  /**
   * Load a ship model from file path with advanced options
   */
  async loadShipModel(
    shipId: string,
    modelPath: string,
    shipName: string,
    options: ModelLoadOptions = {},
    onProgress?: (progress: ModelLoadProgress) => void
  ): Promise<ShipModel | null> {
    if (!this.scene || !this.modelLoader) {
      console.error('Cannot load ship: Scene or ModelLoader not initialized');
      return null;
    }

    try {
      console.log(`Loading ship model: ${shipName} from ${modelPath}`);

      // Use the advanced model loader
      const loadedModel = await this.modelLoader.loadModel(
        shipId,
        modelPath,
        shipName,
        {
          enablePBR: true,
          optimizeForPerformance: true,
          enableAnimations: true,
          ...options
        },
        onProgress
      );

      if (!loadedModel) {
        throw new Error('Failed to load model through ModelLoader');
      }

      // Convert LoadedModel to ShipModel format
      const shipModel: ShipModel = {
        id: shipId,
        name: shipName,
        mesh: loadedModel.rootMesh,
        boundingInfo: loadedModel.boundingInfo,
        animations: loadedModel.animations,
        materials: loadedModel.materials,
      };

      // Store the model
      this.loadedModels.set(shipId, shipModel);

      // Register as scene object
      const sceneObject: SceneObject = {
        id: shipId,
        type: 'ship',
        node: loadedModel.rootMesh,
        visible: true,
        interactive: true,
      };
      this.sceneObjects.set(shipId, sceneObject);

      // Store animations
      loadedModel.animations.forEach((group, index) => {
        this.animationGroups.set(`${shipId}_anim_${index}`, group);
      });

      console.log(`Ship model loaded successfully: ${shipName} (${loadedModel.metadata.polyCount} polygons, ${loadedModel.metadata.loadTime.toFixed(2)}ms)`);
      return shipModel;
    } catch (error) {
      console.error(`Failed to load ship model ${shipName}:`, error);
      return null;
    }
  }


  /**
   * Focus camera on specific ship
   */
  focusOnShip(shipId: string): void {
    const shipModel = this.loadedModels.get(shipId);
    if (!shipModel) {
      console.warn(`Ship not found: ${shipId}`);
      return;
    }

    // Calculate optimal camera position based on ship bounds
    const boundingInfo = shipModel.boundingInfo;
    const center = boundingInfo.boundingBox.center;
    const size = boundingInfo.boundingBox.maximum.subtract(boundingInfo.boundingBox.minimum);
    const maxDimension = Math.max(size.x, size.y, size.z);
    const cameraDistance = maxDimension * 2.5;

    // Focus camera on ship
    babylonEngine.focusOnTarget(center, cameraDistance);
    
    // Update state
    this.currentState.activeShip = shipId;
    this.currentState.cameraTarget = center;

    console.log(`Camera focused on ship: ${shipModel.name}`);
  }

  /**
   * Handle mesh selection for ship interaction
   */
  private handleMeshSelection(mesh: AbstractMesh): void {
    // Find the scene object for this mesh
    for (const [id, sceneObj] of this.sceneObjects) {
      if (sceneObj.node === mesh || sceneObj.node.getChildMeshes().includes(mesh)) {
        this.selectObject(id);
        break;
      }
    }
  }

  /**
   * Select a scene object
   */
  selectObject(objectId: string): void {
    const sceneObject = this.sceneObjects.get(objectId);
    if (!sceneObject || !sceneObject.interactive) return;

    // Clear previous selection
    this.clearSelection();

    // Add to selection
    this.currentState.selectedObjects.push(objectId);

    // Highlight selected object
    this.highlightObject(objectId, true);

    console.log(`Selected object: ${objectId}`);
  }

  /**
   * Clear current selection
   */
  clearSelection(): void {
    // Remove highlighting from previously selected objects
    this.currentState.selectedObjects.forEach(id => {
      this.highlightObject(id, false);
    });

    this.currentState.selectedObjects = [];
  }

  /**
   * Highlight or unhighlight an object
   */
  private highlightObject(objectId: string, highlight: boolean): void {
    const sceneObject = this.sceneObjects.get(objectId);
    if (!sceneObject) return;

    // Apply highlighting effect (can be enhanced with outline effects)
    if (sceneObject.node instanceof AbstractMesh) {
      const mesh = sceneObject.node as AbstractMesh;
      if (mesh.material instanceof StandardMaterial) {
        mesh.material.emissiveColor = highlight 
          ? new Color3(0.3, 0.5, 1.0) 
          : new Color3(0.1, 0.1, 0.2);
      }
    }
  }

  /**
   * Show/hide specific objects
   */
  setObjectVisibility(objectId: string, visible: boolean): void {
    const sceneObject = this.sceneObjects.get(objectId);
    if (!sceneObject) return;

    sceneObject.visible = visible;
    sceneObject.node.setEnabled(visible);
  }

  /**
   * Apply lighting preset
   */
  applyLightingPreset(presetName: string): void {
    if (!this.lightingSystem) return;
    
    this.lightingSystem.setupLighting(presetName);
    console.log(`Applied lighting preset: ${presetName}`);
  }

  /**
   * Update dynamic lighting state
   */
  updateDynamicLighting(state: Partial<DynamicLightingState>): void {
    if (!this.lightingSystem) return;
    
    this.lightingSystem.updateDynamicLighting(state);
  }

  /**
   * Apply material preset to ship
   */
  applyMaterialPreset(shipId: string, presetName: string): void {
    if (!this.materialSystem) return;

    const shipModel = this.loadedModels.get(shipId);
    if (!shipModel) return;

    // Apply material to all meshes in the ship
    const meshes = shipModel.mesh.getChildMeshes();
    meshes.forEach((mesh, index) => {
      this.materialSystem!.applyMaterialToMesh(mesh, presetName, `variant_${index}`);
    });

    // Add shadow casting if shadows are enabled
    if (this.currentState.lighting.shadows && this.lightingSystem) {
      meshes.forEach(mesh => {
        this.lightingSystem!.addShadowCaster(mesh);
        this.lightingSystem!.addShadowReceiver(mesh);
      });
    }
  }

  /**
   * Remove a ship model from the scene
   */
  removeShipModel(shipId: string): void {
    const shipModel = this.loadedModels.get(shipId);
    if (!shipModel) return;

    // Use model loader to properly clean up resources
    if (this.modelLoader) {
      this.modelLoader.removeModel(shipId);
    } else {
      // Fallback cleanup if model loader not available
      shipModel.mesh.dispose();
      shipModel.animations?.forEach(animGroup => {
        animGroup.dispose();
      });
    }

    // Clean up scene manager state
    this.loadedModels.delete(shipId);
    this.sceneObjects.delete(shipId);

    // Clean up animations
    const animKeys = Array.from(this.animationGroups.keys()).filter(key => 
      key.startsWith(`${shipId}_anim_`)
    );
    animKeys.forEach(key => {
      this.animationGroups.delete(key);
    });

    // Update state if this was the active ship
    if (this.currentState.activeShip === shipId) {
      this.currentState.activeShip = null;
    }

    console.log(`Ship model removed: ${shipModel.name}`);
  }

  /**
   * Clear entire scene
   */
  clearScene(): void {
    // Clear all models through model loader
    if (this.modelLoader) {
      this.modelLoader.clearAllModels();
    }

    // Clear scene manager state
    this.loadedModels.clear();
    this.sceneObjects.clear();
    this.animationGroups.clear();

    // Reset state
    this.currentState = {
      activeShip: null,
      selectedObjects: [],
      cameraTarget: Vector3.Zero(),
      lighting: {
        intensity: 1.0,
        color: new Color3(1, 1, 1),
        shadows: true,
      },
      effects: {
        particles: true,
        glow: true,
        trails: false,
      },
    };

    // Reset camera
    babylonEngine.resetCamera();

    console.log('Scene cleared');
  }

  /**
   * Get current scene state
   */
  getSceneState(): SceneState {
    return { ...this.currentState };
  }

  /**
   * Get loaded ship models
   */
  getLoadedModels(): Map<string, ShipModel> {
    return new Map(this.loadedModels);
  }

  /**
   * Get scene objects
   */
  getSceneObjects(): Map<string, SceneObject> {
    return new Map(this.sceneObjects);
  }

  /**
   * Get material system
   */
  getMaterialSystem(): MaterialSystem | null {
    return this.materialSystem;
  }

  /**
   * Get lighting system
   */
  getLightingSystem(): LightingSystem | null {
    return this.lightingSystem;
  }

  /**
   * Get available material presets
   */
  getMaterialPresets() {
    return this.materialSystem?.getPresets() || new Map();
  }

  /**
   * Get available lighting presets
   */
  getLightingPresets() {
    return this.lightingSystem?.getPresets() || new Map();
  }

  /**
   * Get memory usage statistics
   */
  getMemoryStats() {
    if (this.modelLoader) {
      return this.modelLoader.getMemoryStats();
    }
    return {
      totalMemory: 0,
      modelCount: 0,
      models: []
    };
  }

  /**
   * Dispose of all resources
   */
  dispose(): void {
    // Clear scene first
    this.clearScene();

    // Dispose subsystems
    if (this.modelLoader) {
      this.modelLoader.dispose();
      this.modelLoader = null;
    }

    if (this.materialSystem) {
      this.materialSystem.dispose();
      this.materialSystem = null;
    }

    if (this.lightingSystem) {
      this.lightingSystem.dispose();
      this.lightingSystem = null;
    }

    // Clean up any remaining resources
    this.assetContainers.forEach(container => container.dispose());
    this.animationGroups.forEach(group => group.dispose());

    // Clear maps
    this.loadedModels.clear();
    this.sceneObjects.clear();
    this.assetContainers.clear();
    this.animationGroups.clear();

    this.scene = null;
    console.log('SceneManager disposed');
  }
}

// Singleton instance for global access
export const sceneManager = new SceneManager();
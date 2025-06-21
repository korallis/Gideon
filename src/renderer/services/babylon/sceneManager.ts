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
import { CameraSystem, CameraPreset, CameraControls, ViewportBookmark } from './cameraSystem';
import { SelectionSystem, SelectableObject, SelectionOptions, SelectionEvent } from './selectionSystem';

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
  private cameraSystem: CameraSystem | null = null;
  private selectionSystem: SelectionSystem | null = null;
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
    this.cameraSystem = new CameraSystem(this.scene);
    this.selectionSystem = new SelectionSystem(this.scene);

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

    // Handle scene ready events
    this.scene.onReadyObservable.add(() => {
      console.log('Scene ready for rendering');
    });
    
    // Selection handling is now managed by SelectionSystem
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

      // Register ship and its components as selectable objects
      this.registerSelectableObjects(shipId, loadedModel);

      console.log(`Ship model loaded successfully: ${shipName} (${loadedModel.metadata.polyCount} polygons, ${loadedModel.metadata.loadTime.toFixed(2)}ms)`);
      return shipModel;
    } catch (error) {
      console.error(`Failed to load ship model ${shipName}:`, error);
      return null;
    }
  }

  /**
   * Register ship and its components as selectable objects
   */
  private registerSelectableObjects(shipId: string, loadedModel: LoadedModel): void {
    if (!this.selectionSystem) return;

    // Register main ship hull
    this.selectionSystem.registerSelectableObject(
      `${shipId}_hull`,
      loadedModel.rootMesh,
      'ship',
      {
        name: loadedModel.name,
        description: `${loadedModel.name} hull`,
        category: 'hull',
        interactive: true,
        selectable: true,
      }
    );

    // Register individual meshes as ship components
    loadedModel.meshes.forEach((mesh, index) => {
      if (mesh.name && mesh !== loadedModel.rootMesh) {
        const componentType = this.determineComponentType(mesh.name);
        
        this.selectionSystem!.registerSelectableObject(
          `${shipId}_${mesh.name}_${index}`,
          mesh,
          componentType,
          {
            name: mesh.name,
            description: `${mesh.name} component`,
            category: componentType,
            interactive: true,
            selectable: true,
          }
        );
      }
    });

    // Set up selection event handlers
    this.setupSelectionEventHandlers();
  }

  /**
   * Determine component type from mesh name
   */
  private determineComponentType(meshName: string): SelectableObject['type'] {
    const name = meshName.toLowerCase();
    
    if (name.includes('hardpoint') || name.includes('turret') || name.includes('launcher')) {
      return 'hardpoint';
    } else if (name.includes('module') || name.includes('slot') || name.includes('bay')) {
      return 'module';
    } else {
      return 'component';
    }
  }

  /**
   * Set up selection event handlers
   */
  private setupSelectionEventHandlers(): void {
    if (!this.selectionSystem) return;

    // Handle selection events
    this.selectionSystem.addEventListener('select', (event: SelectionEvent) => {
      console.log(`Selected: ${event.object.metadata.name} (${event.object.type})`);
      
      // Focus camera on selected object if requested
      if (this.cameraSystem && event.object.type === 'ship') {
        this.cameraSystem.focusOnMesh(event.object.mesh, 1000);
      }
    });

    this.selectionSystem.addEventListener('hover', (event: SelectionEvent) => {
      // Could show tooltip or info panel here
    });

    this.selectionSystem.addEventListener('deselect', (event: SelectionEvent) => {
      console.log(`Deselected: ${event.object.metadata.name}`);
    });
  }

  /**
   * Focus camera on specific ship
   */
  focusOnShip(shipId: string): void {
    const shipModel = this.loadedModels.get(shipId);
    if (!shipModel || !this.cameraSystem) {
      console.warn(`Ship not found or camera system not initialized: ${shipId}`);
      return;
    }

    // Use camera system to focus on ship mesh
    this.cameraSystem.focusOnMesh(shipModel.mesh, 1200);
    
    // Update state
    this.currentState.activeShip = shipId;
    this.currentState.cameraTarget = shipModel.boundingInfo.boundingBox.center;

    console.log(`Camera focused on ship: ${shipModel.name}`);
  }

  /**
   * Apply camera preset
   */
  applyCameraPreset(presetName: string, targetShipId?: string): void {
    if (!this.cameraSystem) return;

    let target = Vector3.Zero();
    if (targetShipId) {
      const shipModel = this.loadedModels.get(targetShipId);
      if (shipModel) {
        target = shipModel.boundingInfo.boundingBox.center;
      }
    }

    this.cameraSystem.applyPreset(presetName, target);
  }

  /**
   * Update camera controls
   */
  updateCameraControls(controls: Partial<CameraControls>): void {
    if (!this.cameraSystem) return;
    
    this.cameraSystem.updateControls(controls);
  }

  /**
   * Create camera bookmark
   */
  createCameraBookmark(id: string, name: string, description: string = ''): ViewportBookmark | null {
    if (!this.cameraSystem) return null;
    
    return this.cameraSystem.createBookmark(id, name, description);
  }

  /**
   * Apply camera bookmark
   */
  applyCameraBookmark(bookmarkId: string): void {
    if (!this.cameraSystem) return;
    
    this.cameraSystem.applyBookmark(bookmarkId, 1000);
  }

  /**
   * Select object by ID
   */
  selectObject(objectId: string): void {
    if (!this.selectionSystem) return;

    const selectableObjects = this.selectionSystem.getSelectableObjects();
    const object = selectableObjects.get(objectId);
    
    if (object) {
      this.selectionSystem.selectObject(object);
    }
  }

  /**
   * Clear current selection
   */
  clearSelection(): void {
    if (!this.selectionSystem) return;
    
    this.selectionSystem.clearSelection();
  }

  /**
   * Get selected objects
   */
  getSelectedObjects(): SelectableObject[] {
    if (!this.selectionSystem) return [];
    
    return this.selectionSystem.getSelectedObjects();
  }

  /**
   * Focus camera on selected objects
   */
  focusOnSelection(): void {
    if (!this.selectionSystem) return;
    
    this.selectionSystem.focusOnSelection();
  }

  /**
   * Update selection options
   */
  updateSelectionOptions(options: Partial<SelectionOptions>): void {
    if (!this.selectionSystem) return;
    
    this.selectionSystem.updateOptions(options);
  }

  /**
   * Enable/disable selection system
   */
  setSelectionEnabled(enabled: boolean): void {
    if (!this.selectionSystem) return;
    
    this.selectionSystem.setEnabled(enabled);
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
    if (this.cameraSystem) {
      this.cameraSystem.reset();
    }

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
   * Get camera system
   */
  getCameraSystem(): CameraSystem | null {
    return this.cameraSystem;
  }

  /**
   * Get selection system
   */
  getSelectionSystem(): SelectionSystem | null {
    return this.selectionSystem;
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
   * Get available camera presets
   */
  getCameraPresets() {
    return this.cameraSystem?.getPresets() || new Map();
  }

  /**
   * Get camera bookmarks
   */
  getCameraBookmarks() {
    return this.cameraSystem?.getBookmarks() || new Map();
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

    if (this.cameraSystem) {
      this.cameraSystem.dispose();
      this.cameraSystem = null;
    }

    if (this.selectionSystem) {
      this.selectionSystem.dispose();
      this.selectionSystem = null;
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
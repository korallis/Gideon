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

    this.setupEnvironment();
    this.setupEventHandlers();
    console.log('SceneManager initialized successfully');
    return true;
  }

  /**
   * Set up the space environment and backdrop
   */
  private setupEnvironment(): void {
    if (!this.scene) return;

    // Create space backdrop
    this.createSpaceBackdrop();
    
    // Set up environmental lighting
    this.updateLighting();
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
   * Load a ship model from file path
   */
  async loadShipModel(
    shipId: string,
    modelPath: string,
    shipName: string
  ): Promise<ShipModel | null> {
    if (!this.scene) {
      console.error('Cannot load ship: Scene not initialized');
      return null;
    }

    try {
      console.log(`Loading ship model: ${shipName} from ${modelPath}`);

      // Load the model using SceneLoader
      const result = await SceneLoader.ImportMeshAsync(
        '',
        '',
        modelPath,
        this.scene
      );

      if (!result.meshes.length) {
        throw new Error('No meshes found in model file');
      }

      // Get the root mesh
      const rootMesh = result.meshes[0];
      rootMesh.name = `ship_${shipId}`;

      // Calculate bounding info for camera positioning
      const boundingInfo = rootMesh.getBoundingInfo();

      // Process materials for EVE aesthetic
      const materials = this.processMaterials(result.meshes);

      // Store animation groups if any
      if (result.animationGroups?.length) {
        result.animationGroups.forEach((group, index) => {
          this.animationGroups.set(`${shipId}_anim_${index}`, group);
        });
      }

      // Create ship model object
      const shipModel: ShipModel = {
        id: shipId,
        name: shipName,
        mesh: rootMesh,
        boundingInfo,
        animations: result.animationGroups,
        materials,
      };

      // Store the model
      this.loadedModels.set(shipId, shipModel);

      // Register as scene object
      const sceneObject: SceneObject = {
        id: shipId,
        type: 'ship',
        node: rootMesh,
        visible: true,
        interactive: true,
      };
      this.sceneObjects.set(shipId, sceneObject);

      // Store asset container for cleanup
      if (result.meshes.length > 0) {
        const container = this.scene!.createAssetContainer();
        result.meshes.forEach(mesh => container.meshes.push(mesh));
        result.materials.forEach(material => container.materials.push(material));
        result.textures.forEach(texture => container.textures.push(texture));
        this.assetContainers.set(shipId, container);
      }

      console.log(`Ship model loaded successfully: ${shipName}`);
      return shipModel;
    } catch (error) {
      console.error(`Failed to load ship model ${shipName}:`, error);
      return null;
    }
  }

  /**
   * Process and enhance materials for EVE aesthetic
   */
  private processMaterials(meshes: AbstractMesh[]): (StandardMaterial | PBRMaterial)[] {
    const materials: (StandardMaterial | PBRMaterial)[] = [];

    meshes.forEach(mesh => {
      if (mesh.material) {
        // Enhance materials with EVE-style properties
        if (mesh.material instanceof StandardMaterial) {
          mesh.material.specularPower = 64;
          mesh.material.emissiveColor = new Color3(0.1, 0.1, 0.2);
          materials.push(mesh.material);
        } else if (mesh.material instanceof PBRMaterial) {
          mesh.material.metallicFactor = 0.8;
          mesh.material.roughnessFactor = 0.3;
          mesh.material.emissiveColor = new Color3(0.1, 0.1, 0.2);
          materials.push(mesh.material);
        }
      }
    });

    return materials;
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
   * Update scene lighting configuration
   */
  updateLighting(): void {
    if (!this.scene) return;

    const lights = this.scene.lights;
    lights.forEach(light => {
      light.intensity = this.currentState.lighting.intensity;
      if (light.diffuse) {
        light.diffuse = this.currentState.lighting.color;
      }
    });
  }

  /**
   * Remove a ship model from the scene
   */
  removeShipModel(shipId: string): void {
    const shipModel = this.loadedModels.get(shipId);
    if (!shipModel) return;

    // Remove from scene
    shipModel.mesh.dispose();

    // Clean up asset container
    const container = this.assetContainers.get(shipId);
    if (container) {
      container.dispose();
      this.assetContainers.delete(shipId);
    }

    // Clean up animations
    shipModel.animations?.forEach(animGroup => {
      animGroup.dispose();
    });

    // Remove from maps
    this.loadedModels.delete(shipId);
    this.sceneObjects.delete(shipId);

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
    // Remove all loaded models
    const shipIds = Array.from(this.loadedModels.keys());
    shipIds.forEach(id => this.removeShipModel(id));

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
   * Dispose of all resources
   */
  dispose(): void {
    // Clear scene first
    this.clearScene();

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
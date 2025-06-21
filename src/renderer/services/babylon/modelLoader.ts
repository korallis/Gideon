/**
 * 3D Model Loader Service
 * Handles loading and processing of ship models (GLTF/GLB/OBJ formats)
 */

import {
  Scene,
  AbstractMesh,
  Mesh,
  TransformNode,
  Vector3,
  Color3,
  StandardMaterial,
  PBRMaterial,
  Texture,
  AnimationGroup,
  BoundingInfo,
  AssetContainer,
} from '@babylonjs/core';
import { SceneLoader } from '@babylonjs/loaders';
import '@babylonjs/loaders/glTF';
import '@babylonjs/loaders/OBJ';

export interface ModelLoadOptions {
  scale?: Vector3;
  position?: Vector3;
  rotation?: Vector3;
  enableAnimations?: boolean;
  enablePBR?: boolean;
  optimizeForPerformance?: boolean;
  generateLODs?: boolean;
}

export interface LoadedModel {
  id: string;
  name: string;
  meshes: AbstractMesh[];
  rootMesh: AbstractMesh;
  materials: (StandardMaterial | PBRMaterial)[];
  textures: Texture[];
  animations: AnimationGroup[];
  boundingInfo: BoundingInfo;
  container: AssetContainer;
  metadata: ModelMetadata;
}

export interface ModelMetadata {
  originalPath: string;
  format: 'gltf' | 'glb' | 'obj' | 'babylon';
  polyCount: number;
  materialCount: number;
  textureCount: number;
  animationCount: number;
  loadTime: number;
  memoryUsage: number;
}

export interface ModelLoadProgress {
  stage: 'downloading' | 'parsing' | 'processing' | 'optimizing' | 'complete';
  progress: number; // 0-100
  message: string;
}

export class ModelLoader {
  private scene: Scene | null = null;
  private loadedModels = new Map<string, LoadedModel>();
  private loadingPromises = new Map<string, Promise<LoadedModel | null>>();

  constructor(scene: Scene) {
    this.scene = scene;
    this.setupLoaderPlugins();
  }

  /**
   * Configure loader plugins for different formats
   */
  private setupLoaderPlugins(): void {
    if (!this.scene) return;

    // Configure GLTF loader for EVE assets
    SceneLoader.OnPluginActivatedObservable.add((plugin) => {
      if (plugin.name === 'gltf') {
        // Optimize for EVE Online ship models
        (plugin as any).animationStartMode = 0; // Don't auto-start animations
        (plugin as any).compileMaterials = true;
        (plugin as any).useClipPlane = false;
      }
    });
  }

  /**
   * Load a ship model from file path with progress tracking
   */
  async loadModel(
    modelId: string,
    modelPath: string,
    modelName: string,
    options: ModelLoadOptions = {},
    onProgress?: (progress: ModelLoadProgress) => void
  ): Promise<LoadedModel | null> {
    if (!this.scene) {
      throw new Error('Scene not initialized');
    }

    // Check if already loading
    if (this.loadingPromises.has(modelId)) {
      return await this.loadingPromises.get(modelId)!;
    }

    // Check if already loaded
    if (this.loadedModels.has(modelId)) {
      return this.loadedModels.get(modelId)!;
    }

    const loadPromise = this.performModelLoad(modelId, modelPath, modelName, options, onProgress);
    this.loadingPromises.set(modelId, loadPromise);

    try {
      const result = await loadPromise;
      this.loadingPromises.delete(modelId);
      return result;
    } catch (error) {
      this.loadingPromises.delete(modelId);
      throw error;
    }
  }

  /**
   * Perform the actual model loading process
   */
  private async performModelLoad(
    modelId: string,
    modelPath: string,
    modelName: string,
    options: ModelLoadOptions,
    onProgress?: (progress: ModelLoadProgress) => void
  ): Promise<LoadedModel | null> {
    const startTime = performance.now();
    
    try {
      // Stage 1: Downloading/Loading
      onProgress?.({
        stage: 'downloading',
        progress: 0,
        message: `Loading ${modelName}...`
      });

      const format = this.detectModelFormat(modelPath);
      let result: any;

      // Load based on format
      if (format === 'gltf' || format === 'glb') {
        result = await this.loadGLTFModel(modelPath, onProgress);
      } else if (format === 'obj') {
        result = await this.loadOBJModel(modelPath, onProgress);
      } else {
        throw new Error(`Unsupported model format: ${format}`);
      }

      // Stage 2: Processing
      onProgress?.({
        stage: 'processing',
        progress: 60,
        message: 'Processing model data...'
      });

      const processedModel = await this.processLoadedModel(
        modelId,
        modelName,
        modelPath,
        result,
        options,
        format
      );

      // Stage 3: Optimizing
      onProgress?.({
        stage: 'optimizing',
        progress: 80,
        message: 'Optimizing for performance...'
      });

      if (options.optimizeForPerformance) {
        await this.optimizeModel(processedModel);
      }

      // Calculate load time and memory usage
      const loadTime = performance.now() - startTime;
      processedModel.metadata.loadTime = loadTime;
      processedModel.metadata.memoryUsage = this.calculateMemoryUsage(processedModel);

      // Stage 4: Complete
      onProgress?.({
        stage: 'complete',
        progress: 100,
        message: `${modelName} loaded successfully`
      });

      // Store the loaded model
      this.loadedModels.set(modelId, processedModel);

      console.log(`Model loaded: ${modelName} (${loadTime.toFixed(2)}ms)`);
      return processedModel;

    } catch (error) {
      console.error(`Failed to load model ${modelName}:`, error);
      throw error;
    }
  }

  /**
   * Load GLTF/GLB model
   */
  private async loadGLTFModel(
    modelPath: string,
    onProgress?: (progress: ModelLoadProgress) => void
  ): Promise<any> {
    if (!this.scene) throw new Error('Scene not initialized');

    return new Promise((resolve, reject) => {
      SceneLoader.ImportMeshAsync('', '', modelPath, this.scene!)
        .then((result) => {
          onProgress?.({
            stage: 'parsing',
            progress: 40,
            message: 'Parsing GLTF data...'
          });
          resolve(result);
        })
        .catch(reject);
    });
  }

  /**
   * Load OBJ model
   */
  private async loadOBJModel(
    modelPath: string,
    onProgress?: (progress: ModelLoadProgress) => void
  ): Promise<any> {
    if (!this.scene) throw new Error('Scene not initialized');

    return new Promise((resolve, reject) => {
      SceneLoader.ImportMeshAsync('', '', modelPath, this.scene!)
        .then((result) => {
          onProgress?.({
            stage: 'parsing',
            progress: 40,
            message: 'Parsing OBJ data...'
          });
          resolve(result);
        })
        .catch(reject);
    });
  }

  /**
   * Process loaded model data into our format
   */
  private async processLoadedModel(
    modelId: string,
    modelName: string,
    modelPath: string,
    result: any,
    options: ModelLoadOptions,
    format: string
  ): Promise<LoadedModel> {
    if (!this.scene) throw new Error('Scene not initialized');

    const { meshes, materials, textures, animationGroups } = result;

    if (!meshes || meshes.length === 0) {
      throw new Error('No meshes found in model');
    }

    // Find or create root mesh
    let rootMesh = meshes[0];
    if (meshes.length > 1) {
      // Create a parent node for multiple meshes
      rootMesh = new TransformNode(`${modelId}_root`, this.scene);
      meshes.forEach((mesh: AbstractMesh) => {
        if (mesh.parent === null) {
          mesh.setParent(rootMesh);
        }
      });
    }

    // Apply transformations
    if (options.position) {
      rootMesh.position = options.position;
    }
    if (options.rotation) {
      rootMesh.rotation = options.rotation;
    }
    if (options.scale) {
      rootMesh.scaling = options.scale;
    }

    // Process materials for EVE aesthetic
    const processedMaterials = this.processMaterials(materials, options.enablePBR);

    // Calculate bounding info
    const boundingInfo = this.calculateBoundingInfo(meshes);

    // Create asset container for cleanup
    const container = this.scene.createAssetContainer();
    meshes.forEach((mesh: AbstractMesh) => container.meshes.push(mesh));
    materials.forEach((material: any) => container.materials.push(material));
    textures.forEach((texture: any) => container.textures.push(texture));
    animationGroups.forEach((anim: AnimationGroup) => container.animationGroups.push(anim));

    // Create metadata
    const metadata: ModelMetadata = {
      originalPath: modelPath,
      format: format as any,
      polyCount: this.calculatePolyCount(meshes),
      materialCount: materials.length,
      textureCount: textures.length,
      animationCount: animationGroups.length,
      loadTime: 0, // Will be set later
      memoryUsage: 0, // Will be calculated later
    };

    const loadedModel: LoadedModel = {
      id: modelId,
      name: modelName,
      meshes,
      rootMesh,
      materials: processedMaterials,
      textures,
      animations: animationGroups,
      boundingInfo,
      container,
      metadata,
    };

    return loadedModel;
  }

  /**
   * Process materials for EVE Online aesthetic
   */
  private processMaterials(
    materials: any[],
    enablePBR: boolean = true
  ): (StandardMaterial | PBRMaterial)[] {
    return materials.map((material) => {
      if (enablePBR && material instanceof PBRMaterial) {
        // Enhance PBR materials for space ships
        material.metallicFactor = Math.max(material.metallicFactor || 0, 0.7);
        material.roughnessFactor = Math.min(material.roughnessFactor || 1, 0.4);
        material.emissiveColor = material.emissiveColor || new Color3(0.1, 0.1, 0.2);
        return material;
      } else if (material instanceof StandardMaterial) {
        // Enhance standard materials
        material.specularPower = Math.max(material.specularPower || 32, 64);
        material.emissiveColor = material.emissiveColor || new Color3(0.1, 0.1, 0.2);
        return material;
      }
      return material;
    });
  }

  /**
   * Calculate bounding info for all meshes
   */
  private calculateBoundingInfo(meshes: AbstractMesh[]): BoundingInfo {
    if (meshes.length === 0) {
      return new BoundingInfo(Vector3.Zero(), Vector3.Zero());
    }

    let min = meshes[0].getBoundingInfo().boundingBox.minimum.clone();
    let max = meshes[0].getBoundingInfo().boundingBox.maximum.clone();

    for (let i = 1; i < meshes.length; i++) {
      const meshBounds = meshes[i].getBoundingInfo().boundingBox;
      min = Vector3.Minimize(min, meshBounds.minimum);
      max = Vector3.Maximize(max, meshBounds.maximum);
    }

    return new BoundingInfo(min, max);
  }

  /**
   * Calculate total polygon count
   */
  private calculatePolyCount(meshes: AbstractMesh[]): number {
    return meshes.reduce((total, mesh) => {
      if (mesh instanceof Mesh && mesh.geometry) {
        const indices = mesh.getIndices();
        return total + (indices ? indices.length / 3 : 0);
      }
      return total;
    }, 0);
  }

  /**
   * Calculate memory usage estimate
   */
  private calculateMemoryUsage(model: LoadedModel): number {
    let memoryUsage = 0;

    // Estimate mesh memory
    model.meshes.forEach((mesh) => {
      if (mesh instanceof Mesh && mesh.geometry) {
        const vertices = mesh.getTotalVertices();
        memoryUsage += vertices * 32; // Rough estimate: 32 bytes per vertex
      }
    });

    // Estimate texture memory
    model.textures.forEach((texture) => {
      if (texture.getSize) {
        const size = texture.getSize();
        memoryUsage += size.width * size.height * 4; // RGBA
      }
    });

    return memoryUsage;
  }

  /**
   * Optimize model for performance
   */
  private async optimizeModel(model: LoadedModel): Promise<void> {
    // Merge compatible meshes
    await this.mergeMeshes(model);
    
    // Optimize materials
    this.optimizeMaterials(model);
    
    // Generate LODs if requested
    // Note: LOD generation would be implemented here
  }

  /**
   * Merge compatible meshes to reduce draw calls
   */
  private async mergeMeshes(model: LoadedModel): Promise<void> {
    // Group meshes by material
    const materialGroups = new Map<string, Mesh[]>();
    
    model.meshes.forEach((mesh) => {
      if (mesh instanceof Mesh && mesh.material) {
        const materialId = mesh.material.uniqueId.toString();
        if (!materialGroups.has(materialId)) {
          materialGroups.set(materialId, []);
        }
        materialGroups.get(materialId)!.push(mesh);
      }
    });

    // Merge meshes with the same material (if more than one)
    for (const [materialId, meshes] of materialGroups) {
      if (meshes.length > 1) {
        try {
          const merged = Mesh.MergeMeshes(meshes, true, true, undefined, false, true);
          if (merged) {
            merged.name = `${model.id}_merged_${materialId}`;
            console.log(`Merged ${meshes.length} meshes for material ${materialId}`);
          }
        } catch (error) {
          console.warn(`Failed to merge meshes for material ${materialId}:`, error);
        }
      }
    }
  }

  /**
   * Optimize materials
   */
  private optimizeMaterials(model: LoadedModel): void {
    model.materials.forEach((material) => {
      if (material instanceof PBRMaterial) {
        // Optimize PBR materials
        material.forceCompilation();
      } else if (material instanceof StandardMaterial) {
        // Optimize standard materials
        material.forceCompilation();
      }
    });
  }

  /**
   * Detect model format from file path
   */
  private detectModelFormat(modelPath: string): string {
    const extension = modelPath.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'gltf':
        return 'gltf';
      case 'glb':
        return 'glb';
      case 'obj':
        return 'obj';
      case 'babylon':
        return 'babylon';
      default:
        throw new Error(`Unknown model format: ${extension}`);
    }
  }

  /**
   * Get loaded model by ID
   */
  getModel(modelId: string): LoadedModel | null {
    return this.loadedModels.get(modelId) || null;
  }

  /**
   * Get all loaded models
   */
  getAllModels(): Map<string, LoadedModel> {
    return new Map(this.loadedModels);
  }

  /**
   * Remove model from memory
   */
  removeModel(modelId: string): boolean {
    const model = this.loadedModels.get(modelId);
    if (!model) return false;

    // Dispose of all resources
    model.container.dispose();
    model.animations.forEach(anim => anim.dispose());

    this.loadedModels.delete(modelId);
    console.log(`Model removed: ${model.name}`);
    return true;
  }

  /**
   * Clear all loaded models
   */
  clearAllModels(): void {
    for (const [modelId] of this.loadedModels) {
      this.removeModel(modelId);
    }
    console.log('All models cleared');
  }

  /**
   * Get memory usage statistics
   */
  getMemoryStats(): { totalMemory: number; modelCount: number; models: Array<{ id: string; name: string; memory: number }> } {
    const models = Array.from(this.loadedModels.values()).map(model => ({
      id: model.id,
      name: model.name,
      memory: model.metadata.memoryUsage
    }));

    const totalMemory = models.reduce((sum, model) => sum + model.memory, 0);

    return {
      totalMemory,
      modelCount: models.length,
      models
    };
  }

  /**
   * Dispose of the model loader
   */
  dispose(): void {
    this.clearAllModels();
    this.loadingPromises.clear();
    this.scene = null;
    console.log('ModelLoader disposed');
  }
}
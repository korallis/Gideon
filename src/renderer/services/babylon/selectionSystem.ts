/**
 * Selection and Highlighting System
 * Manages object selection, highlighting, and interaction feedback
 */

import {
  Scene,
  AbstractMesh,
  Mesh,
  Color3,
  Vector3,
  Animation,
  StandardMaterial,
  PBRMaterial,
  OutlineRenderer,
  GlowLayer,
  HighlightLayer,
  PickingInfo,
  Ray,
  Tools,
  ActionManager,
  ExecuteCodeAction,
  PointerEventTypes,
  Matrix,
} from '@babylonjs/core';

export interface SelectionOptions {
  enableOutline?: boolean;
  enableGlow?: boolean;
  enableHighlight?: boolean;
  outlineColor?: Color3;
  outlineWidth?: number;
  glowColor?: Color3;
  glowIntensity?: number;
  highlightColor?: Color3;
  animateSelection?: boolean;
  animationDuration?: number;
}

export interface SelectableObject {
  id: string;
  mesh: AbstractMesh;
  type: 'ship' | 'module' | 'hardpoint' | 'component';
  metadata: {
    name: string;
    description?: string;
    category?: string;
    interactive: boolean;
    selectable: boolean;
  };
  originalMaterial?: StandardMaterial | PBRMaterial;
  boundingInfo?: any;
}

export interface SelectionState {
  primary: SelectableObject | null;
  secondary: SelectableObject[];
  hovered: SelectableObject | null;
  multiSelectMode: boolean;
  selectionCount: number;
}

export interface SelectionEvent {
  type: 'select' | 'deselect' | 'hover' | 'unhover' | 'multiselect';
  object: SelectableObject;
  position: Vector3;
  pickingInfo: PickingInfo;
}

export class SelectionSystem {
  private scene: Scene;
  private selectableObjects = new Map<string, SelectableObject>();
  private selectionState: SelectionState;
  private options: SelectionOptions;
  
  // Visual effects
  private outlineRenderer: OutlineRenderer | null = null;
  private glowLayer: GlowLayer | null = null;
  private highlightLayer: HighlightLayer | null = null;
  
  // Event handling
  private eventCallbacks = new Map<string, ((event: SelectionEvent) => void)[]>();
  private isEnabled = true;
  
  // Selection materials
  private selectionMaterials = new Map<string, StandardMaterial | PBRMaterial>();

  constructor(scene: Scene, options: SelectionOptions = {}) {
    this.scene = scene;
    this.options = {
      enableOutline: true,
      enableGlow: true,
      enableHighlight: true,
      outlineColor: new Color3(0.2, 0.8, 1.0),
      outlineWidth: 0.05,
      glowColor: new Color3(0.3, 0.9, 1.0),
      glowIntensity: 0.8,
      highlightColor: new Color3(0.5, 1.0, 1.0),
      animateSelection: true,
      animationDuration: 300,
      ...options,
    };

    this.selectionState = {
      primary: null,
      secondary: [],
      hovered: null,
      multiSelectMode: false,
      selectionCount: 0,
    };

    this.initializeEffects();
    this.setupEventHandlers();
  }

  /**
   * Initialize visual effects systems
   */
  private initializeEffects(): void {
    // Outline renderer for selection borders
    if (this.options.enableOutline) {
      this.outlineRenderer = new OutlineRenderer(this.scene);
    }

    // Glow layer for emissive effects
    if (this.options.enableGlow) {
      this.glowLayer = new GlowLayer('selectionGlow', this.scene);
      this.glowLayer.intensity = this.options.glowIntensity || 0.8;
    }

    // Highlight layer for overlay effects
    if (this.options.enableHighlight) {
      this.highlightLayer = new HighlightLayer('selectionHighlight', this.scene);
    }
  }

  /**
   * Set up scene event handlers for picking and interaction
   */
  private setupEventHandlers(): void {
    // Pointer events for picking
    this.scene.onPointerObservable.add((pointerInfo) => {
      if (!this.isEnabled) return;

      switch (pointerInfo.type) {
        case PointerEventTypes.POINTERDOWN:
          this.handlePointerDown(pointerInfo);
          break;
        case PointerEventTypes.POINTERMOVE:
          this.handlePointerMove(pointerInfo);
          break;
        case PointerEventTypes.POINTERUP:
          this.handlePointerUp(pointerInfo);
          break;
      }
    });

    // Keyboard events for multi-select
    window.addEventListener('keydown', this.handleKeyDown.bind(this));
    window.addEventListener('keyup', this.handleKeyUp.bind(this));
  }

  /**
   * Handle pointer down events
   */
  private handlePointerDown(pointerInfo: any): void {
    if (pointerInfo.pickInfo?.hit && pointerInfo.pickInfo.pickedMesh) {
      const selectableObject = this.findSelectableObject(pointerInfo.pickInfo.pickedMesh);
      
      if (selectableObject && selectableObject.metadata.selectable) {
        if (this.selectionState.multiSelectMode) {
          this.toggleSelection(selectableObject, pointerInfo.pickInfo);
        } else {
          this.selectObject(selectableObject, pointerInfo.pickInfo);
        }
      } else {
        // Clicked on empty space - clear selection
        this.clearSelection();
      }
    }
  }

  /**
   * Handle pointer move events for hover effects
   */
  private handlePointerMove(pointerInfo: any): void {
    if (pointerInfo.pickInfo?.hit && pointerInfo.pickInfo.pickedMesh) {
      const selectableObject = this.findSelectableObject(pointerInfo.pickInfo.pickedMesh);
      
      if (selectableObject && selectableObject.metadata.interactive) {
        this.setHovered(selectableObject, pointerInfo.pickInfo);
      } else {
        this.clearHover();
      }
    } else {
      this.clearHover();
    }
  }

  /**
   * Handle pointer up events
   */
  private handlePointerUp(pointerInfo: any): void {
    // Handle any up events if needed
  }

  /**
   * Handle keyboard events for multi-select mode
   */
  private handleKeyDown(event: KeyboardEvent): void {
    if (event.ctrlKey || event.metaKey) {
      this.selectionState.multiSelectMode = true;
    }
  }

  /**
   * Handle keyboard up events
   */
  private handleKeyUp(event: KeyboardEvent): void {
    if (!event.ctrlKey && !event.metaKey) {
      this.selectionState.multiSelectMode = false;
    }
  }

  /**
   * Register a mesh as selectable
   */
  registerSelectableObject(
    id: string,
    mesh: AbstractMesh,
    type: SelectableObject['type'],
    metadata: SelectableObject['metadata']
  ): void {
    const selectableObject: SelectableObject = {
      id,
      mesh,
      type,
      metadata,
      originalMaterial: mesh.material as StandardMaterial | PBRMaterial,
      boundingInfo: mesh.getBoundingInfo(),
    };

    this.selectableObjects.set(id, selectableObject);

    // Set up action manager for the mesh
    if (!mesh.actionManager) {
      mesh.actionManager = new ActionManager(this.scene);
    }
  }

  /**
   * Find selectable object by mesh
   */
  private findSelectableObject(mesh: AbstractMesh): SelectableObject | null {
    // Check direct match
    for (const [id, obj] of this.selectableObjects) {
      if (obj.mesh === mesh) {
        return obj;
      }
      
      // Check if mesh is a child of the selectable object
      if (obj.mesh.getChildMeshes().includes(mesh)) {
        return obj;
      }
    }
    return null;
  }

  /**
   * Select an object
   */
  selectObject(object: SelectableObject, pickingInfo?: PickingInfo): void {
    // Clear previous selection
    this.clearSelection();

    // Set as primary selection
    this.selectionState.primary = object;
    this.selectionState.selectionCount = 1;

    // Apply visual effects
    this.applySelectionEffects(object, 'primary');

    // Trigger event
    this.emitEvent('select', object, pickingInfo);

    console.log(`Selected: ${object.metadata.name} (${object.type})`);
  }

  /**
   * Toggle selection (for multi-select)
   */
  toggleSelection(object: SelectableObject, pickingInfo?: PickingInfo): void {
    const isSelected = this.isObjectSelected(object);

    if (isSelected) {
      this.deselectObject(object, pickingInfo);
    } else {
      this.addToSelection(object, pickingInfo);
    }
  }

  /**
   * Add object to selection
   */
  addToSelection(object: SelectableObject, pickingInfo?: PickingInfo): void {
    if (this.isObjectSelected(object)) return;

    if (!this.selectionState.primary) {
      this.selectObject(object, pickingInfo);
      return;
    }

    // Add to secondary selection
    this.selectionState.secondary.push(object);
    this.selectionState.selectionCount++;

    // Apply visual effects
    this.applySelectionEffects(object, 'secondary');

    // Trigger event
    this.emitEvent('multiselect', object, pickingInfo);

    console.log(`Added to selection: ${object.metadata.name}`);
  }

  /**
   * Deselect specific object
   */
  deselectObject(object: SelectableObject, pickingInfo?: PickingInfo): void {
    if (this.selectionState.primary === object) {
      // If deselecting primary, promote secondary to primary
      this.removeSelectionEffects(object);
      
      if (this.selectionState.secondary.length > 0) {
        this.selectionState.primary = this.selectionState.secondary.shift()!;
        this.applySelectionEffects(this.selectionState.primary, 'primary');
      } else {
        this.selectionState.primary = null;
      }
    } else {
      // Remove from secondary selection
      const index = this.selectionState.secondary.indexOf(object);
      if (index !== -1) {
        this.selectionState.secondary.splice(index, 1);
        this.removeSelectionEffects(object);
      }
    }

    this.selectionState.selectionCount--;

    // Trigger event
    this.emitEvent('deselect', object, pickingInfo);

    console.log(`Deselected: ${object.metadata.name}`);
  }

  /**
   * Clear all selections
   */
  clearSelection(): void {
    // Remove effects from all selected objects
    if (this.selectionState.primary) {
      this.removeSelectionEffects(this.selectionState.primary);
    }

    this.selectionState.secondary.forEach(obj => {
      this.removeSelectionEffects(obj);
    });

    // Reset state
    this.selectionState.primary = null;
    this.selectionState.secondary = [];
    this.selectionState.selectionCount = 0;

    console.log('Selection cleared');
  }

  /**
   * Set hovered object
   */
  setHovered(object: SelectableObject, pickingInfo?: PickingInfo): void {
    if (this.selectionState.hovered === object) return;

    // Clear previous hover
    this.clearHover();

    // Set new hover
    this.selectionState.hovered = object;
    this.applyHoverEffects(object);

    // Trigger event
    this.emitEvent('hover', object, pickingInfo);
  }

  /**
   * Clear hover state
   */
  clearHover(): void {
    if (this.selectionState.hovered) {
      this.removeHoverEffects(this.selectionState.hovered);
      this.emitEvent('unhover', this.selectionState.hovered);
      this.selectionState.hovered = null;
    }
  }

  /**
   * Apply selection visual effects
   */
  private applySelectionEffects(object: SelectableObject, type: 'primary' | 'secondary'): void {
    const mesh = object.mesh;
    const isPrimary = type === 'primary';

    // Outline effect
    if (this.outlineRenderer && this.options.enableOutline) {
      this.outlineRenderer.addMesh(mesh, this.options.outlineColor!);
    }

    // Glow effect
    if (this.glowLayer && this.options.enableGlow) {
      this.glowLayer.addIncludedOnlyMesh(mesh);
      this.glowLayer.customEmissiveColorSelector = (mesh, subMesh, material, result) => {
        result.set(
          this.options.glowColor!.r * (isPrimary ? 1.0 : 0.6),
          this.options.glowColor!.g * (isPrimary ? 1.0 : 0.6),
          this.options.glowColor!.b * (isPrimary ? 1.0 : 0.6),
          1.0
        );
      };
    }

    // Highlight effect
    if (this.highlightLayer && this.options.enableHighlight) {
      this.highlightLayer.addMesh(
        mesh,
        isPrimary ? this.options.highlightColor! : this.options.highlightColor!.scale(0.6)
      );
    }

    // Animation effect
    if (this.options.animateSelection) {
      this.animateSelection(mesh);
    }
  }

  /**
   * Remove selection visual effects
   */
  private removeSelectionEffects(object: SelectableObject): void {
    const mesh = object.mesh;

    // Remove outline
    if (this.outlineRenderer) {
      this.outlineRenderer.removeMesh(mesh);
    }

    // Remove glow
    if (this.glowLayer) {
      this.glowLayer.removeIncludedOnlyMesh(mesh);
    }

    // Remove highlight
    if (this.highlightLayer) {
      this.highlightLayer.removeMesh(mesh);
    }
  }

  /**
   * Apply hover effects
   */
  private applyHoverEffects(object: SelectableObject): void {
    const mesh = object.mesh;

    // Subtle highlight for hover
    if (this.highlightLayer && !this.isObjectSelected(object)) {
      this.highlightLayer.addMesh(mesh, this.options.highlightColor!.scale(0.3));
    }

    // Change cursor
    this.scene.getEngine().getRenderingCanvas()!.style.cursor = 'pointer';
  }

  /**
   * Remove hover effects
   */
  private removeHoverEffects(object: SelectableObject): void {
    const mesh = object.mesh;

    // Remove hover highlight (only if not selected)
    if (this.highlightLayer && !this.isObjectSelected(object)) {
      this.highlightLayer.removeMesh(mesh);
    }

    // Reset cursor
    this.scene.getEngine().getRenderingCanvas()!.style.cursor = 'default';
  }

  /**
   * Animate selection with scale pulse
   */
  private animateSelection(mesh: AbstractMesh): void {
    const originalScale = mesh.scaling.clone();
    const targetScale = originalScale.scale(1.1);

    // Create pulse animation
    const scaleAnimation = Animation.CreateAndStartAnimation(
      'selectionPulse',
      mesh,
      'scaling',
      60,
      this.options.animationDuration!,
      originalScale,
      targetScale,
      Animation.ANIMATIONLOOPMODE_YOYO
    );

    if (scaleAnimation) {
      // Return to original scale
      setTimeout(() => {
        Animation.CreateAndStartAnimation(
          'selectionReturn',
          mesh,
          'scaling',
          60,
          this.options.animationDuration! / 2,
          mesh.scaling,
          originalScale,
          Animation.ANIMATIONLOOPMODE_CONSTANT
        );
      }, this.options.animationDuration!);
    }
  }

  /**
   * Check if object is selected
   */
  isObjectSelected(object: SelectableObject): boolean {
    return this.selectionState.primary === object || 
           this.selectionState.secondary.includes(object);
  }

  /**
   * Get selected objects
   */
  getSelectedObjects(): SelectableObject[] {
    const selected: SelectableObject[] = [];
    if (this.selectionState.primary) {
      selected.push(this.selectionState.primary);
    }
    selected.push(...this.selectionState.secondary);
    return selected;
  }

  /**
   * Focus camera on selected objects
   */
  focusOnSelection(): void {
    const selected = this.getSelectedObjects();
    if (selected.length === 0) return;

    // Calculate bounding box of all selected objects
    let minPoint = selected[0].mesh.getBoundingInfo().boundingBox.minimum.clone();
    let maxPoint = selected[0].mesh.getBoundingInfo().boundingBox.maximum.clone();

    selected.forEach(obj => {
      const bounds = obj.mesh.getBoundingInfo().boundingBox;
      minPoint = Vector3.Minimize(minPoint, bounds.minimum);
      maxPoint = Vector3.Maximize(maxPoint, bounds.maximum);
    });

    const center = Vector3.Center(minPoint, maxPoint);
    const size = maxPoint.subtract(minPoint);
    const distance = Math.max(size.x, size.y, size.z) * 2;

    // Emit focus event that the camera system can handle
    this.emitEvent('select', selected[0], undefined, { center, distance });
  }

  /**
   * Add event listener
   */
  addEventListener(eventType: string, callback: (event: SelectionEvent) => void): void {
    if (!this.eventCallbacks.has(eventType)) {
      this.eventCallbacks.set(eventType, []);
    }
    this.eventCallbacks.get(eventType)!.push(callback);
  }

  /**
   * Remove event listener
   */
  removeEventListener(eventType: string, callback: (event: SelectionEvent) => void): void {
    const callbacks = this.eventCallbacks.get(eventType);
    if (callbacks) {
      const index = callbacks.indexOf(callback);
      if (index !== -1) {
        callbacks.splice(index, 1);
      }
    }
  }

  /**
   * Emit selection event
   */
  private emitEvent(
    type: SelectionEvent['type'],
    object: SelectableObject,
    pickingInfo?: PickingInfo,
    extraData?: any
  ): void {
    const event: SelectionEvent = {
      type,
      object,
      position: pickingInfo?.pickedPoint || Vector3.Zero(),
      pickingInfo: pickingInfo as PickingInfo,
      ...extraData,
    };

    const callbacks = this.eventCallbacks.get(type);
    if (callbacks) {
      callbacks.forEach(callback => callback(event));
    }
  }

  /**
   * Enable/disable selection system
   */
  setEnabled(enabled: boolean): void {
    this.isEnabled = enabled;
    if (!enabled) {
      this.clearSelection();
      this.clearHover();
    }
  }

  /**
   * Update selection options
   */
  updateOptions(newOptions: Partial<SelectionOptions>): void {
    Object.assign(this.options, newOptions);
    
    // Update visual effects
    if (this.glowLayer && newOptions.glowIntensity !== undefined) {
      this.glowLayer.intensity = newOptions.glowIntensity;
    }
  }

  /**
   * Get current selection state
   */
  getSelectionState(): SelectionState {
    return { ...this.selectionState };
  }

  /**
   * Get selectable objects
   */
  getSelectableObjects(): Map<string, SelectableObject> {
    return new Map(this.selectableObjects);
  }

  /**
   * Dispose of selection system
   */
  dispose(): void {
    this.clearSelection();
    this.clearHover();

    if (this.outlineRenderer) {
      this.outlineRenderer.dispose();
    }

    if (this.glowLayer) {
      this.glowLayer.dispose();
    }

    if (this.highlightLayer) {
      this.highlightLayer.dispose();
    }

    this.selectableObjects.clear();
    this.eventCallbacks.clear();
    this.selectionMaterials.forEach(material => material.dispose());
    this.selectionMaterials.clear();

    console.log('SelectionSystem disposed');
  }
}
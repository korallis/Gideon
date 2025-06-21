/**
 * Selection and Highlighting Demo Component
 * Showcases object selection, highlighting, and interaction features
 */

import React, { useState, useEffect } from 'react';
import { Button, Switch } from '../UI';
import { useSceneManager } from '../../hooks';
import { SelectableObject, SelectionOptions } from '../../services/babylon';
import { cn } from '../../utils';

export const SelectionDemo: React.FC = () => {
  const [selectableObjects, setSelectableObjects] = useState<Map<string, SelectableObject>>(new Map());
  const [selectedObjects, setSelectedObjects] = useState<SelectableObject[]>([]);
  const [hoveredObject, setHoveredObject] = useState<SelectableObject | null>(null);
  const [selectionOptions, setSelectionOptions] = useState<SelectionOptions>({
    enableOutline: true,
    enableGlow: true,
    enableHighlight: true,
    outlineWidth: 0.05,
    glowIntensity: 0.8,
    animateSelection: true,
    animationDuration: 300,
  });
  const [selectionEnabled, setSelectionEnabled] = useState(true);

  const sceneManager = useSceneManager({
    autoInitialize: false,
  });

  // Update selectable objects and selection state
  useEffect(() => {
    if (sceneManager.isInitialized) {
      const selectionSystem = (sceneManager as any).getSelectionSystem();
      
      if (selectionSystem) {
        // Get selectable objects
        const objects = selectionSystem.getSelectableObjects();
        setSelectableObjects(objects);

        // Set up event listeners for real-time updates
        const handleSelectionChange = () => {
          const selected = (sceneManager as any).getSelectedObjects();
          setSelectedObjects(selected);
        };

        const handleHover = (event: any) => {
          setHoveredObject(event.object);
        };

        const handleUnhover = () => {
          setHoveredObject(null);
        };

        selectionSystem.addEventListener('select', handleSelectionChange);
        selectionSystem.addEventListener('deselect', handleSelectionChange);
        selectionSystem.addEventListener('multiselect', handleSelectionChange);
        selectionSystem.addEventListener('hover', handleHover);
        selectionSystem.addEventListener('unhover', handleUnhover);

        // Initial state
        handleSelectionChange();
      }
    }
  }, [sceneManager.isInitialized]);

  const handleSelectionOptionChange = (option: keyof SelectionOptions, value: any) => {
    const newOptions = { ...selectionOptions, [option]: value };
    setSelectionOptions(newOptions);

    if (sceneManager.isInitialized) {
      (sceneManager as any).updateSelectionOptions({ [option]: value });
    }
  };

  const handleToggleSelection = () => {
    const newEnabled = !selectionEnabled;
    setSelectionEnabled(newEnabled);

    if (sceneManager.isInitialized) {
      (sceneManager as any).setSelectionEnabled(newEnabled);
    }
  };

  const handleClearSelection = () => {
    if (sceneManager.isInitialized) {
      (sceneManager as any).clearSelection();
    }
  };

  const handleFocusOnSelection = () => {
    if (sceneManager.isInitialized) {
      (sceneManager as any).focusOnSelection();
    }
  };

  const handleSelectObject = (objectId: string) => {
    if (sceneManager.isInitialized) {
      (sceneManager as any).selectObject(objectId);
    }
  };

  const getObjectsByType = (type: SelectableObject['type']) => {
    return Array.from(selectableObjects.values()).filter(obj => obj.type === type);
  };

  const getSelectionStats = () => {
    const stats = {
      total: selectableObjects.size,
      ships: getObjectsByType('ship').length,
      modules: getObjectsByType('module').length,
      hardpoints: getObjectsByType('hardpoint').length,
      components: getObjectsByType('component').length,
      selected: selectedObjects.length,
    };
    return stats;
  };

  const stats = getSelectionStats();

  return (
    <div className="p-6 bg-primary-surface rounded-lg border border-primary-border">
      <div className="mb-6">
        <h3 className="text-xl font-semibold text-text-primary mb-2">
          Selection & Highlighting System
        </h3>
        <p className="text-sm text-text-secondary">
          Advanced object selection with visual feedback, multi-select, and interaction features.
        </p>
      </div>

      {/* Status */}
      <div className="mb-6 p-4 bg-primary-bg rounded border border-primary-border/50">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-text-primary">Selection Status</span>
          <div className="flex items-center gap-2">
            <Switch
              checked={selectionEnabled}
              onChange={handleToggleSelection}
              size="sm"
              disabled={!sceneManager.isInitialized}
            />
            <span className="text-xs text-text-secondary">
              {selectionEnabled ? 'Enabled' : 'Disabled'}
            </span>
          </div>
        </div>
        
        <div className="grid grid-cols-3 gap-4 text-xs">
          <div>
            <span className="text-text-tertiary">Objects:</span>
            <div className="text-text-primary font-mono">{stats.total}</div>
          </div>
          <div>
            <span className="text-text-tertiary">Selected:</span>
            <div className="text-text-primary font-mono">{stats.selected}</div>
          </div>
          <div>
            <span className="text-text-tertiary">Hovered:</span>
            <div className="text-text-primary font-mono">
              {hoveredObject ? hoveredObject.metadata.name : 'None'}
            </div>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Selection Options */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">Visual Effects</h4>
          
          <div className="space-y-3">
            {/* Effect Toggles */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-3">Effect Types</div>
              
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-primary">Outline</span>
                  <Switch
                    checked={selectionOptions.enableOutline}
                    onChange={(checked) => handleSelectionOptionChange('enableOutline', checked)}
                    size="sm"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-primary">Glow</span>
                  <Switch
                    checked={selectionOptions.enableGlow}
                    onChange={(checked) => handleSelectionOptionChange('enableGlow', checked)}
                    size="sm"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-primary">Highlight</span>
                  <Switch
                    checked={selectionOptions.enableHighlight}
                    onChange={(checked) => handleSelectionOptionChange('enableHighlight', checked)}
                    size="sm"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
                
                <div className="flex items-center justify-between">
                  <span className="text-xs text-text-primary">Animation</span>
                  <Switch
                    checked={selectionOptions.animateSelection}
                    onChange={(checked) => handleSelectionOptionChange('animateSelection', checked)}
                    size="sm"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
              </div>
            </div>

            {/* Effect Parameters */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-3">Parameters</div>
              
              <div className="space-y-2">
                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Outline Width: {selectionOptions.outlineWidth?.toFixed(3)}
                  </label>
                  <input
                    type="range"
                    min="0.01"
                    max="0.1"
                    step="0.005"
                    value={selectionOptions.outlineWidth || 0.05}
                    onChange={(e) => handleSelectionOptionChange('outlineWidth', parseFloat(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>

                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Glow Intensity: {selectionOptions.glowIntensity?.toFixed(2)}
                  </label>
                  <input
                    type="range"
                    min="0.1"
                    max="2.0"
                    step="0.1"
                    value={selectionOptions.glowIntensity || 0.8}
                    onChange={(e) => handleSelectionOptionChange('glowIntensity', parseFloat(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>

                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Animation Duration: {selectionOptions.animationDuration}ms
                  </label>
                  <input
                    type="range"
                    min="100"
                    max="1000"
                    step="50"
                    value={selectionOptions.animationDuration || 300}
                    onChange={(e) => handleSelectionOptionChange('animationDuration', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Object Browser */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">Object Browser</h4>
          
          <div className="space-y-3">
            {/* Object Statistics */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-2">Object Types</div>
              <div className="grid grid-cols-2 gap-2 text-xs">
                <div>
                  <span className="text-text-tertiary">Ships:</span>
                  <span className="ml-1 text-text-primary">{stats.ships}</span>
                </div>
                <div>
                  <span className="text-text-tertiary">Modules:</span>
                  <span className="ml-1 text-text-primary">{stats.modules}</span>
                </div>
                <div>
                  <span className="text-text-tertiary">Hardpoints:</span>
                  <span className="ml-1 text-text-primary">{stats.hardpoints}</span>
                </div>
                <div>
                  <span className="text-text-tertiary">Components:</span>
                  <span className="ml-1 text-text-primary">{stats.components}</span>
                </div>
              </div>
            </div>

            {/* Object List */}
            {stats.total > 0 && (
              <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
                <div className="text-sm text-text-secondary mb-2">Available Objects</div>
                <div className="max-h-32 overflow-y-auto space-y-1">
                  {Array.from(selectableObjects.entries()).map(([id, obj]) => {
                    const isSelected = selectedObjects.includes(obj);
                    return (
                      <div
                        key={id}
                        className={cn(
                          'flex items-center justify-between p-2 rounded cursor-pointer transition-colors',
                          isSelected 
                            ? 'bg-accent-primary/20 border border-accent-primary/50' 
                            : 'bg-primary-surface hover:bg-primary-surface/70 border border-transparent'
                        )}
                        onClick={() => handleSelectObject(id)}
                      >
                        <div className="flex-1 min-w-0">
                          <div className="text-xs font-medium text-text-primary truncate">
                            {obj.metadata.name}
                          </div>
                          <div className="text-xs text-text-tertiary">
                            {obj.type}
                          </div>
                        </div>
                        {isSelected && (
                          <div className="w-2 h-2 bg-accent-primary rounded-full" />
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>
            )}

            {stats.total === 0 && (
              <div className="text-center py-4 text-text-tertiary text-sm">
                No selectable objects found. Load a ship model to see selectable components.
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Selection Actions */}
      <div className="mt-6 flex items-center gap-2">
        <Button
          variant="secondary"
          size="sm"
          onClick={handleClearSelection}
          disabled={!sceneManager.isInitialized || selectedObjects.length === 0}
        >
          Clear Selection
        </Button>
        
        <Button
          variant="primary"
          size="sm"
          onClick={handleFocusOnSelection}
          disabled={!sceneManager.isInitialized || selectedObjects.length === 0}
        >
          Focus on Selection
        </Button>
      </div>

      {/* Selected Objects Info */}
      {selectedObjects.length > 0 && (
        <div className="mt-4 p-3 bg-primary-bg rounded border border-primary-border/50">
          <div className="text-sm text-text-secondary mb-2">Selected Objects ({selectedObjects.length})</div>
          <div className="space-y-1">
            {selectedObjects.map((obj, index) => (
              <div key={`${obj.id}_${index}`} className="flex items-center gap-2 text-xs">
                <div className="w-1 h-1 bg-accent-primary rounded-full" />
                <span className="font-medium text-text-primary">{obj.metadata.name}</span>
                <span className="text-text-tertiary">({obj.type})</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Info */}
      <div className="mt-6 p-3 bg-primary-bg rounded border border-primary-border/50">
        <p className="text-xs text-text-tertiary">
          <strong>Selection Features:</strong> Click to select objects, Ctrl+Click for multi-select, 
          outline/glow/highlight effects, hover feedback, animation on selection, focus camera on selection, 
          and real-time parameter adjustment. Works with ships, modules, hardpoints, and components.
        </p>
      </div>
    </div>
  );
};
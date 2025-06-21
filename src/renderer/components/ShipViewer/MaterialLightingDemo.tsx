/**
 * Material and Lighting Demo Component
 * Showcases advanced PBR materials and dynamic lighting system
 */

import React, { useState, useEffect } from 'react';
import { Button, Select, LoadingSpinner } from '../UI';
import { useSceneManager } from '../../hooks';
import { MaterialPreset, LightingPreset, DynamicLightingState } from '../../services/babylon';
import { cn } from '../../utils';

export const MaterialLightingDemo: React.FC = () => {
  const [selectedMaterialPreset, setSelectedMaterialPreset] = useState('amarr_hull');
  const [selectedLightingPreset, setSelectedLightingPreset] = useState('space_default');
  const [materialPresets, setMaterialPresets] = useState<Map<string, MaterialPreset>>(new Map());
  const [lightingPresets, setLightingPresets] = useState<Map<string, LightingPreset>>(new Map());
  const [dynamicLighting, setDynamicLighting] = useState<DynamicLightingState>({
    timeOfDay: 0.5,
    intensity: 1.0,
    temperature: 0.5,
    contrast: 1.0,
    enableDynamicShadows: true,
    enableVolumetricLighting: false,
  });
  const [isAnimating, setIsAnimating] = useState(false);

  const sceneManager = useSceneManager({
    autoInitialize: false,
  });

  // Load presets when scene manager is ready
  useEffect(() => {
    if (sceneManager.isInitialized) {
      const matPresets = sceneManager.sceneState ? 
        (sceneManager as any).getMaterialPresets() : new Map();
      const lightPresets = sceneManager.sceneState ? 
        (sceneManager as any).getLightingPresets() : new Map();
      
      setMaterialPresets(matPresets);
      setLightingPresets(lightPresets);
    }
  }, [sceneManager.isInitialized]);

  const handleApplyMaterialPreset = () => {
    if (!sceneManager.isInitialized || !sceneManager.sceneState?.activeShip) return;

    // Apply material preset to active ship
    (sceneManager as any).applyMaterialPreset(
      sceneManager.sceneState.activeShip,
      selectedMaterialPreset
    );
  };

  const handleApplyLightingPreset = () => {
    if (!sceneManager.isInitialized) return;

    (sceneManager as any).applyLightingPreset(selectedLightingPreset);
  };

  const handleDynamicLightingChange = (property: keyof DynamicLightingState, value: any) => {
    const newState = { ...dynamicLighting, [property]: value };
    setDynamicLighting(newState);

    if (sceneManager.isInitialized) {
      (sceneManager as any).updateDynamicLighting({ [property]: value });
    }
  };

  const handleStartLightingAnimation = () => {
    if (!sceneManager.isInitialized) return;

    const lightingSystem = (sceneManager as any).getLightingSystem();
    if (lightingSystem) {
      if (isAnimating) {
        lightingSystem.stopLightingAnimation();
        setIsAnimating(false);
      } else {
        lightingSystem.startLightingAnimation(30000);
        setIsAnimating(true);
      }
    }
  };

  const materialPresetOptions = Array.from(materialPresets.entries()).map(([key, preset]) => ({
    value: key,
    label: preset.name,
    description: preset.description,
  }));

  const lightingPresetOptions = Array.from(lightingPresets.entries()).map(([key, preset]) => ({
    value: key,
    label: preset.name,
    description: preset.description,
  }));

  return (
    <div className="p-6 bg-primary-surface rounded-lg border border-primary-border">
      <div className="mb-6">
        <h3 className="text-xl font-semibold text-text-primary mb-2">
          Advanced Materials & Lighting
        </h3>
        <p className="text-sm text-text-secondary">
          Showcase PBR materials and dynamic lighting system optimized for EVE Online ships.
        </p>
      </div>

      {/* Scene Status */}
      <div className="mb-6 p-4 bg-primary-bg rounded border border-primary-border/50">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-text-primary">Systems Status</span>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <div className={cn(
                'w-2 h-2 rounded-full',
                sceneManager.isInitialized ? 'bg-status-success' : 'bg-status-error'
              )} />
              <span className="text-xs text-text-secondary">Scene</span>
            </div>
            <div className="flex items-center gap-2">
              <div className={cn(
                'w-2 h-2 rounded-full',
                materialPresets.size > 0 ? 'bg-status-success' : 'bg-status-warning'
              )} />
              <span className="text-xs text-text-secondary">Materials</span>
            </div>
            <div className="flex items-center gap-2">
              <div className={cn(
                'w-2 h-2 rounded-full',
                lightingPresets.size > 0 ? 'bg-status-success' : 'bg-status-warning'
              )} />
              <span className="text-xs text-text-secondary">Lighting</span>
            </div>
          </div>
        </div>
        
        {sceneManager.sceneState && (
          <div className="text-xs text-text-tertiary">
            Active Ship: {sceneManager.sceneState.activeShip || 'None'} | 
            Models: {sceneManager.loadedModels.size} | 
            Animation: {isAnimating ? 'Running' : 'Stopped'}
          </div>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Material Controls */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">PBR Materials</h4>
          
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Material Preset
              </label>
              <select
                value={selectedMaterialPreset}
                onChange={(e) => setSelectedMaterialPreset(e.target.value)}
                className="w-full px-3 py-2 bg-primary-bg border border-primary-border rounded-md text-text-primary focus:outline-none focus:ring-2 focus:ring-accent-primary"
                disabled={!sceneManager.isInitialized}
              >
                {materialPresetOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {materialPresets.get(selectedMaterialPreset) && (
                <p className="text-xs text-text-tertiary mt-1">
                  {materialPresets.get(selectedMaterialPreset)!.description}
                </p>
              )}
            </div>

            <Button
              variant="primary"
              size="sm"
              onClick={handleApplyMaterialPreset}
              disabled={!sceneManager.isInitialized || !sceneManager.sceneState?.activeShip}
              className="w-full"
            >
              Apply Material to Active Ship
            </Button>

            {/* Material Properties */}
            {materialPresets.get(selectedMaterialPreset) && (
              <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
                <div className="text-xs text-text-secondary mb-2">Material Properties:</div>
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div>
                    <span className="text-text-tertiary">Metallic:</span>
                    <span className="ml-1 text-text-primary">
                      {materialPresets.get(selectedMaterialPreset)!.metallicFactor.toFixed(2)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Roughness:</span>
                    <span className="ml-1 text-text-primary">
                      {materialPresets.get(selectedMaterialPreset)!.roughnessFactor.toFixed(2)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Emissive:</span>
                    <span className="ml-1 text-text-primary">
                      {materialPresets.get(selectedMaterialPreset)!.emissiveIntensity.toFixed(2)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Environment:</span>
                    <span className="ml-1 text-text-primary">
                      {(materialPresets.get(selectedMaterialPreset)!.environmentIntensity || 1).toFixed(2)}
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Lighting Controls */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">Dynamic Lighting</h4>
          
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Lighting Preset
              </label>
              <select
                value={selectedLightingPreset}
                onChange={(e) => setSelectedLightingPreset(e.target.value)}
                className="w-full px-3 py-2 bg-primary-bg border border-primary-border rounded-md text-text-primary focus:outline-none focus:ring-2 focus:ring-accent-primary"
                disabled={!sceneManager.isInitialized}
              >
                {lightingPresetOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {lightingPresets.get(selectedLightingPreset) && (
                <p className="text-xs text-text-tertiary mt-1">
                  {lightingPresets.get(selectedLightingPreset)!.description}
                </p>
              )}
            </div>

            <Button
              variant="primary"
              size="sm"
              onClick={handleApplyLightingPreset}
              disabled={!sceneManager.isInitialized}
              className="w-full"
            >
              Apply Lighting Preset
            </Button>

            {/* Dynamic Lighting Controls */}
            <div className="space-y-3 p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-xs text-text-secondary mb-2">Dynamic Controls:</div>
              
              <div>
                <label className="block text-xs text-text-tertiary mb-1">
                  Intensity: {dynamicLighting.intensity.toFixed(2)}
                </label>
                <input
                  type="range"
                  min="0.1"
                  max="2.0"
                  step="0.1"
                  value={dynamicLighting.intensity}
                  onChange={(e) => handleDynamicLightingChange('intensity', parseFloat(e.target.value))}
                  className="w-full"
                  disabled={!sceneManager.isInitialized}
                />
              </div>

              <div>
                <label className="block text-xs text-text-tertiary mb-1">
                  Temperature: {dynamicLighting.temperature.toFixed(2)}
                </label>
                <input
                  type="range"
                  min="0"
                  max="1"
                  step="0.05"
                  value={dynamicLighting.temperature}
                  onChange={(e) => handleDynamicLightingChange('temperature', parseFloat(e.target.value))}
                  className="w-full"
                  disabled={!sceneManager.isInitialized}
                />
                <div className="flex justify-between text-xs text-text-tertiary mt-1">
                  <span>Cool</span>
                  <span>Warm</span>
                </div>
              </div>

              <div>
                <label className="block text-xs text-text-tertiary mb-1">
                  Contrast: {dynamicLighting.contrast.toFixed(2)}
                </label>
                <input
                  type="range"
                  min="0.5"
                  max="2.0"
                  step="0.1"
                  value={dynamicLighting.contrast}
                  onChange={(e) => handleDynamicLightingChange('contrast', parseFloat(e.target.value))}
                  className="w-full"
                  disabled={!sceneManager.isInitialized}
                />
              </div>

              <Button
                variant={isAnimating ? "outline" : "secondary"}
                size="sm"
                onClick={handleStartLightingAnimation}
                disabled={!sceneManager.isInitialized}
                className="w-full"
              >
                {isAnimating ? 'Stop Animation' : 'Start Light Animation'}
              </Button>
            </div>
          </div>
        </div>
      </div>

      {/* Presets Info */}
      <div className="mt-6 p-3 bg-primary-bg rounded border border-primary-border/50">
        <p className="text-xs text-text-tertiary">
          <strong>Available Features:</strong> EVE faction materials (Amarr, Caldari, Gallente, Minmatar), 
          advanced PBR with environment mapping, dynamic lighting presets (Space, Hangar, Nebula, Combat), 
          real-time parameter adjustment, and animated lighting cycles.
        </p>
      </div>
    </div>
  );
};
/**
 * Ship Viewer Component
 * Complete ship visualization interface with controls
 */

import React, { useState } from 'react';
import { BabylonCanvas } from './BabylonCanvas';
import { ModelLoadDemo } from './ModelLoadDemo';
import { MaterialLightingDemo } from './MaterialLightingDemo';
import { CameraControlsDemo } from './CameraControlsDemo';
import { Button } from '../UI';
import { useSceneManager } from '../../hooks';
import { cn } from '../../utils';

interface ShipViewerProps {
  className?: string;
  width?: string | number;
  height?: string | number;
  showControls?: boolean;
}

export const ShipViewer: React.FC<ShipViewerProps> = ({
  className,
  width = '100%',
  height = '600px',
  showControls = true,
}) => {
  const [engineReady, setEngineReady] = useState(false);
  const [sceneReady, setSceneReady] = useState(false);

  const sceneManager = useSceneManager({
    autoInitialize: false,
  });

  const handleEngineReady = () => {
    setEngineReady(true);
    console.log('3D Engine initialized');
  };

  const handleSceneReady = () => {
    setSceneReady(true);
    console.log('Scene manager ready');
  };

  const handleError = (error: Error) => {
    console.error('Ship viewer error:', error);
  };

  // Demo functions for testing scene management
  const handleLoadDemoShip = async () => {
    if (!sceneManager.isInitialized) return;

    // This would normally load a real ship model
    // For now, we'll create a simple demo placeholder
    console.log('Demo ship loading would happen here');
    
    // In the future, this will be:
    // await sceneManager.loadShip('rifter', '/assets/ships/rifter.glb', 'Rifter');
  };

  const handleResetCamera = () => {
    if (!sceneManager.isInitialized) return;
    
    // Reset camera to default position
    console.log('Camera reset');
  };

  const handleClearScene = () => {
    if (!sceneManager.isInitialized) return;
    
    sceneManager.clearScene();
    console.log('Scene cleared');
  };

  return (
    <div className={cn('relative flex flex-col gap-6', className)}>
      {/* 3D Canvas */}
      <div className="relative flex-1">
        <BabylonCanvas
          width={width}
          height={height}
          onEngineReady={handleEngineReady}
          onSceneReady={handleSceneReady}
          onError={handleError}
          enableShadows={true}
          antialias={true}
          className="rounded-lg border border-primary-border"
        />
      </div>

      {/* Model Loading Demo */}
      <ModelLoadDemo />

      {/* Material and Lighting Demo */}
      <MaterialLightingDemo />

      {/* Camera Controls Demo */}
      <CameraControlsDemo />

      {/* Controls Panel */}
      {showControls && (
        <div className="mt-4 p-4 bg-primary-surface rounded-lg border border-primary-border">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-lg font-semibold text-text-primary">
              Ship Viewer Controls
            </h3>
            <div className="flex items-center gap-2 text-sm text-text-secondary">
              <div className={cn(
                'w-2 h-2 rounded-full',
                engineReady ? 'bg-status-success' : 'bg-status-warning'
              )} />
              Engine: {engineReady ? 'Ready' : 'Loading'}
              <div className={cn(
                'w-2 h-2 rounded-full ml-4',
                sceneReady ? 'bg-status-success' : 'bg-status-warning'
              )} />
              Scene: {sceneReady ? 'Ready' : 'Loading'}
            </div>
          </div>

          <div className="flex flex-wrap gap-2">
            <Button
              variant="primary"
              size="sm"
              onClick={handleLoadDemoShip}
              disabled={!sceneReady}
            >
              Load Demo Ship
            </Button>
            
            <Button
              variant="secondary"
              size="sm"
              onClick={handleResetCamera}
              disabled={!sceneReady}
            >
              Reset Camera
            </Button>
            
            <Button
              variant="outline"
              size="sm"
              onClick={handleClearScene}
              disabled={!sceneReady}
            >
              Clear Scene
            </Button>
          </div>

          {/* Scene Information */}
          {sceneManager.sceneState && (
            <div className="mt-4 p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-2">Scene Information:</div>
              <div className="grid grid-cols-2 gap-4 text-xs">
                <div>
                  <span className="text-text-tertiary">Active Ship:</span>
                  <span className="ml-2 text-text-primary">
                    {sceneManager.sceneState.activeShip || 'None'}
                  </span>
                </div>
                <div>
                  <span className="text-text-tertiary">Selected:</span>
                  <span className="ml-2 text-text-primary">
                    {sceneManager.sceneState.selectedObjects.length} objects
                  </span>
                </div>
                <div>
                  <span className="text-text-tertiary">Loaded Models:</span>
                  <span className="ml-2 text-text-primary">
                    {sceneManager.loadedModels.size}
                  </span>
                </div>
                <div>
                  <span className="text-text-tertiary">Effects:</span>
                  <span className="ml-2 text-text-primary">
                    {Object.values(sceneManager.sceneState.effects).filter(Boolean).length} active
                  </span>
                </div>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
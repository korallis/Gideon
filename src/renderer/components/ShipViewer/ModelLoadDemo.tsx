/**
 * Model Loading Demo Component
 * Demonstrates 3D model loading capabilities with progress tracking
 */

import React, { useState } from 'react';
import { Button, LoadingSpinner } from '../UI';
import { useSceneManager } from '../../hooks';
import { ModelLoadProgress } from '../../services/babylon';
import { cn } from '../../utils';

interface DemoModel {
  id: string;
  name: string;
  path: string;
  description: string;
}

// Demo models (these would come from EVE assets in production)
const DEMO_MODELS: DemoModel[] = [
  {
    id: 'demo_cube',
    name: 'Test Cube',
    path: 'data:model/gltf+json;base64,', // Simple cube GLTF data would go here
    description: 'Simple test geometry for basic loading'
  },
  {
    id: 'demo_ship',
    name: 'Demo Ship',
    path: '/assets/models/demo-ship.glb', // Would be real ship model
    description: 'Example EVE Online ship model'
  }
];

export const ModelLoadDemo: React.FC = () => {
  const [loadingModel, setLoadingModel] = useState<string | null>(null);
  const [loadProgress, setLoadProgress] = useState<ModelLoadProgress | null>(null);
  const [selectedModel, setSelectedModel] = useState<string>('');
  
  const sceneManager = useSceneManager({
    autoInitialize: false,
  });

  const handleLoadModel = async (model: DemoModel) => {
    if (!sceneManager.isInitialized || loadingModel) return;

    setLoadingModel(model.id);
    setLoadProgress(null);

    try {
      const success = await sceneManager.loadShip(
        model.id,
        model.path,
        model.name,
        {
          enablePBR: true,
          optimizeForPerformance: true,
          enableAnimations: true,
        },
        (progress) => {
          setLoadProgress(progress);
        }
      );

      if (success) {
        // Focus camera on the loaded model
        sceneManager.focusOnShip(model.id);
        setSelectedModel(model.id);
      }
    } catch (error) {
      console.error('Failed to load model:', error);
    } finally {
      setLoadingModel(null);
      setLoadProgress(null);
    }
  };

  const handleRemoveModel = (modelId: string) => {
    if (!sceneManager.isInitialized) return;
    
    sceneManager.removeShip(modelId);
    if (selectedModel === modelId) {
      setSelectedModel('');
    }
  };

  const handleClearAll = () => {
    if (!sceneManager.isInitialized) return;
    
    sceneManager.clearScene();
    setSelectedModel('');
  };

  const getStageColor = (stage: string) => {
    switch (stage) {
      case 'downloading': return 'text-blue-400';
      case 'parsing': return 'text-yellow-400';
      case 'processing': return 'text-orange-400';
      case 'optimizing': return 'text-purple-400';
      case 'complete': return 'text-green-400';
      default: return 'text-text-secondary';
    }
  };

  return (
    <div className="p-6 bg-primary-surface rounded-lg border border-primary-border">
      <div className="mb-6">
        <h3 className="text-xl font-semibold text-text-primary mb-2">
          3D Model Loading Demo
        </h3>
        <p className="text-sm text-text-secondary">
          Test the advanced model loading system with progress tracking and optimization.
        </p>
      </div>

      {/* Scene Status */}
      <div className="mb-6 p-4 bg-primary-bg rounded border border-primary-border/50">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-text-primary">Scene Status</span>
          <div className="flex items-center gap-2">
            <div className={cn(
              'w-2 h-2 rounded-full',
              sceneManager.isInitialized ? 'bg-status-success' : 'bg-status-error'
            )} />
            <span className="text-xs text-text-secondary">
              {sceneManager.isInitialized ? 'Ready' : 'Not Initialized'}
            </span>
          </div>
        </div>
        
        {sceneManager.sceneState && (
          <div className="grid grid-cols-3 gap-4 text-xs">
            <div>
              <span className="text-text-tertiary">Active Ship:</span>
              <div className="text-text-primary font-mono">
                {sceneManager.sceneState.activeShip || 'None'}
              </div>
            </div>
            <div>
              <span className="text-text-tertiary">Models Loaded:</span>
              <div className="text-text-primary font-mono">
                {sceneManager.loadedModels.size}
              </div>
            </div>
            <div>
              <span className="text-text-tertiary">Selected:</span>
              <div className="text-text-primary font-mono">
                {sceneManager.sceneState.selectedObjects.length}
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Loading Progress */}
      {loadProgress && (
        <div className="mb-6 p-4 bg-primary-bg rounded border border-primary-border/50">
          <div className="flex items-center justify-between mb-2">
            <span className="text-sm font-medium text-text-primary">Loading Progress</span>
            <span className="text-xs text-text-secondary">{loadProgress.progress}%</span>
          </div>
          
          <div className="mb-2">
            <div className="w-full bg-primary-border rounded-full h-2">
              <div 
                className="bg-accent-primary h-2 rounded-full transition-all duration-300"
                style={{ width: `${loadProgress.progress}%` }}
              />
            </div>
          </div>
          
          <div className="flex items-center gap-2 text-xs">
            <span className="text-text-tertiary">Stage:</span>
            <span className={cn('font-medium capitalize', getStageColor(loadProgress.stage))}>
              {loadProgress.stage}
            </span>
            <span className="text-text-secondary">•</span>
            <span className="text-text-secondary">{loadProgress.message}</span>
          </div>
        </div>
      )}

      {/* Demo Models */}
      <div className="space-y-4">
        <h4 className="text-lg font-medium text-text-primary">Demo Models</h4>
        
        {DEMO_MODELS.map((model) => {
          const isLoaded = sceneManager.loadedModels.has(model.id);
          const isLoading = loadingModel === model.id;
          const isSelected = selectedModel === model.id;
          
          return (
            <div 
              key={model.id}
              className={cn(
                'p-4 rounded-lg border transition-colors',
                isSelected 
                  ? 'border-accent-primary bg-accent-primary/10' 
                  : 'border-primary-border bg-primary-bg hover:bg-primary-surface/50'
              )}
            >
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1">
                    <h5 className="font-medium text-text-primary">{model.name}</h5>
                    {isLoaded && (
                      <span className="px-2 py-0.5 text-xs bg-status-success/20 text-status-success rounded">
                        Loaded
                      </span>
                    )}
                    {isSelected && (
                      <span className="px-2 py-0.5 text-xs bg-accent-primary/20 text-accent-primary rounded">
                        Active
                      </span>
                    )}
                  </div>
                  <p className="text-sm text-text-secondary">{model.description}</p>
                  <p className="text-xs text-text-tertiary font-mono mt-1">{model.id}</p>
                </div>
                
                <div className="flex items-center gap-2">
                  {isLoading ? (
                    <div className="flex items-center gap-2">
                      <LoadingSpinner size="sm" />
                      <span className="text-xs text-text-secondary">Loading...</span>
                    </div>
                  ) : (
                    <>
                      {!isLoaded ? (
                        <Button
                          variant="primary"
                          size="sm"
                          onClick={() => handleLoadModel(model)}
                          disabled={!sceneManager.isInitialized}
                        >
                          Load Model
                        </Button>
                      ) : (
                        <>
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => sceneManager.focusOnShip(model.id)}
                          >
                            Focus
                          </Button>
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => handleRemoveModel(model.id)}
                          >
                            Remove
                          </Button>
                        </>
                      )}
                    </>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Actions */}
      <div className="mt-6 flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          onClick={handleClearAll}
          disabled={!sceneManager.isInitialized || sceneManager.loadedModels.size === 0}
        >
          Clear All Models
        </Button>
        
        <Button
          variant="secondary"
          size="sm"
          onClick={() => sceneManager.refreshState()}
          disabled={!sceneManager.isInitialized}
        >
          Refresh State
        </Button>
      </div>

      {/* Info */}
      <div className="mt-4 p-3 bg-primary-bg rounded border border-primary-border/50">
        <p className="text-xs text-text-tertiary">
          <strong>Note:</strong> In production, this would load real EVE Online ship models from the game's assets.
          The demo system supports GLTF, GLB, and OBJ formats with automatic optimization and progress tracking.
        </p>
      </div>
    </div>
  );
};
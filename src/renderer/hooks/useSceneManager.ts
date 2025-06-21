/**
 * React Hook for 3D Scene Management
 * Provides React integration for Babylon.js scene management
 */

import { useState, useEffect, useCallback, useRef } from 'react';
import { sceneManager, SceneState, ShipModel } from '../services/babylon';

export interface UseSceneManagerOptions {
  autoInitialize?: boolean;
  onSceneReady?: () => void;
  onError?: (error: Error) => void;
}

export interface UseSceneManagerReturn {
  // State
  isInitialized: boolean;
  isLoading: boolean;
  error: string | null;
  sceneState: SceneState | null;
  loadedModels: Map<string, ShipModel>;
  
  // Actions
  initializeScene: () => Promise<boolean>;
  loadShip: (shipId: string, modelPath: string, shipName: string) => Promise<boolean>;
  focusOnShip: (shipId: string) => void;
  selectObject: (objectId: string) => void;
  clearSelection: () => void;
  setObjectVisibility: (objectId: string, visible: boolean) => void;
  removeShip: (shipId: string) => void;
  clearScene: () => void;
  
  // Utilities
  refreshState: () => void;
}

export const useSceneManager = (options: UseSceneManagerOptions = {}): UseSceneManagerReturn => {
  const {
    autoInitialize = true,
    onSceneReady,
    onError,
  } = options;

  // State
  const [isInitialized, setIsInitialized] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [sceneState, setSceneState] = useState<SceneState | null>(null);
  const [loadedModels, setLoadedModels] = useState<Map<string, ShipModel>>(new Map());

  // Refs to track component mount state
  const mountedRef = useRef(true);

  // Initialize scene manager
  const initializeScene = useCallback(async (): Promise<boolean> => {
    if (!mountedRef.current) return false;

    setIsLoading(true);
    setError(null);

    try {
      const success = sceneManager.initialize();
      
      if (!mountedRef.current) return false;

      if (success) {
        setIsInitialized(true);
        setSceneState(sceneManager.getSceneState());
        setLoadedModels(sceneManager.getLoadedModels());
        onSceneReady?.();
        console.log('Scene manager initialized via hook');
      } else {
        throw new Error('Failed to initialize scene manager');
      }

      return success;
    } catch (err) {
      if (!mountedRef.current) return false;

      const error = err instanceof Error ? err : new Error('Unknown scene initialization error');
      setError(error.message);
      onError?.(error);
      console.error('Scene manager initialization failed:', error);
      return false;
    } finally {
      if (mountedRef.current) {
        setIsLoading(false);
      }
    }
  }, [onSceneReady, onError]);

  // Load ship model
  const loadShip = useCallback(async (
    shipId: string,
    modelPath: string,
    shipName: string
  ): Promise<boolean> => {
    if (!isInitialized) {
      console.warn('Cannot load ship: Scene manager not initialized');
      return false;
    }

    setIsLoading(true);
    setError(null);

    try {
      const shipModel = await sceneManager.loadShipModel(shipId, modelPath, shipName);
      
      if (!mountedRef.current) return false;

      if (shipModel) {
        // Refresh state
        setSceneState(sceneManager.getSceneState());
        setLoadedModels(sceneManager.getLoadedModels());
        console.log(`Ship loaded via hook: ${shipName}`);
        return true;
      } else {
        throw new Error(`Failed to load ship: ${shipName}`);
      }
    } catch (err) {
      if (!mountedRef.current) return false;

      const error = err instanceof Error ? err : new Error('Unknown ship loading error');
      setError(error.message);
      onError?.(error);
      return false;
    } finally {
      if (mountedRef.current) {
        setIsLoading(false);
      }
    }
  }, [isInitialized, onError]);

  // Focus on ship
  const focusOnShip = useCallback((shipId: string) => {
    if (!isInitialized) return;

    sceneManager.focusOnShip(shipId);
    setSceneState(sceneManager.getSceneState());
  }, [isInitialized]);

  // Select object
  const selectObject = useCallback((objectId: string) => {
    if (!isInitialized) return;

    sceneManager.selectObject(objectId);
    setSceneState(sceneManager.getSceneState());
  }, [isInitialized]);

  // Clear selection
  const clearSelection = useCallback(() => {
    if (!isInitialized) return;

    sceneManager.clearSelection();
    setSceneState(sceneManager.getSceneState());
  }, [isInitialized]);

  // Set object visibility
  const setObjectVisibility = useCallback((objectId: string, visible: boolean) => {
    if (!isInitialized) return;

    sceneManager.setObjectVisibility(objectId, visible);
    setSceneState(sceneManager.getSceneState());
  }, [isInitialized]);

  // Remove ship
  const removeShip = useCallback((shipId: string) => {
    if (!isInitialized) return;

    sceneManager.removeShipModel(shipId);
    setSceneState(sceneManager.getSceneState());
    setLoadedModels(sceneManager.getLoadedModels());
  }, [isInitialized]);

  // Clear scene
  const clearScene = useCallback(() => {
    if (!isInitialized) return;

    sceneManager.clearScene();
    setSceneState(sceneManager.getSceneState());
    setLoadedModels(sceneManager.getLoadedModels());
  }, [isInitialized]);

  // Refresh state from scene manager
  const refreshState = useCallback(() => {
    if (!isInitialized) return;

    setSceneState(sceneManager.getSceneState());
    setLoadedModels(sceneManager.getLoadedModels());
  }, [isInitialized]);

  // Auto-initialize on mount
  useEffect(() => {
    if (autoInitialize && !isInitialized && !isLoading) {
      initializeScene();
    }
  }, [autoInitialize, isInitialized, isLoading, initializeScene]);

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      mountedRef.current = false;
    };
  }, []);

  return {
    // State
    isInitialized,
    isLoading,
    error,
    sceneState,
    loadedModels,
    
    // Actions
    initializeScene,
    loadShip,
    focusOnShip,
    selectObject,
    clearSelection,
    setObjectVisibility,
    removeShip,
    clearScene,
    
    // Utilities
    refreshState,
  };
};
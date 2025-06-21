/**
 * Camera Controls Demo Component
 * Showcases advanced camera controls and scene navigation
 */

import React, { useState, useEffect } from 'react';
import { Button, LoadingSpinner } from '../UI';
import { useSceneManager } from '../../hooks';
import { CameraPreset, CameraControls, ViewportBookmark } from '../../services/babylon';
import { cn } from '../../utils';

export const CameraControlsDemo: React.FC = () => {
  const [selectedPreset, setSelectedPreset] = useState('isometric');
  const [cameraPresets, setCameraPresets] = useState<Map<string, CameraPreset>>(new Map());
  const [bookmarks, setBookmarks] = useState<Map<string, ViewportBookmark>>(new Map());
  const [isAutoRotating, setIsAutoRotating] = useState(false);
  const [bookmarkName, setBookmarkName] = useState('');
  const [controls, setControls] = useState<CameraControls>({
    panSensibility: 1000,
    wheelPrecision: 50,
    pinchPrecision: 200,
    angularSensibilityX: 1000,
    angularSensibilityY: 1000,
    minZ: 0.1,
    maxZ: 1000,
    lowerRadiusLimit: 5,
    upperRadiusLimit: 200,
    lowerBetaLimit: 0.1,
    upperBetaLimit: Math.PI - 0.1,
    enableAutoRotation: false,
    autoRotationSpeed: 0.5,
  });

  const sceneManager = useSceneManager({
    autoInitialize: false,
  });

  // Load presets and bookmarks when scene manager is ready
  useEffect(() => {
    if (sceneManager.isInitialized) {
      const presets = (sceneManager as any).getCameraPresets();
      const savedBookmarks = (sceneManager as any).getCameraBookmarks();
      
      setCameraPresets(presets);
      setBookmarks(savedBookmarks);

      // Get current controls from camera system
      const cameraSystem = (sceneManager as any).getCameraSystem();
      if (cameraSystem) {
        setControls(cameraSystem.getControls());
        setIsAutoRotating(cameraSystem.isAutoRotating());
      }
    }
  }, [sceneManager.isInitialized]);

  const handleApplyPreset = () => {
    if (!sceneManager.isInitialized) return;

    const activeShip = sceneManager.sceneState?.activeShip;
    (sceneManager as any).applyCameraPreset(selectedPreset, activeShip);
  };

  const handleControlChange = (property: keyof CameraControls, value: any) => {
    const newControls = { ...controls, [property]: value };
    setControls(newControls);

    if (sceneManager.isInitialized) {
      (sceneManager as any).updateCameraControls({ [property]: value });
    }
  };

  const handleAutoRotation = () => {
    if (!sceneManager.isInitialized) return;

    const cameraSystem = (sceneManager as any).getCameraSystem();
    if (cameraSystem) {
      if (isAutoRotating) {
        cameraSystem.stopAutoRotation();
        setIsAutoRotating(false);
      } else {
        cameraSystem.startAutoRotation(controls.autoRotationSpeed);
        setIsAutoRotating(true);
      }
    }
  };

  const handleCreateBookmark = () => {
    if (!sceneManager.isInitialized || !bookmarkName.trim()) return;

    const bookmarkId = `bookmark_${Date.now()}`;
    const bookmark = (sceneManager as any).createCameraBookmark(
      bookmarkId,
      bookmarkName.trim(),
      'User created bookmark'
    );

    if (bookmark) {
      setBookmarks(new Map(bookmarks.set(bookmarkId, bookmark)));
      setBookmarkName('');
    }
  };

  const handleApplyBookmark = (bookmarkId: string) => {
    if (!sceneManager.isInitialized) return;

    (sceneManager as any).applyCameraBookmark(bookmarkId);
  };

  const handleDeleteBookmark = (bookmarkId: string) => {
    if (!sceneManager.isInitialized) return;

    const cameraSystem = (sceneManager as any).getCameraSystem();
    if (cameraSystem && cameraSystem.removeBookmark(bookmarkId)) {
      const newBookmarks = new Map(bookmarks);
      newBookmarks.delete(bookmarkId);
      setBookmarks(newBookmarks);
    }
  };

  const handleResetCamera = () => {
    if (!sceneManager.isInitialized) return;

    const cameraSystem = (sceneManager as any).getCameraSystem();
    if (cameraSystem) {
      cameraSystem.reset(1000);
      if (isAutoRotating) {
        cameraSystem.stopAutoRotation();
        setIsAutoRotating(false);
      }
    }
  };

  const presetOptions = Array.from(cameraPresets.entries()).map(([key, preset]) => ({
    value: key,
    label: preset.name,
    description: preset.description,
  }));

  const formatAngle = (radians: number) => {
    return `${Math.round(radians * 180 / Math.PI)}°`;
  };

  return (
    <div className="p-6 bg-primary-surface rounded-lg border border-primary-border">
      <div className="mb-6">
        <h3 className="text-xl font-semibold text-text-primary mb-2">
          Camera Controls & Navigation
        </h3>
        <p className="text-sm text-text-secondary">
          Advanced camera system with smooth navigation, presets, and viewport bookmarks.
        </p>
      </div>

      {/* Status */}
      <div className="mb-6 p-4 bg-primary-bg rounded border border-primary-border/50">
        <div className="flex items-center justify-between mb-2">
          <span className="text-sm font-medium text-text-primary">Camera Status</span>
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <div className={cn(
                'w-2 h-2 rounded-full',
                sceneManager.isInitialized ? 'bg-status-success' : 'bg-status-error'
              )} />
              <span className="text-xs text-text-secondary">System</span>
            </div>
            <div className="flex items-center gap-2">
              <div className={cn(
                'w-2 h-2 rounded-full',
                isAutoRotating ? 'bg-accent-primary' : 'bg-status-warning'
              )} />
              <span className="text-xs text-text-secondary">Auto-Rotate</span>
            </div>
          </div>
        </div>
        
        <div className="text-xs text-text-tertiary">
          Presets: {cameraPresets.size} | Bookmarks: {bookmarks.size} | 
          Active Ship: {sceneManager.sceneState?.activeShip || 'None'}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Camera Presets */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">Camera Presets</h4>
          
          <div className="space-y-3">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                View Preset
              </label>
              <select
                value={selectedPreset}
                onChange={(e) => setSelectedPreset(e.target.value)}
                className="w-full px-3 py-2 bg-primary-bg border border-primary-border rounded-md text-text-primary focus:outline-none focus:ring-2 focus:ring-accent-primary"
                disabled={!sceneManager.isInitialized}
              >
                {presetOptions.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>
              {cameraPresets.get(selectedPreset) && (
                <p className="text-xs text-text-tertiary mt-1">
                  {cameraPresets.get(selectedPreset)!.description}
                </p>
              )}
            </div>

            <div className="flex gap-2">
              <Button
                variant="primary"
                size="sm"
                onClick={handleApplyPreset}
                disabled={!sceneManager.isInitialized}
                className="flex-1"
              >
                Apply Preset
              </Button>
              
              <Button
                variant="secondary"
                size="sm"
                onClick={handleResetCamera}
                disabled={!sceneManager.isInitialized}
              >
                Reset
              </Button>
            </div>

            {/* Preset Info */}
            {cameraPresets.get(selectedPreset) && (
              <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
                <div className="text-xs text-text-secondary mb-2">Preset Parameters:</div>
                <div className="grid grid-cols-2 gap-2 text-xs">
                  <div>
                    <span className="text-text-tertiary">Alpha:</span>
                    <span className="ml-1 text-text-primary">
                      {formatAngle(cameraPresets.get(selectedPreset)!.alpha)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Beta:</span>
                    <span className="ml-1 text-text-primary">
                      {formatAngle(cameraPresets.get(selectedPreset)!.beta)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Radius:</span>
                    <span className="ml-1 text-text-primary">
                      {cameraPresets.get(selectedPreset)!.radius.toFixed(1)}
                    </span>
                  </div>
                  <div>
                    <span className="text-text-tertiary">Duration:</span>
                    <span className="ml-1 text-text-primary">
                      {(cameraPresets.get(selectedPreset)!.animationDuration || 1000)}ms
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Camera Controls */}
        <div className="space-y-4">
          <h4 className="text-lg font-medium text-text-primary">Control Settings</h4>
          
          <div className="space-y-3">
            {/* Sensitivity Controls */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-3">Sensitivity</div>
              
              <div className="space-y-2">
                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Mouse Wheel: {controls.wheelPrecision}
                  </label>
                  <input
                    type="range"
                    min="10"
                    max="200"
                    step="10"
                    value={controls.wheelPrecision}
                    onChange={(e) => handleControlChange('wheelPrecision', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>

                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Rotation X: {controls.angularSensibilityX}
                  </label>
                  <input
                    type="range"
                    min="100"
                    max="3000"
                    step="100"
                    value={controls.angularSensibilityX}
                    onChange={(e) => handleControlChange('angularSensibilityX', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>

                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Rotation Y: {controls.angularSensibilityY}
                  </label>
                  <input
                    type="range"
                    min="100"
                    max="3000"
                    step="100"
                    value={controls.angularSensibilityY}
                    onChange={(e) => handleControlChange('angularSensibilityY', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
              </div>
            </div>

            {/* Distance Limits */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="text-sm text-text-secondary mb-3">Distance Limits</div>
              
              <div className="space-y-2">
                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Min Distance: {controls.lowerRadiusLimit}
                  </label>
                  <input
                    type="range"
                    min="1"
                    max="20"
                    step="1"
                    value={controls.lowerRadiusLimit}
                    onChange={(e) => handleControlChange('lowerRadiusLimit', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>

                <div>
                  <label className="block text-xs text-text-tertiary mb-1">
                    Max Distance: {controls.upperRadiusLimit}
                  </label>
                  <input
                    type="range"
                    min="50"
                    max="500"
                    step="10"
                    value={controls.upperRadiusLimit}
                    onChange={(e) => handleControlChange('upperRadiusLimit', parseInt(e.target.value))}
                    className="w-full"
                    disabled={!sceneManager.isInitialized}
                  />
                </div>
              </div>
            </div>

            {/* Auto-Rotation */}
            <div className="p-3 bg-primary-bg rounded border border-primary-border/50">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm text-text-secondary">Auto-Rotation</span>
                <Button
                  variant={isAutoRotating ? "outline" : "secondary"}
                  size="sm"
                  onClick={handleAutoRotation}
                  disabled={!sceneManager.isInitialized}
                >
                  {isAutoRotating ? 'Stop' : 'Start'}
                </Button>
              </div>
              
              <div>
                <label className="block text-xs text-text-tertiary mb-1">
                  Speed: {controls.autoRotationSpeed.toFixed(1)}
                </label>
                <input
                  type="range"
                  min="0.1"
                  max="2.0"
                  step="0.1"
                  value={controls.autoRotationSpeed}
                  onChange={(e) => handleControlChange('autoRotationSpeed', parseFloat(e.target.value))}
                  className="w-full"
                  disabled={!sceneManager.isInitialized}
                />
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Viewport Bookmarks */}
      <div className="mt-6 space-y-4">
        <h4 className="text-lg font-medium text-text-primary">Viewport Bookmarks</h4>
        
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="Bookmark name..."
            value={bookmarkName}
            onChange={(e) => setBookmarkName(e.target.value)}
            className="flex-1 px-3 py-2 bg-primary-bg border border-primary-border rounded-md text-text-primary focus:outline-none focus:ring-2 focus:ring-accent-primary"
            disabled={!sceneManager.isInitialized}
          />
          <Button
            variant="primary"
            size="sm"
            onClick={handleCreateBookmark}
            disabled={!sceneManager.isInitialized || !bookmarkName.trim()}
          >
            Save View
          </Button>
        </div>

        {bookmarks.size > 0 && (
          <div className="space-y-2 max-h-32 overflow-y-auto">
            {Array.from(bookmarks.entries()).map(([id, bookmark]) => (
              <div key={id} className="flex items-center justify-between p-2 bg-primary-bg rounded border border-primary-border/50">
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium text-text-primary truncate">
                    {bookmark.name}
                  </div>
                  <div className="text-xs text-text-tertiary">
                    {new Date(bookmark.timestamp).toLocaleTimeString()}
                  </div>
                </div>
                <div className="flex gap-1 ml-2">
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={() => handleApplyBookmark(id)}
                    disabled={!sceneManager.isInitialized}
                  >
                    Apply
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() => handleDeleteBookmark(id)}
                    disabled={!sceneManager.isInitialized}
                  >
                    Delete
                  </Button>
                </div>
              </div>
            ))}
          </div>
        )}

        {bookmarks.size === 0 && (
          <div className="text-center py-4 text-text-tertiary text-sm">
            No bookmarks saved. Create a bookmark to save your current camera position.
          </div>
        )}
      </div>

      {/* Info */}
      <div className="mt-6 p-3 bg-primary-bg rounded border border-primary-border/50">
        <p className="text-xs text-text-tertiary">
          <strong>Camera Features:</strong> Smooth preset transitions, customizable sensitivity controls, 
          distance limits, auto-rotation with adjustable speed, viewport bookmarks, and mouse/touch navigation. 
          Use mouse wheel to zoom, left-click drag to rotate, and right-click drag to pan.
        </p>
      </div>
    </div>
  );
};
/**
 * Babylon.js Canvas Component
 * React component wrapper for 3D ship visualization
 */

import React, { useRef, useEffect, useState } from 'react';
import { babylonEngine } from '../../services/babylon/engine';
import { LoadingSpinner } from '../UI';
import { cn } from '../../utils';

interface BabylonCanvasProps {
  width?: string | number;
  height?: string | number;
  className?: string;
  onEngineReady?: () => void;
  onError?: (error: Error) => void;
  enableShadows?: boolean;
  antialias?: boolean;
}

export const BabylonCanvas: React.FC<BabylonCanvasProps> = ({
  width = '100%',
  height = '400px',
  className,
  onEngineReady,
  onError,
  enableShadows = true,
  antialias = true,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [engineReady, setEngineReady] = useState(false);

  useEffect(() => {
    let mounted = true;

    const initializeEngine = async () => {
      if (!canvasRef.current) return;

      setIsLoading(true);
      setError(null);

      try {
        const success = await babylonEngine.initialize(
          canvasRef.current,
          {
            antialias,
            adaptToDeviceRatio: true,
            preserveDrawingBuffer: true,
            stencil: true,
          },
          {
            enableShadows,
            usePhysics: false,
          }
        );

        if (!mounted) return;

        if (success) {
          setEngineReady(true);
          onEngineReady?.();
        } else {
          throw new Error('Failed to initialize Babylon.js engine');
        }
      } catch (err) {
        if (!mounted) return;
        
        const error = err instanceof Error ? err : new Error('Unknown error occurred');
        setError(error.message);
        onError?.(error);
      } finally {
        if (mounted) {
          setIsLoading(false);
        }
      }
    };

    initializeEngine();

    // Cleanup on unmount
    return () => {
      mounted = false;
      babylonEngine.dispose();
    };
  }, [antialias, enableShadows, onEngineReady, onError]);

  // Handle canvas resize
  useEffect(() => {
    const handleResize = () => {
      if (engineReady && canvasRef.current) {
        const engine = babylonEngine.getEngine();
        if (engine) {
          engine.resize();
        }
      }
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [engineReady]);

  return (
    <div 
      className={cn('relative overflow-hidden bg-primary-bg', className)}
      style={{ width, height }}
    >
      {/* Canvas element */}
      <canvas
        ref={canvasRef}
        className="w-full h-full block"
        style={{ width, height }}
        onContextMenu={(e) => e.preventDefault()} // Disable right-click menu
      />

      {/* Loading overlay */}
      {isLoading && (
        <div className="absolute inset-0 flex items-center justify-center bg-primary-bg/80 backdrop-blur-sm">
          <div className="flex flex-col items-center gap-4">
            <LoadingSpinner size="lg" variant="glow" />
            <div className="text-sm text-text-secondary">
              Initializing 3D Engine...
            </div>
          </div>
        </div>
      )}

      {/* Error state */}
      {error && (
        <div className="absolute inset-0 flex items-center justify-center bg-primary-bg/90 backdrop-blur-sm">
          <div className="flex flex-col items-center gap-4 p-6 bg-primary-surface rounded-lg border border-status-error max-w-md mx-4">
            <div className="text-status-error text-lg font-semibold">
              3D Engine Error
            </div>
            <div className="text-sm text-text-secondary text-center">
              {error}
            </div>
            <button
              onClick={() => window.location.reload()}
              className="px-4 py-2 bg-status-error hover:bg-status-error/80 text-white rounded-md transition-colors"
            >
              Reload Application
            </button>
          </div>
        </div>
      )}

      {/* Debug info (development only) */}
      {process.env.NODE_ENV === 'development' && engineReady && (
        <div className="absolute top-2 left-2 text-xs text-text-tertiary bg-primary-surface/80 px-2 py-1 rounded">
          Babylon.js {engineReady ? 'Ready' : 'Loading'}
        </div>
      )}
    </div>
  );
};
/**
 * Route Guard Component
 * Handles authentication, authorization, and data preloading
 */

import React, { useEffect } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { useCharacterStore } from '../stores';
import { useDatabase } from '../database';
import { AppRoute } from './routes';
// import { LoadingSpinner } from '../components/UI/LoadingSpinner';

interface RouteGuardProps {
  children: React.ReactNode;
  route: AppRoute;
}

export const RouteGuard: React.FC<RouteGuardProps> = ({ children, route }) => {
  const location = useLocation();
  const { activeCharacter, isAuthenticated } = useCharacterStore();
  const { characters, isHealthy } = useDatabase();
  
  const { metadata } = route;

  // Check if database is healthy
  if (!isHealthy) {
    return (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className="flex flex-col items-center justify-center min-h-[50vh] space-y-4"
      >
        <div className="text-red-400 text-lg font-medium">Database Error</div>
        <div className="text-text-secondary text-sm">
          Failed to connect to local database. Please restart the application.
        </div>
      </motion.div>
    );
  }

  // Check authentication requirement
  if (metadata.requiresAuth && !isAuthenticated) {
    return (
      <Navigate 
        to="/settings/accounts" 
        state={{ from: location.pathname }}
        replace 
      />
    );
  }

  // Check character requirement
  if (metadata.requiresCharacter && !activeCharacter) {
    return (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className="flex flex-col items-center justify-center min-h-[50vh] space-y-6"
      >
        <div className="text-center">
          <h3 className="text-xl font-semibold text-text-primary mb-2">
            Character Required
          </h3>
          <p className="text-text-secondary mb-6">
            This feature requires an active character. Please authenticate and select a character.
          </p>
          <Navigate 
            to="/settings/accounts" 
            state={{ from: location.pathname }}
            replace 
          />
        </div>
      </motion.div>
    );
  }

  // Check permissions (if implemented)
  if (metadata.permissions && metadata.permissions.length > 0) {
    // TODO: Implement permission checking when ESI scopes are added
    const hasPermissions = true; // Placeholder
    
    if (!hasPermissions) {
      return (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="flex flex-col items-center justify-center min-h-[50vh] space-y-4"
        >
          <div className="text-yellow-400 text-lg font-medium">Insufficient Permissions</div>
          <div className="text-text-secondary text-sm text-center">
            This feature requires additional ESI permissions. Please update your character authentication.
          </div>
        </motion.div>
      );
    }
  }

  return <>{children}</>;
};

// Higher-order component for route protection
export const withRouteGuard = (Component: React.ComponentType, route: AppRoute) => {
  return (props: any) => (
    <RouteGuard route={route}>
      <Component {...props} />
    </RouteGuard>
  );
};

// Data preloader component
interface DataPreloaderProps {
  children: React.ReactNode;
  route: AppRoute;
}

export const DataPreloader: React.FC<DataPreloaderProps> = ({ children, route }) => {
  const [isPreloading, setIsPreloading] = React.useState(false);
  
  useEffect(() => {
    if (route.metadata.preloadData) {
      setIsPreloading(true);
      
      // Simulate data preloading
      const preloadData = async () => {
        try {
          // TODO: Implement actual data preloading based on route
          await new Promise(resolve => setTimeout(resolve, 500));
        } catch (error) {
          console.warn('Failed to preload data for route:', route.id, error);
        } finally {
          setIsPreloading(false);
        }
      };
      
      preloadData();
    }
  }, [route.id, route.metadata.preloadData]);

  if (isPreloading) {
    return (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className="flex items-center justify-center min-h-[50vh]"
      >
        <div className="text-center space-y-4">
          <motion.div
            animate={{ rotate: 360 }}
            transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
            className="w-8 h-8 border-2 border-gray-600 border-t-accent-blue rounded-full mx-auto"
          />
          <div className="text-text-secondary">Loading {route.metadata.title}...</div>
        </div>
      </motion.div>
    );
  }

  return <>{children}</>;
};
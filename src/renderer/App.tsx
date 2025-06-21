import React, { useEffect } from 'react';
import { motion } from 'framer-motion';
import { NotificationProvider } from './components/Notifications/NotificationProvider';
import { AppRouter, RouteProvider } from './router';
import { useAppStore, initializeStores } from './stores';
import { usePrefetchCriticalData } from './queries';

export const App: React.FC = () => {
  const { initializeApp, isInitialized } = useAppStore();
  const { prefetchAll } = usePrefetchCriticalData();

  useEffect(() => {
    // Initialize the application and all stores
    const initialize = async () => {
      await initializeStores();
      await initializeApp();
      
      // Prefetch critical data after initialization
      try {
        await prefetchAll();
      } catch (error) {
        console.warn('Failed to prefetch critical data:', error);
      }
    };
    
    initialize();
  }, [initializeApp, prefetchAll]);

  if (!isInitialized) {
    return (
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        className="flex items-center justify-center min-h-screen bg-primary-bg"
      >
        <div className="text-center">
          <motion.div
            animate={{ rotate: 360 }}
            transition={{ duration: 2, repeat: Infinity, ease: 'linear' }}
            className="w-16 h-16 border-4 border-accent-blue border-t-transparent rounded-full mx-auto mb-4"
          />
          <h2 className="text-xl font-semibold text-text-primary mb-2">
            Initializing Gideon
          </h2>
          <p className="text-text-secondary">
            Loading EVE Online data and preparing systems...
          </p>
        </div>
      </motion.div>
    );
  }

  return (
    <NotificationProvider>
      <RouteProvider>
        <AppRouter />
      </RouteProvider>
    </NotificationProvider>
  );
};
import React, { useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import { motion, AnimatePresence } from 'framer-motion';
import { Layout } from './components/Layout/Layout';
import { ShipFittingModule } from './components/ShipFitting/ShipFittingModule';
import { CharacterModule } from './components/Character/CharacterModule';
import { MarketModule } from './components/Market/MarketModule';
import { SettingsModule } from './components/Settings/SettingsModule';
import { NotificationProvider } from './components/Notifications/NotificationProvider';
import { useAppStore } from './stores/appStore';

export const App: React.FC = () => {
  const { initializeApp, isInitialized } = useAppStore();

  useEffect(() => {
    // Initialize the application
    initializeApp();
  }, [initializeApp]);

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
      <Layout>
        <AnimatePresence mode="wait">
          <Routes>
            <Route
              path="/"
              element={
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.3 }}
                >
                  <ShipFittingModule />
                </motion.div>
              }
            />
            <Route
              path="/fitting"
              element={
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.3 }}
                >
                  <ShipFittingModule />
                </motion.div>
              }
            />
            <Route
              path="/character"
              element={
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.3 }}
                >
                  <CharacterModule />
                </motion.div>
              }
            />
            <Route
              path="/market"
              element={
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.3 }}
                >
                  <MarketModule />
                </motion.div>
              }
            />
            <Route
              path="/settings"
              element={
                <motion.div
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -20 }}
                  transition={{ duration: 0.3 }}
                >
                  <SettingsModule />
                </motion.div>
              }
            />
          </Routes>
        </AnimatePresence>
      </Layout>
    </NotificationProvider>
  );
};
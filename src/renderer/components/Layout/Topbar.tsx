import React from 'react';
import { motion } from 'framer-motion';
import { useAppStore } from '../../stores';

export const Topbar: React.FC = () => {
  const { ui, updateUI } = useAppStore();

  const toggleSidebar = () => {
    updateUI({ sidebarOpen: !ui.sidebarOpen });
  };

  return (
    <div className="h-full bg-secondary-bg border-b border-border-primary px-6 flex items-center justify-between">
      {/* Left side */}
      <div className="flex items-center space-x-4">
        <motion.button
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          onClick={toggleSidebar}
          className="p-2 rounded-lg bg-border-primary hover:bg-border-accent transition-colors"
        >
          <svg
            className="w-5 h-5 text-text-secondary"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M4 6h16M4 12h16M4 18h16"
            />
          </svg>
        </motion.button>
        
        <div className="flex items-center space-x-2">
          <div className="w-2 h-2 bg-success-green rounded-full animate-pulse"></div>
          <span className="text-sm text-text-secondary">Online</span>
        </div>
      </div>
      
      {/* Center - Module title */}
      <div className="flex-1 flex justify-center">
        <h2 className="text-lg font-semibold text-text-primary capitalize">
          {ui.activeModule} Module
        </h2>
      </div>
      
      {/* Right side */}
      <div className="flex items-center space-x-4">
        {/* Character indicator */}
        <div className="flex items-center space-x-2 px-3 py-2 rounded-lg bg-border-primary">
          <div className="w-6 h-6 bg-accent-orange rounded-full flex items-center justify-center text-xs font-bold text-primary-bg">
            ?
          </div>
          <span className="text-sm text-text-secondary">No Character</span>
        </div>
        
        {/* Notifications */}
        <motion.button
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          className="relative p-2 rounded-lg bg-border-primary hover:bg-border-accent transition-colors"
        >
          <svg
            className="w-5 h-5 text-text-secondary"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M15 17h5l-5-5-5 5h5zm0 0v-1.5"
            />
          </svg>
          {ui.notifications.length > 0 && (
            <div className="absolute -top-1 -right-1 w-3 h-3 bg-error-red rounded-full"></div>
          )}
        </motion.button>
      </div>
    </div>
  );
};
import React from 'react';
import { motion } from 'framer-motion';

export const SettingsModule: React.FC = () => {
  return (
    <div className="h-full p-6 bg-primary-bg">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="h-full"
      >
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-text-primary mb-2">Settings</h1>
          <p className="text-text-secondary">
            Configure Gideon to your preferences
          </p>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-[calc(100%-8rem)]">
          {/* General Settings */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">General</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <span className="text-text-secondary">Auto Update</span>
                <div className="w-12 h-6 bg-accent-blue rounded-full relative">
                  <div className="w-5 h-5 bg-white rounded-full absolute top-0.5 right-0.5"></div>
                </div>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-text-secondary">Telemetry</span>
                <div className="w-12 h-6 bg-border-accent rounded-full relative">
                  <div className="w-5 h-5 bg-white rounded-full absolute top-0.5 left-0.5"></div>
                </div>
              </div>
            </div>
          </div>
          
          {/* Performance Settings */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Performance</h3>
            <div className="space-y-4">
              <div>
                <label className="text-text-secondary text-sm">Max Memory (MB)</label>
                <div className="mt-1 flex items-center space-x-2">
                  <div className="flex-1 h-2 bg-border-accent rounded-full">
                    <div className="w-1/2 h-full bg-accent-blue rounded-full"></div>
                  </div>
                  <span className="text-text-primary text-sm">500</span>
                </div>
              </div>
              <div>
                <label className="text-text-secondary text-sm">Render FPS</label>
                <div className="mt-1 flex items-center space-x-2">
                  <div className="flex-1 h-2 bg-border-accent rounded-full">
                    <div className="w-full h-full bg-success-green rounded-full"></div>
                  </div>
                  <span className="text-text-primary text-sm">60</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
};
import React from 'react';
import { motion } from 'framer-motion';

export const CharacterModule: React.FC = () => {
  return (
    <div className="h-full p-6 bg-primary-bg">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="h-full"
      >
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-text-primary mb-2">Character Management</h1>
          <p className="text-text-secondary">
            Manage your EVE characters and plan skill training
          </p>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-[calc(100%-8rem)]">
          {/* Character Info */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Character Information</h3>
            <div className="flex items-center justify-center h-32 border-2 border-dashed border-border-accent rounded-lg">
              <div className="text-center">
                <div className="text-4xl mb-2">👤</div>
                <p className="text-text-secondary">Connect EVE Character</p>
                <button className="mt-2 px-4 py-2 bg-accent-blue text-primary-bg rounded hover:bg-accent-orange transition-colors">
                  Authenticate with ESI
                </button>
              </div>
            </div>
          </div>
          
          {/* Skill Planning */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Skill Planning</h3>
            <div className="space-y-4">
              <div className="text-center text-text-secondary">
                <div className="text-4xl mb-2">📚</div>
                <p>Skill planning will be available after character authentication</p>
              </div>
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
};
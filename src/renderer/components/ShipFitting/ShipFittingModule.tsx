import React from 'react';
import { motion } from 'framer-motion';

export const ShipFittingModule: React.FC = () => {
  return (
    <div className="h-full p-6 bg-primary-bg">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="h-full"
      >
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-text-primary mb-2">Ship Fitting</h1>
          <p className="text-text-secondary">
            Design and optimize ship fittings with drag-and-drop interface
          </p>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 h-[calc(100%-8rem)]">
          {/* Ship Selection */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Ship Selection</h3>
            <div className="flex items-center justify-center h-32 border-2 border-dashed border-border-accent rounded-lg">
              <div className="text-center">
                <div className="text-4xl mb-2">🚀</div>
                <p className="text-text-secondary">Select a ship</p>
              </div>
            </div>
          </div>
          
          {/* Fitting Interface */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Fitting Interface</h3>
            <div className="space-y-4">
              {['High Slots', 'Mid Slots', 'Low Slots', 'Rig Slots'].map((slotType) => (
                <div key={slotType} className="">
                  <h4 className="text-sm font-medium text-text-secondary mb-2">{slotType}</h4>
                  <div className="grid grid-cols-4 gap-2">
                    {[...Array(4)].map((_, i) => (
                      <div
                        key={i}
                        className="w-12 h-12 border border-border-accent rounded bg-border-primary"
                      />
                    ))}
                  </div>
                </div>
              ))}
            </div>
          </div>
          
          {/* Statistics */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Statistics</h3>
            <div className="space-y-4">
              {[
                { label: 'DPS', value: '0', unit: 'damage/sec' },
                { label: 'Tank', value: '0', unit: 'EHP' },
                { label: 'Speed', value: '0', unit: 'm/s' },
                { label: 'Cap Stable', value: 'No', unit: '' },
              ].map((stat) => (
                <div key={stat.label} className="flex justify-between items-center">
                  <span className="text-text-secondary">{stat.label}</span>
                  <div className="text-right">
                    <div className="text-text-primary font-semibold">{stat.value}</div>
                    <div className="text-xs text-text-secondary">{stat.unit}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
};
import React from 'react';
import { motion } from 'framer-motion';

export const MarketModule: React.FC = () => {
  return (
    <div className="h-full p-6 bg-primary-bg">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="h-full"
      >
        <div className="mb-6">
          <h1 className="text-2xl font-bold text-text-primary mb-2">Market Analysis</h1>
          <p className="text-text-secondary">
            Real-time market data and trading opportunities
          </p>
        </div>
        
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 h-[calc(100%-8rem)]">
          {/* Market Overview */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Market Overview</h3>
            <div className="flex items-center justify-center h-32 border-2 border-dashed border-border-accent rounded-lg">
              <div className="text-center">
                <div className="text-4xl mb-2">📈</div>
                <p className="text-text-secondary">Market data loading...</p>
              </div>
            </div>
          </div>
          
          {/* Price Alerts */}
          <div className="bg-secondary-bg rounded-lg border border-border-primary p-4">
            <h3 className="text-lg font-semibold text-text-primary mb-4">Price Alerts</h3>
            <div className="space-y-4">
              <div className="text-center text-text-secondary">
                <div className="text-4xl mb-2">🔔</div>
                <p>Set up price alerts for items you're interested in</p>
              </div>
            </div>
          </div>
        </div>
      </motion.div>
    </div>
  );
};
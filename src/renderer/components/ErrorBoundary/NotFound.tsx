/**
 * 404 Not Found Component
 * Displays when a route is not found
 */

import React from 'react';
import { motion } from 'framer-motion';
import { useNavigate } from 'react-router-dom';

export const NotFound: React.FC = () => {
  const navigate = useNavigate();

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="flex flex-col items-center justify-center min-h-screen bg-primary-bg text-center"
    >
      <div className="max-w-md mx-auto space-y-6">
        {/* 404 Icon/Number */}
        <motion.div
          initial={{ scale: 0.8 }}
          animate={{ scale: 1 }}
          transition={{ delay: 0.2 }}
          className="text-8xl font-bold text-accent-blue opacity-50"
        >
          404
        </motion.div>

        {/* Title */}
        <motion.h1
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.3 }}
          className="text-3xl font-bold text-text-primary"
        >
          Page Not Found
        </motion.h1>

        {/* Description */}
        <motion.p
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.4 }}
          className="text-text-secondary text-lg"
        >
          The page you're looking for doesn't exist or has been moved.
        </motion.p>

        {/* Actions */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.5 }}
          className="flex flex-col sm:flex-row gap-4 justify-center"
        >
          <button
            onClick={() => navigate(-1)}
            className="px-6 py-3 bg-gray-600 hover:bg-gray-500 text-white rounded-lg transition-colors"
          >
            Go Back
          </button>
          <button
            onClick={() => navigate('/fitting')}
            className="px-6 py-3 bg-accent-blue hover:bg-blue-600 text-white rounded-lg transition-colors"
          >
            Go Home
          </button>
        </motion.div>

        {/* Navigation Links */}
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ delay: 0.6 }}
          className="pt-8 border-t border-gray-700"
        >
          <p className="text-text-secondary mb-4">Or navigate to:</p>
          <div className="flex flex-wrap gap-2 justify-center">
            {[
              { label: 'Ship Fitting', path: '/fitting' },
              { label: 'Character', path: '/character' },
              { label: 'Market', path: '/market' },
              { label: 'Settings', path: '/settings' },
            ].map((link) => (
              <button
                key={link.path}
                onClick={() => navigate(link.path)}
                className="px-4 py-2 text-accent-blue hover:text-blue-400 hover:bg-gray-800 rounded transition-colors"
              >
                {link.label}
              </button>
            ))}
          </div>
        </motion.div>
      </div>
    </motion.div>
  );
};
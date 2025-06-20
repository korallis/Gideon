import React from 'react';
import { NavLink } from 'react-router-dom';
import { motion } from 'framer-motion';

const navItems = [
  { path: '/fitting', label: 'Ship Fitting', icon: '🚀' },
  { path: '/character', label: 'Character', icon: '👤' },
  { path: '/market', label: 'Market', icon: '📈' },
  { path: '/settings', label: 'Settings', icon: '⚙️' },
];

export const Sidebar: React.FC = () => {
  return (
    <div className="h-full bg-secondary-bg flex flex-col">
      {/* Logo/Title */}
      <div className="p-6 border-b border-border-primary">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          className="flex items-center space-x-3"
        >
          <div className="w-8 h-8 bg-accent-blue rounded-lg flex items-center justify-center text-primary-bg font-bold">
            G
          </div>
          <div>
            <h1 className="text-lg font-semibold text-text-primary">Gideon</h1>
            <p className="text-xs text-text-secondary">AI Copilot</p>
          </div>
        </motion.div>
      </div>
      
      {/* Navigation */}
      <nav className="flex-1 p-4">
        <ul className="space-y-2">
          {navItems.map((item, index) => (
            <motion.li
              key={item.path}
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              transition={{ delay: index * 0.1 }}
            >
              <NavLink
                to={item.path}
                className={({ isActive }) =>
                  `flex items-center space-x-3 px-4 py-3 rounded-lg transition-all duration-200 ${
                    isActive
                      ? 'bg-accent-blue text-primary-bg'
                      : 'text-text-secondary hover:bg-border-primary hover:text-text-primary'
                  }`
                }
              >
                <span className="text-lg">{item.icon}</span>
                <span className="font-medium">{item.label}</span>
              </NavLink>
            </motion.li>
          ))}
        </ul>
      </nav>
      
      {/* Footer */}
      <div className="p-4 border-t border-border-primary">
        <div className="text-xs text-text-secondary text-center">
          v0.1.0 Alpha
        </div>
      </div>
    </div>
  );
};
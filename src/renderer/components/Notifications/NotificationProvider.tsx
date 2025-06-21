import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { useAppStore } from '../../stores';

interface NotificationProviderProps {
  children: React.ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const { ui, removeNotification } = useAppStore();

  return (
    <>
      {children}
      
      {/* Notification Container */}
      <div className="fixed top-4 right-4 z-notification space-y-2 max-w-sm">
        <AnimatePresence>
          {ui.notifications.map((notification: any) => (
            <motion.div
              key={notification.id}
              initial={{ opacity: 0, x: 300, scale: 0.3 }}
              animate={{ opacity: 1, x: 0, scale: 1 }}
              exit={{ opacity: 0, x: 300, scale: 0.5 }}
              transition={{ duration: 0.3 }}
              className={`p-4 rounded-lg bg-glass border backdrop-blur-md shadow-lg ${
                notification.type === 'error'
                  ? 'border-error-red'
                  : notification.type === 'success'
                  ? 'border-success-green'
                  : notification.type === 'warning'
                  ? 'border-warning-yellow'
                  : 'border-accent-blue'
              }`}
            >
              <div className="flex items-start space-x-3">
                <div className={`flex-shrink-0 w-5 h-5 rounded-full ${
                  notification.type === 'error'
                    ? 'bg-error-red'
                    : notification.type === 'success'
                    ? 'bg-success-green'
                    : notification.type === 'warning'
                    ? 'bg-warning-yellow'
                    : 'bg-accent-blue'
                }`} />
                
                <div className="flex-1 min-w-0">
                  <h4 className="text-sm font-semibold text-text-primary">
                    {notification.title}
                  </h4>
                  <p className="text-sm text-text-secondary mt-1">
                    {notification.message}
                  </p>
                </div>
                
                <button
                  onClick={() => removeNotification(notification.id)}
                  className="flex-shrink-0 text-text-secondary hover:text-text-primary transition-colors"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </motion.div>
          ))}
        </AnimatePresence>
      </div>
    </>
  );
};
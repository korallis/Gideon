import React from 'react';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { useAppStore } from '@/stores/appStore';

interface LayoutProps {
  children: React.ReactNode;
}

export const Layout: React.FC<LayoutProps> = ({ children }) => {
  const { ui } = useAppStore();

  return (
    <div className="flex h-screen bg-primary-bg text-text-primary overflow-hidden">
      {/* Sidebar */}
      {ui.sidebarOpen && (
        <div className="flex-shrink-0 w-64 border-r border-border-primary">
          <Sidebar />
        </div>
      )}
      
      {/* Main content area */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Top bar */}
        <div className="flex-shrink-0 h-16 border-b border-border-primary">
          <Topbar />
        </div>
        
        {/* Main content */}
        <div className="flex-1 overflow-hidden">
          {children}
        </div>
      </div>
    </div>
  );
};
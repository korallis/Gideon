/**
 * Application Router
 * Main routing component with error boundaries and route management
 */

import React from 'react';
import { createBrowserRouter, RouterProvider, Outlet, useLocation } from 'react-router-dom';
import { AnimatePresence } from 'framer-motion';
import { ErrorBoundary } from '../components/ErrorBoundary/ErrorBoundary';
import { Layout } from '../components/Layout/Layout';
import { appRoutes, convertToRouteObjects } from './routes';
import { RouteGuard, DataPreloader } from './RouteGuard';
import { NotFound } from '../components/ErrorBoundary/NotFound';

// Root layout component
const RootLayout: React.FC = () => {
  const location = useLocation();

  return (
    <ErrorBoundary>
      <Layout>
        <AnimatePresence mode="wait" initial={false}>
          <Outlet key={location.pathname} />
        </AnimatePresence>
      </Layout>
    </ErrorBoundary>
  );
};

// Create router with enhanced error handling
const createAppRouter = () => {
  const routes = convertToRouteObjects(appRoutes);
  
  return createBrowserRouter([
    {
      path: '/',
      element: <RootLayout />,
      errorElement: <NotFound />,
      children: routes,
    },
    {
      path: '*',
      element: <NotFound />,
    },
  ]);
};

// Router provider component
export const AppRouter: React.FC = () => {
  const router = React.useMemo(() => createAppRouter(), []);

  return (
    <ErrorBoundary>
      <RouterProvider router={router} />
    </ErrorBoundary>
  );
};

// Route context for accessing current route metadata
interface RouteContextValue {
  currentRoute?: any;
  breadcrumbs: any[];
  navigationRoutes: any[];
}

export const RouteContext = React.createContext<RouteContextValue>({
  breadcrumbs: [],
  navigationRoutes: [],
});

// Route provider for route metadata
export const RouteProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const location = useLocation();
  
  const contextValue = React.useMemo(() => {
    // Find current route and build breadcrumbs
    const currentRoute = appRoutes.find(route => 
      location.pathname.startsWith(route.path || '')
    );
    
    // TODO: Implement proper breadcrumb building
    const breadcrumbs: any[] = [];
    
    // Get navigation routes (top-level, non-hidden)
    const navigationRoutes = appRoutes.filter(route => 
      !route.metadata.hideInNav && !route.path?.includes(':')
    );

    return {
      currentRoute,
      breadcrumbs,
      navigationRoutes,
    };
  }, [location.pathname]);

  return (
    <RouteContext.Provider value={contextValue}>
      {children}
    </RouteContext.Provider>
  );
};

// Hook to access route context
export const useRouteContext = () => {
  const context = React.useContext(RouteContext);
  if (!context) {
    throw new Error('useRouteContext must be used within RouteProvider');
  }
  return context;
};
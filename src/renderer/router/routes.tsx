/**
 * Application Routes Configuration
 * Centralized routing with lazy loading, guards, and metadata
 */

import React, { Suspense, lazy } from 'react';
import { RouteObject, Navigate } from 'react-router-dom';
import { motion } from 'framer-motion';
import { ErrorBoundary } from '../components/ErrorBoundary/ErrorBoundary';

// Lazy load components for better performance
const ShipFittingModule = lazy(() => 
  import('../components/ShipFitting/ShipFittingModule').then(module => ({
    default: module.ShipFittingModule
  }))
);

const CharacterModule = lazy(() => 
  import('../components/Character/CharacterModule').then(module => ({
    default: module.CharacterModule
  }))
);

const MarketModule = lazy(() => 
  import('../components/Market/MarketModule').then(module => ({
    default: module.MarketModule
  }))
);

const SettingsModule = lazy(() => 
  import('../components/Settings/SettingsModule').then(module => ({
    default: module.SettingsModule
  }))
);

// Route metadata interface
export interface RouteMetadata {
  title: string;
  description?: string;
  icon?: string;
  requiresAuth?: boolean;
  requiresCharacter?: boolean;
  preloadData?: boolean;
  permissions?: string[];
  hideInNav?: boolean;
}

// Extended route object with metadata
export interface AppRoute extends Omit<RouteObject, 'children'> {
  id: string;
  metadata: RouteMetadata;
  children?: AppRoute[];
}

// Loading component for route transitions
const RouteLoader: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <ErrorBoundary>
    <Suspense 
      fallback={
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="flex items-center justify-center min-h-[50vh]"
        >
          <motion.div
            animate={{ rotate: 360 }}
            transition={{ duration: 1, repeat: Infinity, ease: 'linear' }}
            className="w-12 h-12 border-4 border-gray-600 border-t-accent-blue rounded-full"
          />
        </motion.div>
      }
    >
      {children}
    </Suspense>
  </ErrorBoundary>
);

// Page wrapper with animations
const PageWrapper: React.FC<{ children: React.ReactNode; className?: string }> = ({ 
  children, 
  className = '' 
}) => (
  <motion.div
    initial={{ opacity: 0, y: 20 }}
    animate={{ opacity: 1, y: 0 }}
    exit={{ opacity: 0, y: -20 }}
    transition={{ duration: 0.3 }}
    className={`w-full h-full ${className}`}
  >
    {children}
  </motion.div>
);

// Route definitions
export const appRoutes: AppRoute[] = [
  {
    id: 'home',
    path: '/',
    element: <Navigate to="/fitting" replace />,
    metadata: {
      title: 'Home',
      description: 'Redirects to ship fitting module',
      hideInNav: true,
    },
  },
  {
    id: 'fitting',
    path: '/fitting',
    element: (
      <RouteLoader>
        <PageWrapper>
          <ShipFittingModule />
        </PageWrapper>
      </RouteLoader>
    ),
    metadata: {
      title: 'Ship Fitting',
      description: 'Design and optimize ship fittings',
      icon: 'ship',
      preloadData: true,
    },
    children: [
      {
        id: 'fitting-new',
        path: 'new',
        element: (
          <RouteLoader>
            <PageWrapper>
              <ShipFittingModule mode="new" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'New Fitting',
          description: 'Create a new ship fitting',
          hideInNav: true,
        },
      },
      {
        id: 'fitting-edit',
        path: ':fittingId/edit',
        element: (
          <RouteLoader>
            <PageWrapper>
              <ShipFittingModule mode="edit" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Edit Fitting',
          description: 'Edit an existing ship fitting',
          hideInNav: true,
        },
      },
      {
        id: 'fitting-compare',
        path: 'compare',
        element: (
          <RouteLoader>
            <PageWrapper>
              <ShipFittingModule mode="compare" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Compare Fittings',
          description: 'Compare multiple ship fittings',
          hideInNav: true,
        },
      },
    ],
  },
  {
    id: 'character',
    path: '/character',
    element: (
      <RouteLoader>
        <PageWrapper>
          <CharacterModule />
        </PageWrapper>
      </RouteLoader>
    ),
    metadata: {
      title: 'Character',
      description: 'Manage character skills and attributes',
      icon: 'user',
      requiresAuth: true,
      preloadData: true,
    },
    children: [
      {
        id: 'character-skills',
        path: 'skills',
        element: (
          <RouteLoader>
            <PageWrapper>
              <CharacterModule view="skills" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Skills',
          description: 'View and plan character skills',
          requiresCharacter: true,
        },
      },
      {
        id: 'character-attributes',
        path: 'attributes',
        element: (
          <RouteLoader>
            <PageWrapper>
              <CharacterModule view="attributes" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Attributes',
          description: 'View character attributes and implants',
          requiresCharacter: true,
        },
      },
      {
        id: 'character-plans',
        path: 'plans',
        element: (
          <RouteLoader>
            <PageWrapper>
              <CharacterModule view="plans" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Skill Plans',
          description: 'Create and manage skill training plans',
          requiresCharacter: true,
        },
      },
    ],
  },
  {
    id: 'market',
    path: '/market',
    element: (
      <RouteLoader>
        <PageWrapper>
          <MarketModule />
        </PageWrapper>
      </RouteLoader>
    ),
    metadata: {
      title: 'Market',
      description: 'Market analysis and price tracking',
      icon: 'trending-up',
      preloadData: true,
    },
    children: [
      {
        id: 'market-browser',
        path: 'browser',
        element: (
          <RouteLoader>
            <PageWrapper>
              <MarketModule view="browser" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Market Browser',
          description: 'Browse and search market data',
        },
      },
      {
        id: 'market-watchlist',
        path: 'watchlist',
        element: (
          <RouteLoader>
            <PageWrapper>
              <MarketModule view="watchlist" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Watchlist',
          description: 'Track prices for watched items',
        },
      },
      {
        id: 'market-portfolio',
        path: 'portfolio',
        element: (
          <RouteLoader>
            <PageWrapper>
              <MarketModule view="portfolio" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Portfolio',
          description: 'Track your market investments',
        },
      },
      {
        id: 'market-item',
        path: 'item/:typeId',
        element: (
          <RouteLoader>
            <PageWrapper>
              <MarketModule view="item" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Item Details',
          description: 'Detailed market data for specific item',
          hideInNav: true,
        },
      },
    ],
  },
  {
    id: 'settings',
    path: '/settings',
    element: (
      <RouteLoader>
        <PageWrapper>
          <SettingsModule />
        </PageWrapper>
      </RouteLoader>
    ),
    metadata: {
      title: 'Settings',
      description: 'Application settings and preferences',
      icon: 'settings',
    },
    children: [
      {
        id: 'settings-general',
        path: 'general',
        element: (
          <RouteLoader>
            <PageWrapper>
              <SettingsModule section="general" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'General',
          description: 'General application settings',
        },
      },
      {
        id: 'settings-accounts',
        path: 'accounts',
        element: (
          <RouteLoader>
            <PageWrapper>
              <SettingsModule section="accounts" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Accounts',
          description: 'EVE Online account management',
        },
      },
      {
        id: 'settings-performance',
        path: 'performance',
        element: (
          <RouteLoader>
            <PageWrapper>
              <SettingsModule section="performance" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Performance',
          description: 'Performance and optimization settings',
        },
      },
      {
        id: 'settings-data',
        path: 'data',
        element: (
          <RouteLoader>
            <PageWrapper>
              <SettingsModule section="data" />
            </PageWrapper>
          </RouteLoader>
        ),
        metadata: {
          title: 'Data Management',
          description: 'Database and cache management',
        },
      },
    ],
  },
];

// Helper to convert AppRoute[] to RouteObject[] for React Router
export const convertToRouteObjects = (routes: AppRoute[]): RouteObject[] => {
  return routes.map(({ id, metadata, children, ...route }) => ({
    ...route,
    children: children ? convertToRouteObjects(children) : undefined,
  }));
};

// Helper to find route by ID
export const findRouteById = (routes: AppRoute[], id: string): AppRoute | undefined => {
  for (const route of routes) {
    if (route.id === id) return route;
    if (route.children) {
      const found = findRouteById(route.children, id);
      if (found) return found;
    }
  }
  return undefined;
};

// Helper to get all routes (flattened)
export const getAllRoutes = (routes: AppRoute[]): AppRoute[] => {
  const result: AppRoute[] = [];
  for (const route of routes) {
    result.push(route);
    if (route.children) {
      result.push(...getAllRoutes(route.children));
    }
  }
  return result;
};

// Helper to get navigation routes (excluding hidden ones)
export const getNavigationRoutes = (routes: AppRoute[]): AppRoute[] => {
  return routes.filter(route => !route.metadata.hideInNav);
};

// Helper to build breadcrumbs
export const buildBreadcrumbs = (routes: AppRoute[], currentPath: string): AppRoute[] => {
  const segments = currentPath.split('/').filter(Boolean);
  const breadcrumbs: AppRoute[] = [];
  
  let currentRoutes = routes;
  let currentPath_build = '';
  
  for (const segment of segments) {
    currentPath_build += '/' + segment;
    const route = currentRoutes.find(r => r.path === currentPath_build || r.path === segment);
    if (route) {
      breadcrumbs.push(route);
      currentRoutes = route.children || [];
    }
  }
  
  return breadcrumbs;
};
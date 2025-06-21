/**
 * Router Hooks
 * Custom hooks for navigation and route management
 */

import { useNavigate, useLocation, useParams } from 'react-router-dom';
import { appRoutes, findRouteById, AppRoute } from './routes';
import { useRouteContext } from './Router';

// Enhanced navigation hook with route metadata
export const useAppNavigation = () => {
  const navigate = useNavigate();
  const location = useLocation();
  
  return {
    navigate,
    location,
    
    // Navigation methods
    goToFitting: (fittingId?: string) => {
      if (fittingId) {
        navigate(`/fitting/${fittingId}/edit`);
      } else {
        navigate('/fitting');
      }
    },
    
    goToCharacter: (view?: string) => {
      if (view) {
        navigate(`/character/${view}`);
      } else {
        navigate('/character');
      }
    },
    
    goToMarket: (view?: string, typeId?: number) => {
      if (view === 'item' && typeId) {
        navigate(`/market/item/${typeId}`);
      } else if (view) {
        navigate(`/market/${view}`);
      } else {
        navigate('/market');
      }
    },
    
    goToSettings: (section?: string) => {
      if (section) {
        navigate(`/settings/${section}`);
      } else {
        navigate('/settings');
      }
    },
    
    // Back navigation
    goBack: () => navigate(-1),
    
    // Route utilities
    isCurrentPath: (path: string) => location.pathname === path,
    isCurrentRoute: (routeId: string) => {
      const route = findRouteById(appRoutes, routeId);
      return route ? location.pathname.startsWith(route.path || '') : false;
    },
  };
};

// Hook to get current route information
export const useCurrentRoute = (): AppRoute | undefined => {
  const location = useLocation();
  
  // Find the most specific matching route
  const findMatchingRoute = (routes: AppRoute[], path: string): AppRoute | undefined => {
    for (const route of routes) {
      if (route.path && path.startsWith(route.path)) {
        // Check children first for more specific matches
        if (route.children) {
          const childMatch = findMatchingRoute(route.children, path);
          if (childMatch) return childMatch;
        }
        return route;
      }
    }
    return undefined;
  };
  
  return findMatchingRoute(appRoutes, location.pathname);
};

// Hook to get route parameters with type safety
export const useRouteParams = <T extends Record<string, string> = {}>(): T => {
  return useParams() as T;
};

// Hook for route-specific data loading
export const useRouteData = () => {
  const currentRoute = useCurrentRoute();
  const params = useRouteParams();
  
  return {
    currentRoute,
    params,
    shouldPreloadData: currentRoute?.metadata.preloadData || false,
    requiresAuth: currentRoute?.metadata.requiresAuth || false,
    requiresCharacter: currentRoute?.metadata.requiresCharacter || false,
  };
};

// Hook for breadcrumb navigation
export const useBreadcrumbs = () => {
  const location = useLocation();
  
  const buildBreadcrumbs = (): AppRoute[] => {
    const segments = location.pathname.split('/').filter(Boolean);
    const breadcrumbs: AppRoute[] = [];
    
    let currentPath = '';
    let currentRoutes = appRoutes;
    
    for (const segment of segments) {
      currentPath += '/' + segment;
      
      // Find matching route
      const route = currentRoutes.find(r => {
        if (r.path === currentPath) return true;
        if (r.path?.includes(':')) {
          // Handle dynamic routes
          const pathPattern = r.path.replace(/:[^/]+/g, '[^/]+');
          const regex = new RegExp(`^${pathPattern}$`);
          return regex.test(currentPath);
        }
        return false;
      });
      
      if (route) {
        breadcrumbs.push(route);
        currentRoutes = route.children || [];
      }
    }
    
    return breadcrumbs;
  };
  
  return buildBreadcrumbs();
};

// Hook for checking route permissions
export const useRoutePermissions = () => {
  const currentRoute = useCurrentRoute();
  
  // TODO: Integrate with actual authentication system
  const isAuthenticated = false; // Placeholder
  const hasCharacter = false; // Placeholder
  const userPermissions: string[] = []; // Placeholder
  
  const canAccessRoute = (route?: AppRoute): boolean => {
    if (!route) return true;
    
    if (route.metadata.requiresAuth && !isAuthenticated) return false;
    if (route.metadata.requiresCharacter && !hasCharacter) return false;
    
    if (route.metadata.permissions) {
      const hasAllPermissions = route.metadata.permissions.every(
        permission => userPermissions.includes(permission)
      );
      if (!hasAllPermissions) return false;
    }
    
    return true;
  };
  
  return {
    canAccessCurrentRoute: canAccessRoute(currentRoute),
    canAccessRoute,
    isAuthenticated,
    hasCharacter,
    userPermissions,
  };
};

// Hook for route-based title management
export const useRouteTitle = (): string => {
  const currentRoute = useCurrentRoute();
  const params = useRouteParams();
  
  if (!currentRoute) return 'Gideon';
  
  let title = currentRoute.metadata.title;
  
  // Dynamic title generation based on params
  if (params.fittingId && currentRoute.id === 'fitting-edit') {
    title = `Edit Fitting ${params.fittingId}`;
  }
  
  if (params.typeId && currentRoute.id === 'market-item') {
    title = `Market - Item ${params.typeId}`;
  }
  
  return `${title} - Gideon`;
};
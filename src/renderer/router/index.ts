/**
 * Router Module Barrel Export
 * Centralized access to all routing functionality
 */

// Main router components
export { AppRouter, RouteProvider, useRouteContext } from './Router';

// Route configuration
export { 
  appRoutes, 
  convertToRouteObjects,
  findRouteById,
  getAllRoutes,
  getNavigationRoutes,
  buildBreadcrumbs,
} from './routes';
export type { AppRoute, RouteMetadata } from './routes';

// Route guards
export { RouteGuard, DataPreloader, withRouteGuard } from './RouteGuard';

// Navigation hooks
export {
  useAppNavigation,
  useCurrentRoute,
  useRouteParams,
  useRouteData,
  useBreadcrumbs,
  useRoutePermissions,
  useRouteTitle,
} from './hooks';

// Legacy navigation utilities (for backward compatibility)
export const useNavigationRoutes = () => {
  const { navigationRoutes } = useRouteContext();
  return navigationRoutes;
};

// Route metadata utilities
export const getRouteTitle = (routeId: string): string => {
  const route = findRouteById(appRoutes, routeId);
  return route?.metadata.title || 'Gideon';
};

export const getRouteDescription = (routeId: string): string => {
  const route = findRouteById(appRoutes, routeId);
  return route?.metadata.description || '';
};

export const isRouteAccessible = (
  routeId: string, 
  isAuthenticated: boolean, 
  hasCharacter: boolean
): boolean => {
  const route = findRouteById(appRoutes, routeId);
  if (!route) return false;
  
  if (route.metadata.requiresAuth && !isAuthenticated) return false;
  if (route.metadata.requiresCharacter && !hasCharacter) return false;
  
  return true;
};
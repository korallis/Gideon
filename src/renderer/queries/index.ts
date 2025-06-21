/**
 * Queries Barrel Export
 * Centralized access to all React Query hooks and utilities
 */

// Query client and configuration
export {
  queryClient,
  queryKeys,
  queryInvalidation,
  queryPrefetch,
  queryErrorHandlers,
  mutationHelpers,
} from './queryClient';

// Character queries
export {
  useCharacterQuery,
  useCharacterSkillsQuery,
  useTokenVerificationQuery,
  useCharacterAttributesQuery,
  useCharacterImplantsQuery,
  useCharacterSkillQueueQuery,
  useUpdateCharacterMutation,
  useRefreshCharacterMutation,
  useBulkCharacterSyncMutation,
  useActiveCharacterData,
  useCharacterWithData,
} from './characterQueries';

// Market queries
export {
  useMarketOrdersQuery,
  useMarketPricesQuery,
  useMarketHistoryQuery,
  useMarketDataQuery,
  useRegionalPricesQuery,
  useMarketSearchQuery,
  useUpdateWatchlistMutation,
  useCreatePriceAlertMutation,
  useBulkMarketRefreshMutation,
  useWatchedItemsData,
  usePortfolioValue,
} from './marketQueries';

// Universe queries
export {
  useTypeQuery,
  useTypesByCategoryQuery,
  useTypesByGroupQuery,
  useTypeSearchQuery,
  useRegionQuery,
  useRegionsQuery,
  useSystemQuery,
  useStationQuery,
  useBatchTypesQuery,
  useShipTypesQuery,
  useModuleTypesQuery,
  usePopularTypesQuery,
  useTypeWithDetails,
  useFittingRelevantTypes,
  useUpdateUniverseDataMutation,
  usePrefetchPopularDataMutation,
  useMultiCategorySearch,
} from './universeQueries';

// Query composition helpers
export const useAppQueries = () => {
  // Essential queries that should run on app initialization
  const regionsQuery = useRegionsQuery();
  const popularTypesQuery = usePopularTypesQuery();
  
  return {
    regions: regionsQuery.data || [],
    popularTypes: popularTypesQuery.types || [],
    isInitializing: regionsQuery.isLoading || popularTypesQuery.isLoading,
    hasInitializationError: regionsQuery.error || popularTypesQuery.hasError,
  };
};

// Character-related query composition
export const useCharacterQueries = (characterId: number) => {
  const characterData = useCharacterWithData(characterId);
  
  return {
    ...characterData,
    // Additional character-specific compositions can be added here
  };
};

// Market analysis composition
export const useMarketAnalysis = (typeId: number, regionId?: number) => {
  const marketData = useMarketDataQuery(typeId, regionId);
  const typeData = useTypeQuery(typeId);
  
  return {
    type: typeData.data,
    market: marketData,
    isComplete: !marketData.isLoading && !typeData.isLoading,
    hasData: Boolean(marketData.analysis && typeData.data),
  };
};

// Fitting-related queries composition
export const useFittingQueries = () => {
  const fittingTypes = useFittingRelevantTypes();
  
  return {
    ships: fittingTypes.ships,
    modules: fittingTypes.modules,
    isLoading: fittingTypes.isLoading,
    hasError: fittingTypes.hasError,
    refetch: fittingTypes.refetch,
  };
};

// Data prefetching utility
export const usePrefetchCriticalData = () => {
  const prefetchUniverse = usePrefetchPopularDataMutation();
  
  const prefetchAll = async () => {
    try {
      await prefetchUniverse.mutateAsync();
      console.log('Critical data prefetched successfully');
    } catch (error) {
      console.error('Failed to prefetch critical data:', error);
    }
  };
  
  return {
    prefetchAll,
    isLoading: prefetchUniverse.isPending,
    error: prefetchUniverse.error,
  };
};

// Query invalidation helpers for common scenarios
export const useQueryInvalidation = () => {
  const queryClient = useQueryClient();
  
  return {
    invalidateCharacter: (characterId: number) => 
      queryClient.invalidateQueries({ queryKey: ['characters', characterId] }),
    invalidateMarketType: (typeId: number) => 
      queryClient.invalidateQueries({ queryKey: ['market', typeId] }),
    invalidateAllMarket: () => 
      queryClient.invalidateQueries({ queryKey: ['market'] }),
    invalidateUniverse: () => 
      queryClient.invalidateQueries({ queryKey: ['universe'] }),
    clearAll: () => queryClient.clear(),
  };
};

// Import the required hooks directly
import { useQueryClient } from '@tanstack/react-query';
import { useRegionsQuery } from './universeQueries';
import { usePopularTypesQuery } from './universeQueries';
import { useCharacterWithData } from './characterQueries';
import { useMarketDataQuery } from './marketQueries';
import { useTypeQuery } from './universeQueries';
import { useFittingRelevantTypes } from './universeQueries';
import { usePrefetchPopularDataMutation } from './universeQueries';
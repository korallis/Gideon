import { QueryClient } from '@tanstack/react-query';

// Default options for all queries
const defaultQueryOptions = {
  queries: {
    // Cache time - how long data stays in cache when component unmounts
    staleTime: 5 * 60 * 1000, // 5 minutes
    gcTime: 10 * 60 * 1000, // 10 minutes (formerly cacheTime)
    
    // Retry configuration
    retry: (failureCount: number, error: any) => {
      // Don't retry on 4xx errors (client errors)
      if (error?.status >= 400 && error?.status < 500) {
        return false;
      }
      
      // Retry up to 3 times for other errors
      return failureCount < 3;
    },
    
    // Retry delay with exponential backoff
    retryDelay: (attemptIndex: number) => 
      Math.min(1000 * 2 ** attemptIndex, 30000),
    
    // Refetch on window focus for critical data
    refetchOnWindowFocus: false,
    
    // Refetch on reconnect
    refetchOnReconnect: true,
  },
  
  mutations: {
    // Retry mutations once
    retry: 1,
  },
};

// Create the query client
export const queryClient = new QueryClient({
  defaultOptions: defaultQueryOptions,
});

// Query key factory for consistent key generation
export const queryKeys = {
  // Character queries
  characters: {
    all: ['characters'] as const,
    authenticated: () => [...queryKeys.characters.all, 'authenticated'] as const,
    detail: (characterId: number) => [...queryKeys.characters.all, characterId] as const,
    skills: (characterId: number) => [...queryKeys.characters.detail(characterId), 'skills'] as const,
    attributes: (characterId: number) => [...queryKeys.characters.detail(characterId), 'attributes'] as const,
    implants: (characterId: number) => [...queryKeys.characters.detail(characterId), 'implants'] as const,
    skillQueue: (characterId: number) => [...queryKeys.characters.detail(characterId), 'skillQueue'] as const,
    assets: (characterId: number) => [...queryKeys.characters.detail(characterId), 'assets'] as const,
    wallet: (characterId: number) => [...queryKeys.characters.detail(characterId), 'wallet'] as const,
    location: (characterId: number) => [...queryKeys.characters.detail(characterId), 'location'] as const,
  },
  
  // Market queries
  market: {
    all: ['market'] as const,
    orders: (typeId: number, regionId?: number) => [
      ...queryKeys.market.all, 
      'orders', 
      typeId, 
      regionId || 'all'
    ] as const,
    prices: (typeId: number) => [...queryKeys.market.all, 'prices', typeId] as const,
    history: (typeId: number, regionId: number) => [
      ...queryKeys.market.all, 
      'history', 
      typeId, 
      regionId
    ] as const,
    types: () => [...queryKeys.market.all, 'types'] as const,
    regions: () => [...queryKeys.market.all, 'regions'] as const,
  },
  
  // Universe queries
  universe: {
    all: ['universe'] as const,
    types: {
      all: () => [...queryKeys.universe.all, 'types'] as const,
      detail: (typeId: number) => [...queryKeys.universe.types.all(), typeId] as const,
      category: (categoryId: number) => [...queryKeys.universe.types.all(), 'category', categoryId] as const,
      group: (groupId: number) => [...queryKeys.universe.types.all(), 'group', groupId] as const,
      search: (query: string) => [...queryKeys.universe.types.all(), 'search', query] as const,
    },
    regions: {
      all: () => [...queryKeys.universe.all, 'regions'] as const,
      detail: (regionId: number) => [...queryKeys.universe.regions.all(), regionId] as const,
    },
    systems: {
      all: () => [...queryKeys.universe.all, 'systems'] as const,
      detail: (systemId: number) => [...queryKeys.universe.systems.all(), systemId] as const,
      region: (regionId: number) => [...queryKeys.universe.systems.all(), 'region', regionId] as const,
    },
    stations: {
      all: () => [...queryKeys.universe.all, 'stations'] as const,
      detail: (stationId: number) => [...queryKeys.universe.stations.all(), stationId] as const,
      system: (systemId: number) => [...queryKeys.universe.stations.all(), 'system', systemId] as const,
    },
  },
  
  // ESI status
  esi: {
    all: ['esi'] as const,
    status: () => [...queryKeys.esi.all, 'status'] as const,
    verify: (token: string) => [...queryKeys.esi.all, 'verify', token] as const,
  },
} as const;

// Query invalidation helpers
export const queryInvalidation = {
  // Invalidate all character data for a specific character
  invalidateCharacter: (characterId: number) => {
    queryClient.invalidateQueries({
      queryKey: queryKeys.characters.detail(characterId),
    });
  },
  
  // Invalidate all market data for a specific type
  invalidateMarketType: (typeId: number) => {
    queryClient.invalidateQueries({
      queryKey: [...queryKeys.market.all, 'orders', typeId],
    });
    queryClient.invalidateQueries({
      queryKey: queryKeys.market.prices(typeId),
    });
  },
  
  // Invalidate all market data
  invalidateAllMarket: () => {
    queryClient.invalidateQueries({
      queryKey: queryKeys.market.all,
    });
  },
  
  // Invalidate universe data
  invalidateUniverse: () => {
    queryClient.invalidateQueries({
      queryKey: queryKeys.universe.all,
    });
  },
  
  // Clear all cache
  clearAll: () => {
    queryClient.clear();
  },
};

// Query prefetching helpers
export const queryPrefetch = {
  // Prefetch character data
  prefetchCharacter: async (characterId: number) => {
    await Promise.all([
      queryClient.prefetchQuery({
        queryKey: queryKeys.characters.detail(characterId),
        queryFn: () => {
          // Will be implemented with actual ESI calls
          return Promise.resolve(null);
        },
        staleTime: 5 * 60 * 1000,
      }),
      queryClient.prefetchQuery({
        queryKey: queryKeys.characters.skills(characterId),
        queryFn: () => {
          // Will be implemented with actual ESI calls
          return Promise.resolve(null);
        },
        staleTime: 10 * 60 * 1000,
      }),
    ]);
  },
  
  // Prefetch market data for common items
  prefetchPopularMarketData: async (typeIds: number[]) => {
    const prefetchPromises = typeIds.map(typeId =>
      queryClient.prefetchQuery({
        queryKey: queryKeys.market.prices(typeId),
        queryFn: () => {
          // Will be implemented with actual ESI calls
          return Promise.resolve(null);
        },
        staleTime: 2 * 60 * 1000, // Market data is more volatile
      })
    );
    
    await Promise.all(prefetchPromises);
  },
};

// Error handlers
export const queryErrorHandlers = {
  // Handle authentication errors
  handleAuthError: (error: any) => {
    if (error?.status === 401 || error?.status === 403) {
      // Clear character-related queries
      queryClient.invalidateQueries({
        queryKey: queryKeys.characters.all,
      });
      
      // Redirect to authentication or show auth modal
      console.warn('Authentication error, clearing character cache');
    }
  },
  
  // Handle rate limiting
  handleRateLimit: (error: any) => {
    if (error?.status === 420 || error?.status === 429) {
      // Implement exponential backoff
      const retryAfter = error?.headers?.['retry-after'] || 60;
      console.warn(`Rate limited, retry after ${retryAfter} seconds`);
    }
  },
  
  // Generic error handler
  handleGenericError: (error: any, queryKey: unknown[]) => {
    console.error('Query error:', {
      error,
      queryKey,
      timestamp: new Date().toISOString(),
    });
    
    // Could send to error reporting service
  },
};

// Mutation helpers
export const mutationHelpers = {
  // Optimistic update helper
  optimisticUpdate: <T>(
    queryKey: readonly unknown[],
    updateFn: (oldData: T | undefined) => T
  ) => {
    queryClient.setQueryData(queryKey, updateFn);
  },
  
  // Rollback helper for failed mutations
  rollback: (queryKey: readonly unknown[], previousData: unknown) => {
    queryClient.setQueryData(queryKey, previousData);
  },
};

// Query client event listeners
queryClient.getQueryCache().subscribe((event) => {
  if (event?.type === 'observerResultsUpdated') {
    // Log query updates in development
    if (process.env.NODE_ENV === 'development') {
      console.debug('Query updated:', event.query.queryKey);
    }
  }
});

queryClient.getMutationCache().subscribe((event) => {
  if (event?.type === 'updated') {
    // Handle mutation state changes
    const mutation = event.mutation;
    
    if (mutation.state.status === 'error') {
      const error = mutation.state.error;
      queryErrorHandlers.handleGenericError(error, ['mutation', mutation.options.mutationKey]);
      
      // Handle specific error types
      queryErrorHandlers.handleAuthError(error);
      queryErrorHandlers.handleRateLimit(error);
    }
  }
});
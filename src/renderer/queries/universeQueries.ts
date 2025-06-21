import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys, queryErrorHandlers } from './queryClient';
import { Type, Region, SolarSystem, Station } from '../../shared/types';
import { useDataStore } from '../stores';

// Mock universe service - will be replaced with actual SDE/ESI implementation
const mockUniverseService = {
  getType: async (typeId: number): Promise<Type> => {
    await new Promise(resolve => setTimeout(resolve, 100));
    
    // Mock type data
    return {
      typeId,
      name: `Type ${typeId}`,
      description: `Description for type ${typeId}`,
      groupId: Math.floor(typeId / 100),
      groupName: `Group ${Math.floor(typeId / 100)}`,
      categoryId: Math.floor(typeId / 1000),
      categoryName: typeId < 1000 ? 'Ship' : typeId < 2000 ? 'Module' : 'Charge',
      volume: Math.random() * 100 + 1,
      mass: Math.random() * 1000000 + 1000,
      radius: Math.random() * 100 + 10,
      marketGroupId: typeId > 500 ? Math.floor(typeId / 10) : undefined,
      iconId: Math.floor(Math.random() * 1000),
      attributes: [
        {
          attributeId: 182,
          attributeName: 'requiredSkill1',
          value: 3300,
          unit: 'typeID',
          displayName: 'Required Skill 1',
          description: 'The skill required to use this item',
        },
      ],
      effects: [
        {
          effectId: 1,
          effectName: 'online',
          effectCategory: 0,
          isDefault: true,
        },
      ],
    };
  },
  
  getTypesByCategory: async (categoryId: number): Promise<Type[]> => {
    await new Promise(resolve => setTimeout(resolve, 200));
    
    // Generate mock types for category
    const types: Type[] = [];
    for (let i = 0; i < 20; i++) {
      const typeId = categoryId * 1000 + i;
      types.push(await mockUniverseService.getType(typeId));
    }
    return types;
  },
  
  getTypesByGroup: async (groupId: number): Promise<Type[]> => {
    await new Promise(resolve => setTimeout(resolve, 150));
    
    const types: Type[] = [];
    for (let i = 0; i < 10; i++) {
      const typeId = groupId * 100 + i;
      types.push(await mockUniverseService.getType(typeId));
    }
    return types;
  },
  
  searchTypes: async (query: string, limit = 50): Promise<Type[]> => {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Mock search results
    const results: Type[] = [];
    for (let i = 0; i < Math.min(limit, 25); i++) {
      const typeId = query.length * 100 + i;
      const type = await mockUniverseService.getType(typeId);
      type.name = `${query} ${i + 1}`;
      results.push(type);
    }
    return results;
  },
  
  getRegion: async (regionId: number): Promise<Region> => {
    await new Promise(resolve => setTimeout(resolve, 100));
    
    return {
      regionId,
      name: `Region ${regionId}`,
      description: `Description for region ${regionId}`,
      constellations: Array.from({ length: 10 }, (_, i) => regionId * 1000 + i),
    };
  },
  
  getAllRegions: async (): Promise<Region[]> => {
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const commonRegions = [
      { regionId: 10000002, name: 'The Forge', description: 'Caldari space containing Jita' },
      { regionId: 10000043, name: 'Domain', description: 'Amarr Empire space' },
      { regionId: 10000032, name: 'Sinq Laison', description: 'Gallente Federation space' },
      { regionId: 10000030, name: 'Heimatar', description: 'Minmatar Republic space' },
      { regionId: 10000042, name: 'Metropolis', description: 'Minmatar space containing Hek' },
    ];
    
    return commonRegions.map(region => ({
      ...region,
      constellations: Array.from({ length: 8 }, (_, i) => region.regionId * 1000 + i),
    }));
  },
  
  getSystem: async (systemId: number): Promise<SolarSystem> => {
    await new Promise(resolve => setTimeout(resolve, 100));
    
    return {
      systemId,
      name: `System ${systemId}`,
      constellationId: Math.floor(systemId / 100),
      regionId: Math.floor(systemId / 100000),
      securityStatus: Math.random(),
      securityClass: Math.random() > 0.5 ? 'B' : 'A',
      planets: Array.from({ length: Math.floor(Math.random() * 10) }, (_, i) => systemId * 100 + i),
      stations: Array.from({ length: Math.floor(Math.random() * 5) }, (_, i) => systemId * 10 + i),
      star: {
        typeId: 45033,
        typeName: 'G2 V',
      },
    };
  },
  
  getStation: async (stationId: number): Promise<Station> => {
    await new Promise(resolve => setTimeout(resolve, 100));
    
    return {
      stationId,
      name: `Station ${stationId}`,
      systemId: Math.floor(stationId / 10),
      typeId: 52678,
      corporationId: 1000035,
      services: ['market', 'repair', 'reprocess', 'refine'],
      reprocessingEfficiency: 0.5,
      reprocessingStationsTake: 0.05,
    };
  },
};

// Type queries
export const useTypeQuery = (typeId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.types.detail(typeId),
    queryFn: () => mockUniverseService.getType(typeId),
    enabled: enabled && typeId > 0,
    staleTime: 24 * 60 * 60 * 1000, // Type data rarely changes - 24 hours
    onError: (error) => {
      queryErrorHandlers.handleGenericError(error, queryKeys.universe.types.detail(typeId));
    },
  });
};

// Types by category query
export const useTypesByCategoryQuery = (categoryId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.types.category(categoryId),
    queryFn: () => mockUniverseService.getTypesByCategory(categoryId),
    enabled: enabled && categoryId > 0,
    staleTime: 24 * 60 * 60 * 1000,
  });
};

// Types by group query
export const useTypesByGroupQuery = (groupId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.types.group(groupId),
    queryFn: () => mockUniverseService.getTypesByGroup(groupId),
    enabled: enabled && groupId > 0,
    staleTime: 24 * 60 * 60 * 1000,
  });
};

// Type search query
export const useTypeSearchQuery = (query: string, limit = 50) => {
  return useQuery({
    queryKey: queryKeys.universe.types.search(query),
    queryFn: () => mockUniverseService.searchTypes(query, limit),
    enabled: query.length >= 2, // Only search with 2+ characters
    staleTime: 5 * 60 * 1000, // Search results can be cached for 5 minutes
  });
};

// Region queries
export const useRegionQuery = (regionId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.regions.detail(regionId),
    queryFn: () => mockUniverseService.getRegion(regionId),
    enabled: enabled && regionId > 0,
    staleTime: 24 * 60 * 60 * 1000,
  });
};

export const useRegionsQuery = () => {
  return useQuery({
    queryKey: queryKeys.universe.regions.all(),
    queryFn: () => mockUniverseService.getAllRegions(),
    staleTime: 24 * 60 * 60 * 1000,
  });
};

// System queries
export const useSystemQuery = (systemId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.systems.detail(systemId),
    queryFn: () => mockUniverseService.getSystem(systemId),
    enabled: enabled && systemId > 0,
    staleTime: 24 * 60 * 60 * 1000,
  });
};

// Station query
export const useStationQuery = (stationId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.universe.stations.detail(stationId),
    queryFn: () => mockUniverseService.getStation(stationId),
    enabled: enabled && stationId > 0,
    staleTime: 24 * 60 * 60 * 1000,
  });
};

// Batch type queries for multiple types
export const useBatchTypesQuery = (typeIds: number[]) => {
  const queries = typeIds.map(typeId => 
    useTypeQuery(typeId, typeIds.length > 0)
  );
  
  const isLoading = queries.some(q => q.isLoading);
  const hasError = queries.some(q => q.error);
  const allData = queries.map(q => q.data).filter(Boolean);
  
  return {
    types: allData,
    isLoading,
    hasError,
    refetchAll: () => {
      queries.forEach(q => q.refetch());
    },
  };
};

// Ship types query (specific category)
export const useShipTypesQuery = () => {
  return useTypesByCategoryQuery(6); // Category 6 is ships
};

// Module types query (specific category)
export const useModuleTypesQuery = () => {
  return useTypesByCategoryQuery(7); // Category 7 is modules
};

// Popular/frequently used types query
export const usePopularTypesQuery = () => {
  const { popularItems } = useDataStore();
  
  return useBatchTypesQuery(popularItems.slice(0, 20)); // Top 20 popular items
};

// Custom hooks for common patterns
export const useTypeWithDetails = (typeId: number) => {
  const typeQuery = useTypeQuery(typeId);
  
  // Also fetch market data if this is a tradeable item
  const hasMarketGroup = typeQuery.data?.marketGroupId !== undefined;
  
  return {
    type: typeQuery.data,
    isLoading: typeQuery.isLoading,
    error: typeQuery.error,
    isMarketable: hasMarketGroup,
    refetch: typeQuery.refetch,
  };
};

// Hook for ship fitting relevant types
export const useFittingRelevantTypes = () => {
  const shipsQuery = useShipTypesQuery();
  const modulesQuery = useModuleTypesQuery();
  
  return {
    ships: shipsQuery.data || [],
    modules: modulesQuery.data || [],
    isLoading: shipsQuery.isLoading || modulesQuery.isLoading,
    hasError: shipsQuery.error || modulesQuery.error,
    refetch: () => {
      shipsQuery.refetch();
      modulesQuery.refetch();
    },
  };
};

// Universe data mutation for updating local cache
export const useUpdateUniverseDataMutation = () => {
  const queryClient = useQueryClient();
  const { updateTypes } = useDataStore();
  
  return useMutation({
    mutationFn: async (types: Type[]) => {
      // Mock API call to update universe data
      await new Promise(resolve => setTimeout(resolve, 500));
      return types;
    },
    onSuccess: (types) => {
      // Update individual type queries
      types.forEach(type => {
        queryClient.setQueryData(queryKeys.universe.types.detail(type.typeId), type);
      });
      
      // Update data store
      updateTypes(types);
      
      // Update category/group caches
      const categorizedTypes: Record<number, Type[]> = {};
      const groupedTypes: Record<number, Type[]> = {};
      
      types.forEach(type => {
        if (!categorizedTypes[type.categoryId]) {
          categorizedTypes[type.categoryId] = [];
        }
        categorizedTypes[type.categoryId].push(type);
        
        if (!groupedTypes[type.groupId]) {
          groupedTypes[type.groupId] = [];
        }
        groupedTypes[type.groupId].push(type);
      });
      
      // Update category and group queries
      Object.entries(categorizedTypes).forEach(([categoryId, categoryTypes]) => {
        queryClient.setQueryData(
          queryKeys.universe.types.category(parseInt(categoryId)),
          categoryTypes
        );
      });
      
      Object.entries(groupedTypes).forEach(([groupId, groupTypes]) => {
        queryClient.setQueryData(
          queryKeys.universe.types.group(parseInt(groupId)),
          groupTypes
        );
      });
    },
  });
};

// Prefetch popular universe data
export const usePrefetchPopularDataMutation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: async () => {
      // Common typeIds that should be prefetched
      const popularTypeIds = [
        588, // Rifter
        589, // Rupture  
        590, // Hurricane
        591, // Tempest
        // Add more popular ship/module IDs
      ];
      
      const popularRegionIds = [
        10000002, // The Forge (Jita)
        10000043, // Domain (Amarr)
        10000032, // Sinq Laison (Dodixie)
        10000030, // Heimatar (Rens)
      ];
      
      // Prefetch types
      await Promise.all(
        popularTypeIds.map(typeId =>
          queryClient.prefetchQuery({
            queryKey: queryKeys.universe.types.detail(typeId),
            queryFn: () => mockUniverseService.getType(typeId),
            staleTime: 24 * 60 * 60 * 1000,
          })
        )
      );
      
      // Prefetch regions
      await Promise.all(
        popularRegionIds.map(regionId =>
          queryClient.prefetchQuery({
            queryKey: queryKeys.universe.regions.detail(regionId),
            queryFn: () => mockUniverseService.getRegion(regionId),
            staleTime: 24 * 60 * 60 * 1000,
          })
        )
      );
      
      return { typesCount: popularTypeIds.length, regionsCount: popularRegionIds.length };
    },
  });
};

// Hook for searching across multiple type categories
export const useMultiCategorySearch = (query: string, categories: number[]) => {
  const searchQuery = useTypeSearchQuery(query);
  const categoryQueries = categories.map(categoryId =>
    useTypesByCategoryQuery(categoryId, query.length >= 2)
  );
  
  const allResults = React.useMemo(() => {
    const searchResults = searchQuery.data || [];
    const categoryResults = categoryQueries.flatMap(q => q.data || []);
    
    // Combine and deduplicate results
    const combined = [...searchResults, ...categoryResults];
    const unique = combined.filter((type, index, arr) => 
      arr.findIndex(t => t.typeId === type.typeId) === index
    );
    
    // Filter by query if we have category results
    if (query.length >= 2) {
      return unique.filter(type =>
        type.name.toLowerCase().includes(query.toLowerCase()) ||
        type.description.toLowerCase().includes(query.toLowerCase())
      );
    }
    
    return unique;
  }, [searchQuery.data, categoryQueries, query]);
  
  const isLoading = searchQuery.isLoading || categoryQueries.some(q => q.isLoading);
  const hasError = searchQuery.error || categoryQueries.some(q => q.error);
  
  return {
    results: allResults,
    isLoading,
    hasError,
    refetch: () => {
      searchQuery.refetch();
      categoryQueries.forEach(q => q.refetch());
    },
  };
};

// React import for useMemo
import React from 'react';
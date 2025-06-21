/**
 * Database Hooks
 * React hooks for interacting with the local IndexedDB database
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { db, CharacterData, FittingData, WatchlistItem, TypeData } from './database';
import { queryKeys } from '../queries/queryClient';
import { Character, ShipFitting } from '../../shared/types';

// Character database hooks
export const useCharacterStorage = () => {
  const queryClient = useQueryClient();

  const getStoredCharacters = useQuery({
    queryKey: ['database', 'characters'],
    queryFn: () => db.characters.toArray(),
    staleTime: 30 * 1000, // 30 seconds
  });

  const getActiveCharacter = useQuery({
    queryKey: ['database', 'activeCharacter'],
    queryFn: () => db.getActiveCharacter(),
    staleTime: 30 * 1000,
  });

  const syncCharacter = useMutation({
    mutationFn: async (character: Character) => {
      await db.syncCharacterData(character);
      return character;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'characters'] });
    },
  });

  const setActiveCharacter = useMutation({
    mutationFn: async (characterId: number) => {
      await db.setActiveCharacter(characterId);
      return characterId;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'activeCharacter'] });
      queryClient.invalidateQueries({ queryKey: ['database', 'characters'] });
    },
  });

  return {
    storedCharacters: getStoredCharacters.data || [],
    activeCharacter: getActiveCharacter.data,
    isLoading: getStoredCharacters.isLoading || getActiveCharacter.isLoading,
    syncCharacter: syncCharacter.mutateAsync,
    setActiveCharacter: setActiveCharacter.mutateAsync,
    isSyncing: syncCharacter.isPending || setActiveCharacter.isPending,
  };
};

// Type/Universe data storage hooks
export const useTypeStorage = () => {
  const queryClient = useQueryClient();

  const getPopularTypes = useQuery({
    queryKey: ['database', 'popularTypes'],
    queryFn: () => db.getPopularTypes(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const cacheType = useMutation({
    mutationFn: async (type: any) => {
      await db.cacheType(type);
      return type;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'popularTypes'] });
    },
  });

  const searchStoredTypes = useMutation({
    mutationFn: async ({ query, limit = 50 }: { query: string; limit?: number }) => {
      return await db.searchTypes(query, limit);
    },
  });

  return {
    popularTypes: getPopularTypes.data || [],
    isLoading: getPopularTypes.isLoading,
    cacheType: cacheType.mutateAsync,
    searchTypes: searchStoredTypes.mutateAsync,
    isSearching: searchStoredTypes.isPending,
  };
};

// Fitting storage hooks
export const useFittingStorage = () => {
  const queryClient = useQueryClient();

  const getFittingsForCharacter = (characterId: number) => 
    useQuery({
      queryKey: ['database', 'fittings', characterId],
      queryFn: () => db.getFittingsForCharacter(characterId),
      enabled: characterId > 0,
      staleTime: 30 * 1000,
    });

  const getFavoriteFittings = useQuery({
    queryKey: ['database', 'favoriteFittings'],
    queryFn: () => db.getFavoriteFittings(),
    staleTime: 30 * 1000,
  });

  const saveFitting = useMutation({
    mutationFn: async ({
      fitting,
      characterId,
      metadata,
    }: {
      fitting: ShipFitting;
      characterId?: number;
      metadata?: { tags?: string[]; isFavorite?: boolean };
    }) => {
      return await db.saveFitting(fitting, characterId, metadata);
    },
    onSuccess: (_, variables) => {
      if (variables.characterId) {
        queryClient.invalidateQueries({ 
          queryKey: ['database', 'fittings', variables.characterId] 
        });
      }
      if (variables.metadata?.isFavorite) {
        queryClient.invalidateQueries({ 
          queryKey: ['database', 'favoriteFittings'] 
        });
      }
    },
  });

  const searchFittings = useMutation({
    mutationFn: async (query: string) => {
      return await db.searchFittings(query);
    },
  });

  return {
    getFittingsForCharacter,
    favoriteFittings: getFavoriteFittings.data || [],
    saveFitting: saveFitting.mutateAsync,
    searchFittings: searchFittings.mutateAsync,
    isLoading: getFavoriteFittings.isLoading,
    isSaving: saveFitting.isPending,
    isSearching: searchFittings.isPending,
  };
};

// Market watchlist hooks
export const useWatchlistStorage = () => {
  const queryClient = useQueryClient();

  const getWatchlist = useQuery({
    queryKey: ['database', 'watchlist'],
    queryFn: () => db.getWatchlist(),
    staleTime: 30 * 1000,
  });

  const addToWatchlist = useMutation({
    mutationFn: async ({
      typeId,
      typeName,
      regionId,
      regionName,
    }: {
      typeId: number;
      typeName: string;
      regionId: number;
      regionName: string;
    }) => {
      await db.addToWatchlist(typeId, typeName, regionId, regionName);
      return { typeId, regionId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'watchlist'] });
    },
  });

  const removeFromWatchlist = useMutation({
    mutationFn: async ({ typeId, regionId }: { typeId: number; regionId: number }) => {
      await db.removeFromWatchlist(typeId, regionId);
      return { typeId, regionId };
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'watchlist'] });
    },
  });

  return {
    watchlist: getWatchlist.data || [],
    isLoading: getWatchlist.isLoading,
    addToWatchlist: addToWatchlist.mutateAsync,
    removeFromWatchlist: removeFromWatchlist.mutateAsync,
    isUpdating: addToWatchlist.isPending || removeFromWatchlist.isPending,
  };
};

// Market data caching hooks
export const useMarketDataStorage = () => {
  const queryClient = useQueryClient();

  const getStoredMarketData = (typeId: number, regionId: number) =>
    useQuery({
      queryKey: ['database', 'marketData', typeId, regionId],
      queryFn: () => db.getMarketData(typeId, regionId),
      enabled: typeId > 0 && regionId > 0,
      staleTime: 60 * 1000, // 1 minute
    });

  const cacheMarketData = useMutation({
    mutationFn: async ({
      typeId,
      regionId,
      data,
    }: {
      typeId: number;
      regionId: number;
      data: any;
    }) => {
      await db.cacheMarketData(typeId, regionId, data);
      return { typeId, regionId };
    },
    onSuccess: ({ typeId, regionId }) => {
      queryClient.invalidateQueries({ 
        queryKey: ['database', 'marketData', typeId, regionId] 
      });
    },
  });

  const cleanExpiredData = useMutation({
    mutationFn: async (maxAge?: number) => {
      await db.cleanExpiredMarketData(maxAge);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database', 'marketData'] });
    },
  });

  return {
    getStoredMarketData,
    cacheMarketData: cacheMarketData.mutateAsync,
    cleanExpiredData: cleanExpiredData.mutateAsync,
    isCaching: cacheMarketData.isPending,
    isCleaning: cleanExpiredData.isPending,
  };
};

// Settings storage hooks
export const useSettingsStorage = () => {
  const queryClient = useQueryClient();

  const getAllSettings = useQuery({
    queryKey: ['database', 'settings'],
    queryFn: () => db.getAllSettings(),
    staleTime: 60 * 1000, // 1 minute
  });

  const getSetting = (key: string, defaultValue?: any) =>
    useQuery({
      queryKey: ['database', 'setting', key],
      queryFn: () => db.getSetting(key, defaultValue),
      staleTime: 60 * 1000,
    });

  const setSetting = useMutation({
    mutationFn: async ({ key, value }: { key: string; value: any }) => {
      await db.setSetting(key, value);
      return { key, value };
    },
    onSuccess: ({ key }) => {
      queryClient.invalidateQueries({ queryKey: ['database', 'setting', key] });
      queryClient.invalidateQueries({ queryKey: ['database', 'settings'] });
    },
  });

  return {
    allSettings: getAllSettings.data || {},
    getSetting,
    setSetting: setSetting.mutateAsync,
    isLoading: getAllSettings.isLoading,
    isSetting: setSetting.isPending,
  };
};

// Database maintenance hooks
export const useDatabaseMaintenance = () => {
  const queryClient = useQueryClient();

  const getStats = useQuery({
    queryKey: ['database', 'stats'],
    queryFn: () => db.getStats(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  const vacuum = useMutation({
    mutationFn: async () => {
      await db.vacuum();
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['database'] });
    },
  });

  const reset = useMutation({
    mutationFn: async () => {
      await db.delete();
      await db.open();
    },
    onSuccess: () => {
      queryClient.clear();
    },
  });

  const healthCheck = useQuery({
    queryKey: ['database', 'health'],
    queryFn: async () => {
      const isHealthy = await db.open().then(() => true).catch(() => false);
      return { isHealthy, timestamp: new Date() };
    },
    staleTime: 30 * 1000,
    refetchInterval: 60 * 1000, // Check every minute
  });

  return {
    stats: getStats.data,
    isHealthy: healthCheck.data?.isHealthy ?? false,
    vacuum: vacuum.mutateAsync,
    reset: reset.mutateAsync,
    isLoading: getStats.isLoading,
    isVacuuming: vacuum.isPending,
    isResetting: reset.isPending,
    refetchStats: getStats.refetch,
  };
};

// Cache management hooks
export const useCacheStorage = () => {
  const setCache = useMutation({
    mutationFn: async ({
      key,
      data,
      category,
      ttl,
    }: {
      key: string;
      data: any;
      category: 'character' | 'market' | 'universe' | 'fitting';
      ttl?: number;
    }) => {
      await db.setCache(key, data, category, ttl);
      return key;
    },
  });

  const getCache = useMutation({
    mutationFn: async (key: string) => {
      return await db.getCache(key);
    },
  });

  const clearCacheByCategory = useMutation({
    mutationFn: async (category: 'character' | 'market' | 'universe' | 'fitting') => {
      await db.clearCacheByCategory(category);
      return category;
    },
  });

  const clearExpiredCache = useMutation({
    mutationFn: async () => {
      await db.clearExpiredCache();
    },
  });

  return {
    setCache: setCache.mutateAsync,
    getCache: getCache.mutateAsync,
    clearCacheByCategory: clearCacheByCategory.mutateAsync,
    clearExpiredCache: clearExpiredCache.mutateAsync,
    isLoading: setCache.isPending || getCache.isPending,
  };
};

// Combined hook for common database operations
export const useDatabase = () => {
  const characters = useCharacterStorage();
  const types = useTypeStorage();
  const fittings = useFittingStorage();
  const watchlist = useWatchlistStorage();
  const settings = useSettingsStorage();
  const maintenance = useDatabaseMaintenance();
  const cache = useCacheStorage();

  return {
    characters,
    types,
    fittings,
    watchlist,
    settings,
    maintenance,
    cache,
    isHealthy: maintenance.isHealthy,
  };
};
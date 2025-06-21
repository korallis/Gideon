import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query';
import { queryKeys, queryErrorHandlers } from './queryClient';
import { MarketOrder, MarketPrice, MarketHistory, PriceAnalysis } from '../../shared/types';
import { useMarketStore } from '../stores';

// Mock market service - will be replaced with actual ESI implementation
const mockMarketService = {
  getMarketOrders: async (typeId: number, regionId = 10000002): Promise<MarketOrder[]> => {
    await new Promise(resolve => setTimeout(resolve, 300));
    
    // Generate mock market orders
    const orders: MarketOrder[] = [];
    
    // Generate sell orders
    for (let i = 0; i < 10; i++) {
      orders.push({
        orderId: Date.now() + i,
        typeId,
        typeName: `Item ${typeId}`,
        locationId: 60003760,
        locationName: 'Jita IV - Moon 4 - Caldari Navy Assembly Plant',
        volumeTotal: Math.floor(Math.random() * 1000) + 100,
        volumeRemain: Math.floor(Math.random() * 800) + 50,
        minVolume: 1,
        price: 1000000 + Math.random() * 200000,
        isBuyOrder: false,
        duration: 90,
        issued: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000),
        range: 'station',
      });
    }
    
    // Generate buy orders
    for (let i = 0; i < 8; i++) {
      orders.push({
        orderId: Date.now() + i + 100,
        typeId,
        typeName: `Item ${typeId}`,
        locationId: 60003760,
        locationName: 'Jita IV - Moon 4 - Caldari Navy Assembly Plant',
        volumeTotal: Math.floor(Math.random() * 500) + 50,
        volumeRemain: Math.floor(Math.random() * 400) + 25,
        minVolume: 1,
        price: 900000 + Math.random() * 100000,
        isBuyOrder: true,
        duration: 90,
        issued: new Date(Date.now() - Math.random() * 7 * 24 * 60 * 60 * 1000),
        range: 'region',
      });
    }
    
    return orders.sort((a, b) => {
      if (a.isBuyOrder !== b.isBuyOrder) {
        return a.isBuyOrder ? 1 : -1; // Sell orders first
      }
      return a.isBuyOrder ? b.price - a.price : a.price - b.price; // Best prices first
    });
  },
  
  getMarketPrices: async (typeId: number): Promise<MarketPrice> => {
    await new Promise(resolve => setTimeout(resolve, 200));
    
    return {
      typeId,
      averagePrice: 950000 + Math.random() * 100000,
      adjustedPrice: 980000 + Math.random() * 50000,
    };
  },
  
  getMarketHistory: async (typeId: number, regionId = 10000002): Promise<MarketHistory[]> => {
    await new Promise(resolve => setTimeout(resolve, 400));
    
    const history: MarketHistory[] = [];
    
    // Generate 30 days of history
    for (let i = 29; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);
      
      const basePrice = 950000;
      const variation = (Math.random() - 0.5) * 100000;
      
      history.push({
        date,
        orderCount: Math.floor(Math.random() * 100) + 50,
        volume: Math.floor(Math.random() * 10000) + 1000,
        highest: basePrice + variation + Math.random() * 50000,
        average: basePrice + variation,
        lowest: basePrice + variation - Math.random() * 50000,
      });
    }
    
    return history;
  },
  
  getRegionalPrices: async (typeId: number, regionIds: number[]) => {
    await new Promise(resolve => setTimeout(resolve, 500));
    
    const prices: Record<number, { sellPrice: number; buyPrice: number; volume: number }> = {};
    
    regionIds.forEach(regionId => {
      const basePrice = 950000 + (regionId % 1000) * 1000; // Slight regional variation
      prices[regionId] = {
        sellPrice: basePrice + Math.random() * 50000,
        buyPrice: basePrice - Math.random() * 50000,
        volume: Math.floor(Math.random() * 5000) + 500,
      };
    });
    
    return prices;
  },
};

// Market orders query
export const useMarketOrdersQuery = (typeId: number, regionId?: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.market.orders(typeId, regionId),
    queryFn: () => mockMarketService.getMarketOrders(typeId, regionId),
    enabled: enabled && typeId > 0,
    staleTime: 2 * 60 * 1000, // Market data is volatile - 2 minutes
    refetchInterval: 5 * 60 * 1000, // Auto-refresh every 5 minutes
    onError: (error) => {
      queryErrorHandlers.handleGenericError(error, queryKeys.market.orders(typeId, regionId));
    },
  });
};

// Market prices query
export const useMarketPricesQuery = (typeId: number, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.market.prices(typeId),
    queryFn: () => mockMarketService.getMarketPrices(typeId),
    enabled: enabled && typeId > 0,
    staleTime: 5 * 60 * 1000, // Price aggregates are less volatile
    onError: (error) => {
      queryErrorHandlers.handleGenericError(error, queryKeys.market.prices(typeId));
    },
  });
};

// Market history query
export const useMarketHistoryQuery = (typeId: number, regionId = 10000002, enabled = true) => {
  return useQuery({
    queryKey: queryKeys.market.history(typeId, regionId),
    queryFn: () => mockMarketService.getMarketHistory(typeId, regionId),
    enabled: enabled && typeId > 0,
    staleTime: 60 * 60 * 1000, // Historical data doesn't change - 1 hour
    onError: (error) => {
      queryErrorHandlers.handleGenericError(error, queryKeys.market.history(typeId, regionId));
    },
  });
};

// Combined market data query
export const useMarketDataQuery = (typeId: number, regionId?: number) => {
  const ordersQuery = useMarketOrdersQuery(typeId, regionId);
  const pricesQuery = useMarketPricesQuery(typeId);
  const historyQuery = useMarketHistoryQuery(typeId, regionId);
  
  const isLoading = ordersQuery.isLoading || pricesQuery.isLoading || historyQuery.isLoading;
  const hasError = ordersQuery.error || pricesQuery.error || historyQuery.error;
  
  // Calculate derived data
  const analysis = React.useMemo(() => {
    if (!ordersQuery.data || !pricesQuery.data) return null;
    
    const orders = ordersQuery.data;
    const buyOrders = orders.filter(o => o.isBuyOrder);
    const sellOrders = orders.filter(o => !o.isBuyOrder);
    
    const bestBuy = Math.max(...buyOrders.map(o => o.price), 0);
    const bestSell = Math.min(...sellOrders.map(o => o.price), Infinity);
    
    const spread = bestSell - bestBuy;
    const spreadPercent = bestBuy > 0 ? (spread / bestBuy) * 100 : 0;
    
    const volume24h = historyQuery.data?.[historyQuery.data.length - 1]?.volume || 0;
    
    return {
      typeId,
      regionId: regionId || 10000002,
      buyOrders,
      sellOrders,
      bestBuy,
      bestSell,
      spread,
      spreadPercent,
      volume24h,
      priceHistory: historyQuery.data || [],
      trend: 'stable' as const, // TODO: Calculate trend from history
    } as PriceAnalysis;
  }, [ordersQuery.data, pricesQuery.data, historyQuery.data, typeId, regionId]);
  
  return {
    orders: ordersQuery.data || [],
    prices: pricesQuery.data,
    history: historyQuery.data || [],
    analysis,
    isLoading,
    hasError,
    refetch: () => {
      ordersQuery.refetch();
      pricesQuery.refetch();
      historyQuery.refetch();
    },
  };
};

// Regional price comparison query
export const useRegionalPricesQuery = (typeId: number, regionIds: number[]) => {
  return useQuery({
    queryKey: [...queryKeys.market.all, 'regional', typeId, regionIds],
    queryFn: () => mockMarketService.getRegionalPrices(typeId, regionIds),
    enabled: typeId > 0 && regionIds.length > 0,
    staleTime: 5 * 60 * 1000,
  });
};

// Infinite query for market search/pagination
export const useMarketSearchQuery = (searchParams: {
  query?: string;
  categoryId?: number;
  regionId?: number;
  minPrice?: number;
  maxPrice?: number;
}) => {
  return useInfiniteQuery({
    queryKey: [...queryKeys.market.all, 'search', searchParams],
    queryFn: async ({ pageParam = 0 }) => {
      await new Promise(resolve => setTimeout(resolve, 300));
      
      // Mock search results
      const results = Array.from({ length: 20 }, (_, i) => ({
        typeId: pageParam * 20 + i + 1,
        typeName: `${searchParams.query || 'Item'} ${pageParam * 20 + i + 1}`,
        price: 100000 + Math.random() * 900000,
        volume: Math.floor(Math.random() * 1000) + 100,
        change24h: (Math.random() - 0.5) * 20, // -10% to +10%
      }));
      
      return {
        results,
        nextCursor: pageParam < 5 ? pageParam + 1 : undefined, // Limit to 5 pages
        hasMore: pageParam < 5,
      };
    },
    getNextPageParam: (lastPage) => lastPage.nextCursor,
    enabled: Boolean(searchParams.query || searchParams.categoryId),
  });
};

// Market mutations
export const useUpdateWatchlistMutation = () => {
  const queryClient = useQueryClient();
  const { addToWatchlist, removeFromWatchlist } = useMarketStore();
  
  return useMutation({
    mutationFn: async ({ typeId, action }: { typeId: number; action: 'add' | 'remove' }) => {
      // Mock API call for syncing watchlist
      await new Promise(resolve => setTimeout(resolve, 200));
      return { typeId, action };
    },
    onSuccess: ({ typeId, action }) => {
      if (action === 'add') {
        addToWatchlist(typeId);
      } else {
        removeFromWatchlist(typeId);
      }
      
      // Prefetch market data for newly watched items
      if (action === 'add') {
        queryClient.prefetchQuery({
          queryKey: queryKeys.market.orders(typeId),
          queryFn: () => mockMarketService.getMarketOrders(typeId),
        });
      }
    },
  });
};

// Price alert mutation
export const useCreatePriceAlertMutation = () => {
  const { addPriceAlert } = useMarketStore();
  
  return useMutation({
    mutationFn: async (alert: {
      typeId: number;
      typeName: string;
      condition: 'above' | 'below' | 'change';
      threshold: number;
      regionId: number;
    }) => {
      // Mock API call
      await new Promise(resolve => setTimeout(resolve, 200));
      return alert;
    },
    onSuccess: (alert) => {
      addPriceAlert({
        ...alert,
        isActive: true,
        triggered: false,
      });
    },
  });
};

// Bulk market data refresh mutation
export const useBulkMarketRefreshMutation = () => {
  const queryClient = useQueryClient();
  const { watchedItems } = useMarketStore();
  
  return useMutation({
    mutationFn: async () => {
      // Refresh all watched items
      const refreshPromises = watchedItems.map(async (typeId) => {
        const [orders, prices] = await Promise.all([
          mockMarketService.getMarketOrders(typeId),
          mockMarketService.getMarketPrices(typeId),
        ]);
        return { typeId, orders, prices };
      });
      
      const results = await Promise.allSettled(refreshPromises);
      return results;
    },
    onSuccess: (results) => {
      results.forEach((result) => {
        if (result.status === 'fulfilled') {
          const { typeId, orders, prices } = result.value;
          queryClient.setQueryData(queryKeys.market.orders(typeId), orders);
          queryClient.setQueryData(queryKeys.market.prices(typeId), prices);
        }
      });
    },
  });
};

// Custom hooks for common patterns
export const useWatchedItemsData = () => {
  const { watchedItems } = useMarketStore();
  
  // Query market data for all watched items
  const queries = watchedItems.map(typeId => ({
    typeId,
    ...useMarketDataQuery(typeId),
  }));
  
  const isLoading = queries.some(q => q.isLoading);
  const hasError = queries.some(q => q.hasError);
  
  return {
    watchedData: queries,
    isLoading,
    hasError,
    refetchAll: () => {
      queries.forEach(q => q.refetch());
    },
  };
};

// Hook for portfolio value calculation
export const usePortfolioValue = () => {
  const { portfolioItems } = useMarketStore();
  
  // Get current prices for all portfolio items
  const priceQueries = portfolioItems.map(item => 
    useMarketPricesQuery(item.typeId)
  );
  
  const totalValue = React.useMemo(() => {
    return portfolioItems.reduce((total, item, index) => {
      const currentPrice = priceQueries[index]?.data?.averagePrice || item.averageBuyPrice;
      return total + (item.quantity * currentPrice);
    }, 0);
  }, [portfolioItems, priceQueries]);
  
  const totalInvested = portfolioItems.reduce((total, item) => total + item.totalInvested, 0);
  const profit = totalValue - totalInvested;
  const profitPercent = totalInvested > 0 ? (profit / totalInvested) * 100 : 0;
  
  const isLoading = priceQueries.some(q => q.isLoading);
  
  return {
    totalValue,
    totalInvested,
    profit,
    profitPercent,
    isLoading,
    items: portfolioItems.map((item, index) => ({
      ...item,
      currentPrice: priceQueries[index]?.data?.averagePrice || item.averageBuyPrice,
      currentValue: item.quantity * (priceQueries[index]?.data?.averagePrice || item.averageBuyPrice),
      profit: (item.quantity * (priceQueries[index]?.data?.averagePrice || item.averageBuyPrice)) - item.totalInvested,
    })),
  };
};

// React import for useMemo
import React from 'react';
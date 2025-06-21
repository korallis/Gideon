import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { MarketOrder, MarketPrice, MarketHistory, PriceAnalysis, Region, Type } from '../../shared/types';

interface MarketState {
  // Current market data
  marketOrders: Record<number, MarketOrder[]>; // typeId -> orders
  marketPrices: Record<number, MarketPrice>; // typeId -> price data
  priceHistory: Record<number, MarketHistory[]>; // typeId -> history
  priceAnalysis: Record<number, PriceAnalysis>; // typeId -> analysis
  
  // Market configuration
  selectedRegions: number[];
  selectedSystems: number[];
  priceSource: 'buy' | 'sell' | 'average';
  orderRange: 'station' | 'system' | 'region' | 'all';
  
  // Tracking and alerts
  watchedItems: number[]; // typeIds being watched
  priceAlerts: PriceAlert[];
  portfolioItems: PortfolioItem[];
  
  // Market search and filtering
  searchQuery: string;
  selectedCategories: number[];
  priceRange: [number, number] | null;
  volumeRange: [number, number] | null;
  sortBy: 'name' | 'price' | 'volume' | 'change';
  sortDirection: 'asc' | 'desc';
  
  // UI state
  showAdvancedFilters: boolean;
  selectedView: 'list' | 'grid' | 'chart';
  selectedItem: number | null;
  comparisonItems: number[];
  
  // Loading and error states
  isLoading: boolean;
  isUpdating: boolean;
  lastUpdated: Record<number, Date>; // typeId -> last update time
  errors: Record<number, string>; // typeId -> error message
  
  // Actions
  setSelectedRegions: (regions: number[]) => void;
  setSelectedSystems: (systems: number[]) => void;
  setPriceSource: (source: 'buy' | 'sell' | 'average') => void;
  setOrderRange: (range: 'station' | 'system' | 'region' | 'all') => void;
  
  // Market data actions
  loadMarketData: (typeId: number, regionId?: number) => Promise<void>;
  refreshMarketData: (typeId: number) => Promise<void>;
  loadPriceHistory: (typeId: number, regionId?: number) => Promise<void>;
  analyzePrices: (typeId: number) => Promise<void>;
  
  // Watchlist actions
  addToWatchlist: (typeId: number) => void;
  removeFromWatchlist: (typeId: number) => void;
  clearWatchlist: () => void;
  
  // Price alerts
  addPriceAlert: (alert: Omit<PriceAlert, 'id' | 'createdAt'>) => void;
  removePriceAlert: (alertId: string) => void;
  updatePriceAlert: (alertId: string, updates: Partial<PriceAlert>) => void;
  checkPriceAlerts: () => void;
  
  // Portfolio management
  addToPortfolio: (item: Omit<PortfolioItem, 'id' | 'addedAt'>) => void;
  removeFromPortfolio: (itemId: string) => void;
  updatePortfolioItem: (itemId: string, updates: Partial<PortfolioItem>) => void;
  calculatePortfolioValue: () => number;
  
  // Search and filtering
  setSearchQuery: (query: string) => void;
  setSelectedCategories: (categories: number[]) => void;
  setPriceRange: (range: [number, number] | null) => void;
  setVolumeRange: (range: [number, number] | null) => void;
  setSortBy: (sortBy: 'name' | 'price' | 'volume' | 'change') => void;
  setSortDirection: (direction: 'asc' | 'desc') => void;
  
  // UI actions
  toggleAdvancedFilters: () => void;
  setSelectedView: (view: 'list' | 'grid' | 'chart') => void;
  setSelectedItem: (typeId: number | null) => void;
  addToComparison: (typeId: number) => void;
  removeFromComparison: (typeId: number) => void;
  clearComparison: () => void;
  
  // Utility actions
  setLoading: (loading: boolean) => void;
  setUpdating: (updating: boolean) => void;
  setError: (typeId: number, error: string | null) => void;
  clearErrors: () => void;
}

interface PriceAlert {
  id: string;
  typeId: number;
  typeName: string;
  condition: 'above' | 'below' | 'change';
  threshold: number;
  regionId: number;
  isActive: boolean;
  triggered: boolean;
  createdAt: Date;
  lastTriggered?: Date;
}

interface PortfolioItem {
  id: string;
  typeId: number;
  typeName: string;
  quantity: number;
  averageBuyPrice: number;
  totalInvested: number;
  regionId: number;
  notes?: string;
  addedAt: Date;
}

export const useMarketStore = create<MarketState>()(
  devtools(
    persist(
      (set, get) => ({
        // Initial state
        marketOrders: {},
        marketPrices: {},
        priceHistory: {},
        priceAnalysis: {},
        selectedRegions: [10000002], // Jita region
        selectedSystems: [],
        priceSource: 'sell',
        orderRange: 'region',
        watchedItems: [],
        priceAlerts: [],
        portfolioItems: [],
        searchQuery: '',
        selectedCategories: [],
        priceRange: null,
        volumeRange: null,
        sortBy: 'name',
        sortDirection: 'asc',
        showAdvancedFilters: false,
        selectedView: 'list',
        selectedItem: null,
        comparisonItems: [],
        isLoading: false,
        isUpdating: false,
        lastUpdated: {},
        errors: {},

        // Actions
        setSelectedRegions: (regions) => set({ selectedRegions: regions }),
        setSelectedSystems: (systems) => set({ selectedSystems: systems }),
        setPriceSource: (source) => set({ priceSource: source }),
        setOrderRange: (range) => set({ orderRange: range }),

        loadMarketData: async (typeId, regionId = 10000002) => {
          set({ isLoading: true });
          get().setError(typeId, null);

          try {
            // TODO: Implement actual ESI market data loading
            // This is a placeholder for the actual market data integration
            
            await new Promise(resolve => setTimeout(resolve, 500)); // Simulate API call

            // Mock market orders
            const mockOrders: MarketOrder[] = [
              {
                orderId: 1,
                typeId,
                typeName: 'Mock Item',
                locationId: 60003760,
                locationName: 'Jita IV - Moon 4 - Caldari Navy Assembly Plant',
                volumeTotal: 1000,
                volumeRemain: 800,
                minVolume: 1,
                price: 1000000,
                isBuyOrder: false,
                duration: 90,
                issued: new Date(),
                range: 'station',
              },
              {
                orderId: 2,
                typeId,
                typeName: 'Mock Item',
                locationId: 60003760,
                locationName: 'Jita IV - Moon 4 - Caldari Navy Assembly Plant',
                volumeTotal: 500,
                volumeRemain: 500,
                minVolume: 1,
                price: 950000,
                isBuyOrder: true,
                duration: 90,
                issued: new Date(),
                range: 'station',
              },
            ];

            // Mock price data
            const mockPrice: MarketPrice = {
              typeId,
              averagePrice: 975000,
              adjustedPrice: 980000,
            };

            set({
              marketOrders: {
                ...get().marketOrders,
                [typeId]: mockOrders,
              },
              marketPrices: {
                ...get().marketPrices,
                [typeId]: mockPrice,
              },
              lastUpdated: {
                ...get().lastUpdated,
                [typeId]: new Date(),
              },
            });

          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to load market data';
            get().setError(typeId, errorMessage);
          } finally {
            set({ isLoading: false });
          }
        },

        refreshMarketData: async (typeId) => {
          set({ isUpdating: true });
          await get().loadMarketData(typeId);
          set({ isUpdating: false });
        },

        loadPriceHistory: async (typeId, regionId = 10000002) => {
          try {
            // TODO: Implement actual price history loading
            await new Promise(resolve => setTimeout(resolve, 300));

            // Mock price history
            const mockHistory: MarketHistory[] = Array.from({ length: 30 }, (_, i) => ({
              date: new Date(Date.now() - (29 - i) * 24 * 60 * 60 * 1000),
              orderCount: Math.floor(Math.random() * 100) + 50,
              volume: Math.floor(Math.random() * 10000) + 1000,
              highest: 1000000 + Math.random() * 100000,
              average: 950000 + Math.random() * 50000,
              lowest: 900000 + Math.random() * 50000,
            }));

            set({
              priceHistory: {
                ...get().priceHistory,
                [typeId]: mockHistory,
              },
            });

          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to load price history';
            get().setError(typeId, errorMessage);
          }
        },

        analyzePrices: async (typeId) => {
          try {
            const orders = get().marketOrders[typeId] || [];
            const history = get().priceHistory[typeId] || [];

            if (orders.length === 0) {
              await get().loadMarketData(typeId);
            }

            if (history.length === 0) {
              await get().loadPriceHistory(typeId);
            }

            // Mock price analysis
            const buyOrders = orders.filter(o => o.isBuyOrder);
            const sellOrders = orders.filter(o => !o.isBuyOrder);
            const bestBuy = Math.max(...buyOrders.map(o => o.price), 0);
            const bestSell = Math.min(...sellOrders.map(o => o.price), Infinity);

            const mockAnalysis: PriceAnalysis = {
              typeId,
              regionId: 10000002,
              buyOrders,
              sellOrders,
              bestBuy,
              bestSell,
              spread: bestSell - bestBuy,
              spreadPercent: ((bestSell - bestBuy) / bestBuy) * 100,
              volume24h: history.slice(-1)[0]?.volume || 0,
              priceHistory: history,
              trend: Math.random() > 0.5 ? 'up' : 'down',
            };

            set({
              priceAnalysis: {
                ...get().priceAnalysis,
                [typeId]: mockAnalysis,
              },
            });

          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to analyze prices';
            get().setError(typeId, errorMessage);
          }
        },

        // Watchlist actions
        addToWatchlist: (typeId) => {
          const watchedItems = [...get().watchedItems];
          if (!watchedItems.includes(typeId)) {
            watchedItems.push(typeId);
            set({ watchedItems });
          }
        },

        removeFromWatchlist: (typeId) => {
          set({
            watchedItems: get().watchedItems.filter(id => id !== typeId),
          });
        },

        clearWatchlist: () => set({ watchedItems: [] }),

        // Price alerts
        addPriceAlert: (alert) => {
          const newAlert: PriceAlert = {
            ...alert,
            id: `alert_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            createdAt: new Date(),
          };

          set({
            priceAlerts: [...get().priceAlerts, newAlert],
          });
        },

        removePriceAlert: (alertId) => {
          set({
            priceAlerts: get().priceAlerts.filter(alert => alert.id !== alertId),
          });
        },

        updatePriceAlert: (alertId, updates) => {
          set({
            priceAlerts: get().priceAlerts.map(alert =>
              alert.id === alertId ? { ...alert, ...updates } : alert
            ),
          });
        },

        checkPriceAlerts: () => {
          const alerts = get().priceAlerts;
          const prices = get().marketPrices;

          alerts.forEach(alert => {
            if (!alert.isActive || alert.triggered) return;

            const currentPrice = prices[alert.typeId];
            if (!currentPrice) return;

            let shouldTrigger = false;
            const price = alert.condition === 'above' || alert.condition === 'below' 
              ? currentPrice.averagePrice 
              : currentPrice.averagePrice;

            switch (alert.condition) {
              case 'above':
                shouldTrigger = price > alert.threshold;
                break;
              case 'below':
                shouldTrigger = price < alert.threshold;
                break;
              case 'change':
                // TODO: Implement price change detection
                break;
            }

            if (shouldTrigger) {
              get().updatePriceAlert(alert.id, {
                triggered: true,
                lastTriggered: new Date(),
              });
            }
          });
        },

        // Portfolio management
        addToPortfolio: (item) => {
          const newItem: PortfolioItem = {
            ...item,
            id: `portfolio_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            addedAt: new Date(),
          };

          set({
            portfolioItems: [...get().portfolioItems, newItem],
          });
        },

        removeFromPortfolio: (itemId) => {
          set({
            portfolioItems: get().portfolioItems.filter(item => item.id !== itemId),
          });
        },

        updatePortfolioItem: (itemId, updates) => {
          set({
            portfolioItems: get().portfolioItems.map(item =>
              item.id === itemId ? { ...item, ...updates } : item
            ),
          });
        },

        calculatePortfolioValue: () => {
          const portfolioItems = get().portfolioItems;
          const marketPrices = get().marketPrices;

          return portfolioItems.reduce((total, item) => {
            const currentPrice = marketPrices[item.typeId]?.averagePrice || item.averageBuyPrice;
            return total + (item.quantity * currentPrice);
          }, 0);
        },

        // Search and filtering
        setSearchQuery: (query) => set({ searchQuery: query }),
        setSelectedCategories: (categories) => set({ selectedCategories: categories }),
        setPriceRange: (range) => set({ priceRange: range }),
        setVolumeRange: (range) => set({ volumeRange: range }),
        setSortBy: (sortBy) => set({ sortBy }),
        setSortDirection: (direction) => set({ sortDirection: direction }),

        // UI actions
        toggleAdvancedFilters: () => set({ showAdvancedFilters: !get().showAdvancedFilters }),
        setSelectedView: (view) => set({ selectedView: view }),
        setSelectedItem: (typeId) => set({ selectedItem: typeId }),
        
        addToComparison: (typeId) => {
          const comparisonItems = [...get().comparisonItems];
          if (!comparisonItems.includes(typeId) && comparisonItems.length < 5) {
            comparisonItems.push(typeId);
            set({ comparisonItems });
          }
        },

        removeFromComparison: (typeId) => {
          set({
            comparisonItems: get().comparisonItems.filter(id => id !== typeId),
          });
        },

        clearComparison: () => set({ comparisonItems: [] }),

        // Utility actions
        setLoading: (loading) => set({ isLoading: loading }),
        setUpdating: (updating) => set({ isUpdating: updating }),
        
        setError: (typeId, error) => {
          const errors = { ...get().errors };
          if (error) {
            errors[typeId] = error;
          } else {
            delete errors[typeId];
          }
          set({ errors });
        },

        clearErrors: () => set({ errors: {} }),
      }),
      {
        name: 'market-store',
        partialize: (state) => ({
          selectedRegions: state.selectedRegions,
          selectedSystems: state.selectedSystems,
          priceSource: state.priceSource,
          orderRange: state.orderRange,
          watchedItems: state.watchedItems,
          priceAlerts: state.priceAlerts,
          portfolioItems: state.portfolioItems,
          selectedCategories: state.selectedCategories,
          sortBy: state.sortBy,
          sortDirection: state.sortDirection,
          selectedView: state.selectedView,
        }),
      }
    ),
    {
      name: 'market-store',
    }
  )
);
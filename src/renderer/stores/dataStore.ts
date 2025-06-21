import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { Type, Region, SolarSystem, Station } from '../../shared/types';

interface DataState {
  // Universe data
  types: Record<number, Type>; // typeId -> type data
  regions: Record<number, Region>; // regionId -> region data
  systems: Record<number, SolarSystem>; // systemId -> system data
  stations: Record<number, Station>; // stationId -> station data
  
  // Data organization
  typesByCategory: Record<number, number[]>; // categoryId -> typeIds
  typesByGroup: Record<number, number[]>; // groupId -> typeIds
  typesByMarketGroup: Record<number, number[]>; // marketGroupId -> typeIds
  ships: Type[];
  modules: Type[];
  drones: Type[];
  charges: Type[];
  
  // Data loading state
  isLoading: boolean;
  isUpdating: boolean;
  loadingProgress: {
    current: number;
    total: number;
    stage: string;
  };
  lastUpdated: Date | null;
  dataVersion: string | null;
  
  // Error handling
  errors: string[];
  failedRequests: number;
  
  // Search and filtering
  searchIndex: Record<string, number[]>; // search term -> typeIds
  popularItems: number[]; // frequently accessed typeIds
  
  // Actions
  loadUniverseData: () => Promise<void>;
  loadTypes: () => Promise<void>;
  loadRegions: () => Promise<void>;
  loadSystems: () => Promise<void>;
  loadStations: () => Promise<void>;
  
  // Data retrieval
  getType: (typeId: number) => Type | null;
  getTypesByCategory: (categoryId: number) => Type[];
  getTypesByGroup: (groupId: number) => Type[];
  getShips: () => Type[];
  getModules: () => Type[];
  getDrones: () => Type[];
  getCharges: () => Type[];
  
  // Search
  searchTypes: (query: string) => Type[];
  addToSearchIndex: (typeId: number, searchTerms: string[]) => void;
  updatePopularItems: (typeId: number) => void;
  
  // Data management
  updateType: (typeId: number, data: Partial<Type>) => void;
  updateTypes: (types: Type[]) => void;
  clearData: () => void;
  checkDataVersion: () => Promise<boolean>;
  
  // Utility
  setLoading: (loading: boolean) => void;
  setUpdating: (updating: boolean) => void;
  setProgress: (current: number, total: number, stage: string) => void;
  addError: (error: string) => void;
  clearErrors: () => void;
}

export const useDataStore = create<DataState>()(
  devtools(
    persist(
      (set, get) => ({
        // Initial state
        types: {},
        regions: {},
        systems: {},
        stations: {},
        typesByCategory: {},
        typesByGroup: {},
        typesByMarketGroup: {},
        ships: [],
        modules: [],
        drones: [],
        charges: [],
        isLoading: false,
        isUpdating: false,
        loadingProgress: { current: 0, total: 0, stage: '' },
        lastUpdated: null,
        dataVersion: null,
        errors: [],
        failedRequests: 0,
        searchIndex: {},
        popularItems: [],

        // Actions
        loadUniverseData: async () => {
          set({ isLoading: true, errors: [], failedRequests: 0 });
          
          try {
            get().setProgress(0, 4, 'Loading types...');
            await get().loadTypes();
            
            get().setProgress(1, 4, 'Loading regions...');
            await get().loadRegions();
            
            get().setProgress(2, 4, 'Loading systems...');
            await get().loadSystems();
            
            get().setProgress(3, 4, 'Loading stations...');
            await get().loadStations();
            
            get().setProgress(4, 4, 'Organizing data...');
            
            // Organize types by category
            const types = Object.values(get().types);
            const typesByCategory: Record<number, number[]> = {};
            const typesByGroup: Record<number, number[]> = {};
            const typesByMarketGroup: Record<number, number[]> = {};
            
            const ships: Type[] = [];
            const modules: Type[] = [];
            const drones: Type[] = [];
            const charges: Type[] = [];
            
            types.forEach(type => {
              // By category
              if (!typesByCategory[type.categoryId]) {
                typesByCategory[type.categoryId] = [];
              }
              typesByCategory[type.categoryId].push(type.typeId);
              
              // By group
              if (!typesByGroup[type.groupId]) {
                typesByGroup[type.groupId] = [];
              }
              typesByGroup[type.groupId].push(type.typeId);
              
              // By market group
              if (type.marketGroupId) {
                if (!typesByMarketGroup[type.marketGroupId]) {
                  typesByMarketGroup[type.marketGroupId] = [];
                }
                typesByMarketGroup[type.marketGroupId].push(type.typeId);
              }
              
              // Categorize by type
              if (type.categoryName === 'Ship') {
                ships.push(type);
              } else if (type.categoryName === 'Module') {
                modules.push(type);
              } else if (type.categoryName === 'Drone') {
                drones.push(type);
              } else if (type.categoryName === 'Charge') {
                charges.push(type);
              }
            });
            
            set({
              typesByCategory,
              typesByGroup,
              typesByMarketGroup,
              ships,
              modules,
              drones,
              charges,
              lastUpdated: new Date(),
              dataVersion: '1.0.0',
            });
            
          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Failed to load universe data';
            get().addError(errorMessage);
          } finally {
            set({ isLoading: false });
          }
        },

        loadTypes: async () => {
          try {
            // TODO: Load from EVE SDE or ESI
            // This is a placeholder for actual data loading
            
            await new Promise(resolve => setTimeout(resolve, 1000)); // Simulate loading
            
            // Mock some basic ship and module types
            const mockTypes: Record<number, Type> = {
              670: {
                typeId: 670,
                name: 'Capsule',
                description: 'The pilot capsule',
                groupId: 29,
                groupName: 'Capsule',
                categoryId: 2,
                categoryName: 'Ship',
                volume: 500,
                mass: 10000,
                radius: 25,
                attributes: [],
                effects: [],
              },
              588: {
                typeId: 588,
                name: 'Rifter',
                description: 'Minmatar frigate',
                groupId: 25,
                groupName: 'Frigate',
                categoryId: 6,
                categoryName: 'Ship',
                volume: 27500,
                mass: 1067000,
                radius: 32,
                marketGroupId: 1361,
                attributes: [],
                effects: [],
              },
              // Add more mock types as needed
            };
            
            set({ types: mockTypes });
            
          } catch (error) {
            get().addError(`Failed to load types: ${error instanceof Error ? error.message : 'Unknown error'}`);
            set({ failedRequests: get().failedRequests + 1 });
          }
        },

        loadRegions: async () => {
          try {
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // Mock regions
            const mockRegions: Record<number, Region> = {
              10000002: {
                regionId: 10000002,
                name: 'The Forge',
                description: 'The Forge region',
                constellations: [20000020],
              },
              // Add more mock regions
            };
            
            set({ regions: mockRegions });
            
          } catch (error) {
            get().addError(`Failed to load regions: ${error instanceof Error ? error.message : 'Unknown error'}`);
            set({ failedRequests: get().failedRequests + 1 });
          }
        },

        loadSystems: async () => {
          try {
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // Mock systems
            const mockSystems: Record<number, SolarSystem> = {
              30000142: {
                systemId: 30000142,
                name: 'Jita',
                constellationId: 20000020,
                regionId: 10000002,
                securityStatus: 0.946,
                securityClass: 'B',
                planets: [],
                stations: [60003760],
                star: {
                  typeId: 45033,
                  typeName: 'G2 V',
                },
              },
              // Add more mock systems
            };
            
            set({ systems: mockSystems });
            
          } catch (error) {
            get().addError(`Failed to load systems: ${error instanceof Error ? error.message : 'Unknown error'}`);
            set({ failedRequests: get().failedRequests + 1 });
          }
        },

        loadStations: async () => {
          try {
            await new Promise(resolve => setTimeout(resolve, 500));
            
            // Mock stations
            const mockStations: Record<number, Station> = {
              60003760: {
                stationId: 60003760,
                name: 'Jita IV - Moon 4 - Caldari Navy Assembly Plant',
                systemId: 30000142,
                typeId: 52678,
                corporationId: 1000035,
                services: ['market', 'repair', 'reprocess'],
                reprocessingEfficiency: 0.5,
                reprocessingStationsTake: 0.05,
              },
              // Add more mock stations
            };
            
            set({ stations: mockStations });
            
          } catch (error) {
            get().addError(`Failed to load stations: ${error instanceof Error ? error.message : 'Unknown error'}`);
            set({ failedRequests: get().failedRequests + 1 });
          }
        },

        // Data retrieval
        getType: (typeId) => {
          return get().types[typeId] || null;
        },

        getTypesByCategory: (categoryId) => {
          const typeIds = get().typesByCategory[categoryId] || [];
          return typeIds.map(id => get().types[id]).filter(Boolean);
        },

        getTypesByGroup: (groupId) => {
          const typeIds = get().typesByGroup[groupId] || [];
          return typeIds.map(id => get().types[id]).filter(Boolean);
        },

        getShips: () => get().ships,
        getModules: () => get().modules,
        getDrones: () => get().drones,
        getCharges: () => get().charges,

        // Search
        searchTypes: (query) => {
          const normalizedQuery = query.toLowerCase().trim();
          if (!normalizedQuery) return [];

          const types = Object.values(get().types);
          return types.filter(type => 
            type.name.toLowerCase().includes(normalizedQuery) ||
            type.description.toLowerCase().includes(normalizedQuery) ||
            type.groupName.toLowerCase().includes(normalizedQuery) ||
            type.categoryName.toLowerCase().includes(normalizedQuery)
          ).slice(0, 50); // Limit results
        },

        addToSearchIndex: (typeId, searchTerms) => {
          const searchIndex = { ...get().searchIndex };
          
          searchTerms.forEach(term => {
            const normalizedTerm = term.toLowerCase();
            if (!searchIndex[normalizedTerm]) {
              searchIndex[normalizedTerm] = [];
            }
            if (!searchIndex[normalizedTerm].includes(typeId)) {
              searchIndex[normalizedTerm].push(typeId);
            }
          });
          
          set({ searchIndex });
        },

        updatePopularItems: (typeId) => {
          const popularItems = [...get().popularItems];
          const index = popularItems.indexOf(typeId);
          
          if (index >= 0) {
            popularItems.splice(index, 1);
          }
          
          popularItems.unshift(typeId);
          set({ popularItems: popularItems.slice(0, 20) }); // Keep top 20
        },

        // Data management
        updateType: (typeId, data) => {
          set({
            types: {
              ...get().types,
              [typeId]: { ...get().types[typeId], ...data },
            },
          });
        },

        updateTypes: (types) => {
          const typesMap = { ...get().types };
          types.forEach(type => {
            typesMap[type.typeId] = type;
          });
          set({ types: typesMap });
        },

        clearData: () => {
          set({
            types: {},
            regions: {},
            systems: {},
            stations: {},
            typesByCategory: {},
            typesByGroup: {},
            typesByMarketGroup: {},
            ships: [],
            modules: [],
            drones: [],
            charges: [],
            searchIndex: {},
            popularItems: [],
            lastUpdated: null,
            dataVersion: null,
            errors: [],
          });
        },

        checkDataVersion: async () => {
          try {
            // TODO: Check against SDE version or ESI
            return true; // Mock: always up to date
          } catch (error) {
            get().addError(`Failed to check data version: ${error instanceof Error ? error.message : 'Unknown error'}`);
            return false;
          }
        },

        // Utility
        setLoading: (loading) => set({ isLoading: loading }),
        setUpdating: (updating) => set({ isUpdating: updating }),
        
        setProgress: (current, total, stage) => {
          set({
            loadingProgress: { current, total, stage },
          });
        },

        addError: (error) => {
          set({
            errors: [...get().errors, error],
          });
        },

        clearErrors: () => set({ errors: [] }),
      }),
      {
        name: 'data-store',
        partialize: (state) => ({
          types: state.types,
          regions: state.regions,
          systems: state.systems,
          stations: state.stations,
          typesByCategory: state.typesByCategory,
          typesByGroup: state.typesByGroup,
          typesByMarketGroup: state.typesByMarketGroup,
          ships: state.ships,
          modules: state.modules,
          drones: state.drones,
          charges: state.charges,
          lastUpdated: state.lastUpdated,
          dataVersion: state.dataVersion,
          searchIndex: state.searchIndex,
          popularItems: state.popularItems,
        }),
      }
    ),
    {
      name: 'data-store',
    }
  )
);
import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { ShipFitting, FittedModule, FittingStats, Type } from '../../shared/types';

interface FittingState {
  // Current fitting being edited
  currentFitting: ShipFitting | null;
  
  // Saved fittings
  savedFittings: ShipFitting[];
  favoriteFittings: string[];
  recentFittings: string[];
  
  // Fitting process state
  isEditing: boolean;
  hasUnsavedChanges: boolean;
  selectedSlot: { type: string; position: number } | null;
  draggedModule: Type | null;
  
  // Ship and module data
  availableShips: Type[];
  availableModules: Type[];
  moduleCategories: Record<string, Type[]>;
  
  // Calculation state
  fittingStats: FittingStats | null;
  isCalculating: boolean;
  calculationError: string | null;
  
  // UI state
  showModuleBrowser: boolean;
  moduleBrowserFilter: string;
  selectedCategory: string | null;
  showFittingComparison: boolean;
  comparisonFittings: string[];
  
  // Loading and error states
  isLoading: boolean;
  error: string | null;
  
  // Actions
  createNewFitting: (shipTypeId: number) => void;
  loadFitting: (fittingId: string) => void;
  saveFitting: (fitting: ShipFitting) => void;
  deleteFitting: (fittingId: string) => void;
  duplicateFitting: (fittingId: string) => void;
  
  // Fitting editing
  addModule: (module: FittedModule) => void;
  removeModule: (slotType: string, position: number) => void;
  updateModule: (slotType: string, position: number, updates: Partial<FittedModule>) => void;
  swapModules: (from: { type: string; position: number }, to: { type: string; position: number }) => void;
  clearSlot: (slotType: string, position: number) => void;
  
  // Fitting management
  toggleFavorite: (fittingId: string) => void;
  addToRecent: (fittingId: string) => void;
  exportFitting: (fittingId: string, format: 'eft' | 'dna' | 'xml') => string;
  importFitting: (data: string, format: 'eft' | 'dna' | 'xml') => Promise<ShipFitting>;
  
  // Calculations
  calculateStats: () => Promise<void>;
  
  // UI actions
  setSelectedSlot: (slot: { type: string; position: number } | null) => void;
  setDraggedModule: (module: Type | null) => void;
  toggleModuleBrowser: () => void;
  setModuleBrowserFilter: (filter: string) => void;
  setSelectedCategory: (category: string | null) => void;
  toggleFittingComparison: () => void;
  addToComparison: (fittingId: string) => void;
  removeFromComparison: (fittingId: string) => void;
  
  // Data management
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  clearError: () => void;
}

export const useFittingStore = create<FittingState>()(
  devtools(
    persist(
      (set, get) => ({
        // Initial state
        currentFitting: null,
        savedFittings: [],
        favoriteFittings: [],
        recentFittings: [],
        isEditing: false,
        hasUnsavedChanges: false,
        selectedSlot: null,
        draggedModule: null,
        availableShips: [],
        availableModules: [],
        moduleCategories: {},
        fittingStats: null,
        isCalculating: false,
        calculationError: null,
        showModuleBrowser: false,
        moduleBrowserFilter: '',
        selectedCategory: null,
        showFittingComparison: false,
        comparisonFittings: [],
        isLoading: false,
        error: null,

        // Actions
        createNewFitting: (shipTypeId) => {
          const newFitting: ShipFitting = {
            id: `fitting_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            name: 'New Fitting',
            shipTypeId,
            shipTypeName: 'Unknown Ship', // TODO: Get from ship data
            description: '',
            modules: [],
            droneBay: [],
            cargoHold: [],
            stats: {
              ehp: { shield: 0, armor: 0, hull: 0, total: 0 },
              dps: { weapon: 0, drone: 0, total: 0 },
              tank: { shieldRecharge: 0, armorRepair: 0, hullRepair: 0 },
              capacitor: { capacity: 0, recharge: 0, stable: false, stablePercent: 0 },
              targeting: { maxTargets: 0, maxRange: 0, scanResolution: 0, signatureRadius: 0 },
              propulsion: { maxVelocity: 0, agility: 0, warpSpeed: 0, mass: 0 },
              cargoSpace: { total: 0, used: 0, remaining: 0 },
            },
            tags: [],
            isFavorite: false,
            createdBy: 'Local User',
            createdAt: new Date(),
            modifiedAt: new Date(),
          };

          set({
            currentFitting: newFitting,
            isEditing: true,
            hasUnsavedChanges: false,
          });
        },

        loadFitting: (fittingId) => {
          const fitting = get().savedFittings.find(f => f.id === fittingId);
          if (fitting) {
            set({
              currentFitting: { ...fitting },
              isEditing: true,
              hasUnsavedChanges: false,
            });
            get().addToRecent(fittingId);
          }
        },

        saveFitting: (fitting) => {
          const savedFittings = [...get().savedFittings];
          const existingIndex = savedFittings.findIndex(f => f.id === fitting.id);
          
          const updatedFitting = {
            ...fitting,
            modifiedAt: new Date(),
          };

          if (existingIndex >= 0) {
            savedFittings[existingIndex] = updatedFitting;
          } else {
            savedFittings.push(updatedFitting);
          }

          set({
            savedFittings,
            currentFitting: updatedFitting,
            hasUnsavedChanges: false,
          });
        },

        deleteFitting: (fittingId) => {
          set({
            savedFittings: get().savedFittings.filter(f => f.id !== fittingId),
            favoriteFittings: get().favoriteFittings.filter(id => id !== fittingId),
            recentFittings: get().recentFittings.filter(id => id !== fittingId),
            comparisonFittings: get().comparisonFittings.filter(id => id !== fittingId),
            currentFitting: get().currentFitting?.id === fittingId ? null : get().currentFitting,
          });
        },

        duplicateFitting: (fittingId) => {
          const fitting = get().savedFittings.find(f => f.id === fittingId);
          if (fitting) {
            const duplicated: ShipFitting = {
              ...fitting,
              id: `fitting_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
              name: `${fitting.name} (Copy)`,
              createdAt: new Date(),
              modifiedAt: new Date(),
            };
            
            get().saveFitting(duplicated);
          }
        },

        addModule: (module) => {
          const currentFitting = get().currentFitting;
          if (!currentFitting) return;

          const updatedModules = [...currentFitting.modules];
          const existingIndex = updatedModules.findIndex(
            m => m.slot === module.slot && m.position === module.position
          );

          if (existingIndex >= 0) {
            updatedModules[existingIndex] = module;
          } else {
            updatedModules.push(module);
          }

          const updatedFitting = {
            ...currentFitting,
            modules: updatedModules,
          };

          set({
            currentFitting: updatedFitting,
            hasUnsavedChanges: true,
          });

          // Trigger stats recalculation
          get().calculateStats();
        },

        removeModule: (slotType, position) => {
          const currentFitting = get().currentFitting;
          if (!currentFitting) return;

          const updatedModules = currentFitting.modules.filter(
            m => !(m.slot === slotType && m.position === position)
          );

          const updatedFitting = {
            ...currentFitting,
            modules: updatedModules,
          };

          set({
            currentFitting: updatedFitting,
            hasUnsavedChanges: true,
          });

          // Trigger stats recalculation
          get().calculateStats();
        },

        updateModule: (slotType, position, updates) => {
          const currentFitting = get().currentFitting;
          if (!currentFitting) return;

          const updatedModules = currentFitting.modules.map(module => {
            if (module.slot === slotType && module.position === position) {
              return { ...module, ...updates };
            }
            return module;
          });

          const updatedFitting = {
            ...currentFitting,
            modules: updatedModules,
          };

          set({
            currentFitting: updatedFitting,
            hasUnsavedChanges: true,
          });

          // Trigger stats recalculation
          get().calculateStats();
        },

        swapModules: (from, to) => {
          const currentFitting = get().currentFitting;
          if (!currentFitting) return;

          const modules = [...currentFitting.modules];
          const fromModule = modules.find(m => m.slot === from.type && m.position === from.position);
          const toModule = modules.find(m => m.slot === to.type && m.position === to.position);

          if (fromModule) {
            fromModule.slot = to.type as any;
            fromModule.position = to.position;
          }

          if (toModule) {
            toModule.slot = from.type as any;
            toModule.position = from.position;
          }

          const updatedFitting = {
            ...currentFitting,
            modules,
          };

          set({
            currentFitting: updatedFitting,
            hasUnsavedChanges: true,
          });

          // Trigger stats recalculation
          get().calculateStats();
        },

        clearSlot: (slotType, position) => {
          get().removeModule(slotType, position);
        },

        toggleFavorite: (fittingId) => {
          const favoriteFittings = [...get().favoriteFittings];
          const index = favoriteFittings.indexOf(fittingId);
          
          if (index >= 0) {
            favoriteFittings.splice(index, 1);
          } else {
            favoriteFittings.push(fittingId);
          }

          set({ favoriteFittings });

          // Update the fitting's favorite status
          const savedFittings = get().savedFittings.map(fitting => {
            if (fitting.id === fittingId) {
              return { ...fitting, isFavorite: !fitting.isFavorite };
            }
            return fitting;
          });

          set({ savedFittings });
        },

        addToRecent: (fittingId) => {
          const recentFittings = [fittingId, ...get().recentFittings.filter(id => id !== fittingId)];
          set({ recentFittings: recentFittings.slice(0, 10) }); // Keep only 10 recent
        },

        exportFitting: (fittingId, format) => {
          const fitting = get().savedFittings.find(f => f.id === fittingId);
          if (!fitting) return '';

          // TODO: Implement actual export formats
          switch (format) {
            case 'eft':
              return `[${fitting.shipTypeName}, ${fitting.name}]\n${fitting.modules.map(m => m.moduleName).join('\n')}`;
            case 'dna':
              return `${fitting.shipTypeId}:${fitting.modules.map(m => `${m.moduleId}:1`).join(':')}::`;
            case 'xml':
              return `<fitting name="${fitting.name}">${fitting.modules.map(m => `<module>${m.moduleName}</module>`).join('')}</fitting>`;
            default:
              return '';
          }
        },

        importFitting: async (data, format) => {
          // TODO: Implement actual import parsing
          const mockFitting: ShipFitting = {
            id: `imported_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
            name: 'Imported Fitting',
            shipTypeId: 1,
            shipTypeName: 'Unknown Ship',
            description: `Imported from ${format.toUpperCase()}`,
            modules: [],
            droneBay: [],
            cargoHold: [],
            stats: {
              ehp: { shield: 0, armor: 0, hull: 0, total: 0 },
              dps: { weapon: 0, drone: 0, total: 0 },
              tank: { shieldRecharge: 0, armorRepair: 0, hullRepair: 0 },
              capacitor: { capacity: 0, recharge: 0, stable: false, stablePercent: 0 },
              targeting: { maxTargets: 0, maxRange: 0, scanResolution: 0, signatureRadius: 0 },
              propulsion: { maxVelocity: 0, agility: 0, warpSpeed: 0, mass: 0 },
              cargoSpace: { total: 0, used: 0, remaining: 0 },
            },
            tags: ['imported'],
            isFavorite: false,
            createdBy: 'Import',
            createdAt: new Date(),
            modifiedAt: new Date(),
          };

          return mockFitting;
        },

        calculateStats: async () => {
          const currentFitting = get().currentFitting;
          if (!currentFitting) return;

          set({ isCalculating: true, calculationError: null });

          try {
            // TODO: Implement actual ship fitting calculations
            // This is a placeholder for the calculation engine
            
            await new Promise(resolve => setTimeout(resolve, 100)); // Simulate calculation

            const mockStats: FittingStats = {
              ehp: { shield: 10000, armor: 15000, hull: 5000, total: 30000 },
              dps: { weapon: 500, drone: 100, total: 600 },
              tank: { shieldRecharge: 100, armorRepair: 50, hullRepair: 25 },
              capacitor: { capacity: 2000, recharge: 200, stable: true, stablePercent: 75 },
              targeting: { maxTargets: 8, maxRange: 80000, scanResolution: 300, signatureRadius: 150 },
              propulsion: { maxVelocity: 250, agility: 3.5, warpSpeed: 3.0, mass: 1000000 },
              cargoSpace: { total: 500, used: 100, remaining: 400 },
            };

            set({ 
              fittingStats: mockStats,
              currentFitting: {
                ...currentFitting,
                stats: mockStats,
              }
            });

          } catch (error) {
            const errorMessage = error instanceof Error ? error.message : 'Calculation failed';
            set({ calculationError: errorMessage });
          } finally {
            set({ isCalculating: false });
          }
        },

        // UI actions
        setSelectedSlot: (slot) => set({ selectedSlot: slot }),
        setDraggedModule: (module) => set({ draggedModule: module }),
        toggleModuleBrowser: () => set({ showModuleBrowser: !get().showModuleBrowser }),
        setModuleBrowserFilter: (filter) => set({ moduleBrowserFilter: filter }),
        setSelectedCategory: (category) => set({ selectedCategory: category }),
        toggleFittingComparison: () => set({ showFittingComparison: !get().showFittingComparison }),
        
        addToComparison: (fittingId) => {
          const comparisonFittings = [...get().comparisonFittings];
          if (!comparisonFittings.includes(fittingId) && comparisonFittings.length < 3) {
            comparisonFittings.push(fittingId);
            set({ comparisonFittings });
          }
        },

        removeFromComparison: (fittingId) => {
          set({
            comparisonFittings: get().comparisonFittings.filter(id => id !== fittingId),
          });
        },

        // Data management
        setLoading: (loading) => set({ isLoading: loading }),
        setError: (error) => set({ error }),
        clearError: () => set({ error: null }),
      }),
      {
        name: 'fitting-store',
        partialize: (state) => ({
          savedFittings: state.savedFittings,
          favoriteFittings: state.favoriteFittings,
          recentFittings: state.recentFittings,
        }),
      }
    ),
    {
      name: 'fitting-store',
    }
  )
);
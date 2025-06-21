/**
 * Store Barrel Export
 * Centralized access to all Zustand stores
 */

export { useAppStore } from './appStore';
export { useCharacterStore } from './characterStore';
export { useFittingStore } from './fittingStore';
export { useMarketStore } from './marketStore';
export { useSettingsStore, initializeSettings } from './settingsStore';
export { useDataStore } from './dataStore';

// Type definitions for store selectors
export type StoreSelector<T> = (state: T) => any;

// Import the actual stores for proper typing
import { useAppStore } from './appStore';
import { useCharacterStore } from './characterStore';
import { useFittingStore } from './fittingStore';
import { useMarketStore } from './marketStore';
import { useSettingsStore, initializeSettings } from './settingsStore';
import { useDataStore } from './dataStore';

// Store type helpers
export type AppStore = ReturnType<typeof useAppStore>;
export type CharacterStore = ReturnType<typeof useCharacterStore>;
export type FittingStore = ReturnType<typeof useFittingStore>;
export type MarketStore = ReturnType<typeof useMarketStore>;
export type SettingsStore = ReturnType<typeof useSettingsStore>;
export type DataStore = ReturnType<typeof useDataStore>;

// Note: Selectors are available but commented out due to TypeScript complexity
// Use the stores directly in components instead

// Store initialization helper
export const initializeStores = async () => {
  try {
    // Initialize settings first
    await initializeSettings();
    
    // Initialize other stores that need setup
    const dataStore = useDataStore.getState();
    
    // Check if we need to load universe data
    if (!dataStore.lastUpdated || dataStore.dataVersion === null) {
      console.log('Loading universe data...');
      await dataStore.loadUniverseData();
    }
    
    console.log('All stores initialized successfully');
    
  } catch (error) {
    console.error('Failed to initialize stores:', error);
  }
};

// Helper for accessing multiple stores in components
export const useStores = () => ({
  app: useAppStore(),
  character: useCharacterStore(),
  fitting: useFittingStore(),
  market: useMarketStore(),
  settings: useSettingsStore(),
  data: useDataStore(),
});
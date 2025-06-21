/**
 * Database Module Barrel Export
 * Centralized access to all database functionality
 */

// Core database
export { db, dbUtils, GideonDatabase } from './database';

// Database types
export type {
  CharacterData,
  TypeData,
  MarketData,
  FittingData,
  CacheEntry,
  AppSettings,
  SkillPlan,
  WatchlistItem,
} from './database';

// Database hooks
export {
  useCharacterStorage,
  useTypeStorage,
  useFittingStorage,
  useWatchlistStorage,
  useMarketDataStorage,
  useSettingsStorage,
  useDatabaseMaintenance,
  useCacheStorage,
  useDatabase,
} from './hooks';

// Database initialization and setup
export const initializeDatabase = async () => {
  try {
    const { db } = await import('./database');
    await db.open();
    
    // Perform initial database maintenance
    await db.clearExpiredCache();
    
    // Set default settings if they don't exist
    const defaultSettings = {
      'app.theme': 'dark',
      'app.autoRefresh': true,
      'app.refreshInterval': 300000, // 5 minutes
      'market.defaultRegion': 10000002, // The Forge
      'fitting.showAdvanced': false,
      'character.autoSync': true,
    };

    for (const [key, value] of Object.entries(defaultSettings)) {
      const existing = await db.getSetting(key);
      if (existing === undefined) {
        await db.setSetting(key, value);
      }
    }

    console.log('Database initialized successfully');
    return true;
  } catch (error) {
    console.error('Failed to initialize database:', error);
    return false;
  }
};

// Database health check utility
export const checkDatabaseHealth = async (): Promise<{
  isHealthy: boolean;
  stats: any;
  error?: string;
}> => {
  try {
    const { dbUtils, db } = await import('./database');
    const isHealthy = await dbUtils.healthCheck();
    const stats = isHealthy ? await db.getStats() : null;
    
    return {
      isHealthy,
      stats,
    };
  } catch (error) {
    return {
      isHealthy: false,
      stats: null,
      error: error instanceof Error ? error.message : 'Unknown error',
    };
  }
};
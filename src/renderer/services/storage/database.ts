/**
 * Database Configuration
 * Dexie-based IndexedDB wrapper for local data persistence
 */

import Dexie, { Table } from 'dexie';

// Database schema interfaces
export interface Character {
  id: number;
  name: string;
  corporation_id: number;
  alliance_id?: number;
  skills: any[];
  attributes: any;
  implants: any[];
  last_updated: Date;
  access_token?: string;
  refresh_token?: string;
  token_expires?: Date;
}

export interface MarketData {
  id?: number;
  type_id: number;
  region_id: number;
  orders: any[];
  history: any[];
  last_updated: Date;
}

export interface FittingData {
  id?: number;
  name: string;
  ship_type_id: number;
  modules: any[];
  created_by: string;
  created_at: Date;
  modified_at: Date;
  tags: string[];
  is_favorite: boolean;
}

export interface CacheEntry {
  key: string;
  value: any;
  expires_at: Date;
  created_at: Date;
}

export interface AppSettings {
  key: string;
  value: any;
  modified_at: Date;
}

export interface UniverseData {
  id: number;
  name: string;
  type: 'type' | 'group' | 'category' | 'region' | 'system' | 'station';
  data: any;
  last_updated: Date;
}

export class GideonDatabase extends Dexie {
  // Character data
  characters!: Table<Character>;
  
  // Market data
  market_data!: Table<MarketData>;
  
  // Ship fittings
  fittings!: Table<FittingData>;
  
  // Cache for API responses
  cache!: Table<CacheEntry>;
  
  // Application settings
  settings!: Table<AppSettings>;
  
  // EVE Universe data (types, groups, etc.)
  universe!: Table<UniverseData>;

  constructor() {
    super('GideonDatabase');
    
    this.version(1).stores({
      characters: '++id, name, corporation_id, alliance_id, last_updated',
      market_data: '++id, type_id, region_id, last_updated',
      fittings: '++id, name, ship_type_id, created_by, created_at, is_favorite, *tags',
      cache: 'key, expires_at, created_at',
      settings: 'key, modified_at',
      universe: 'id, name, type, last_updated'
    });

    // Hook for data cleanup on open
    this.ready.then(() => {
      this.cleanupExpiredCache();
    });
  }

  /**
   * Clean up expired cache entries
   */
  private async cleanupExpiredCache(): Promise<void> {
    const now = new Date();
    await this.cache.where('expires_at').below(now).delete();
  }

  /**
   * Get database statistics
   */
  public async getStats(): Promise<{
    characters: number;
    fittings: number;
    marketData: number;
    cacheEntries: number;
    universeData: number;
  }> {
    const [characters, fittings, marketData, cacheEntries, universeData] = await Promise.all([
      this.characters.count(),
      this.fittings.count(),
      this.market_data.count(),
      this.cache.count(),
      this.universe.count()
    ]);

    return {
      characters,
      fittings,
      marketData,
      cacheEntries,
      universeData
    };
  }

  /**
   * Clear all data (for reset functionality)
   */
  public async clearAllData(): Promise<void> {
    await Promise.all([
      this.characters.clear(),
      this.market_data.clear(),
      this.fittings.clear(),
      this.cache.clear(),
      this.universe.clear()
      // Keep settings intact
    ]);
  }

  /**
   * Export data for backup
   */
  public async exportData(): Promise<any> {
    const [characters, fittings, settings] = await Promise.all([
      this.characters.toArray(),
      this.fittings.toArray(),
      this.settings.toArray()
    ]);

    return {
      version: 1,
      exported_at: new Date().toISOString(),
      characters: characters.map(char => ({
        ...char,
        // Remove sensitive tokens from export
        access_token: undefined,
        refresh_token: undefined
      })),
      fittings,
      settings
    };
  }

  /**
   * Import data from backup
   */
  public async importData(data: any): Promise<void> {
    if (data.version !== 1) {
      throw new Error('Unsupported backup version');
    }

    await this.transaction('rw', [this.characters, this.fittings, this.settings], async () => {
      if (data.characters) {
        await this.characters.bulkPut(data.characters);
      }
      if (data.fittings) {
        await this.fittings.bulkPut(data.fittings);
      }
      if (data.settings) {
        await this.settings.bulkPut(data.settings);
      }
    });
  }
}

// Export singleton instance
export const db = new GideonDatabase();
/**
 * Dexie Database Configuration
 * Local IndexedDB storage for EVE Online data, character information, and application state
 */

import Dexie, { Table } from 'dexie';
import {
  Character,
  Type,
  Region,
  SolarSystem,
  Station,
  MarketOrder,
  MarketPrice,
  MarketHistory,
  ShipFitting,
} from '../../shared/types';

// Database interfaces for local storage
export interface CharacterData extends Character {
  lastUpdated: Date;
  isActive: boolean;
}

export interface TypeData extends Type {
  lastUpdated: Date;
  popularity: number; // Track how often this type is accessed
}

export interface MarketData {
  id?: number;
  typeId: number;
  regionId: number;
  orders: MarketOrder[];
  prices: MarketPrice;
  history: MarketHistory[];
  lastUpdated: Date;
}

export interface FittingData extends Omit<ShipFitting, 'id'> {
  id?: number;
  characterId?: number;
  lastModified: Date;
  tags: string[];
  isFavorite: boolean;
}

export interface CacheEntry {
  key: string;
  data: any;
  expiry: Date;
  category: 'character' | 'market' | 'universe' | 'fitting';
}

export interface AppSettings {
  id?: number;
  key: string;
  value: any;
  lastModified: Date;
}

export interface SkillPlan {
  id?: number;
  characterId: number;
  name: string;
  description?: string;
  skills: Array<{
    skillId: number;
    skillName: string;
    targetLevel: number;
    priority: number;
  }>;
  createdAt: Date;
  lastModified: Date;
  isActive: boolean;
}

export interface WatchlistItem {
  id?: number;
  typeId: number;
  typeName: string;
  regionId: number;
  regionName: string;
  addedAt: Date;
  priceAlerts: Array<{
    condition: 'above' | 'below' | 'change';
    threshold: number;
    isActive: boolean;
  }>;
}

// Main Database Class
export class GideonDatabase extends Dexie {
  // Character tables
  characters!: Table<CharacterData>;
  skillPlans!: Table<SkillPlan>;
  
  // Universe data tables
  types!: Table<TypeData>;
  regions!: Table<Region>;
  systems!: Table<SolarSystem>;
  stations!: Table<Station>;
  
  // Market data tables
  marketData!: Table<MarketData>;
  watchlist!: Table<WatchlistItem>;
  
  // Fitting tables
  fittings!: Table<FittingData>;
  
  // Application data tables
  cache!: Table<CacheEntry>;
  settings!: Table<AppSettings>;

  constructor() {
    super('GideonDatabase');
    
    this.version(1).stores({
      // Character storage
      characters: 'characterId, name, corporationId, allianceId, lastUpdated, isActive',
      skillPlans: '++id, characterId, name, isActive, lastModified',
      
      // Universe data storage
      types: 'typeId, name, categoryId, groupId, marketGroupId, lastUpdated, popularity',
      regions: 'regionId, name',
      systems: 'systemId, name, regionId, constellationId, securityStatus',
      stations: 'stationId, name, systemId, typeId',
      
      // Market data storage
      marketData: '++id, [typeId+regionId], typeId, regionId, lastUpdated',
      watchlist: '++id, typeId, regionId, addedAt',
      
      // Fitting storage
      fittings: '++id, characterId, shipTypeId, name, lastModified, isFavorite',
      
      // Application storage
      cache: 'key, category, expiry',
      settings: '++id, key, lastModified',
    });

    // Database event hooks
    this.characters.hook('creating', (primKey, obj: any, trans) => {
      obj.lastUpdated = new Date();
    });

    this.characters.hook('updating', (modifications: any, primKey, obj, trans) => {
      modifications.lastUpdated = new Date();
    });

    this.types.hook('creating', (primKey, obj: any, trans) => {
      obj.lastUpdated = new Date();
      obj.popularity = obj.popularity || 0;
    });

    this.fittings.hook('creating', (primKey, obj: any, trans) => {
      obj.lastModified = new Date();
    });

    this.fittings.hook('updating', (modifications: any, primKey, obj, trans) => {
      modifications.lastModified = new Date();
    });

    this.settings.hook('creating', (primKey, obj: any, trans) => {
      obj.lastModified = new Date();
    });

    this.settings.hook('updating', (modifications: any, primKey, obj, trans) => {
      modifications.lastModified = new Date();
    });
  }

  // Character management methods
  async getActiveCharacter(): Promise<CharacterData | undefined> {
    return await this.characters.where('isActive').equals(1).first();
  }

  async setActiveCharacter(characterId: number): Promise<void> {
    await this.transaction('rw', this.characters, async () => {
      // Deactivate all characters
      await this.characters.toCollection().modify({ isActive: false });
      // Activate the selected character
      await this.characters.update(characterId, { isActive: true });
    });
  }

  async syncCharacterData(character: Character): Promise<void> {
    await this.characters.put({
      ...character,
      lastUpdated: new Date(),
      isActive: false, // Will be set active separately
    });
  }

  // Universe data management
  async cacheType(type: Type): Promise<void> {
    const existing = await this.types.get(type.typeId);
    const popularity = existing ? existing.popularity + 1 : 1;
    
    await this.types.put({
      ...type,
      lastUpdated: new Date(),
      popularity,
    });
  }

  async getPopularTypes(limit = 20): Promise<TypeData[]> {
    return await this.types
      .orderBy('popularity')
      .reverse()
      .limit(limit)
      .toArray();
  }

  async searchTypes(query: string, limit = 50): Promise<TypeData[]> {
    const searchTerm = query.toLowerCase();
    return await this.types
      .where('name')
      .startsWithIgnoreCase(query)
      .or('description')
      .startsWithIgnoreCase(query)
      .limit(limit)
      .toArray();
  }

  // Market data management
  async cacheMarketData(typeId: number, regionId: number, data: {
    orders: MarketOrder[];
    prices: MarketPrice;
    history: MarketHistory[];
  }): Promise<void> {
    await this.marketData.put({
      typeId,
      regionId,
      orders: data.orders,
      prices: data.prices,
      history: data.history,
      lastUpdated: new Date(),
    });
  }

  async getMarketData(typeId: number, regionId: number): Promise<MarketData | undefined> {
    return await this.marketData.where('[typeId+regionId]').equals([typeId, regionId]).first();
  }

  async cleanExpiredMarketData(maxAge = 5 * 60 * 1000): Promise<void> {
    const cutoff = new Date(Date.now() - maxAge);
    await this.marketData.where('lastUpdated').below(cutoff).delete();
  }

  // Fitting management
  async saveFitting(fitting: ShipFitting, characterId?: number, metadata?: {
    tags?: string[];
    isFavorite?: boolean;
  }): Promise<number> {
    const { id, ...fittingWithoutId } = fitting;
    const fittingData: FittingData = {
      ...fittingWithoutId,
      characterId,
      lastModified: new Date(),
      tags: metadata?.tags || [],
      isFavorite: metadata?.isFavorite || false,
    };

    const result = await this.fittings.add(fittingData);
    return typeof result === 'number' ? result : parseInt(String(result));
  }

  async getFittingsForCharacter(characterId: number): Promise<FittingData[]> {
    return await this.fittings.where('characterId').equals(characterId).toArray();
  }

  async getFavoriteFittings(): Promise<FittingData[]> {
    return await this.fittings.where('isFavorite').equals(1).toArray();
  }

  async searchFittings(query: string): Promise<FittingData[]> {
    const searchTerm = query.toLowerCase();
    return await this.fittings
      .where('name')
      .startsWithIgnoreCase(query)
      .or('description')
      .startsWithIgnoreCase(query)
      .toArray();
  }

  // Cache management
  async setCache(key: string, data: any, category: CacheEntry['category'], ttl = 300000): Promise<void> {
    const expiry = new Date(Date.now() + ttl);
    await this.cache.put({ key, data, expiry, category });
  }

  async getCache(key: string): Promise<any | null> {
    const entry = await this.cache.get(key);
    if (!entry) return null;
    
    if (entry.expiry < new Date()) {
      await this.cache.delete(key);
      return null;
    }
    
    return entry.data;
  }

  async clearExpiredCache(): Promise<void> {
    const now = new Date();
    await this.cache.where('expiry').below(now).delete();
  }

  async clearCacheByCategory(category: CacheEntry['category']): Promise<void> {
    await this.cache.where('category').equals(category).delete();
  }

  // Settings management
  async setSetting(key: string, value: any): Promise<void> {
    const existing = await this.settings.where('key').equals(key).first();
    if (existing) {
      await this.settings.update(existing.id!, { value, lastModified: new Date() });
    } else {
      await this.settings.add({ key, value, lastModified: new Date() });
    }
  }

  async getSetting(key: string, defaultValue?: any): Promise<any> {
    const setting = await this.settings.where('key').equals(key).first();
    return setting ? setting.value : defaultValue;
  }

  async getAllSettings(): Promise<Record<string, any>> {
    const settings = await this.settings.toArray();
    return settings.reduce((acc, setting) => {
      acc[setting.key] = setting.value;
      return acc;
    }, {} as Record<string, any>);
  }

  // Watchlist management
  async addToWatchlist(typeId: number, typeName: string, regionId: number, regionName: string): Promise<void> {
    const existing = await this.watchlist.where('[typeId+regionId]').equals([typeId, regionId]).first();
    if (!existing) {
      await this.watchlist.add({
        typeId,
        typeName,
        regionId,
        regionName,
        addedAt: new Date(),
        priceAlerts: [],
      });
    }
  }

  async removeFromWatchlist(typeId: number, regionId: number): Promise<void> {
    await this.watchlist.where('[typeId+regionId]').equals([typeId, regionId]).delete();
  }

  async getWatchlist(): Promise<WatchlistItem[]> {
    return await this.watchlist.orderBy('addedAt').reverse().toArray();
  }

  // Database maintenance
  async vacuum(): Promise<void> {
    // Clean expired cache
    await this.clearExpiredCache();
    
    // Clean old market data (older than 1 hour)
    await this.cleanExpiredMarketData(60 * 60 * 1000);
    
    // Update type popularity based on recent access
    const recentTypes = await this.cache
      .where('category')
      .equals('universe')
      .and(entry => entry.expiry > new Date(Date.now() - 24 * 60 * 60 * 1000))
      .toArray();
    
    for (const entry of recentTypes) {
      if (entry.key.includes('type-')) {
        const typeId = parseInt(entry.key.split('-')[1]);
        if (typeId) {
          const existing = await this.types.get(typeId);
          if (existing) {
            await this.types.update(typeId, { popularity: existing.popularity + 1 });
          }
        }
      }
    }
  }

  async getStats(): Promise<{
    characters: number;
    types: number;
    fittings: number;
    marketData: number;
    cacheEntries: number;
    watchlistItems: number;
    dbSize: number;
  }> {
    const [characters, types, fittings, marketData, cacheEntries, watchlistItems] = await Promise.all([
      this.characters.count(),
      this.types.count(),
      this.fittings.count(),
      this.marketData.count(),
      this.cache.count(),
      this.watchlist.count(),
    ]);

    return {
      characters,
      types,
      fittings,
      marketData,
      cacheEntries,
      watchlistItems,
      dbSize: 0, // TODO: Calculate actual DB size
    };
  }
}

// Create and export the database instance
export const db = new GideonDatabase();

// Database initialization and error handling
db.open().catch((error) => {
  console.error('Failed to open database:', error);
});

// Export database utilities
export const dbUtils = {
  async reset(): Promise<void> {
    await db.delete();
    await db.open();
  },

  async backup(): Promise<Blob> {
    // TODO: Implement database backup functionality
    throw new Error('Backup functionality not yet implemented');
  },

  async restore(data: Blob): Promise<void> {
    // TODO: Implement database restore functionality
    throw new Error('Restore functionality not yet implemented');
  },

  async healthCheck(): Promise<boolean> {
    try {
      await db.settings.limit(1).toArray();
      return true;
    } catch {
      return false;
    }
  },
};
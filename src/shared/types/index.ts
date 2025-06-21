/**
 * Shared Type Definitions
 * Common types used across main and renderer processes
 */

// Application Types
export interface AppConfig {
  theme: 'dark' | 'light' | 'auto';
  language: string;
  autoUpdate: boolean;
  telemetry: boolean;
  cache: {
    maxSize: number;
    ttl: number;
    cleanupInterval: number;
  };
  performance: {
    maxMemory: number;
    renderFPS: number;
    backgroundTasks: boolean;
  };
}

export interface UIState {
  sidebarOpen: boolean;
  activeModule: string;
  notifications: Notification[];
  loading: boolean;
  error: string | null;
}

export interface FeatureFlags {
  enableBetaFeatures: boolean;
  enableAnalytics: boolean;
  enableAutoUpdates: boolean;
  enableOfflineMode: boolean;
  enableDebugMode: boolean;
}

export interface ApiConfig {
  esiBaseUrl: string;
  esiClientId: string;
  timeout: number;
  retryAttempts: number;
}

// Character Types
export interface Character {
  id: number;
  name: string;
  corporationId: number;
  allianceId?: number;
  birthday: string;
  gender: 'male' | 'female';
  raceId: number;
  bloodlineId: number;
  ancestryId: number;
  securityStatus: number;
  totalSp: number;
  unallocatedSp: number;
  isMain: boolean;
  lastUpdated: Date;
}

export interface CharacterSkills {
  characterId: number;
  skills: Skill[];
  totalSp: number;
  unallocatedSp: number;
  attributes: CharacterAttributes;
  implants: Implant[];
  skillQueue: SkillQueueEntry[];
}

export interface Skill {
  skillId: number;
  name: string;
  groupId: number;
  skillPointsInSkill: number;
  trainedSkillLevel: number;
  activeSkillLevel: number;
  rank: number;
  primaryAttribute: string;
  secondaryAttribute: string;
}

export interface CharacterAttributes {
  intelligence: number;
  memory: number;
  charisma: number;
  perception: number;
  willpower: number;
}

export interface Implant {
  implantId: number;
  name: string;
  slot: number;
  attributes: Record<string, number>;
}

export interface SkillQueueEntry {
  skillId: number;
  queuedLevel: number;
  trainingStartSp: number;
  levelStartSp: number;
  levelEndSp: number;
  startDate?: Date;
  finishDate?: Date;
}

// Ship Fitting Types
export interface ShipFitting {
  id: string;
  name: string;
  shipTypeId: number;
  shipTypeName: string;
  description?: string;
  modules: FittedModule[];
  droneBay: FittedDrone[];
  cargoHold: CargoItem[];
  fuelBay?: CargoItem[];
  stats: FittingStats;
  tags: string[];
  isFavorite: boolean;
  createdBy: string;
  createdAt: Date;
  modifiedAt: Date;
}

export interface FittedModule {
  moduleId: number;
  moduleName: string;
  slot: SlotType;
  position: number;
  state: ModuleState;
  ammoTypeId?: number;
  ammoTypeName?: string;
  charges?: number;
}

export interface FittedDrone {
  droneTypeId: number;
  droneTypeName: string;
  quantity: number;
  inBay: number;
  inSpace: number;
}

export interface CargoItem {
  typeId: number;
  typeName: string;
  quantity: number;
  volume: number;
}

export interface FittingStats {
  ehp: {
    shield: number;
    armor: number;
    hull: number;
    total: number;
  };
  dps: {
    weapon: number;
    drone: number;
    total: number;
  };
  tank: {
    shieldRecharge: number;
    armorRepair: number;
    hullRepair: number;
  };
  capacitor: {
    capacity: number;
    recharge: number;
    stable: boolean;
    stablePercent: number;
  };
  targeting: {
    maxTargets: number;
    maxRange: number;
    scanResolution: number;
    signatureRadius: number;
  };
  propulsion: {
    maxVelocity: number;
    agility: number;
    warpSpeed: number;
    mass: number;
  };
  cargoSpace: {
    total: number;
    used: number;
    remaining: number;
  };
}

export type SlotType = 'high' | 'med' | 'low' | 'rig' | 'subsystem' | 'service';
export type ModuleState = 'offline' | 'online' | 'active' | 'overloaded';

// Market Types
export interface MarketOrder {
  orderId: number;
  typeId: number;
  typeName: string;
  locationId: number;
  locationName: string;
  volumeTotal: number;
  volumeRemain: number;
  minVolume: number;
  price: number;
  isBuyOrder: boolean;
  duration: number;
  issued: Date;
  range: string;
  jumps?: number;
}

export interface MarketPrice {
  typeId: number;
  averagePrice: number;
  adjustedPrice: number;
}

export interface MarketHistory {
  date: Date;
  orderCount: number;
  volume: number;
  highest: number;
  average: number;
  lowest: number;
}

export interface PriceAnalysis {
  typeId: number;
  regionId: number;
  buyOrders: MarketOrder[];
  sellOrders: MarketOrder[];
  bestBuy: number;
  bestSell: number;
  spread: number;
  spreadPercent: number;
  volume24h: number;
  priceHistory: MarketHistory[];
  trend: 'up' | 'down' | 'stable';
}

// Universe Types
export interface Type {
  typeId: number;
  name: string;
  description: string;
  groupId: number;
  groupName: string;
  categoryId: number;
  categoryName: string;
  volume: number;
  mass: number;
  radius: number;
  marketGroupId?: number;
  iconId?: number;
  attributes: TypeAttribute[];
  effects: TypeEffect[];
}

export interface TypeAttribute {
  attributeId: number;
  attributeName: string;
  value: number;
  unit?: string;
  displayName?: string;
  description?: string;
  iconId?: number;
}

export interface TypeEffect {
  effectId: number;
  effectName: string;
  effectCategory: number;
  isDefault: boolean;
  dischargeAttributeId?: number;
  durationAttributeId?: number;
  rangeAttributeId?: number;
}

export interface Region {
  regionId: number;
  name: string;
  description: string;
  constellations: number[];
}

export interface SolarSystem {
  systemId: number;
  name: string;
  constellationId: number;
  regionId: number;
  securityStatus: number;
  securityClass: string;
  planets: number[];
  stations: number[];
  star?: {
    typeId: number;
    typeName: string;
  };
}

export interface Station {
  stationId: number;
  name: string;
  systemId: number;
  typeId: number;
  raceId?: number;
  corporationId: number;
  services: string[];
  dockingCostPerVolume?: number;
  maxShipVolumeDockable?: number;
  officeRentalCost?: number;
  reprocessingEfficiency?: number;
  reprocessingStationsTake?: number;
}

// Authentication Types
export interface AuthenticationState {
  isAuthenticated: boolean;
  characters: AuthenticatedCharacter[];
  activeCharacterId?: number;
  scopes: string[];
  expiresAt?: Date;
}

export interface AuthenticatedCharacter {
  characterId: number;
  characterName: string;
  corporationId: number;
  allianceId?: number;
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  scopes: string[];
  isMain: boolean;
}

// Error Types
export interface AppError {
  code: string;
  message: string;
  details?: any;
  timestamp: Date;
  stack?: string;
}

export interface ValidationError extends AppError {
  field: string;
  value: any;
  constraint: string;
}

// Notification Types
export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  timestamp: Date;
  duration?: number;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  label: string;
  action: () => void;
  style?: 'primary' | 'secondary' | 'danger';
}

// Settings Types
export interface UserSettings {
  general: GeneralSettings;
  appearance: AppearanceSettings;
  performance: PerformanceSettings;
  notifications: NotificationSettings;
  shortcuts: ShortcutSettings;
}

export interface GeneralSettings {
  language: string;
  region: string;
  timezone: string;
  currency: string;
  autoSave: boolean;
  checkForUpdates: boolean;
}

export interface AppearanceSettings {
  theme: 'dark' | 'light' | 'auto';
  colorScheme: 'default' | 'eve' | 'custom';
  fontSize: 'small' | 'medium' | 'large';
  enableAnimations: boolean;
  showTooltips: boolean;
  compactMode: boolean;
}

export interface PerformanceSettings {
  maxConcurrentRequests: number;
  cacheTimeout: number;
  enableDebugMode: boolean;
  logLevel: 'error' | 'warn' | 'info' | 'debug';
  memoryLimit: number;
}

export interface NotificationSettings {
  enableDesktopNotifications: boolean;
  enableSoundAlerts: boolean;
  skillTrainingComplete: boolean;
  marketPriceAlerts: boolean;
  maintenanceAlerts: boolean;
}

export interface ShortcutSettings {
  [action: string]: string;
}

// Search and Filter Types
export interface SearchFilters {
  query?: string;
  categories?: number[];
  groups?: number[];
  metaLevels?: number[];
  priceRange?: [number, number];
  volumeRange?: [number, number];
  publishedOnly?: boolean;
}

export interface SortOption {
  field: string;
  direction: 'asc' | 'desc';
  label: string;
}

// Export utility types
export type DeepPartial<T> = {
  [P in keyof T]?: T[P] extends object ? DeepPartial<T[P]> : T[P];
};

export type RequiredKeys<T, K extends keyof T> = T & Required<Pick<T, K>>;

export type Optional<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>;
/**
 * Shared type definitions for the Gideon application
 */

// EVE Online related types
export interface EVECharacter {
  characterId: number;
  name: string;
  corporationId: number;
  allianceId?: number;
  securityStatus: number;
  birthday: string;
}

export interface EVESkill {
  skillId: number;
  name: string;
  level: number;
  skillpoints: number;
  trainedLevel: number;
}

export interface EVEShip {
  typeId: number;
  name: string;
  groupId: number;
  categoryId: number;
  description: string;
  mass: number;
  volume: number;
  capacity?: number;
  // Ship specific attributes
  hiSlots: number;
  midSlots: number;
  lowSlots: number;
  rigSlots: number;
  subSlots?: number;
  maxVelocity: number;
  agility: number;
  droneBay: number;
  droneCapacity: number;
}

export interface EVEModule {
  typeId: number;
  name: string;
  groupId: number;
  categoryId: number;
  description: string;
  mass: number;
  volume: number;
  // Module specific attributes
  slot: ModuleSlot;
  cpu: number;
  powergrid: number;
  calibration?: number; // For rigs
  attributes: Record<string, number>;
}

export enum ModuleSlot {
  HIGH = 'high',
  MID = 'mid',
  LOW = 'low',
  RIG = 'rig',
  SUBSYSTEM = 'subsystem',
}

export interface ShipFitting {
  id: string;
  name: string;
  shipTypeId: number;
  description?: string;
  modules: FittingModule[];
  createdAt: string;
  updatedAt: string;
  tags: string[];
  author?: string;
  activity?: FittingActivity;
}

export interface FittingModule {
  typeId: number;
  slot: ModuleSlot;
  slotIndex: number;
  charge?: number;
  ammo?: number;
  script?: number;
  state: ModuleState;
}

export enum ModuleState {
  OFFLINE = 'offline',
  ONLINE = 'online',
  ACTIVE = 'active',
  OVERHEATED = 'overheated',
}

export enum FittingActivity {
  PVP = 'pvp',
  PVE = 'pve',
  MINING = 'mining',
  EXPLORATION = 'exploration',
  HAULING = 'hauling',
  INDUSTRY = 'industry',
}

// Performance calculation results
export interface FittingStats {
  dps: DPSStats;
  tank: TankStats;
  capacitor: CapacitorStats;
  navigation: NavigationStats;
  targeting: TargetingStats;
  cost: CostStats;
}

export interface DPSStats {
  total: number;
  turret: number;
  missile: number;
  drone: number;
  alpha: number;
  volley: number;
}

export interface TankStats {
  ehp: number;
  shield: number;
  armor: number;
  hull: number;
  shieldRecharge: number;
  armorRepair: number;
  hullRepair: number;
  sustainableTank: number;
}

export interface CapacitorStats {
  capacity: number;
  recharge: number;
  usage: number;
  stable: boolean;
  stability: number;
  timeToEmpty: number;
}

export interface NavigationStats {
  maxVelocity: number;
  agility: number;
  alignTime: number;
  warpSpeed: number;
  signature: number;
}

export interface TargetingStats {
  maxTargets: number;
  maxRange: number;
  scanResolution: number;
  signatureResolution: number;
}

export interface CostStats {
  total: number;
  ship: number;
  modules: number;
  insurance: number;
  replacement: number;
}

// Market data types
export interface MarketPrice {
  typeId: number;
  regionId: number;
  buy: number;
  sell: number;
  volume: number;
  timestamp: string;
}

export interface MarketOrder {
  orderId: number;
  typeId: number;
  regionId: number;
  locationId: number;
  price: number;
  volume: number;
  minVolume: number;
  duration: number;
  issued: string;
  isBuyOrder: boolean;
}

// ESI API types
export interface ESIAuthToken {
  accessToken: string;
  refreshToken: string;
  expiresAt: number;
  characterId: number;
  scopes: string[];
}

// Application state types
export interface AppConfig {
  theme: 'dark' | 'light';
  language: string;
  autoUpdate: boolean;
  telemetry: boolean;
  cache: CacheConfig;
  performance: PerformanceConfig;
}

export interface CacheConfig {
  maxSize: number; // MB
  ttl: number; // seconds
  cleanupInterval: number; // seconds
}

export interface PerformanceConfig {
  maxMemory: number; // MB
  renderFPS: number;
  backgroundTasks: boolean;
}

// UI state types
export interface UIState {
  sidebarOpen: boolean;
  activeModule: string;
  notifications: Notification[];
  loading: boolean;
  error: string | null;
}

export interface Notification {
  id: string;
  type: 'info' | 'success' | 'warning' | 'error';
  title: string;
  message: string;
  timestamp: string;
  duration?: number;
}
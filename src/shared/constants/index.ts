/**
 * Shared constants for the Gideon application
 */

// Application constants
export const APP_NAME = 'Gideon - EVE Online AI Copilot';
export const APP_VERSION = '0.1.0';
export const APP_DESCRIPTION = 'Comprehensive desktop application for EVE Online ship fitting, character planning, and market analysis';

// ESI API constants
export const ESI_BASE_URL = 'https://esi.evetech.net';
export const ESI_VERSION = 'latest';
export const ESI_DATASOURCE = 'tranquility';
export const ESI_CLIENT_ID = 'your-esi-client-id'; // Will be configured
export const ESI_REDIRECT_URI = 'http://localhost:3000/callback';

// Required ESI scopes
export const ESI_SCOPES = [
  'esi-skills.read_skills.v1',
  'esi-skills.read_skillqueue.v1',
  'esi-characters.read_character_info.v1',
  'esi-assets.read_assets.v1',
  'esi-clones.read_clones.v1',
  'esi-markets.structure_markets.v1',
  'esi-characters.read_corporation_roles.v1',
  'esi-wallet.read_character_wallet.v1',
];

// EVE Online game constants
export const EVE_REGIONS = {
  THE_FORGE: 10000002,
  DOMAIN: 10000043,
  SINQ_LAISON: 10000032,
  HEIMATAR: 10000030,
  METROPOLIS: 10000042,
} as const;

export const EVE_STATIONS = {
  JITA_4_4: 60003760,
  AMARR_VIII: 60008494,
  DODIXIE_IX: 60011866,
  RENS_VI: 60004588,
  HEK_VIII: 60005686,
} as const;

// Ship categories and groups
export const SHIP_CATEGORIES = {
  SHIP: 6,
  STRUCTURE: 65,
  DEPLOYABLE: 22,
} as const;

export const SHIP_GROUPS = {
  // Frigates
  FRIGATE: 25,
  ASSAULT_FRIGATE: 324,
  INTERCEPTOR: 831,
  COVERT_OPS: 830,
  ELECTRONIC_ATTACK_FRIGATE: 893,
  STEALTH_BOMBER: 834,
  
  // Destroyers
  DESTROYER: 420,
  INTERDICTOR: 541,
  COMMAND_DESTROYER: 1534,
  
  // Cruisers
  CRUISER: 26,
  HEAVY_ASSAULT_CRUISER: 358,
  RECON_SHIP: 833,
  HEAVY_INTERDICTOR: 894,
  LOGISTICS_CRUISER: 832,
  
  // Battlecruisers
  BATTLECRUISER: 419,
  COMMAND_SHIP: 540,
  
  // Battleships
  BATTLESHIP: 27,
  MARAUDER: 1202,
  BLACK_OPS: 898,
  
  // Capitals
  DREADNOUGHT: 485,
  CARRIER: 547,
  SUPERCARRIER: 659,
  TITAN: 30,
  FORCE_AUXILIARY: 1538,
} as const;

// Module slots
export const MODULE_SLOTS = {
  HIGH: 'high',
  MID: 'mid',
  LOW: 'low',
  RIG: 'rig',
  SUBSYSTEM: 'subsystem',
} as const;

// Attribute IDs for calculations
export const ATTRIBUTES = {
  // Ship attributes
  MAX_VELOCITY: 37,
  AGILITY: 70,
  MASS: 4,
  VOLUME: 161,
  CAPACITY: 38,
  
  // Slot attributes
  HI_SLOTS: 14,
  MID_SLOTS: 13,
  LOW_SLOTS: 12,
  RIG_SLOTS: 1137,
  SUBSYSTEM_SLOTS: 1367,
  
  // Fitting attributes
  CPU: 48,
  POWERGRID: 11,
  CALIBRATION: 1132,
  
  // Tank attributes
  HP: 9,
  SHIELD_CAPACITY: 263,
  ARMOR_HP: 265,
  STRUCTURE_HP: 9,
  
  // Damage attributes
  DAMAGE_MULTIPLIER: 64,
  RATE_OF_FIRE: 51,
  CHARGE_SIZE: 128,
  
  // Capacitor attributes
  CAPACITOR_CAPACITY: 482,
  CAPACITOR_RECHARGE: 55,
  
  // Targeting attributes
  MAX_TARGETS: 192,
  MAX_TARGET_RANGE: 76,
  SCAN_RESOLUTION: 564,
  SIGNATURE_RADIUS: 552,
} as const;

// UI constants
export const THEME_COLORS = {
  // EVE-inspired sci-fi color palette
  PRIMARY_BG: '#0a0e1a',
  SECONDARY_BG: '#1a1f2e',
  ACCENT_BLUE: '#00d4ff',
  ACCENT_ORANGE: '#ff8c00',
  SUCCESS_GREEN: '#00ff88',
  WARNING_YELLOW: '#ffcc00',
  ERROR_RED: '#ff4444',
  TEXT_PRIMARY: '#ffffff',
  TEXT_SECONDARY: '#a0a8b8',
  BORDER_PRIMARY: '#2a3441',
  BORDER_ACCENT: '#3a4552',
} as const;

export const ANIMATIONS = {
  FAST: 150,
  NORMAL: 300,
  SLOW: 500,
  VERY_SLOW: 1000,
} as const;

// Performance constants
export const PERFORMANCE = {
  MAX_MEMORY_MB: 500,
  TARGET_FPS: 60,
  RENDER_DISTANCE: 1000,
  LOD_LEVELS: 3,
} as const;

// Cache constants
export const CACHE = {
  DEFAULT_TTL: 300000, // 5 minutes
  MARKET_DATA_TTL: 900000, // 15 minutes
  STATIC_DATA_TTL: 86400000, // 24 hours
  CHARACTER_DATA_TTL: 300000, // 5 minutes
} as const;

// File paths and extensions
export const FILE_EXTENSIONS = {
  FITTING_EFT: '.txt',
  FITTING_DNA: '.dna',
  FITTING_XML: '.xml',
  FITTING_JSON: '.json',
} as const;

export const MIME_TYPES = {
  JSON: 'application/json',
  XML: 'application/xml',
  TEXT: 'text/plain',
} as const;
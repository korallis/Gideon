/**
 * Shared Application Constants
 * Constants used across main and renderer processes
 */

// Application Information
export const APP_NAME = 'Gideon';
export const APP_DESCRIPTION = 'EVE Online AI Copilot';
export const APP_VERSION = '0.1.0';
export const APP_AUTHOR = 'Gideon Team';

// Window Configuration
export const WINDOW_CONFIG = {
  MIN_WIDTH: 1200,
  MIN_HEIGHT: 800,
  DEFAULT_WIDTH: 1400,
  DEFAULT_HEIGHT: 900,
  SIDEBAR_WIDTH: 280,
  TOPBAR_HEIGHT: 60,
} as const;

// API Configuration
export const ESI_CONFIG = {
  BASE_URL: 'https://esi.evetech.net/latest',
  OAUTH_URL: 'https://login.eveonline.com/v2/oauth',
  SCOPES: [
    'esi-skills.read_skills.v1',
    'esi-skills.read_skillqueue.v1',
    'esi-characters.read_character_info.v1',
    'esi-characters.read_attributes.v1',
    'esi-characters.read_implants.v1',
    'esi-assets.read_assets.v1',
    'esi-wallet.read_character_wallet.v1',
    'esi-location.read_location.v1',
    'esi-location.read_ship_type.v1',
    'esi-markets.read_character_orders.v1',
  ],
  RATE_LIMIT: {
    ERROR_LIMIT: 100,
    RESET_TIME: 60000, // 1 minute
  },
} as const;

// Database Configuration
export const DATABASE_CONFIG = {
  NAME: 'GideonDatabase',
  VERSION: 1,
  CACHE_EXPIRY: {
    SHORT: 5 * 60 * 1000, // 5 minutes
    MEDIUM: 30 * 60 * 1000, // 30 minutes
    LONG: 24 * 60 * 60 * 1000, // 24 hours
  },
} as const;

// Performance Configuration
export const PERFORMANCE_CONFIG = {
  MAX_MEMORY_MB: 500,
  STARTUP_TIMEOUT_MS: 5000,
  UI_RESPONSE_TIMEOUT_MS: 100,
  MAX_CONCURRENT_REQUESTS: 5,
  REQUEST_TIMEOUT_MS: 30000,
  CALCULATION_ACCURACY: 0.001, // 0.1%
} as const;

// UI Constants
export const UI_CONFIG = {
  ANIMATION_DURATION: 200,
  TOAST_DURATION: 5000,
  TOOLTIP_DELAY: 500,
  DEBOUNCE_DELAY: 300,
  VIRTUAL_LIST_ITEM_HEIGHT: 60,
  PAGINATION_SIZE: 50,
} as const;

// Theme Constants
export const THEME_CONFIG = {
  COLORS: {
    PRIMARY: '#4A9EFF',
    SECONDARY: '#FF6B35', 
    SUCCESS: '#4CAF50',
    WARNING: '#FFC107',
    ERROR: '#F44336',
    INFO: '#2196F3',
  },
  BREAKPOINTS: {
    MOBILE: 768,
    TABLET: 1024,
    DESKTOP: 1200,
    WIDE: 1600,
  },
  Z_INDEX: {
    MODAL: 1000,
    TOOLTIP: 1001,
    NOTIFICATION: 1002,
    OVERLAY: 1003,
  },
} as const;

// EVE Online Constants
export const EVE_CONSTANTS = {
  // Skill related
  SKILL_LEVELS: [0, 1, 2, 3, 4, 5] as const,
  MAX_SKILL_LEVEL: 5,
  ALPHA_SP_LIMIT: 5000000,
  
  // Attribute related
  ATTRIBUTES: ['intelligence', 'memory', 'charisma', 'perception', 'willpower'] as const,
  MIN_ATTRIBUTE_VALUE: 17,
  MAX_ATTRIBUTE_VALUE: 35,
  
  // Ship related
  SLOT_TYPES: ['high', 'med', 'low', 'rig', 'subsystem', 'service'] as const,
  MODULE_STATES: ['offline', 'online', 'active', 'overloaded'] as const,
  
  // Market related
  JITA_REGION_ID: 10000002,
  JITA_SYSTEM_ID: 30000142,
  AMARR_REGION_ID: 10000043,
  DODIXIE_REGION_ID: 10000032,
  RENS_REGION_ID: 10000030,
  HEK_REGION_ID: 10000042,
  
  // Time related
  SKILL_TRAINING_MULTIPLIER: 250,
  CACHE_TIMEOUT: {
    MARKET: 5 * 60 * 1000, // 5 minutes
    SKILLS: 60 * 60 * 1000, // 1 hour
    CHARACTER: 30 * 60 * 1000, // 30 minutes
    UNIVERSE: 24 * 60 * 60 * 1000, // 24 hours
  },
} as const;

// File paths and directories
export const PATHS = {
  USER_DATA: 'userData',
  LOGS: 'logs',
  CACHE: 'cache',
  SETTINGS: 'settings.json',
  DATABASE: 'gideon.db',
  BACKUPS: 'backups',
} as const;

// Keyboard shortcuts
export const SHORTCUTS = {
  SAVE_FITTING: 'ctrl+s',
  NEW_FITTING: 'ctrl+n',
  OPEN_FITTING: 'ctrl+o',
  COPY_FITTING: 'ctrl+c',
  PASTE_FITTING: 'ctrl+v',
  SEARCH: 'ctrl+f',
  TOGGLE_SIDEBAR: 'ctrl+b',
  REFRESH: 'f5',
  TOGGLE_DEVTOOLS: 'f12',
  QUIT: 'ctrl+q',
} as const;

// Notification types
export const NOTIFICATION_TYPES = {
  SUCCESS: 'success',
  ERROR: 'error',
  WARNING: 'warning',
  INFO: 'info',
} as const;

// Error codes
export const ERROR_CODES = {
  // Network errors
  NETWORK_ERROR: 'NETWORK_ERROR',
  TIMEOUT_ERROR: 'TIMEOUT_ERROR',
  RATE_LIMIT_ERROR: 'RATE_LIMIT_ERROR',
  
  // Authentication errors
  AUTH_ERROR: 'AUTH_ERROR',
  TOKEN_EXPIRED: 'TOKEN_EXPIRED',
  INVALID_TOKEN: 'INVALID_TOKEN',
  INSUFFICIENT_SCOPES: 'INSUFFICIENT_SCOPES',
  
  // Validation errors
  VALIDATION_ERROR: 'VALIDATION_ERROR',
  INVALID_FITTING: 'INVALID_FITTING',
  INVALID_CHARACTER: 'INVALID_CHARACTER',
  
  // Application errors
  CALCULATION_ERROR: 'CALCULATION_ERROR',
  STORAGE_ERROR: 'STORAGE_ERROR',
  IMPORT_ERROR: 'IMPORT_ERROR',
  EXPORT_ERROR: 'EXPORT_ERROR',
} as const;

// Feature flags
export const FEATURE_FLAGS = {
  ENABLE_BETA_FEATURES: false,
  ENABLE_ANALYTICS: false,
  ENABLE_AUTO_UPDATES: true,
  ENABLE_OFFLINE_MODE: true,
  ENABLE_DEBUG_MODE: false,
  ENABLE_3D_VISUALIZATION: true,
  ENABLE_ADVANCED_CALCULATIONS: true,
} as const;

// Regular expressions
export const REGEX = {
  EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  CHARACTER_NAME: /^[a-zA-Z0-9'\-\s]+$/,
  FITTING_NAME: /^[a-zA-Z0-9'\-\s\[\]()]+$/,
  ISK_VALUE: /^\d+(\.\d{1,2})?$/,
} as const;

// Default settings
export const DEFAULT_SETTINGS = {
  general: {
    language: 'en',
    region: 'us',
    timezone: 'UTC',
    currency: 'ISK',
    autoSave: true,
    checkForUpdates: true,
  },
  appearance: {
    theme: 'dark' as const,
    colorScheme: 'eve' as const,
    fontSize: 'medium' as const,
    enableAnimations: true,
    showTooltips: true,
    compactMode: false,
  },
  performance: {
    maxConcurrentRequests: 5,
    cacheTimeout: 300000, // 5 minutes
    enableDebugMode: false,
    logLevel: 'warn' as const,
    memoryLimit: 500,
  },
  notifications: {
    enableDesktopNotifications: true,
    enableSoundAlerts: false,
    skillTrainingComplete: true,
    marketPriceAlerts: true,
    maintenanceAlerts: true,
  },
} as const;

// Export all constants as a single object for convenience
export const CONSTANTS = {
  APP_NAME,
  APP_DESCRIPTION,
  APP_VERSION,
  APP_AUTHOR,
  WINDOW_CONFIG,
  ESI_CONFIG,
  DATABASE_CONFIG,
  PERFORMANCE_CONFIG,
  UI_CONFIG,
  THEME_CONFIG,
  EVE_CONSTANTS,
  PATHS,
  SHORTCUTS,
  NOTIFICATION_TYPES,
  ERROR_CODES,
  FEATURE_FLAGS,
  REGEX,
  DEFAULT_SETTINGS,
} as const;
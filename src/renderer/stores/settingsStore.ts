import { create } from 'zustand';
import { devtools, persist } from 'zustand/middleware';
import { UserSettings } from '../../shared/types';

interface SettingsState extends UserSettings {
  // Additional UI state
  isDirty: boolean;
  lastSaved: Date | null;
  backupAvailable: boolean;
  
  // Actions
  updateGeneralSettings: (settings: Partial<UserSettings['general']>) => void;
  updateAppearanceSettings: (settings: Partial<UserSettings['appearance']>) => void;
  updatePerformanceSettings: (settings: Partial<UserSettings['performance']>) => void;
  updateNotificationSettings: (settings: Partial<UserSettings['notifications']>) => void;
  updateShortcutSettings: (settings: Partial<UserSettings['shortcuts']>) => void;
  
  // Bulk operations
  resetToDefaults: () => void;
  importSettings: (settings: UserSettings) => void;
  exportSettings: () => UserSettings;
  saveSettings: () => Promise<void>;
  loadSettings: () => Promise<void>;
  
  // Backup and restore
  createBackup: () => void;
  restoreFromBackup: () => void;
  
  // Validation
  validateSettings: () => { isValid: boolean; errors: string[] };
  
  // Utility
  setDirty: (dirty: boolean) => void;
}

const defaultSettings: UserSettings = {
  general: {
    language: 'en',
    region: 'us',
    timezone: 'UTC',
    currency: 'ISK',
    autoSave: true,
    checkForUpdates: true,
  },
  appearance: {
    theme: 'dark',
    colorScheme: 'default',
    fontSize: 'medium',
    enableAnimations: true,
    showTooltips: true,
    compactMode: false,
  },
  performance: {
    maxConcurrentRequests: 5,
    cacheTimeout: 300000, // 5 minutes
    enableDebugMode: false,
    logLevel: 'warn',
    memoryLimit: 500,
  },
  notifications: {
    enableDesktopNotifications: true,
    enableSoundAlerts: false,
    skillTrainingComplete: true,
    marketPriceAlerts: true,
    maintenanceAlerts: true,
  },
  shortcuts: {
    'save-fitting': 'ctrl+s',
    'new-fitting': 'ctrl+n',
    'open-fitting': 'ctrl+o',
    'copy-fitting': 'ctrl+c',
    'paste-fitting': 'ctrl+v',
    'search': 'ctrl+f',
    'toggle-sidebar': 'ctrl+b',
    'refresh': 'f5',
    'toggle-devtools': 'f12',
    'quit': 'ctrl+q',
  },
};

export const useSettingsStore = create<SettingsState>()(
  devtools(
    persist(
      (set, get) => ({
        // Initial state
        ...defaultSettings,
        isDirty: false,
        lastSaved: null,
        backupAvailable: false,

        // Actions
        updateGeneralSettings: (settings) => {
          set({
            general: { ...get().general, ...settings },
            isDirty: true,
          });
        },

        updateAppearanceSettings: (settings) => {
          set({
            appearance: { ...get().appearance, ...settings },
            isDirty: true,
          });

          // Apply theme changes immediately
          if (settings.theme) {
            document.documentElement.setAttribute('data-theme', settings.theme);
          }
          
          if (settings.fontSize) {
            document.documentElement.setAttribute('data-font-size', settings.fontSize);
          }
          
          if (settings.compactMode !== undefined) {
            document.documentElement.setAttribute('data-compact', settings.compactMode.toString());
          }
        },

        updatePerformanceSettings: (settings) => {
          set({
            performance: { ...get().performance, ...settings },
            isDirty: true,
          });

          // Apply performance settings immediately where applicable
          if (settings.enableDebugMode !== undefined) {
            if (settings.enableDebugMode) {
              console.log('Debug mode enabled');
            }
          }
        },

        updateNotificationSettings: (settings) => {
          set({
            notifications: { ...get().notifications, ...settings },
            isDirty: true,
          });

          // Request notification permission if enabling desktop notifications
          if (settings.enableDesktopNotifications && typeof Notification !== 'undefined') {
            if (Notification.permission === 'default') {
              Notification.requestPermission();
            }
          }
        },

        updateShortcutSettings: (settings) => {
          const currentShortcuts = get().shortcuts;
          const newShortcuts: Record<string, string> = {};
          
          // Copy existing shortcuts
          Object.entries(currentShortcuts).forEach(([key, value]) => {
            if (typeof value === 'string') {
              newShortcuts[key] = value;
            }
          });
          
          // Apply updates
          Object.entries(settings).forEach(([key, value]) => {
            if (typeof value === 'string') {
              newShortcuts[key] = value;
            }
          });
          
          set({
            shortcuts: newShortcuts,
            isDirty: true,
          });
        },

        // Bulk operations
        resetToDefaults: () => {
          set({
            ...defaultSettings,
            isDirty: true,
          });
        },

        importSettings: (settings) => {
          const validation = get().validateSettings();
          if (!validation.isValid) {
            throw new Error(`Invalid settings: ${validation.errors.join(', ')}`);
          }

          set({
            ...settings,
            isDirty: true,
          });
        },

        exportSettings: () => {
          const state = get();
          return {
            general: state.general,
            appearance: state.appearance,
            performance: state.performance,
            notifications: state.notifications,
            shortcuts: state.shortcuts,
          };
        },

        saveSettings: async () => {
          const settings = get().exportSettings();
          
          try {
            // Save to Electron store via IPC
            if (window.electronAPI) {
              const userDataPath = await window.electronAPI.app.getPath('userData');
              const settingsFile = `${userDataPath}/settings.json`;
              await window.electronAPI.fs.writeFile(settingsFile, JSON.stringify(settings, null, 2));
            }

            set({
              isDirty: false,
              lastSaved: new Date(),
            });

          } catch (error) {
            console.error('Failed to save settings:', error);
            throw error;
          }
        },

        loadSettings: async () => {
          try {
            if (window.electronAPI) {
              const userDataPath = await window.electronAPI.app.getPath('userData');
              const settingsFile = `${userDataPath}/settings.json`;
              const exists = await window.electronAPI.fs.exists(settingsFile);
              
              if (exists) {
                const result = await window.electronAPI.fs.readFile(settingsFile);
                if (result.success && result.content) {
                  const savedSettings = JSON.parse(result.content);
                  get().importSettings(savedSettings);
                  set({ lastSaved: new Date() });
                }
              }
            }
          } catch (error) {
            console.error('Failed to load settings:', error);
            // Continue with defaults on error
          }
        },

        // Backup and restore
        createBackup: () => {
          const settings = get().exportSettings();
          const backup = {
            settings,
            timestamp: new Date().toISOString(),
            version: '1.0.0',
          };

          try {
            localStorage.setItem('gideon-settings-backup', JSON.stringify(backup));
            set({ backupAvailable: true });
          } catch (error) {
            console.error('Failed to create settings backup:', error);
          }
        },

        restoreFromBackup: () => {
          try {
            const backupData = localStorage.getItem('gideon-settings-backup');
            if (backupData) {
              const backup = JSON.parse(backupData);
              get().importSettings(backup.settings);
              set({ isDirty: true });
            }
          } catch (error) {
            console.error('Failed to restore settings from backup:', error);
            throw error;
          }
        },

        // Validation
        validateSettings: () => {
          const errors: string[] = [];
          const state = get();

          // Validate general settings
          if (!['en', 'es', 'fr', 'de', 'ru', 'zh', 'ja'].includes(state.general.language)) {
            errors.push('Invalid language setting');
          }

          // Validate appearance settings
          if (!['dark', 'light', 'auto'].includes(state.appearance.theme)) {
            errors.push('Invalid theme setting');
          }

          if (!['small', 'medium', 'large'].includes(state.appearance.fontSize)) {
            errors.push('Invalid font size setting');
          }

          // Validate performance settings
          if (state.performance.maxConcurrentRequests < 1 || state.performance.maxConcurrentRequests > 20) {
            errors.push('Max concurrent requests must be between 1 and 20');
          }

          if (state.performance.cacheTimeout < 60000 || state.performance.cacheTimeout > 3600000) {
            errors.push('Cache timeout must be between 1 minute and 1 hour');
          }

          if (state.performance.memoryLimit < 256 || state.performance.memoryLimit > 2048) {
            errors.push('Memory limit must be between 256MB and 2GB');
          }

          if (!['error', 'warn', 'info', 'debug'].includes(state.performance.logLevel)) {
            errors.push('Invalid log level setting');
          }

          // Validate shortcuts
          const shortcuts = Object.values(state.shortcuts).filter((shortcut): shortcut is string => 
            typeof shortcut === 'string'
          );
          const duplicateShortcuts = shortcuts.filter((shortcut, index) => 
            shortcuts.indexOf(shortcut) !== index
          );
          
          if (duplicateShortcuts.length > 0) {
            errors.push(`Duplicate keyboard shortcuts: ${duplicateShortcuts.join(', ')}`);
          }

          return {
            isValid: errors.length === 0,
            errors,
          };
        },

        // Utility
        setDirty: (dirty) => set({ isDirty: dirty }),
      }),
      {
        name: 'settings-store',
        partialize: (state) => ({
          general: state.general,
          appearance: state.appearance,
          performance: state.performance,
          notifications: state.notifications,
          shortcuts: state.shortcuts,
          lastSaved: state.lastSaved,
          backupAvailable: state.backupAvailable,
        }),
      }
    ),
    {
      name: 'settings-store',
    }
  )
);

// Initialize settings on app start
export const initializeSettings = async () => {
  const store = useSettingsStore.getState();
  
  try {
    await store.loadSettings();
    
    // Apply initial theme and appearance settings
    const { appearance } = store;
    document.documentElement.setAttribute('data-theme', appearance.theme);
    document.documentElement.setAttribute('data-font-size', appearance.fontSize);
    document.documentElement.setAttribute('data-compact', appearance.compactMode.toString());
    
    // Check for backup availability
    const backupExists = localStorage.getItem('gideon-settings-backup') !== null;
    if (backupExists) {
      store.setDirty(false);
      useSettingsStore.setState({ backupAvailable: true });
    }
    
  } catch (error) {
    console.error('Failed to initialize settings:', error);
  }
};
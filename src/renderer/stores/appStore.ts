import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { AppConfig, UIState } from '../../shared/types';

interface AppState {
  // App configuration
  config: AppConfig;
  
  // UI state
  ui: UIState;
  
  // App lifecycle
  isInitialized: boolean;
  isLoading: boolean;
  
  // Actions
  initializeApp: () => Promise<void>;
  setLoading: (loading: boolean) => void;
  updateConfig: (config: Partial<AppConfig>) => Promise<void>;
  updateUI: (ui: Partial<UIState>) => void;
  addNotification: (notification: Omit<UIState['notifications'][0], 'id' | 'timestamp'>) => void;
  removeNotification: (id: string) => void;
}

const defaultConfig: AppConfig = {
  theme: 'dark',
  language: 'en',
  autoUpdate: true,
  telemetry: false,
  cache: {
    maxSize: 256, // 256MB
    ttl: 300, // 5 minutes
    cleanupInterval: 600, // 10 minutes
  },
  performance: {
    maxMemory: 500, // 500MB
    renderFPS: 60,
    backgroundTasks: true,
  },
};

const defaultUI: UIState = {
  sidebarOpen: true,
  activeModule: 'fitting',
  notifications: [],
  loading: false,
  error: null,
};

export const useAppStore = create<AppState>()(
  devtools(
    (set, get) => ({
      config: defaultConfig,
      ui: defaultUI,
      isInitialized: false,
      isLoading: false,

      initializeApp: async () => {
        set({ isLoading: true });
        
        try {
          // Initialize database first
          const { initializeDatabase } = await import('../database');
          const dbInitialized = await initializeDatabase();
          
          if (!dbInitialized) {
            throw new Error('Database initialization failed');
          }
          
          // Load saved configuration from database
          const { db } = await import('../database');
          const savedTheme = await db.getSetting('app.theme', defaultConfig.theme);
          const savedLanguage = await db.getSetting('app.language', defaultConfig.language);
          const savedAutoUpdate = await db.getSetting('app.autoUpdate', defaultConfig.autoUpdate);
          const savedTelemetry = await db.getSetting('app.telemetry', defaultConfig.telemetry);
          
          const savedConfig = {
            ...defaultConfig,
            theme: savedTheme,
            language: savedLanguage,
            autoUpdate: savedAutoUpdate,
            telemetry: savedTelemetry,
          };
          
          set({ config: savedConfig });
          
          // Also try to load from Electron config file as fallback/migration
          if (window.electronAPI) {
            try {
              const configPath = await window.electronAPI.app.getPath('userData');
              const configFile = `${configPath}/app-config.json`;
              const configExists = await window.electronAPI.fs.exists(configFile);
              if (configExists) {
                const result = await window.electronAPI.fs.readFile(configFile);
                if (result.success && result.content) {
                  const electronConfig = JSON.parse(result.content);
                  // Migrate any settings that aren't in database yet
                  for (const [key, value] of Object.entries(electronConfig)) {
                    const dbKey = `app.${key}`;
                    const existing = await db.getSetting(dbKey);
                    if (existing === undefined) {
                      await db.setSetting(dbKey, value);
                    }
                  }
                }
              }
            } catch (error) {
              console.warn('Failed to load/migrate Electron configuration:', error);
            }
          }
          
          set({ isInitialized: true });
        } catch (error) {
          console.error('Failed to initialize app:', error);
          get().addNotification({
            type: 'error',
            title: 'Initialization Failed',
            message: 'Failed to initialize the application. Please restart.',
          });
        } finally {
          set({ isLoading: false });
        }
      },

      setLoading: (loading) => set({ isLoading: loading }),

      updateConfig: async (configUpdate) => {
        const newConfig = { ...get().config, ...configUpdate };
        set({ config: newConfig });
        
        // Persist configuration to database
        try {
          const { db } = await import('../database');
          
          // Save each config property to database
          for (const [key, value] of Object.entries(configUpdate)) {
            const dbKey = `app.${key}`;
            await db.setSetting(dbKey, value);
          }
        } catch (error) {
          console.warn('Failed to save configuration to database:', error);
        }
        
        // Also persist to Electron config file as backup
        if (window.electronAPI) {
          try {
            const configPath = await window.electronAPI.app.getPath('userData');
            const configFile = `${configPath}/app-config.json`;
            await window.electronAPI.fs.writeFile(configFile, JSON.stringify(newConfig, null, 2));
          } catch (error) {
            console.warn('Failed to save configuration to file:', error);
          }
        }
      },

      updateUI: (uiUpdate) => {
        set({ ui: { ...get().ui, ...uiUpdate } });
      },

      addNotification: (notification) => {
        const id = Math.random().toString(36).substr(2, 9);
        const timestamp = new Date();
        const newNotification = { ...notification, id, timestamp };
        
        set((state) => ({
          ui: {
            ...state.ui,
            notifications: [...state.ui.notifications, newNotification],
          },
        }));
        
        // Auto-remove notification after duration
        if (notification.duration !== undefined) {
          setTimeout(() => {
            get().removeNotification(id);
          }, notification.duration);
        }
      },

      removeNotification: (id) => {
        set((state) => ({
          ui: {
            ...state.ui,
            notifications: state.ui.notifications.filter((n: any) => n.id !== id),
          },
        }));
      },
    }),
    {
      name: 'app-store',
    }
  )
);
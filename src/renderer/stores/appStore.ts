import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import { AppConfig, UIState } from '@/types';

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
  updateConfig: (config: Partial<AppConfig>) => void;
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
          // Simulate initialization tasks
          await new Promise(resolve => setTimeout(resolve, 1000));
          
          // Load saved configuration
          if (window.electronAPI) {
            const savedConfig = await window.electronAPI.retrieveData('app-config');
            if (savedConfig) {
              set({ config: { ...defaultConfig, ...savedConfig } });
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

      updateConfig: (configUpdate) => {
        const newConfig = { ...get().config, ...configUpdate };
        set({ config: newConfig });
        
        // Persist configuration
        if (window.electronAPI) {
          window.electronAPI.storeData('app-config', newConfig);
        }
      },

      updateUI: (uiUpdate) => {
        set({ ui: { ...get().ui, ...uiUpdate } });
      },

      addNotification: (notification) => {
        const id = Math.random().toString(36).substr(2, 9);
        const timestamp = new Date().toISOString();
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
            notifications: state.ui.notifications.filter(n => n.id !== id),
          },
        }));
      },
    }),
    {
      name: 'app-store',
    }
  )
);
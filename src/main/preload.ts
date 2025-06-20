import { contextBridge, ipcRenderer } from 'electron';

/**
 * Secure IPC Bridge
 * Exposes main process functionality to renderer process safely
 */
contextBridge.exposeInMainWorld('electronAPI', {
  // =============================================================================
  // Application Information
  // =============================================================================
  app: {
    getVersion: () => ipcRenderer.invoke('app:get-version'),
    getName: () => ipcRenderer.invoke('app:get-name'),
    getPlatform: () => ipcRenderer.invoke('app:get-platform'),
    getPath: (name: string) => ipcRenderer.invoke('app:get-path', name),
  },

  // =============================================================================
  // Window Management
  // =============================================================================
  window: {
    minimize: () => ipcRenderer.invoke('window:minimize'),
    maximize: () => ipcRenderer.invoke('window:maximize'),
    close: () => ipcRenderer.invoke('window:close'),
    isMaximized: () => ipcRenderer.invoke('window:is-maximized'),
    getBounds: () => ipcRenderer.invoke('window:get-bounds'),
    setBounds: (bounds: any) => ipcRenderer.invoke('window:set-bounds', bounds),
  },

  // =============================================================================
  // File System Operations
  // =============================================================================
  fs: {
    readFile: (filePath: string) => ipcRenderer.invoke('fs:read-file', filePath),
    writeFile: (filePath: string, content: string) => ipcRenderer.invoke('fs:write-file', filePath, content),
    exists: (filePath: string) => ipcRenderer.invoke('fs:exists', filePath),
    mkdir: (dirPath: string) => ipcRenderer.invoke('fs:mkdir', dirPath),
  },

  // =============================================================================
  // Dialog Operations
  // =============================================================================
  dialog: {
    showOpen: (options: any) => ipcRenderer.invoke('dialog:show-open', options),
    showSave: (options: any) => ipcRenderer.invoke('dialog:show-save', options),
    showMessage: (options: any) => ipcRenderer.invoke('dialog:show-message', options),
    showError: (title: string, content: string) => ipcRenderer.invoke('dialog:show-error', title, content),
  },

  // =============================================================================
  // Shell Operations
  // =============================================================================
  shell: {
    openExternal: (url: string) => ipcRenderer.invoke('shell:open-external', url),
    showItemInFolder: (fullPath: string) => ipcRenderer.invoke('shell:show-item-in-folder', fullPath),
    openPath: (path: string) => ipcRenderer.invoke('shell:open-path', path),
  },

  // =============================================================================
  // Theme and Appearance
  // =============================================================================
  theme: {
    getNativeTheme: () => ipcRenderer.invoke('theme:get-native-theme'),
    setThemeSource: (source: 'system' | 'light' | 'dark') => ipcRenderer.invoke('theme:set-theme-source', source),
    onThemeChanged: (callback: (data: any) => void) => {
      ipcRenderer.on('theme:changed', (_event, data) => callback(data));
      return () => ipcRenderer.removeAllListeners('theme:changed');
    },
  },

  // =============================================================================
  // System Information
  // =============================================================================
  system: {
    getInfo: () => ipcRenderer.invoke('system:get-info'),
  },

  // =============================================================================
  // Secure Storage
  // =============================================================================
  storage: {
    isEncryptionAvailable: () => ipcRenderer.invoke('storage:is-encryption-available'),
    encryptString: (plainText: string) => ipcRenderer.invoke('storage:encrypt-string', plainText),
    decryptString: (encrypted: Buffer) => ipcRenderer.invoke('storage:decrypt-string', encrypted),
  },

  // =============================================================================
  // Network Operations
  // =============================================================================
  network: {
    isOnline: () => ipcRenderer.invoke('network:is-online'),
  },

  // =============================================================================
  // Development Tools (only available in development)
  // =============================================================================
  dev: {
    openDevTools: () => ipcRenderer.invoke('dev:open-devtools'),
    reload: () => ipcRenderer.invoke('dev:reload'),
    forceReload: () => ipcRenderer.invoke('dev:force-reload'),
  },

  // =============================================================================
  // Event Management
  // =============================================================================
  events: {
    // Generic event listener with cleanup
    on: (channel: string, callback: (data: any) => void) => {
      ipcRenderer.on(channel, (_event, data) => callback(data));
      return () => ipcRenderer.removeAllListeners(channel);
    },
    
    // Remove all listeners for a channel
    removeAllListeners: (channel: string) => {
      ipcRenderer.removeAllListeners(channel);
    },
  },
});

/**
 * TypeScript definitions for the Electron API
 * These types ensure type safety when using the API from the renderer process
 */
export interface ElectronAPI {
  // Application Information
  app: {
    getVersion: () => Promise<string>;
    getName: () => Promise<string>;
    getPlatform: () => Promise<{
      platform: string;
      arch: string;
      version: string;
      isPackaged: boolean;
    }>;
    getPath: (name: string) => Promise<string>;
  };

  // Window Management
  window: {
    minimize: () => Promise<void>;
    maximize: () => Promise<void>;
    close: () => Promise<void>;
    isMaximized: () => Promise<boolean>;
    getBounds: () => Promise<{
      x: number;
      y: number;
      width: number;
      height: number;
    }>;
    setBounds: (bounds: {
      x?: number;
      y?: number;
      width?: number;
      height?: number;
    }) => Promise<void>;
  };

  // File System Operations
  fs: {
    readFile: (filePath: string) => Promise<{
      success: boolean;
      content?: string;
      error?: string;
    }>;
    writeFile: (filePath: string, content: string) => Promise<{
      success: boolean;
      error?: string;
    }>;
    exists: (filePath: string) => Promise<boolean>;
    mkdir: (dirPath: string) => Promise<{
      success: boolean;
      error?: string;
    }>;
  };

  // Dialog Operations
  dialog: {
    showOpen: (options: any) => Promise<{
      canceled: boolean;
      filePaths?: string[];
    }>;
    showSave: (options: any) => Promise<{
      canceled: boolean;
      filePath?: string;
    }>;
    showMessage: (options: any) => Promise<{
      response: number;
      checkboxChecked?: boolean;
    }>;
    showError: (title: string, content: string) => Promise<void>;
  };

  // Shell Operations
  shell: {
    openExternal: (url: string) => Promise<{
      success: boolean;
      error?: string;
    }>;
    showItemInFolder: (fullPath: string) => Promise<void>;
    openPath: (path: string) => Promise<string>;
  };

  // Theme and Appearance
  theme: {
    getNativeTheme: () => Promise<{
      shouldUseDarkColors: boolean;
      themeSource: string;
      shouldUseHighContrastColors: boolean;
      shouldUseInvertedColorScheme: boolean;
    }>;
    setThemeSource: (source: 'system' | 'light' | 'dark') => Promise<void>;
    onThemeChanged: (callback: (data: {
      shouldUseDarkColors: boolean;
      themeSource: string;
    }) => void) => (() => void);
  };

  // System Information
  system: {
    getInfo: () => Promise<{
      platform: string;
      arch: string;
      version: string;
      userInfo: any;
      totalMemory: number;
      freeMemory: number;
      cpus: any[];
      networkInterfaces: any;
    }>;
  };

  // Secure Storage
  storage: {
    isEncryptionAvailable: () => Promise<boolean>;
    encryptString: (plainText: string) => Promise<Buffer>;
    decryptString: (encrypted: Buffer) => Promise<string>;
  };

  // Network Operations
  network: {
    isOnline: () => Promise<boolean>;
  };

  // Development Tools
  dev: {
    openDevTools: () => Promise<void>;
    reload: () => Promise<void>;
    forceReload: () => Promise<void>;
  };

  // Event Management
  events: {
    on: (channel: string, callback: (data: any) => void) => (() => void);
    removeAllListeners: (channel: string) => void;
  };
}

declare global {
  interface Window {
    electronAPI: ElectronAPI;
  }
}
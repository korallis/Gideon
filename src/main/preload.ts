import { contextBridge, ipcRenderer } from 'electron';

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // App info
  getAppVersion: () => ipcRenderer.invoke('app-version'),
  getAppName: () => ipcRenderer.invoke('app-name'),
  
  // Window controls (will be added as needed)
  minimizeWindow: () => ipcRenderer.invoke('window-minimize'),
  maximizeWindow: () => ipcRenderer.invoke('window-maximize'),
  closeWindow: () => ipcRenderer.invoke('window-close'),
  
  // File system operations (will be added as needed)
  openFile: (filters?: any) => ipcRenderer.invoke('dialog-open-file', filters),
  saveFile: (defaultPath?: string, filters?: any) => ipcRenderer.invoke('dialog-save-file', defaultPath, filters),
  
  // ESI API operations (will be added as needed)
  authenticateESI: (scopes: string[]) => ipcRenderer.invoke('esi-authenticate', scopes),
  getCharacterData: (characterId: number) => ipcRenderer.invoke('esi-character-data', characterId),
  
  // Local storage operations (will be added as needed)
  storeData: (key: string, data: any) => ipcRenderer.invoke('store-data', key, data),
  retrieveData: (key: string) => ipcRenderer.invoke('retrieve-data', key),
  
  // Listeners for main process events
  onAppUpdate: (callback: (info: any) => void) => {
    ipcRenderer.on('app-update', (_event, info) => callback(info));
  },
  
  onESIAuthResult: (callback: (result: any) => void) => {
    ipcRenderer.on('esi-auth-result', (_event, result) => callback(result));
  },
  
  // Remove listeners
  removeAllListeners: (channel: string) => {
    ipcRenderer.removeAllListeners(channel);
  },
});

// Type definitions for the exposed API
export interface ElectronAPI {
  // App info
  getAppVersion: () => Promise<string>;
  getAppName: () => Promise<string>;
  
  // Window controls
  minimizeWindow: () => Promise<void>;
  maximizeWindow: () => Promise<void>;
  closeWindow: () => Promise<void>;
  
  // File system
  openFile: (filters?: any) => Promise<string | null>;
  saveFile: (defaultPath?: string, filters?: any) => Promise<string | null>;
  
  // ESI API
  authenticateESI: (scopes: string[]) => Promise<any>;
  getCharacterData: (characterId: number) => Promise<any>;
  
  // Local storage
  storeData: (key: string, data: any) => Promise<void>;
  retrieveData: (key: string) => Promise<any>;
  
  // Event listeners
  onAppUpdate: (callback: (info: any) => void) => void;
  onESIAuthResult: (callback: (result: any) => void) => void;
  removeAllListeners: (channel: string) => void;
}

declare global {
  interface Window {
    electronAPI: ElectronAPI;
  }
}
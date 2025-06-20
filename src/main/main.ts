import { app, BrowserWindow, ipcMain, Menu, shell, dialog, nativeTheme } from 'electron';
import * as path from 'path';
import * as fs from 'fs';
import * as os from 'os';
import { isDev } from '../shared/utils/env';
import { APP_NAME, WINDOW_CONFIG } from '../shared/constants';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) {
  app.quit();
}

let mainWindow: BrowserWindow | null = null;

const createWindow = (): void => {
  // Create the browser window.
  mainWindow = new BrowserWindow({
    width: WINDOW_CONFIG.DEFAULT_WIDTH,
    height: WINDOW_CONFIG.DEFAULT_HEIGHT,
    minWidth: WINDOW_CONFIG.MIN_WIDTH,
    minHeight: WINDOW_CONFIG.MIN_HEIGHT,
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
      enableRemoteModule: false,
      webSecurity: true,
      sandbox: false, // Required for some Electron APIs
      allowRunningInsecureContent: false,
    },
    icon: path.join(__dirname, '../../public/icon.png'),
    titleBarStyle: process.platform === 'darwin' ? 'hiddenInset' : 'default',
    show: false, // Don't show until ready-to-show
    backgroundColor: '#0D1117', // Dark theme background
    title: APP_NAME,
    vibrancy: process.platform === 'darwin' ? 'dark' : undefined,
  });

  // Load the appropriate URL based on environment
  if (isDev) {
    mainWindow.loadURL('http://localhost:5173');
    // Open DevTools in development
    mainWindow.webContents.openDevTools();
  } else {
    mainWindow.loadFile(path.join(__dirname, '../renderer/index.html'));
  }

  // Show window when ready to prevent visual flash
  mainWindow.once('ready-to-show', () => {
    mainWindow?.show();
    
    // Focus on window on creation
    if (isDev) {
      mainWindow?.webContents.focus();
    }
  });

  // Handle window closed
  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Security: Prevent new window creation
  mainWindow.webContents.setWindowOpenHandler(() => {
    return { action: 'deny' };
  });
};

// This method will be called when Electron has finished initialization
app.whenReady().then(() => {
  // Set application menu
  if (process.platform === 'darwin') {
    // macOS menu
    const template = [
      {
        label: app.getName(),
        submenu: [
          { role: 'about' },
          { type: 'separator' },
          { role: 'services' },
          { type: 'separator' },
          { role: 'hide' },
          { role: 'hideothers' },
          { role: 'unhide' },
          { type: 'separator' },
          { role: 'quit' }
        ]
      },
      {
        label: 'File',
        submenu: [
          { role: 'close' }
        ]
      },
      {
        label: 'Edit',
        submenu: [
          { role: 'undo' },
          { role: 'redo' },
          { type: 'separator' },
          { role: 'cut' },
          { role: 'copy' },
          { role: 'paste' },
          { role: 'selectall' }
        ]
      },
      {
        label: 'View',
        submenu: [
          { role: 'reload' },
          { role: 'forceReload' },
          { role: 'toggleDevTools' },
          { type: 'separator' },
          { role: 'resetZoom' },
          { role: 'zoomIn' },
          { role: 'zoomOut' },
          { type: 'separator' },
          { role: 'togglefullscreen' }
        ]
      },
      {
        label: 'Window',
        submenu: [
          { role: 'minimize' },
          { role: 'close' }
        ]
      }
    ];
    // @ts-ignore - Electron types issue
    Menu.setApplicationMenu(Menu.buildFromTemplate(template));
  } else {
    // Windows/Linux menu
    Menu.setApplicationMenu(null);
  }

  createWindow();

  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

// Quit when all windows are closed, except on macOS
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

// Security: Prevent navigation to external URLs
app.on('web-contents-created', (_, contents) => {
  contents.on('will-navigate', (navigationEvent, navigationUrl) => {
    const parsedUrl = new URL(navigationUrl);
    
    if (parsedUrl.origin !== 'http://localhost:5173' && parsedUrl.origin !== 'file://') {
      navigationEvent.preventDefault();
    }
  });
});

// =============================================================================
// IPC Handlers - Secure communication between main and renderer processes
// =============================================================================

/**
 * Application Information
 */
ipcMain.handle('app:get-version', () => {
  return app.getVersion();
});

ipcMain.handle('app:get-name', () => {
  return app.getName();
});

ipcMain.handle('app:get-platform', () => {
  return {
    platform: process.platform,
    arch: process.arch,
    version: process.version,
    isPackaged: app.isPackaged,
  };
});

ipcMain.handle('app:get-path', (_, name: string) => {
  return app.getPath(name as any);
});

/**
 * Window Management
 */
ipcMain.handle('window:minimize', () => {
  mainWindow?.minimize();
});

ipcMain.handle('window:maximize', () => {
  if (mainWindow?.isMaximized()) {
    mainWindow.unmaximize();
  } else {
    mainWindow?.maximize();
  }
});

ipcMain.handle('window:close', () => {
  mainWindow?.close();
});

ipcMain.handle('window:is-maximized', () => {
  return mainWindow?.isMaximized() || false;
});

ipcMain.handle('window:get-bounds', () => {
  return mainWindow?.getBounds();
});

ipcMain.handle('window:set-bounds', (_, bounds: any) => {
  mainWindow?.setBounds(bounds);
});

/**
 * File System Operations
 */
ipcMain.handle('fs:read-file', async (_, filePath: string) => {
  try {
    const content = await fs.promises.readFile(filePath, 'utf-8');
    return { success: true, content };
  } catch (error) {
    return { success: false, error: error instanceof Error ? error.message : 'Unknown error' };
  }
});

ipcMain.handle('fs:write-file', async (_, filePath: string, content: string) => {
  try {
    await fs.promises.writeFile(filePath, content, 'utf-8');
    return { success: true };
  } catch (error) {
    return { success: false, error: error instanceof Error ? error.message : 'Unknown error' };
  }
});

ipcMain.handle('fs:exists', async (_, filePath: string) => {
  try {
    await fs.promises.access(filePath);
    return true;
  } catch {
    return false;
  }
});

ipcMain.handle('fs:mkdir', async (_, dirPath: string) => {
  try {
    await fs.promises.mkdir(dirPath, { recursive: true });
    return { success: true };
  } catch (error) {
    return { success: false, error: error instanceof Error ? error.message : 'Unknown error' };
  }
});

/**
 * Dialog Operations
 */
ipcMain.handle('dialog:show-open', async (_, options: any) => {
  if (!mainWindow) return { canceled: true };
  
  const result = await dialog.showOpenDialog(mainWindow, options);
  return result;
});

ipcMain.handle('dialog:show-save', async (_, options: any) => {
  if (!mainWindow) return { canceled: true };
  
  const result = await dialog.showSaveDialog(mainWindow, options);
  return result;
});

ipcMain.handle('dialog:show-message', async (_, options: any) => {
  if (!mainWindow) return { response: 0 };
  
  const result = await dialog.showMessageBox(mainWindow, options);
  return result;
});

ipcMain.handle('dialog:show-error', async (_, title: string, content: string) => {
  dialog.showErrorBox(title, content);
});

/**
 * Shell Operations
 */
ipcMain.handle('shell:open-external', async (_, url: string) => {
  try {
    await shell.openExternal(url);
    return { success: true };
  } catch (error) {
    return { success: false, error: error instanceof Error ? error.message : 'Unknown error' };
  }
});

ipcMain.handle('shell:show-item-in-folder', (_, fullPath: string) => {
  return shell.showItemInFolder(fullPath);
});

ipcMain.handle('shell:open-path', async (_, path: string) => {
  const result = await shell.openPath(path);
  return result;
});

/**
 * Theme and Appearance
 */
ipcMain.handle('theme:get-native-theme', () => {
  return {
    shouldUseDarkColors: nativeTheme.shouldUseDarkColors,
    themeSource: nativeTheme.themeSource,
    shouldUseHighContrastColors: nativeTheme.shouldUseHighContrastColors,
    shouldUseInvertedColorScheme: nativeTheme.shouldUseInvertedColorScheme,
  };
});

ipcMain.handle('theme:set-theme-source', (_, source: 'system' | 'light' | 'dark') => {
  nativeTheme.themeSource = source;
});

// Listen for theme changes
nativeTheme.on('updated', () => {
  mainWindow?.webContents.send('theme:changed', {
    shouldUseDarkColors: nativeTheme.shouldUseDarkColors,
    themeSource: nativeTheme.themeSource,
  });
});

/**
 * System Information
 */
ipcMain.handle('system:get-info', () => {
  return {
    platform: process.platform,
    arch: process.arch,
    version: process.version,
    userInfo: os.userInfo(),
    totalMemory: os.totalmem(),
    freeMemory: os.freemem(),
    cpus: os.cpus(),
    networkInterfaces: os.networkInterfaces(),
  };
});

/**
 * Secure Storage (using Electron's built-in safeStorage)
 */
ipcMain.handle('storage:is-encryption-available', () => {
  return (app as any).safeStorage?.isEncryptionAvailable() || false;
});

ipcMain.handle('storage:encrypt-string', (_, plainText: string) => {
  if (!(app as any).safeStorage?.isEncryptionAvailable()) {
    throw new Error('Encryption not available');
  }
  return (app as any).safeStorage.encryptString(plainText);
});

ipcMain.handle('storage:decrypt-string', (_, encrypted: Buffer) => {
  if (!(app as any).safeStorage?.isEncryptionAvailable()) {
    throw new Error('Encryption not available');
  }
  return (app as any).safeStorage.decryptString(encrypted);
});

/**
 * Network Status
 */
ipcMain.handle('network:is-online', () => {
  return require('dns').promises.resolve('google.com')
    .then(() => true)
    .catch(() => false);
});

/**
 * Development Tools
 */
if (isDev) {
  ipcMain.handle('dev:open-devtools', () => {
    mainWindow?.webContents.openDevTools();
  });

  ipcMain.handle('dev:reload', () => {
    mainWindow?.webContents.reload();
  });

  ipcMain.handle('dev:force-reload', () => {
    mainWindow?.webContents.reloadIgnoringCache();
  });
}

// Security: Remove unused IPC handlers on app quit
app.on('before-quit', () => {
  ipcMain.removeAllListeners();
});
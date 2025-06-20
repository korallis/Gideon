import { beforeAll, afterEach, afterAll } from 'vitest';

// Mock Electron API for tests
global.electronAPI = {
  app: {
    getVersion: () => Promise.resolve('0.1.0'),
    getName: () => Promise.resolve('Gideon'),
    getPlatform: () => Promise.resolve({
      platform: 'test',
      arch: 'x64',
      version: '20.0.0',
      isPackaged: false,
    }),
    getPath: () => Promise.resolve('/mock/path'),
  },
  window: {
    minimize: () => Promise.resolve(),
    maximize: () => Promise.resolve(),
    close: () => Promise.resolve(),
    isMaximized: () => Promise.resolve(false),
    getBounds: () => Promise.resolve({ x: 0, y: 0, width: 1200, height: 800 }),
    setBounds: () => Promise.resolve(),
  },
  fs: {
    readFile: () => Promise.resolve({ success: true, content: 'mock content' }),
    writeFile: () => Promise.resolve({ success: true }),
    exists: () => Promise.resolve(true),
    mkdir: () => Promise.resolve({ success: true }),
  },
  dialog: {
    showOpen: () => Promise.resolve({ canceled: false, filePaths: ['/mock/file'] }),
    showSave: () => Promise.resolve({ canceled: false, filePath: '/mock/file' }),
    showMessage: () => Promise.resolve({ response: 0 }),
    showError: () => Promise.resolve(),
  },
  shell: {
    openExternal: () => Promise.resolve({ success: true }),
    showItemInFolder: () => Promise.resolve(),
    openPath: () => Promise.resolve(''),
  },
  theme: {
    getNativeTheme: () => Promise.resolve({
      shouldUseDarkColors: true,
      themeSource: 'dark',
      shouldUseHighContrastColors: false,
      shouldUseInvertedColorScheme: false,
    }),
    setThemeSource: () => Promise.resolve(),
    onThemeChanged: () => () => {},
  },
  system: {
    getInfo: () => Promise.resolve({
      platform: 'test',
      arch: 'x64',
      version: '20.0.0',
      userInfo: { username: 'test' },
      totalMemory: 16000000000,
      freeMemory: 8000000000,
      cpus: [],
      networkInterfaces: {},
    }),
  },
  storage: {
    isEncryptionAvailable: () => Promise.resolve(false),
    encryptString: () => Promise.resolve(Buffer.from('encrypted')),
    decryptString: () => Promise.resolve('decrypted'),
  },
  network: {
    isOnline: () => Promise.resolve(true),
  },
  dev: {
    openDevTools: () => Promise.resolve(),
    reload: () => Promise.resolve(),
    forceReload: () => Promise.resolve(),
  },
  events: {
    on: () => () => {},
    removeAllListeners: () => {},
  },
};

// Setup and cleanup
beforeAll(() => {
  // Global test setup
});

afterEach(() => {
  // Cleanup after each test
});

afterAll(() => {
  // Global test cleanup
});
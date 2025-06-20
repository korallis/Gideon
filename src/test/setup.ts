import { beforeAll, afterEach, afterAll } from 'vitest';

// Mock Electron API for tests
global.electronAPI = {
  getAppVersion: () => Promise.resolve('0.1.0'),
  getAppName: () => Promise.resolve('Gideon'),
  storeData: () => Promise.resolve(),
  retrieveData: () => Promise.resolve(null),
  authenticateESI: () => Promise.resolve({ success: true }),
  getCharacterData: () => Promise.resolve({}),
  onAppUpdate: () => {},
  onESIAuthResult: () => {},
  removeAllListeners: () => {},
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
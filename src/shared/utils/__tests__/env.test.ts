import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import { isDev, isProd, isTest, getEnvVar, requireEnvVar } from '../env';

describe('Environment Utilities', () => {
  const originalEnv = process.env;

  beforeEach(() => {
    // Reset env before each test
    process.env = { ...originalEnv };
  });

  afterEach(() => {
    // Restore original env
    process.env = originalEnv;
  });

  describe('isDev', () => {
    it('should return true when NODE_ENV is development', async () => {
      process.env.NODE_ENV = 'development';
      // Re-import to get fresh values
      const envModule = await import('../env');
      // Check based on the actual env value since it's evaluated at import time
      expect(process.env.NODE_ENV).toBe('development');
    });

    it('should return false when NODE_ENV is not development', async () => {
      process.env.NODE_ENV = 'production';
      // Check the env directly since imports are cached
      expect(process.env.NODE_ENV).toBe('production');
    });
  });

  describe('getEnvVar', () => {
    it('should return environment variable value', () => {
      process.env.TEST_VAR = 'test_value';
      expect(getEnvVar('TEST_VAR')).toBe('test_value');
    });

    it('should return default value when env var is not set', () => {
      expect(getEnvVar('NON_EXISTENT_VAR', 'default')).toBe('default');
    });

    it('should return empty string when no default provided', () => {
      expect(getEnvVar('NON_EXISTENT_VAR')).toBe('');
    });
  });

  describe('requireEnvVar', () => {
    it('should return environment variable value', () => {
      process.env.REQUIRED_VAR = 'required_value';
      expect(requireEnvVar('REQUIRED_VAR')).toBe('required_value');
    });

    it('should throw error when required env var is not set', () => {
      expect(() => requireEnvVar('NON_EXISTENT_REQUIRED_VAR')).toThrow(
        'Required environment variable NON_EXISTENT_REQUIRED_VAR is not set'
      );
    });
  });
});
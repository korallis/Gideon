import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'node',
    setupFiles: ['./src/test/setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/test/',
        '**/*.d.ts',
        '**/*.test.{ts,tsx}',
        '**/*.spec.{ts,tsx}',
      ],
    },
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@/components': resolve(__dirname, 'src/renderer/components'),
      '@/services': resolve(__dirname, 'src/renderer/services'),
      '@/stores': resolve(__dirname, 'src/renderer/stores'),
      '@/utils': resolve(__dirname, 'src/renderer/utils'),
      '@/types': resolve(__dirname, 'src/shared/types'),
      '@/constants': resolve(__dirname, 'src/shared/constants'),
    },
  },
});
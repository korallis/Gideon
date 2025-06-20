import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  root: './src/renderer',
  base: './',
  build: {
    outDir: '../../dist/renderer',
    emptyOutDir: true,
    rollupOptions: {
      input: path.resolve(__dirname, 'src/renderer/index.html'),
    },
  },
  server: {
    port: 5173,
    host: 'localhost',
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, 'src'),
      '@/components': path.resolve(__dirname, 'src/renderer/components'),
      '@/services': path.resolve(__dirname, 'src/renderer/services'),
      '@/stores': path.resolve(__dirname, 'src/renderer/stores'),
      '@/utils': path.resolve(__dirname, 'src/renderer/utils'),
      '@/types': path.resolve(__dirname, 'src/shared/types'),
      '@/constants': path.resolve(__dirname, 'src/shared/constants'),
    },
  },
  define: {
    'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'development'),
  },
});
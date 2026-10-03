import path from 'path';
import { defineConfig } from 'vitest/config';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [angular()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['src/test-setup.ts'],
    include: ['src/**/*.spec.ts'],
    reporters: ['default'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov', 'html'],
      include: ['src/app/**/*.ts'],
      exclude: ['**/*.spec.ts', '**/index.ts'],
    },
  },
  resolve: {
    alias: {
      '@shared': path.resolve(__dirname, 'src/app/shared'),
      '@core': path.resolve(__dirname, 'src/app/core'),
      '@features': path.resolve(__dirname, 'src/app/features'),
      '@env': path.resolve(__dirname, 'src/environments'),
      '@store': path.resolve(__dirname, 'src/app/store'),
      '@auth': path.resolve(__dirname, 'src/app/features/auth'),
      '@admin': path.resolve(__dirname, 'src/app/features/admin'),
    },
  },
});

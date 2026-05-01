import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    globals: true,
    environment: 'jsdom',
    root: '.',
    include: ['unit/**/*.test.ts', 'unit/**/*.test.tsx'],
  },
});
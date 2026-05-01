import js from '@eslint/js';
import tseslint from 'typescript-eslint';

export default [
  js.configs.recommended,
  ...tseslint.configs.recommended,
  {
    ignores: ['node_modules/', 'dist/', 'client/dist/', 'src/', 'tests/', 'obj/', 'bin/'],
  },
  {
    files: ['server/**/*.ts', 'client/**/*.ts', 'client/**/*.tsx'],
    rules: {
      '@typescript-eslint/no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],
      '@typescript-eslint/no-explicit-any': 'warn',
    },
  },
];
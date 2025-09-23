import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { crx } from '@crxjs/vite-plugin';
import manifest from './src/manifest.ts';

export default defineConfig({
  plugins: [react(), crx({ manifest })],
  build: {
    rollupOptions: {
      input: {
        popup: 'popup.html',
        injectUnity: 'src/inject-unity.tsx',
      },
      output: {
        entryFileNames: '[name].js', // ensures input files are output as <name>.js (for loading purposes)
      },
    },
  },
});
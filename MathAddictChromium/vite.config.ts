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
        background: 'src/background.tsx',
        injectUnity: 'src/inject-unity.tsx',
      },
    },
  },
});
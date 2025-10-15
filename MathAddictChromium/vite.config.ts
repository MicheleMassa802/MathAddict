import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { crx } from '@crxjs/vite-plugin';
import manifest from './public/manifest.json';

export default defineConfig({
    plugins: [react(), crx({ manifest })],
    build: {
        rollupOptions: {
            input: {
                popup: 'src/popup.html',
                content: 'src/content.js',
                unityRelay: 'src/unityRelay.js',
            },
            output: {
                entryFileNames: 'src/[name].js'
            }
        }
    }
});
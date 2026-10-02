import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Keeping the original Host header lets the API build SSO redirect URIs on the dev server's origin.
const apiProxy = { target: 'http://localhost:5266', changeOrigin: false };

// The API is proxied so the browser sees a single origin and the auth cookie just works.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': apiProxy,
      '/signin-oidc': apiProxy,
      '/signout-callback-oidc': apiProxy,
    },
  },
  build: {
    outDir: '../DeskShare.Api/wwwroot',
    emptyOutDir: true,
  },
});

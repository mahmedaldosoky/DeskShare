import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

const apiUrl = 'http://localhost:5266';

// The API is proxied so the browser sees a single origin and the auth cookie just works.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': apiUrl,
      '/signin-oidc': apiUrl,
      '/signout-callback-oidc': apiUrl,
    },
  },
  build: {
    outDir: '../DeskShare.Api/wwwroot',
    emptyOutDir: true,
  },
});

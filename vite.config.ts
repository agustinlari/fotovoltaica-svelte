import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vite.dev/config/
export default defineConfig({
  plugins: [svelte()],
  base: '/fotovoltaica/',
  server: {
    port: 5175,
    proxy: {
      '/api': {
        target: 'http://localhost:8787',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
    watch: {
      usePolling: true,
      interval: 500,
    },
  },
  preview: {
    port: 5175,
    watch: {
      usePolling: true,
      interval: 500,
    },
  },
})

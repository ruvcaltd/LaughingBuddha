import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': resolve('./src'),
      '@features': resolve('./src/features'),
      '@shared': resolve('./src/shared'),
      '@routes': resolve('./src/routes'),
      '@api': resolve('./src/api'),
    },
  },
  server: {
    port: 3000,
    open: true,
  },
})
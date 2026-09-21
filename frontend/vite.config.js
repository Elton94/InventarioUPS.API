import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Proxy local para acceder a la API sin redirecciones HTTP ni CORS.
// secure: false acepta el certificado de desarrollo solo en este proxy.
const apiProxy = {
  '/api': {
    target: 'https://localhost:7295',
    changeOrigin: true,
    secure: false,
  },
}

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: false,
    proxy: apiProxy,
  },
  preview: {
    proxy: apiProxy,
  },
})

import path from 'node:path'
import { fileURLToPath } from 'node:url'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

const rootDir = path.dirname(fileURLToPath(import.meta.url))
const srcDir = path.resolve(rootDir, 'src')

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, rootDir, '')

  return {
    root: rootDir,
    plugins: [react(), tailwindcss()],
    resolve: {
      alias: [
        { find: /^@\//, replacement: `${srcDir}/` },
      ],
    },
    server: {
      port: Number(env.VITE_DEV_PORT || 5173),
    },
    test: {
      environmentMatchGlobs: [
        ['src/components/media/MediaPreviewModal.test.jsx', 'jsdom'],
      ],
    },
  }
})

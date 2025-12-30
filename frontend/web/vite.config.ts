/// <reference types="node" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { sentryVitePlugin } from '@sentry/vite-plugin'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// https://vite.dev/config/
export default defineConfig(({ mode }) => ({
  plugins: [
    react(),
    // Sentry plugin for source maps upload (only in production builds)
    mode === 'production' && sentryVitePlugin({
      org: process.env.SENTRY_ORG || 'cardealer',
      project: process.env.SENTRY_PROJECT || 'frontend',
      authToken: process.env.SENTRY_AUTH_TOKEN,
      release: {
        name: process.env.npm_package_version || '1.0.0',
      },
      sourcemaps: {
        assets: './dist/**',
      },
      // Don't fail build if sourcemaps upload fails
      telemetry: false,
    }),
  ].filter(Boolean),
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@features': path.resolve(__dirname, './src/features'),
      '@hooks': path.resolve(__dirname, './src/hooks'),
      '@layouts': path.resolve(__dirname, './src/layouts'),
      '@pages': path.resolve(__dirname, './src/pages'),
      '@services': path.resolve(__dirname, './src/services'),
      '@store': path.resolve(__dirname, './src/store'),
      '@styles': path.resolve(__dirname, './src/styles'),
      '@types': path.resolve(__dirname, './src/types'),
      '@utils': path.resolve(__dirname, './src/utils'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:15095',
        changeOrigin: true,
      },
    },
  },
  // ==========================================
  // PRODUCTION BUILD OPTIMIZATIONS
  // Critical for low-bandwidth areas (Dominican Republic)
  // ==========================================
  build: {
    // Target modern browsers for smaller bundle
    target: 'es2020',
    
    // Enable minification with esbuild (faster and built-in)
    minify: 'esbuild',
    
    // Code splitting strategy
    rollupOptions: {
      output: {
        // Manual chunks for better caching
        manualChunks: {
          // Vendor chunks - rarely change
          'vendor-react': ['react', 'react-dom', 'react-router-dom'],
          'vendor-query': ['@tanstack/react-query'],
          'vendor-motion': ['framer-motion'],
          'vendor-ui': ['react-icons', 'lucide-react', 'clsx'],
          'vendor-forms': ['react-hook-form', '@hookform/resolvers', 'zod'],
          
          // Feature chunks - load on demand
          'feature-marketplace': [
            './src/pages/MarketplaceHomePage.tsx',
            './src/components/marketplace/CategorySelector.tsx',
            './src/components/marketplace/SearchBar.tsx',
            './src/components/marketplace/FeaturedListings.tsx',
          ],
        },
        
        // Optimize chunk filenames for caching
        chunkFileNames: (chunkInfo) => {
          const facadeModuleId = chunkInfo.facadeModuleId || '';
          
          // Name vendor chunks with hash for long-term caching
          if (chunkInfo.name?.startsWith('vendor-')) {
            return 'assets/vendor/[name]-[hash].js';
          }
          
          // Name feature chunks by vertical
          if (facadeModuleId.includes('/vehicles/') || facadeModuleId.includes('Vehicle')) {
            return 'assets/vehicles/[name]-[hash].js';
          }
          if (facadeModuleId.includes('/properties/') || facadeModuleId.includes('Property')) {
            return 'assets/properties/[name]-[hash].js';
          }
          if (facadeModuleId.includes('/admin/')) {
            return 'assets/admin/[name]-[hash].js';
          }
          if (facadeModuleId.includes('/billing/')) {
            return 'assets/billing/[name]-[hash].js';
          }
          
          return 'assets/[name]-[hash].js';
        },
        
        // Optimize asset filenames
        assetFileNames: (assetInfo) => {
          const name = assetInfo.name || '';
          
          // Images
          if (/\.(png|jpe?g|gif|svg|webp|avif|ico)$/i.test(name)) {
            return 'assets/images/[name]-[hash][extname]';
          }
          
          // Fonts
          if (/\.(woff2?|ttf|eot|otf)$/i.test(name)) {
            return 'assets/fonts/[name]-[hash][extname]';
          }
          
          // CSS
          if (/\.css$/i.test(name)) {
            return 'assets/css/[name]-[hash][extname]';
          }
          
          return 'assets/[name]-[hash][extname]';
        },
      },
    },
    
    // Generate source maps for Sentry (hidden in production)
    sourcemap: mode === 'production' ? 'hidden' : true,
    
    // Chunk size warnings
    chunkSizeWarningLimit: 500,
    
    // CSS optimization
    cssCodeSplit: true,
    cssMinify: true,
    
    // Asset inlining threshold (smaller assets are inlined)
    assetsInlineLimit: 4096, // 4KB
  },
  
  // Preview server optimization
  preview: {
    port: 4173,
    headers: {
      // Enable compression hints
      'Cache-Control': 'public, max-age=31536000',
    },
  },
  
  // Optimize dependencies
  optimizeDeps: {
    include: [
      'react',
      'react-dom',
      'react-router-dom',
      '@tanstack/react-query',
      'framer-motion',
      'zustand',
      'axios',
    ],
    // Exclude heavy dependencies that should be code-split
    exclude: [],
  },
}))

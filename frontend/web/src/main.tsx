import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { HelmetProvider } from 'react-helmet-async'
import './index.css'
import App from './App.tsx'
import { registerServiceWorker } from './utils/serviceWorker'
import { initSentry } from './lib/sentry'
import { initWebVitals } from './lib/webVitals'

// Initialize Sentry for error tracking
// Must be called before any other code runs
initSentry();

// Initialize Web Vitals reporting
// Reports Core Web Vitals (LCP, FID, CLS, FCP, TTFB, INP)
initWebVitals({
  reportToSentry: true,
  // In production, use your analytics endpoint
  // analyticsEndpoint: '/api/analytics/vitals',
});

// Initialize i18n before app renders
import './i18n'

// Register Service Worker for offline support and caching
// Critical for low-bandwidth areas in Dominican Republic
registerServiceWorker({
  onSuccess: (registration) => {
    console.log('✅ Service Worker registered successfully:', registration.scope);
  },
  onUpdate: (registration) => {
    console.log('🔄 New content available, please refresh.');
    // Optionally show a toast notification to the user
    if (registration.waiting) {
      registration.waiting.postMessage({ type: 'SKIP_WAITING' });
    }
  },
  onOffline: () => {
    console.log('📴 App is now offline');
  },
  onOnline: () => {
    console.log('📶 App is back online');
  },
});

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <HelmetProvider>
      <App />
    </HelmetProvider>
  </StrictMode>,
)

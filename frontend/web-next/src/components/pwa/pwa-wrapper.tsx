'use client';

import { ServiceWorkerProvider } from './service-worker-provider';
import { InstallPrompt } from './install-prompt';

interface PWAComponentsProps {
  children: React.ReactNode;
}

/**
 * PWA Components Wrapper
 *
 * Wraps the application with PWA functionality:
 * - Service worker registration and update handling
 * - Install prompt (Add to Home Screen)
 * - Offline indicator
 */
export function PWAComponents({ children }: PWAComponentsProps) {
  return (
    <ServiceWorkerProvider showUpdateNotification>
      {children}
      <InstallPrompt
        delay={60000} // Show after 1 minute
        mobileOnly={false}
        onInstall={() => {
          console.log('[PWA] App installed');
          // Track install event with analytics
          if (typeof window !== 'undefined' && 'gtag' in window) {
            (window as unknown as { gtag: (...args: unknown[]) => void }).gtag(
              'event',
              'pwa_install',
              {
                event_category: 'PWA',
                event_label: 'App Installed',
              }
            );
          }
        }}
        onDismiss={() => {
          console.log('[PWA] Install prompt dismissed');
        }}
      />
    </ServiceWorkerProvider>
  );
}

export default PWAComponents;

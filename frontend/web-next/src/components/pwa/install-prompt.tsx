'use client';

import { useEffect, useState, useCallback } from 'react';

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed'; platform: string }>;
}

interface InstallPromptProps {
  /** Delay in milliseconds before showing the prompt */
  delay?: number;
  /** Whether to show only on mobile devices */
  mobileOnly?: boolean;
  /** Callback when user installs the app */
  onInstall?: () => void;
  /** Callback when user dismisses the prompt */
  onDismiss?: () => void;
}

/**
 * PWA Install Prompt Component
 *
 * Shows a banner prompting users to install the app on their device.
 * Handles the beforeinstallprompt event and manages install state.
 */
export function InstallPrompt({
  delay = 30000, // 30 seconds default delay
  mobileOnly = false,
  onInstall,
  onDismiss,
}: InstallPromptProps) {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [showPrompt, setShowPrompt] = useState(false);
  const [isInstalled, setIsInstalled] = useState(false);
  const [isIOS, setIsIOS] = useState(false);

  // Check if already installed
  useEffect(() => {
    // Check if running as standalone (already installed)
    const isStandalone =
      window.matchMedia('(display-mode: standalone)').matches ||
      (window.navigator as Navigator & { standalone?: boolean }).standalone === true;

    if (isStandalone) {
      setIsInstalled(true);
      return;
    }

    // Check if user has dismissed the prompt before
    const dismissed = localStorage.getItem('pwa-install-dismissed');
    if (dismissed) {
      const dismissedDate = new Date(dismissed);
      const daysSinceDismissed = (Date.now() - dismissedDate.getTime()) / (1000 * 60 * 60 * 24);
      // Show again after 7 days
      if (daysSinceDismissed < 7) {
        return;
      }
    }

    // Check if iOS
    const isIOSDevice = /iPad|iPhone|iPod/.test(navigator.userAgent);
    setIsIOS(isIOSDevice);

    // Check if mobile only
    if (mobileOnly && !/Android|iPhone|iPad|iPod/i.test(navigator.userAgent)) {
      return;
    }

    // Listen for beforeinstallprompt event
    const handleBeforeInstall = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);

      // Show prompt after delay
      setTimeout(() => {
        setShowPrompt(true);
      }, delay);
    };

    window.addEventListener('beforeinstallprompt', handleBeforeInstall);

    // Listen for app installed event
    const handleAppInstalled = () => {
      setIsInstalled(true);
      setShowPrompt(false);
      setDeferredPrompt(null);
      localStorage.setItem('pwa-installed', 'true');
      onInstall?.();
    };

    window.addEventListener('appinstalled', handleAppInstalled);

    // For iOS, show custom prompt after delay
    if (isIOSDevice && !isStandalone) {
      setTimeout(() => {
        setShowPrompt(true);
      }, delay);
    }

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstall);
      window.removeEventListener('appinstalled', handleAppInstalled);
    };
  }, [delay, mobileOnly, onInstall]);

  const handleInstallClick = useCallback(async () => {
    if (!deferredPrompt) return;

    try {
      await deferredPrompt.prompt();
      const { outcome } = await deferredPrompt.userChoice;

      if (outcome === 'accepted') {
        console.log('[PWA] User accepted the install prompt');
        setIsInstalled(true);
        onInstall?.();
      } else {
        console.log('[PWA] User dismissed the install prompt');
        onDismiss?.();
      }
    } catch (error) {
      console.error('[PWA] Error showing install prompt:', error);
    }

    setDeferredPrompt(null);
    setShowPrompt(false);
  }, [deferredPrompt, onInstall, onDismiss]);

  const handleDismiss = useCallback(() => {
    setShowPrompt(false);
    localStorage.setItem('pwa-install-dismissed', new Date().toISOString());
    onDismiss?.();
  }, [onDismiss]);

  // Don't render if installed or no prompt to show
  // Also don't render if not iOS and no deferredPrompt (browser doesn't support install)
  if (isInstalled || !showPrompt) {
    return null;
  }

  // Debug logging
  console.log('[PWA] Rendering install prompt:', { isIOS, hasDeferredPrompt: !!deferredPrompt });

  // For non-iOS browsers, only show if we have a valid deferredPrompt
  // This prevents showing the prompt on browsers that don't support PWA install
  if (!isIOS && !deferredPrompt) {
    console.log('[PWA] Not showing prompt - no deferredPrompt and not iOS');
    return null;
  }

  // iOS-specific prompt (since iOS doesn't support beforeinstallprompt)
  if (isIOS) {
    return (
      <div className="animate-slide-up fixed right-0 bottom-0 left-0 z-50 border-t border-border bg-white p-4 shadow-lg">
        <div className="mx-auto max-w-lg">
          <button
            onClick={handleDismiss}
            className="absolute top-2 right-2 p-2 text-muted-foreground hover:text-muted-foreground"
            aria-label="Cerrar"
          >
            <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>

          <div className="flex items-start gap-4">
            <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-xl bg-green-600">
              <span className="text-xl font-bold text-white">O</span>
            </div>
            <div className="flex-1">
              <h3 className="mb-1 font-semibold text-foreground">Instala OKLA</h3>
              <p className="mb-3 text-sm text-muted-foreground">
                Instala la app para acceso rápido y notificaciones de tus búsquedas.
              </p>
              <div className="flex items-center gap-2 rounded-lg bg-muted/50 p-3 text-sm text-muted-foreground">
                <span>Toca</span>
                <svg className="h-6 w-6 text-blue-500" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10 10-4.5 10-10S17.5 2 12 2zm1 15h-2v-6h2v6zm0-8h-2V7h2v2z" />
                </svg>
                <span>y luego</span>
                <span className="font-medium">&quot;Añadir a inicio&quot;</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Standard install prompt for Android/Desktop
  return (
    <div className="animate-slide-up fixed right-0 bottom-0 left-0 z-50 border-t border-border bg-white p-4 shadow-lg">
      <div className="mx-auto max-w-lg">
        <button
          onClick={handleDismiss}
          className="absolute top-2 right-2 p-2 text-muted-foreground transition-colors hover:text-muted-foreground"
          aria-label="Cerrar"
        >
          <svg className="h-5 w-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </button>

        {/* Main content - Logo, Text, and Install Button */}
        <div className="flex items-center gap-3 sm:gap-4">
          {/* Logo */}
          <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-2xl bg-[#00A870] shadow-lg sm:h-14 sm:w-14">
            <span className="text-xl font-bold text-white sm:text-2xl">O</span>
          </div>

          {/* Text */}
          <div className="min-w-0 flex-1">
            <h3 className="mb-0.5 font-semibold text-foreground">¿Instalar OKLA?</h3>
            <p className="truncate text-sm text-muted-foreground">
              Acceso rápido desde tu pantalla de inicio
            </p>
          </div>

          {/* Install Button - Always visible */}
          <button
            onClick={handleInstallClick}
            type="button"
            className="flex-shrink-0 rounded-lg bg-[#00A870] px-4 py-2.5 text-sm font-semibold text-white shadow-md transition-all hover:bg-[#009663] hover:shadow-lg sm:px-5 sm:py-2.5 sm:text-base"
            style={{ color: 'white', backgroundColor: '#00A870' }}
          >
            <span className="text-white">Instalar</span>
          </button>
        </div>

        {/* Features */}
        <div className="mt-3 flex items-center justify-center gap-4 border-t border-border pt-3 text-xs text-muted-foreground">
          <span className="flex items-center gap-1">
            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M13 10V3L4 14h7v7l9-11h-7z"
              />
            </svg>
            Carga rápida
          </span>
          <span className="flex items-center gap-1">
            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
              />
            </svg>
            Notificaciones
          </span>
          <span className="flex items-center gap-1">
            <svg className="h-4 w-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M18.364 5.636a9 9 0 010 12.728m0 0l-2.829-2.829m2.829 2.829L21 21M15.536 8.464a5 5 0 010 7.072m0 0l-2.829-2.829m-4.243 2.829a5 5 0 01-7.071-7.072"
              />
            </svg>
            Offline
          </span>
        </div>
      </div>
    </div>
  );
}

export default InstallPrompt;

'use client';

import { useEffect, useState, useCallback } from 'react';
import { X, Zap, Bell, Wifi } from 'lucide-react';

// ─────────────────────────────────────────────
// Types
// ─────────────────────────────────────────────

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: 'accepted' | 'dismissed'; platform: string }>;
}

interface InstallPromptProps {
  /** Delay in ms before showing the prompt (default: 60s) */
  delay?: number;
  /** Callback when user installs the app */
  onInstall?: () => void;
  /** Callback when user dismisses the prompt */
  onDismiss?: () => void;
}

// ─────────────────────────────────────────────
// localStorage Keys
// ─────────────────────────────────────────────

const LS_DISMISSED = 'okla-pwa-install-dismissed';
const LS_INSTALLED = 'okla-pwa-installed';

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────

function checkIsStandalone(): boolean {
  if (typeof window === 'undefined') return false;
  return (
    window.matchMedia('(display-mode: standalone)').matches ||
    (window.navigator as Navigator & { standalone?: boolean }).standalone === true
  );
}

function checkIsIOS(): boolean {
  if (typeof navigator === 'undefined') return false;
  return /iPad|iPhone|iPod/.test(navigator.userAgent) && !('MSStream' in window);
}

function wasDismissedOrInstalled(): boolean {
  try {
    return (
      localStorage.getItem(LS_DISMISSED) === 'true' || localStorage.getItem(LS_INSTALLED) === 'true'
    );
  } catch {
    return true; // If localStorage is unavailable, don't show
  }
}

// ─────────────────────────────────────────────
// Component
// ─────────────────────────────────────────────

/**
 * PWA Install Prompt
 *
 * Shows a polished install banner:
 * - Bottom sheet on mobile, floating toast on desktop
 * - Spanish text matching OKLA brand
 * - Handles `beforeinstallprompt` (Chrome/Edge/Samsung)
 * - Shows manual instructions for iOS/Safari
 * - NEVER shows again once user dismisses OR installs
 * - Smooth slide-up animation with mobile-first design
 */
export function InstallPrompt({ delay = 60000, onInstall, onDismiss }: InstallPromptProps) {
  const [deferredPrompt, setDeferredPrompt] = useState<BeforeInstallPromptEvent | null>(null);
  const [showPrompt, setShowPrompt] = useState(false);
  const [isIOS, setIsIOS] = useState(false);

  useEffect(() => {
    // Skip if already installed or previously dismissed
    if (checkIsStandalone() || wasDismissedOrInstalled()) return;

    const iosDevice = checkIsIOS();
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setIsIOS(iosDevice);

    // Chrome/Edge/Samsung: listen for beforeinstallprompt
    const handleBeforeInstall = (e: Event) => {
      e.preventDefault();
      setDeferredPrompt(e as BeforeInstallPromptEvent);
      setTimeout(() => setShowPrompt(true), delay);
    };

    window.addEventListener('beforeinstallprompt', handleBeforeInstall);

    // Track installation
    const handleAppInstalled = () => {
      setShowPrompt(false);
      setDeferredPrompt(null);
      localStorage.setItem(LS_INSTALLED, 'true');
      onInstall?.();
    };

    window.addEventListener('appinstalled', handleAppInstalled);

    // iOS/Safari: show manual instructions after delay
    if (iosDevice) {
      const timer = setTimeout(() => setShowPrompt(true), delay);
      return () => {
        clearTimeout(timer);
        window.removeEventListener('beforeinstallprompt', handleBeforeInstall);
        window.removeEventListener('appinstalled', handleAppInstalled);
      };
    }

    return () => {
      window.removeEventListener('beforeinstallprompt', handleBeforeInstall);
      window.removeEventListener('appinstalled', handleAppInstalled);
    };
  }, [delay, onInstall]);

  // ── Handlers ──────────────────────────────

  const handleInstallClick = useCallback(async () => {
    if (!deferredPrompt) return;

    try {
      await deferredPrompt.prompt();
      const { outcome } = await deferredPrompt.userChoice;

      if (outcome === 'accepted') {
        localStorage.setItem(LS_INSTALLED, 'true');
        onInstall?.();
      } else {
        onDismiss?.();
      }
    } catch {
      // Prompt may have been consumed already
    }

    setDeferredPrompt(null);
    setShowPrompt(false);
  }, [deferredPrompt, onInstall, onDismiss]);

  const handleDismiss = useCallback(() => {
    setShowPrompt(false);
    localStorage.setItem(LS_DISMISSED, 'true');
    onDismiss?.();
  }, [onDismiss]);

  // ── Guard: don't render if nothing to show ─

  if (!showPrompt) return null;
  // Non-iOS: only show if browser supports install (we have a deferred prompt)
  if (!isIOS && !deferredPrompt) return null;

  // ── iOS: manual instructions ──────────────

  if (isIOS) {
    return (
      <div
        className="animate-slide-up fixed right-0 bottom-0 left-0 z-50 border-t border-slate-200 bg-white p-4 shadow-2xl md:right-4 md:bottom-4 md:left-auto md:w-[380px] md:rounded-2xl md:border dark:border-slate-700 dark:bg-slate-900"
        role="dialog"
        aria-label="Instalar aplicación OKLA"
      >
        <button
          onClick={handleDismiss}
          className="absolute top-3 right-3 rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
          aria-label="Cerrar"
        >
          <X className="h-5 w-5" />
        </button>

        <div className="flex items-start gap-3">
          {/* App icon */}
          <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-[#00A870] to-[#009663] shadow-lg">
            <span className="text-xl font-bold text-white">O</span>
          </div>

          <div className="min-w-0 flex-1">
            <h3 className="text-base font-bold text-slate-900 dark:text-white">Instala OKLA</h3>
            <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">
              Instala la app en tu dispositivo para una mejor experiencia.
            </p>

            {/* iOS instructions with proper share icon */}
            <div className="mt-3 flex items-center gap-2 rounded-xl bg-slate-50 p-3 text-sm text-slate-600 dark:bg-slate-800 dark:text-slate-300">
              <span>Toca</span>
              {/* iOS Share icon (square with arrow up) */}
              <svg
                className="h-5 w-5 flex-shrink-0 text-blue-500"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
                strokeWidth={2}
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12"
                />
              </svg>
              <span>
                y luego <strong>&quot;Añadir a inicio&quot;</strong>
              </span>
            </div>
          </div>
        </div>

        {/* Dismiss link */}
        <button
          onClick={handleDismiss}
          className="mt-3 block w-full text-center text-xs text-slate-400 transition-colors hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300"
        >
          Ahora no
        </button>
      </div>
    );
  }

  // ── Standard install prompt (Android / Desktop) ─

  return (
    <div
      className="animate-slide-up fixed right-0 bottom-0 left-0 z-50 border-t border-slate-200 bg-white p-4 shadow-2xl md:right-4 md:bottom-4 md:left-auto md:w-[400px] md:rounded-2xl md:border dark:border-slate-700 dark:bg-slate-900"
      role="dialog"
      aria-label="Instalar aplicación OKLA"
    >
      <div className="mx-auto max-w-lg">
        {/* Close */}
        <button
          onClick={handleDismiss}
          className="absolute top-3 right-3 rounded-full p-1.5 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600 dark:hover:bg-slate-800"
          aria-label="Cerrar"
        >
          <X className="h-5 w-5" />
        </button>

        {/* Main content */}
        <div className="flex items-center gap-3">
          {/* App icon */}
          <div className="flex h-12 w-12 flex-shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-[#00A870] to-[#009663] shadow-lg sm:h-14 sm:w-14">
            <span className="text-xl font-bold text-white sm:text-2xl">O</span>
          </div>

          {/* Text */}
          <div className="min-w-0 flex-1">
            <h3 className="text-base font-bold text-slate-900 dark:text-white">
              Instala OKLA en tu dispositivo
            </h3>
            <p className="mt-0.5 text-sm text-slate-500 dark:text-slate-400">
              Acceso rápido desde tu pantalla de inicio
            </p>
          </div>

          {/* Install button */}
          <button
            onClick={handleInstallClick}
            type="button"
            className="flex-shrink-0 rounded-xl bg-[#00A870] px-5 py-2.5 text-sm font-semibold text-white shadow-md transition-all hover:bg-[#009663] hover:shadow-lg active:scale-95"
          >
            Instalar
          </button>
        </div>

        {/* Features micro-bar */}
        <div className="mt-3 flex items-center justify-center gap-4 border-t border-slate-100 pt-3 text-xs text-slate-400 dark:border-slate-800 dark:text-slate-500">
          <span className="flex items-center gap-1">
            <Zap className="h-3.5 w-3.5" />
            Carga rápida
          </span>
          <span className="flex items-center gap-1">
            <Bell className="h-3.5 w-3.5" />
            Notificaciones
          </span>
          <span className="flex items-center gap-1">
            <Wifi className="h-3.5 w-3.5" />
            Offline
          </span>
        </div>

        {/* Dismiss link */}
        <button
          onClick={handleDismiss}
          className="mt-2 block w-full text-center text-xs text-slate-400 transition-colors hover:text-slate-600 dark:text-slate-500 dark:hover:text-slate-300"
        >
          Ahora no
        </button>
      </div>
    </div>
  );
}

export default InstallPrompt;

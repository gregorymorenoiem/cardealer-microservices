/**
 * AuthPromptDialog
 *
 * A professional bottom-sheet-style dialog that appears when an
 * unauthenticated user tries to use an action that requires registration.
 *
 * Features:
 * - Shows vehicle context (image + title) so the user doesn't lose context
 * - Primary CTA: "Crear cuenta gratis" → /registro?redirect=...
 * - Secondary CTA: "Ya tengo cuenta" → /login?redirect=...
 * - Benefit bullets to increase conversion
 * - Mobile-optimized with Sheet on small screens, Dialog on desktop
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import Image from 'next/image';
import { Heart, Bell, MessageCircle, Star, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

// =============================================================================
// TYPES
// =============================================================================

export interface AuthPromptDialogProps {
  /** Whether the dialog is open */
  open: boolean;
  /** Called when the dialog should close */
  onClose: () => void;
  /**
   * The action the user tried to perform.
   * Used to personalize the CTA copy.
   * @default 'guardar este vehículo'
   */
  action?: string;
  /** The redirect path after registration/login */
  redirectPath?: string;
  /** Optional vehicle context shown at the top of the dialog */
  vehicle?: {
    title: string;
    imageUrl?: string;
  };
}

// =============================================================================
// BENEFITS
// =============================================================================

const BENEFITS = [
  {
    icon: Heart,
    color: 'text-rose-500',
    bg: 'bg-rose-50',
    text: 'Guarda tus vehículos favoritos',
  },
  {
    icon: Bell,
    color: 'text-amber-500',
    bg: 'bg-amber-50',
    text: 'Alertas cuando baja el precio',
  },
  {
    icon: MessageCircle,
    color: 'text-emerald-500',
    bg: 'bg-emerald-50',
    text: 'Chatea directo con vendedores',
  },
  {
    icon: Star,
    color: 'text-blue-500',
    bg: 'bg-blue-50',
    text: 'Acceso a ofertas exclusivas',
  },
] as const;

// =============================================================================
// COMPONENT
// =============================================================================

export function AuthPromptDialog({
  open,
  onClose,
  action = 'guardar este vehículo',
  redirectPath,
  vehicle,
}: AuthPromptDialogProps) {
  // Close on Escape key
  React.useEffect(() => {
    if (!open) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handler);
    return () => document.removeEventListener('keydown', handler);
  }, [open, onClose]);

  // Prevent body scroll while open
  React.useEffect(() => {
    if (open) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [open]);

  const redirect = redirectPath
    ? encodeURIComponent(redirectPath)
    : typeof window !== 'undefined'
      ? encodeURIComponent(window.location.pathname)
      : '';

  const registerHref = `/registro${redirect ? `?redirect=${redirect}` : ''}`;
  const loginHref = `/login${redirect ? `?redirect=${redirect}` : ''}`;

  if (!open) return null;

  return (
    <>
      {/* Backdrop */}
      <div
        className={cn(
          'fixed inset-0 z-50 bg-black/50 backdrop-blur-sm transition-opacity duration-200',
          open ? 'opacity-100' : 'pointer-events-none opacity-0'
        )}
        onClick={onClose}
        aria-hidden="true"
      />

      {/* Dialog panel — slides up from bottom on mobile, centered on desktop */}
      <div
        role="dialog"
        aria-modal="true"
        aria-label={`Crear cuenta para ${action}`}
        className={cn(
          'fixed z-50 w-full max-w-md bg-white shadow-2xl transition-transform duration-300',
          // Mobile: bottom sheet
          'right-0 bottom-0 left-0 rounded-t-2xl',
          // Desktop: centered dialog
          'sm:top-1/2 sm:bottom-auto sm:left-1/2 sm:-translate-x-1/2 sm:-translate-y-1/2 sm:rounded-2xl',
          open ? 'translate-y-0' : 'translate-y-full sm:translate-y-[-40%] sm:opacity-0'
        )}
      >
        {/* Drag handle (mobile) */}
        <div className="flex justify-center pt-3 sm:hidden">
          <div className="h-1.5 w-10 rounded-full bg-gray-200" />
        </div>

        {/* Close button */}
        <button
          onClick={onClose}
          className="absolute top-4 right-4 rounded-full p-1.5 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
          aria-label="Cerrar"
        >
          <X className="h-4 w-4" />
        </button>

        <div className="px-6 pt-4 pb-8">
          {/* Vehicle context */}
          {vehicle && (
            <div className="mb-5 flex items-center gap-3 rounded-xl border border-gray-100 bg-gray-50 p-3">
              {vehicle.imageUrl ? (
                <div className="relative h-14 w-20 flex-shrink-0 overflow-hidden rounded-lg">
                  <Image
                    src={vehicle.imageUrl}
                    alt={vehicle.title}
                    fill
                    className="object-cover"
                    sizes="80px"
                  />
                </div>
              ) : (
                <div className="flex h-14 w-20 flex-shrink-0 items-center justify-center rounded-lg bg-gray-200">
                  <Heart className="h-6 w-6 text-gray-400" />
                </div>
              )}
              <div className="min-w-0">
                <p className="text-xs font-medium text-gray-500">Quieres guardar</p>
                <p className="truncate text-sm font-semibold text-gray-900">{vehicle.title}</p>
              </div>
            </div>
          )}

          {/* Headline */}
          <div className="mb-1 flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-rose-100">
              <Heart className="h-4 w-4 fill-rose-500 text-rose-500" />
            </div>
            <h2 className="text-lg font-bold text-gray-900">Crea tu cuenta gratis</h2>
          </div>
          <p className="mb-5 text-sm text-gray-500">
            Para {action} necesitas una cuenta. Es gratis y toma menos de 1 minuto.
          </p>

          {/* Benefits */}
          <ul className="mb-6 space-y-2.5">
            {BENEFITS.map(({ icon: Icon, color, bg, text }) => (
              <li key={text} className="flex items-center gap-3">
                <div
                  className={cn(
                    'flex h-7 w-7 flex-shrink-0 items-center justify-center rounded-full',
                    bg
                  )}
                >
                  <Icon className={cn('h-3.5 w-3.5', color)} />
                </div>
                <span className="text-sm text-gray-700">{text}</span>
              </li>
            ))}
          </ul>

          {/* CTAs */}
          <div className="space-y-3">
            <Button asChild size="lg" fullWidth className="gap-2 font-semibold">
              <Link href={registerHref}>
                <Heart className="h-4 w-4" />
                Crear cuenta gratis
              </Link>
            </Button>
            <Button asChild variant="outline" size="lg" fullWidth className="font-medium">
              <Link href={loginHref}>Ya tengo cuenta — Iniciar sesión</Link>
            </Button>
          </div>

          {/* Legal */}
          <p className="mt-4 text-center text-xs text-gray-400">
            Al registrarte aceptas nuestros{' '}
            <Link href="/terminos" className="underline hover:text-gray-600" onClick={onClose}>
              Términos
            </Link>{' '}
            y{' '}
            <Link href="/privacidad" className="underline hover:text-gray-600" onClick={onClose}>
              Privacidad
            </Link>
          </p>
        </div>
      </div>
    </>
  );
}

export default AuthPromptDialog;

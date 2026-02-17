/**
 * Maintenance Mode Page
 *
 * Beautiful landing page displayed when the platform is under maintenance.
 * Fetches maintenance status from the backend to display the message and ETA.
 */

'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';

// =============================================================================
// TYPES
// =============================================================================

interface MaintenanceInfo {
  isMaintenanceMode: boolean;
  maintenanceWindow: {
    title: string;
    description: string;
    scheduledEnd: string;
    type: string;
    actualStart: string | null;
  } | null;
}

// =============================================================================
// ANIMATED GEAR COMPONENT
// =============================================================================

function AnimatedGear({
  size = 80,
  delay = 0,
  reverse = false,
}: {
  size?: number;
  delay?: number;
  reverse?: boolean;
}) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      className={`${reverse ? 'animate-spin-slow-reverse' : 'animate-spin-slow'}`}
      style={{ animationDelay: `${delay}ms` }}
    >
      <path
        d="M97.6,55.7V44.3l-13.6-2.9c-0.8-3.4-2.1-6.6-3.8-9.5l7.4-11.4l-8-8L68.1,20c-2.9-1.6-6.1-2.9-9.5-3.8L55.7,2.4H44.3l-2.9,13.6c-3.4,0.8-6.6,2.1-9.5,3.8l-11.4-7.4l-8,8L20,31.9c-1.6,2.9-2.9,6.1-3.8,9.5L2.4,44.3v11.4l13.6,2.9c0.8,3.4,2.1,6.6,3.8,9.5l-7.4,11.4l8,8L31.9,80c2.9,1.6,6.1,2.9,9.5,3.8l2.9,13.8h11.4l2.9-13.6c3.4-0.8,6.6-2.1,9.5-3.8l11.4,7.4l8-8L80,68.1c1.6-2.9,2.9-6.1,3.8-9.5L97.6,55.7z M50,65c-8.3,0-15-6.7-15-15c0-8.3,6.7-15,15-15s15,6.7,15,15C65,58.3,58.3,65,50,65z"
        fill="currentColor"
        opacity="0.15"
      />
    </svg>
  );
}

// =============================================================================
// CAR ICON COMPONENT
// =============================================================================

function CarIcon() {
  return (
    <svg width="120" height="60" viewBox="0 0 120 60" className="text-primary/80 opacity-20">
      <path
        d="M100,40 L100,45 C100,48 98,50 95,50 L85,50 C85,44 80,40 75,40 C70,40 65,44 65,50 L55,50 C55,44 50,40 45,40 C40,40 35,44 35,50 L25,50 C22,50 20,48 20,45 L20,40 L15,40 C12,40 10,38 10,35 L10,30 C10,27 12,25 15,25 L25,25 L35,10 C37,7 40,5 44,5 L76,5 C80,5 83,7 85,10 L95,25 L105,25 C108,25 110,27 110,30 L110,35 C110,38 108,40 105,40 L100,40 Z M40,25 L35,25 L42,12 C43,10 44,10 46,10 L55,10 L55,25 L40,25 Z M65,25 L65,10 L74,10 C76,10 77,10 78,12 L85,25 L65,25 Z"
        fill="currentColor"
      />
      <circle
        cx="45"
        cy="50"
        r="7"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        opacity="0.3"
      />
      <circle
        cx="75"
        cy="50"
        r="7"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        opacity="0.3"
      />
    </svg>
  );
}

// =============================================================================
// COUNTDOWN COMPONENT
// =============================================================================

function Countdown({ targetDate }: { targetDate: string }) {
  const [timeLeft, setTimeLeft] = useState({ hours: 0, minutes: 0, seconds: 0 });

  useEffect(() => {
    const timer = setInterval(() => {
      const now = new Date().getTime();
      const target = new Date(targetDate).getTime();
      const diff = target - now;

      if (diff <= 0) {
        setTimeLeft({ hours: 0, minutes: 0, seconds: 0 });
        return;
      }

      setTimeLeft({
        hours: Math.floor(diff / (1000 * 60 * 60)),
        minutes: Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60)),
        seconds: Math.floor((diff % (1000 * 60)) / 1000),
      });
    }, 1000);

    return () => clearInterval(timer);
  }, [targetDate]);

  return (
    <div className="flex items-center justify-center gap-4">
      {[
        { value: timeLeft.hours, label: 'Horas' },
        { value: timeLeft.minutes, label: 'Minutos' },
        { value: timeLeft.seconds, label: 'Segundos' },
      ].map(item => (
        <div key={item.label} className="flex flex-col items-center">
          <div className="flex h-16 w-16 items-center justify-center rounded-xl border border-white/10 bg-white/10 backdrop-blur-sm sm:h-20 sm:w-20">
            <span className="text-2xl font-bold text-white sm:text-3xl">
              {String(item.value).padStart(2, '0')}
            </span>
          </div>
          <span className="mt-2 text-xs tracking-wider text-primary/60/70 uppercase">
            {item.label}
          </span>
        </div>
      ))}
    </div>
  );
}

// =============================================================================
// PROGRESS BAR
// =============================================================================

function ProgressBar() {
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    const interval = setInterval(() => {
      setProgress(prev => {
        if (prev >= 100) return 0;
        return prev + 0.5;
      });
    }, 100);
    return () => clearInterval(interval);
  }, []);

  return (
    <div className="mx-auto w-full max-w-md">
      <div className="h-1.5 w-full overflow-hidden rounded-full bg-white/10">
        <div
          className="h-full rounded-full bg-gradient-to-r from-[#00A870] via-primary to-[#00A870] transition-all duration-100"
          style={{ width: `${progress}%` }}
        />
      </div>
    </div>
  );
}

// =============================================================================
// MAIN PAGE
// =============================================================================

export default function MantenimientoPage() {
  const [maintenanceInfo, setMaintenanceInfo] = useState<MaintenanceInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    let isFirstLoad = true;

    async function fetchStatus() {
      try {
        // BFF pattern: empty NEXT_PUBLIC_API_URL in prod = same-origin, proxied by Next.js rewrites
        const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:18443';
        const res = await fetch(`${apiUrl}/api/maintenance/status`, {
          cache: 'no-store',
        });
        if (res.ok) {
          const data = await res.json();
          setMaintenanceInfo(data);

          // If maintenance mode is OFF, redirect to home
          if (data.isMaintenanceMode === false) {
            router.replace('/');
            return;
          }
        } else if (!isFirstLoad) {
          // If API fails on subsequent polls (service might be down/restarting),
          // redirect to home as maintenance likely ended
          router.replace('/');
          return;
        }
      } catch (err) {
        console.error('Error fetching maintenance status:', err);
        // On network error during polling (not first load), redirect home
        if (!isFirstLoad) {
          router.replace('/');
          return;
        }
      } finally {
        setLoading(false);
        isFirstLoad = false;
      }
    }

    fetchStatus();
    // Poll every 10 seconds for faster redirect when maintenance ends
    const interval = setInterval(fetchStatus, 10000);
    return () => clearInterval(interval);
  }, [router]);

  const message =
    maintenanceInfo?.maintenanceWindow?.description ||
    'Estamos realizando mejoras en el sitio. Volveremos pronto.';

  const scheduledEnd = maintenanceInfo?.maintenanceWindow?.scheduledEnd;

  return (
    <div className="relative flex min-h-screen flex-col items-center justify-center overflow-hidden px-4">
      {/* Background decorations */}
      <div className="pointer-events-none absolute inset-0">
        {/* Gradient orbs */}
        <div className="absolute -top-40 -left-40 h-96 w-96 rounded-full bg-[#00A870]/20 blur-3xl" />
        <div className="absolute -right-40 -bottom-40 h-96 w-96 rounded-full bg-primary/20 blur-3xl" />
        <div className="absolute top-1/2 left-1/2 h-64 w-64 -translate-x-1/2 -translate-y-1/2 rounded-full bg-[#007850]/10 blur-3xl" />

        {/* Gears decoration */}
        <div className="absolute top-10 left-10 text-[#00A870]">
          <AnimatedGear size={120} />
        </div>
        <div className="absolute top-20 right-16 text-primary/80">
          <AnimatedGear size={80} delay={500} reverse />
        </div>
        <div className="absolute bottom-20 left-20 text-primary">
          <AnimatedGear size={60} delay={1000} />
        </div>
        <div className="absolute right-10 bottom-32 text-[#00A870]">
          <AnimatedGear size={100} delay={750} reverse />
        </div>

        {/* Grid pattern */}
        <div
          className="absolute inset-0 opacity-[0.03]"
          style={{
            backgroundImage: `linear-gradient(rgba(255,255,255,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(255,255,255,0.1) 1px, transparent 1px)`,
            backgroundSize: '60px 60px',
          }}
        />
      </div>

      {/* Main content */}
      <div className="relative z-10 flex max-w-2xl flex-col items-center text-center">
        {/* Logo */}
        <div className="mb-10">
          <div className="flex flex-col items-center justify-center gap-4">
            <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-gradient-to-br from-[#00A870] to-[#007850] shadow-xl shadow-[#00A870]/40">
              <svg width="36" height="36" viewBox="0 0 24 24" fill="none" className="text-white">
                <path
                  d="M19 17h2c.6 0 1-.4 1-1v-3c0-.9-.7-1.7-1.5-1.9C18.7 10.6 16 10 16 10s-1.3-2-2.2-3.3C13 5.6 11.7 5 10.5 5H5.8C4.6 5 3.5 5.9 3.1 7L2 10c-.7.3-1 1-1 1.8V16c0 .6.4 1 1 1h2"
                  stroke="currentColor"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <circle cx="7.5" cy="17.5" r="2.5" stroke="currentColor" strokeWidth="1.5" />
                <circle cx="16.5" cy="17.5" r="2.5" stroke="currentColor" strokeWidth="1.5" />
              </svg>
            </div>
            <h2 className="text-5xl font-extrabold tracking-wider text-white drop-shadow-lg sm:text-6xl">
              OKLA
            </h2>
            <p className="text-sm tracking-widest text-primary/60/60 uppercase">
              Marketplace de Vehículos
            </p>
          </div>
        </div>

        {/* Car icon */}
        <div className="mb-6">
          <CarIcon />
        </div>

        {/* Wrench + Gear icon */}
        <div className="mb-6 flex h-20 w-20 items-center justify-center rounded-full border border-white/10 bg-white/5 backdrop-blur-sm">
          <svg width="40" height="40" viewBox="0 0 24 24" fill="none" className="text-[#00A870]">
            <path
              d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"
              stroke="currentColor"
              strokeWidth="1.5"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
          </svg>
        </div>

        {/* Title */}
        <h1 className="mb-4 text-4xl font-bold text-white sm:text-5xl">
          Sitio en{' '}
          <span className="bg-gradient-to-r from-[#00A870] to-primary/60 bg-clip-text text-transparent">
            Mantenimiento
          </span>
        </h1>

        {/* Message */}
        <p className="mb-8 max-w-lg text-lg leading-relaxed text-primary-foreground/70">
          {loading ? (
            <span className="inline-flex items-center gap-2">
              <span className="h-4 w-4 animate-spin rounded-full border-2 border-[#00A870] border-t-transparent" />
              Cargando información...
            </span>
          ) : (
            message
          )}
        </p>

        {/* Progress bar */}
        <div className="mb-10 w-full max-w-sm">
          <ProgressBar />
          <p className="mt-3 text-sm text-primary/60/50">Trabajando en las mejoras...</p>
        </div>

        {/* Countdown (if ETA available) */}
        {scheduledEnd && (
          <div className="mb-10">
            <p className="mb-4 text-sm font-medium tracking-wider text-primary/60/60 uppercase">
              Tiempo estimado de regreso
            </p>
            <Countdown targetDate={scheduledEnd} />
          </div>
        )}

        {/* Divider */}
        <div className="mb-8 h-px w-32 bg-gradient-to-r from-transparent via-[#00A870]/30 to-transparent" />

        {/* Contact info */}
        <div className="space-y-3 text-sm text-primary/40/50">
          <p>¿Necesitas ayuda urgente? Contáctanos:</p>
          <div className="flex flex-wrap items-center justify-center gap-4">
            <a
              href="mailto:soporte@okla.com.do"
              className="inline-flex items-center gap-2 rounded-lg border border-[#00A870]/20 bg-[#00A870]/5 px-4 py-2 text-primary/60 transition-colors hover:bg-[#00A870]/15 hover:text-white"
            >
              <svg
                width="16"
                height="16"
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.5"
              >
                <rect x="2" y="4" width="20" height="16" rx="2" />
                <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
              </svg>
              soporte@okla.com.do
            </a>
            <a
              href="https://wa.me/18091234567"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 rounded-lg border border-[#00A870]/20 bg-[#00A870]/5 px-4 py-2 text-primary/60 transition-colors hover:bg-[#00A870]/15 hover:text-white"
            >
              <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                <path d="M17.472 14.382c-.297-.149-1.758-.867-2.03-.967-.273-.099-.471-.148-.67.15-.197.297-.767.966-.94 1.164-.173.199-.347.223-.644.075-.297-.15-1.255-.463-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.298-.347.446-.52.149-.174.198-.298.298-.497.099-.198.05-.371-.025-.52-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.5-.669-.51-.173-.008-.371-.01-.57-.01-.198 0-.52.074-.792.372-.272.297-1.04 1.016-1.04 2.479 0 1.462 1.065 2.875 1.213 3.074.149.198 2.096 3.2 5.077 4.487.709.306 1.262.489 1.694.625.712.227 1.36.195 1.871.118.571-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413-.074-.124-.272-.198-.57-.347m-5.421 7.403h-.004a9.87 9.87 0 01-5.031-1.378l-.361-.214-3.741.982.998-3.648-.235-.374a9.86 9.86 0 01-1.51-5.26c.001-5.45 4.436-9.884 9.888-9.884 2.64 0 5.122 1.03 6.988 2.898a9.825 9.825 0 012.893 6.994c-.003 5.45-4.437 9.884-9.885 9.884m8.413-18.297A11.815 11.815 0 0012.05 0C5.495 0 .16 5.335.157 11.892c0 2.096.547 4.142 1.588 5.945L.057 24l6.305-1.654a11.882 11.882 0 005.683 1.448h.005c6.554 0 11.89-5.335 11.893-11.893a11.821 11.821 0 00-3.48-8.413z" />
              </svg>
              WhatsApp
            </a>
          </div>
        </div>

        {/* Footer */}
        <div className="mt-12 text-xs text-primary/60/30">
          <p>
            © {new Date().getFullYear()} OKLA — Marketplace de Vehículos en República Dominicana
          </p>
        </div>
      </div>

      {/* CSS for custom animations */}
      <style jsx global>{`
        @keyframes spin-slow {
          from {
            transform: rotate(0deg);
          }
          to {
            transform: rotate(360deg);
          }
        }
        @keyframes spin-slow-reverse {
          from {
            transform: rotate(360deg);
          }
          to {
            transform: rotate(0deg);
          }
        }
        .animate-spin-slow {
          animation: spin-slow 20s linear infinite;
        }
        .animate-spin-slow-reverse {
          animation: spin-slow-reverse 15s linear infinite;
        }
      `}</style>
    </div>
  );
}

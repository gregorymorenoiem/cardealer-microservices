/**
 * Dashboard Layout
 *
 * Layout with sidebar for authenticated user pages.
 * Protected by AuthGuard - requires authentication.
 *
 * Security:
 * - Middleware handles server-side route protection (first layer)
 * - AuthGuard provides client-side defense-in-depth (second layer)
 */

'use client';

import * as React from 'react';
import Link from 'next/link';
import { useRouter, usePathname } from 'next/navigation';
import {
  Home,
  Heart,
  Bell,
  MessageSquare,
  Search,
  Car,
  Settings,
  User,
  LogOut,
  Bookmark,
  Clock,
  ShieldCheck,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useAuth } from '@/hooks/use-auth';
import { AuthGuard } from '@/components/auth/auth-guard';

// =============================================================================
// NAVIGATION LINKS
// =============================================================================

const sidebarLinks = [
  { href: '/dashboard', label: 'Dashboard', icon: Home },
  { href: '/cuenta/favoritos', label: 'Favoritos', icon: Heart },
  { href: '/cuenta/alertas', label: 'Alertas', icon: Bell },
  { href: '/cuenta/mensajes', label: 'Mensajes', icon: MessageSquare },
  { href: '/cuenta/busquedas-guardadas', label: 'Búsquedas Guardadas', icon: Bookmark },
  { href: '/cuenta/historial', label: 'Historial', icon: Clock },
  { href: '/cuenta/mis-vehiculos', label: 'Mis Vehículos', icon: Car },
];

const settingsLinks = [
  { href: '/cuenta/perfil', label: 'Perfil', icon: User },
  { href: '/cuenta/seguridad', label: 'Seguridad', icon: ShieldCheck },
  { href: '/cuenta/configuracion', label: 'Configuración', icon: Settings },
];

// =============================================================================
// MAIN LAYOUT (with AuthGuard wrapper)
// =============================================================================

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <AuthGuard>
      <DashboardLayoutContent>{children}</DashboardLayoutContent>
    </AuthGuard>
  );
}

// =============================================================================
// LAYOUT CONTENT
// =============================================================================

function DashboardLayoutContent({ children }: { children: React.ReactNode }) {
  const { logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  const handleLogout = async () => {
    await logout();
    router.push('/login');
  };

  const isActivePath = (href: string) => {
    if (href === '/dashboard') return pathname === '/dashboard';
    return pathname.startsWith(href);
  };

  return (
    <div className="bg-muted/50 min-h-screen">
      {/* Header */}
      <header className="border-border bg-card sticky top-0 z-40 border-b">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
              <span className="text-lg font-bold text-white">O</span>
            </div>
            <span className="text-foreground text-xl font-bold">OKLA</span>
          </Link>

          <div className="flex items-center gap-4">
            <Link
              href="/buscar"
              className="text-muted-foreground hover:text-foreground hidden items-center gap-2 text-sm sm:flex"
            >
              <Search className="h-4 w-4" />
              Buscar
            </Link>
            <Link
              href="/cuenta/mensajes"
              className="text-muted-foreground hover:text-foreground relative p-2"
            >
              <MessageSquare className="h-5 w-5" />
            </Link>
            <Link
              href="/cuenta/notificaciones"
              className="text-muted-foreground hover:text-foreground relative p-2"
            >
              <Bell className="h-5 w-5" />
            </Link>
            <div className="bg-muted flex h-8 w-8 items-center justify-center rounded-full">
              <User className="text-muted-foreground h-4 w-4" />
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className="bg-card hidden min-h-[calc(100vh-4rem)] w-64 flex-col border-r lg:flex">
          <nav className="flex-1 space-y-1 p-4">
            {sidebarLinks.map(link => {
              const Icon = link.icon;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={cn(
                    'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                    isActivePath(link.href)
                      ? 'bg-muted text-foreground font-medium'
                      : 'text-foreground hover:bg-muted hover:text-foreground'
                  )}
                >
                  <Icon className="h-4 w-4" />
                  {link.label}
                </Link>
              );
            })}

            <div className="border-border mt-4 border-t pt-4">
              <p className="text-muted-foreground mb-2 px-3 text-xs font-semibold tracking-wider uppercase">
                Cuenta
              </p>
              {settingsLinks.map(link => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    className={cn(
                      'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                      isActivePath(link.href)
                        ? 'bg-muted text-foreground font-medium'
                        : 'text-foreground hover:bg-muted hover:text-foreground'
                    )}
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                );
              })}
            </div>
          </nav>

          <div className="border-border border-t p-4">
            <button
              type="button"
              onClick={handleLogout}
              className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-red-600 transition-colors hover:bg-red-50"
            >
              <LogOut className="h-4 w-4" />
              Cerrar Sesión
            </button>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-4 lg:p-6">{children}</main>
      </div>
    </div>
  );
}

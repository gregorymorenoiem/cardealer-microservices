/**
 * Dashboard Layout
 *
 * Layout with sidebar for authenticated user pages
 */

import { Metadata } from 'next';
import Link from 'next/link';
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

export const metadata: Metadata = {
  title: 'Dashboard | OKLA',
  description: 'Tu centro de control en OKLA',
};

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

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="sticky top-0 z-40 border-b bg-white">
        <div className="container flex h-16 items-center justify-between">
          <Link href="/" className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-600">
              <span className="text-lg font-bold text-white">O</span>
            </div>
            <span className="text-xl font-bold text-gray-900">OKLA</span>
          </Link>

          <div className="flex items-center gap-4">
            <Link
              href="/buscar"
              className="hidden items-center gap-2 text-sm text-gray-600 hover:text-gray-900 sm:flex"
            >
              <Search className="h-4 w-4" />
              Buscar
            </Link>
            <Link
              href="/cuenta/mensajes"
              className="relative p-2 text-gray-600 hover:text-gray-900"
            >
              <MessageSquare className="h-5 w-5" />
              <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] text-white">
                3
              </span>
            </Link>
            <Link
              href="/cuenta/notificaciones"
              className="relative p-2 text-gray-600 hover:text-gray-900"
            >
              <Bell className="h-5 w-5" />
              <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] text-white">
                5
              </span>
            </Link>
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-200">
              <User className="h-4 w-4 text-gray-600" />
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className="hidden min-h-[calc(100vh-4rem)] w-64 flex-col border-r bg-white lg:flex">
          <nav className="flex-1 space-y-1 p-4">
            {sidebarLinks.map(link => {
              const Icon = link.icon;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-700 transition-colors hover:bg-gray-100 hover:text-gray-900"
                >
                  <Icon className="h-4 w-4" />
                  {link.label}
                </Link>
              );
            })}

            <div className="mt-4 border-t pt-4">
              <p className="mb-2 px-3 text-xs font-semibold tracking-wider text-gray-400 uppercase">
                Cuenta
              </p>
              {settingsLinks.map(link => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-700 transition-colors hover:bg-gray-100 hover:text-gray-900"
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                );
              })}
            </div>
          </nav>

          <div className="border-t p-4">
            <Link
              href="/logout"
              className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-red-600 transition-colors hover:bg-red-50"
            >
              <LogOut className="h-4 w-4" />
              Cerrar Sesión
            </Link>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 p-4 lg:p-6">{children}</main>
      </div>
    </div>
  );
}

/**
 * Admin Layout
 *
 * Layout for admin panel with sidebar navigation
 */

import { Metadata } from 'next';
import Link from 'next/link';
import {
  LayoutDashboard,
  Users,
  Car,
  Building,
  BarChart3,
  FileText,
  Settings,
  LogOut,
  Bell,
  Shield,
  Flag,
  CreditCard,
  MessageSquare,
  HelpCircle,
  AlertTriangle,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Admin Panel | OKLA',
  description: 'Panel de administración de OKLA',
};

const mainLinks = [
  { href: '/admin', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/admin/usuarios', label: 'Usuarios', icon: Users },
  { href: '/admin/vehiculos', label: 'Vehículos', icon: Car },
  { href: '/admin/dealers', label: 'Dealers', icon: Building },
  { href: '/admin/reportes', label: 'Reportes', icon: Flag },
  { href: '/admin/moderacion', label: 'Moderación', icon: AlertTriangle },
];

const managementLinks = [
  { href: '/admin/facturacion', label: 'Facturación', icon: CreditCard },
  { href: '/admin/analytics', label: 'Analytics', icon: BarChart3 },
  { href: '/admin/contenido', label: 'Contenido', icon: FileText },
  { href: '/admin/mensajes', label: 'Mensajes', icon: MessageSquare },
];

const settingsLinks = [
  { href: '/admin/roles', label: 'Roles y Permisos', icon: Shield },
  { href: '/admin/configuracion', label: 'Configuración', icon: Settings },
];

export default function AdminLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <header className="sticky top-0 z-40 bg-slate-900 text-white">
        <div className="flex h-16 items-center justify-between px-4 lg:px-6">
          <div className="flex items-center gap-4">
            <Link href="/admin" className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-600">
                <span className="text-lg font-bold text-white">O</span>
              </div>
              <div>
                <span className="text-xl font-bold">OKLA</span>
                <span className="ml-2 text-sm text-slate-400">Admin</span>
              </div>
            </Link>
          </div>

          <div className="flex items-center gap-4">
            <Link
              href="/admin/notificaciones"
              className="relative p-2 text-slate-300 hover:text-white"
            >
              <Bell className="h-5 w-5" />
              <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] text-white">
                8
              </span>
            </Link>
            <Link href="/ayuda" className="p-2 text-slate-300 hover:text-white">
              <HelpCircle className="h-5 w-5" />
            </Link>
            <div className="flex items-center gap-2 border-l border-slate-700 pl-4">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-emerald-600">
                <span className="text-sm font-medium">A</span>
              </div>
              <div className="hidden sm:block">
                <p className="text-sm font-medium">Admin</p>
                <p className="text-xs text-slate-400">Super Admin</p>
              </div>
            </div>
          </div>
        </div>
      </header>

      <div className="flex">
        {/* Sidebar */}
        <aside className="hidden min-h-[calc(100vh-4rem)] w-64 flex-col border-r bg-white lg:flex">
          <nav className="flex-1 space-y-1 overflow-y-auto p-4">
            {mainLinks.map(link => {
              const Icon = link.icon;
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-slate-100 hover:text-slate-900"
                >
                  <Icon className="h-4 w-4" />
                  {link.label}
                </Link>
              );
            })}

            <div className="mt-4 border-t pt-4">
              <p className="mb-2 px-3 text-xs font-semibold tracking-wider text-gray-400 uppercase">
                Gestión
              </p>
              {managementLinks.map(link => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-slate-100 hover:text-slate-900"
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                );
              })}
            </div>

            <div className="mt-4 border-t pt-4">
              <p className="mb-2 px-3 text-xs font-semibold tracking-wider text-gray-400 uppercase">
                Sistema
              </p>
              {settingsLinks.map(link => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-slate-100 hover:text-slate-900"
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
              href="/"
              className="flex items-center gap-3 rounded-lg px-3 py-2 text-sm text-gray-600 transition-colors hover:bg-gray-100"
            >
              <LayoutDashboard className="h-4 w-4" />
              Ir al Sitio
            </Link>
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
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">{children}</main>
      </div>
    </div>
  );
}

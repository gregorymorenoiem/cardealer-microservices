/**
 * Dealer Layout
 *
 * Layout for dealer portal with sidebar navigation
 */

import { Metadata } from 'next';
import Link from 'next/link';
import {
  LayoutDashboard,
  Car,
  Users,
  BarChart3,
  Calendar,
  MessageSquare,
  MapPin,
  FileText,
  CreditCard,
  Settings,
  LogOut,
  Building,
  TrendingUp,
  Bell,
  HelpCircle,
} from 'lucide-react';

export const metadata: Metadata = {
  title: 'Portal Dealer | OKLA',
  description: 'Gestiona tu inventario y ventas en OKLA',
};

const mainLinks = [
  { href: '/dealer', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/dealer/inventario', label: 'Inventario', icon: Car },
  { href: '/dealer/leads', label: 'Leads', icon: Users },
  { href: '/dealer/analytics', label: 'Analytics', icon: BarChart3 },
  { href: '/dealer/citas', label: 'Citas', icon: Calendar },
  { href: '/dealer/mensajes', label: 'Mensajes', icon: MessageSquare },
];

const managementLinks = [
  { href: '/dealer/empleados', label: 'Empleados', icon: Users },
  { href: '/dealer/ubicaciones', label: 'Ubicaciones', icon: MapPin },
  { href: '/dealer/pricing', label: 'Pricing IA', icon: TrendingUp },
  { href: '/dealer/reportes', label: 'Reportes', icon: FileText },
];

const settingsLinks = [
  { href: '/dealer/perfil', label: 'Perfil Dealer', icon: Building },
  { href: '/dealer/documentos', label: 'Documentos', icon: FileText },
  { href: '/dealer/facturacion', label: 'Facturación', icon: CreditCard },
  { href: '/dealer/suscripcion', label: 'Suscripción', icon: CreditCard },
  { href: '/dealer/configuracion', label: 'Configuración', icon: Settings },
];

export default function DealerLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <header className="sticky top-0 z-40 border-b bg-white">
        <div className="flex h-16 items-center justify-between px-4 lg:px-6">
          <div className="flex items-center gap-4">
            <Link href="/dealer" className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-emerald-600">
                <span className="text-lg font-bold text-white">O</span>
              </div>
              <div className="hidden sm:block">
                <span className="text-xl font-bold text-gray-900">OKLA</span>
                <span className="ml-2 text-sm text-gray-500">Dealer Portal</span>
              </div>
            </Link>
          </div>

          <div className="flex items-center gap-4">
            <Link
              href="/dealer/mensajes"
              className="relative p-2 text-gray-600 hover:text-gray-900"
            >
              <MessageSquare className="h-5 w-5" />
              <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] text-white">
                5
              </span>
            </Link>
            <Link
              href="/dealer/notificaciones"
              className="relative p-2 text-gray-600 hover:text-gray-900"
            >
              <Bell className="h-5 w-5" />
              <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-emerald-500 text-[10px] text-white">
                3
              </span>
            </Link>
            <Link href="/ayuda" className="p-2 text-gray-600 hover:text-gray-900">
              <HelpCircle className="h-5 w-5" />
            </Link>
            <div className="flex items-center gap-2 border-l pl-4">
              <div className="flex h-8 w-8 items-center justify-center rounded-full bg-emerald-100">
                <Building className="h-4 w-4 text-emerald-600" />
              </div>
              <div className="hidden sm:block">
                <p className="text-sm font-medium">AutoMax RD</p>
                <p className="text-xs text-gray-500">Plan Pro</p>
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
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-emerald-50 hover:text-emerald-700"
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
                    className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-emerald-50 hover:text-emerald-700"
                  >
                    <Icon className="h-4 w-4" />
                    {link.label}
                  </Link>
                );
              })}
            </div>

            <div className="mt-4 border-t pt-4">
              <p className="mb-2 px-3 text-xs font-semibold tracking-wider text-gray-400 uppercase">
                Configuración
              </p>
              {settingsLinks.map(link => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.href}
                    href={link.href}
                    className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm text-gray-700 transition-colors hover:bg-emerald-50 hover:text-emerald-700"
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
        <main className="flex-1 overflow-y-auto p-4 lg:p-6">{children}</main>
      </div>
    </div>
  );
}

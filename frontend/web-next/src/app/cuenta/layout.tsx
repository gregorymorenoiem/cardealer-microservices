/**
 * Account Layout
 *
 * Layout for authenticated user account pages with sidebar navigation
 */

import * as React from 'react';
import Link from 'next/link';
import { redirect } from 'next/navigation';
import {
  User,
  Car,
  Heart,
  Bell,
  MessageSquare,
  Settings,
  Shield,
  CreditCard,
  LogOut,
  Home,
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { authService } from '@/services/auth';

interface AccountLayoutProps {
  children: React.ReactNode;
}

const accountNavItems = [
  {
    title: 'General',
    items: [
      { href: '/cuenta', label: 'Dashboard', icon: Home },
      { href: '/cuenta/perfil', label: 'Mi Perfil', icon: User },
      { href: '/cuenta/mis-vehiculos', label: 'Mis Vehículos', icon: Car },
    ],
  },
  {
    title: 'Actividad',
    items: [
      { href: '/cuenta/favoritos', label: 'Favoritos', icon: Heart },
      { href: '/cuenta/mensajes', label: 'Mensajes', icon: MessageSquare },
      { href: '/cuenta/notificaciones', label: 'Notificaciones', icon: Bell },
    ],
  },
  {
    title: 'Cuenta',
    items: [
      { href: '/cuenta/seguridad', label: 'Seguridad', icon: Shield },
      { href: '/cuenta/pagos', label: 'Pagos', icon: CreditCard },
      { href: '/cuenta/configuracion', label: 'Configuración', icon: Settings },
    ],
  },
];

export default async function AccountLayout({ children }: AccountLayoutProps) {
  // Check if user is authenticated
  const user = await authService.getCurrentUser().catch(() => null);

  if (!user) {
    redirect('/login?redirect=/cuenta');
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-gray-200 bg-white">
        <div className="container mx-auto px-4">
          <div className="flex h-16 items-center justify-between">
            <Link href="/" className="text-primary text-2xl font-bold">
              OKLA
            </Link>
            <nav className="hidden items-center gap-4 md:flex">
              <Link href="/vehiculos">
                <Button variant="ghost">Ver Vehículos</Button>
              </Link>
              <Link href="/vender">
                <Button>Publicar Vehículo</Button>
              </Link>
            </nav>
          </div>
        </div>
      </header>

      <div className="container mx-auto px-4 py-6">
        <div className="flex flex-col gap-6 lg:flex-row">
          {/* Sidebar */}
          <aside className="flex-shrink-0 lg:w-64">
            <nav className="overflow-hidden rounded-lg border border-gray-200 bg-white">
              {/* User info */}
              <div className="border-b border-gray-200 p-4">
                <div className="flex items-center gap-3">
                  <div className="bg-primary/10 flex h-10 w-10 items-center justify-center rounded-full">
                    {user.avatarUrl ? (
                      <img
                        src={user.avatarUrl}
                        alt={user.fullName}
                        className="h-10 w-10 rounded-full object-cover"
                      />
                    ) : (
                      <User className="text-primary h-5 w-5" />
                    )}
                  </div>
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium text-gray-900">{user.fullName}</p>
                    <p className="truncate text-sm text-gray-500">{user.email}</p>
                  </div>
                </div>
              </div>

              {/* Navigation */}
              <div className="p-2">
                {accountNavItems.map((section, index) => (
                  <div key={section.title}>
                    {index > 0 && <Separator className="my-2" />}
                    <p className="px-3 py-2 text-xs font-medium tracking-wider text-gray-500 uppercase">
                      {section.title}
                    </p>
                    {section.items.map(item => (
                      <Link
                        key={item.href}
                        href={item.href}
                        className="flex items-center gap-3 rounded-lg px-3 py-2 text-gray-700 transition-colors hover:bg-gray-100 hover:text-gray-900"
                      >
                        <item.icon className="h-5 w-5" />
                        <span>{item.label}</span>
                      </Link>
                    ))}
                  </div>
                ))}

                <Separator className="my-2" />

                {/* Logout */}
                <form
                  action={async () => {
                    'use server';
                    await authService.logout();
                    redirect('/login');
                  }}
                >
                  <button
                    type="submit"
                    className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-red-600 transition-colors hover:bg-red-50"
                  >
                    <LogOut className="h-5 w-5" />
                    <span>Cerrar Sesión</span>
                  </button>
                </form>
              </div>
            </nav>
          </aside>

          {/* Main content */}
          <main className="min-w-0 flex-1">{children}</main>
        </div>
      </div>
    </div>
  );
}

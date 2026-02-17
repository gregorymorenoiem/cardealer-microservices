/**
 * Admin Group Layout
 *
 * Layout separado para el panel de administración.
 * NO incluye el Navbar ni Footer del sitio público.
 */

import { Metadata } from 'next';

export const metadata: Metadata = {
  title: {
    default: 'Admin Panel | OKLA',
    template: '%s | Admin OKLA',
  },
  description: 'Panel de administración de OKLA',
  robots: {
    index: false,
    follow: false,
  },
};

export default function AdminGroupLayout({ children }: { children: React.ReactNode }) {
  return <>{children}</>;
}

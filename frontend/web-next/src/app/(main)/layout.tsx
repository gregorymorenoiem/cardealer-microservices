/**
 * Main Layout
 * Layout for pages that should have the standard Navbar and Footer.
 * The MainLayoutShell conditionally hides Navbar/Footer for routes
 * that provide their own layout (e.g., dealer portal).
 */

import { MainLayoutShell } from '@/components/layout/main-layout-shell';

export default function MainLayout({ children }: { children: React.ReactNode }) {
  return <MainLayoutShell>{children}</MainLayoutShell>;
}

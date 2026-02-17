import type { ReactNode } from 'react';
import Navbar from '@/components/organisms/Navbar';
import Footer from '@/components/organisms/Footer';
import { MaintenanceBanner } from '@/components/marketplace/MaintenanceBanner';
import { EarlyBirdBanner } from '@/components/marketplace/EarlyBirdBanner';

interface MainLayoutProps {
  children: ReactNode;
}

/**
 * MainLayout - Layout principal para páginas públicas
 * Incluye Navbar + banners + contenido + Footer
 */
export default function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      <MaintenanceBanner />
      <EarlyBirdBanner />
      <Navbar />
      <main className="flex-1">{children}</main>
      <Footer />
    </div>
  );
}

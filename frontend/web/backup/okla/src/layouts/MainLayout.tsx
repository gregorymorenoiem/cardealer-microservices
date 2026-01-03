import type { ReactNode } from 'react';
import Navbar from '@/components/organisms/Navbar';
import Footer from '@/components/organisms/Footer';

interface MainLayoutProps {
  children: ReactNode;
}

/**
 * MainLayout - Layout principal para páginas públicas
 * Incluye Navbar + contenido + Footer
 */
export default function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      <main className="flex-1">
        {children}
      </main>
      <Footer />
    </div>
  );
}

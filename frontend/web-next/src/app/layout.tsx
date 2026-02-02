import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import { Navbar, Footer } from '@/components/layout';
import { Providers } from './providers';
import { Toaster } from 'sonner';
import './globals.css';

const inter = Inter({
  subsets: ['latin'],
  display: 'swap',
  variable: '--font-inter',
});

export const metadata: Metadata = {
  title: {
    default: 'OKLA - Marketplace de Vehículos #1 en República Dominicana',
    template: '%s | OKLA',
  },
  description:
    'Encuentra el vehículo perfecto o vende el tuyo en OKLA. El marketplace de autos más grande de República Dominicana con miles de opciones verificadas.',
  keywords: [
    'carros',
    'vehículos',
    'autos',
    'República Dominicana',
    'comprar carro',
    'vender carro',
    'marketplace',
    'OKLA',
    'dealer',
    'usados',
    'nuevos',
  ],
  authors: [{ name: 'OKLA' }],
  creator: 'OKLA',
  publisher: 'OKLA',
  formatDetection: {
    email: false,
    address: false,
    telephone: false,
  },
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL || 'https://okla.com.do'),
  alternates: {
    canonical: '/',
    languages: {
      'es-DO': '/es-DO',
      'en-US': '/en-US',
    },
  },
  openGraph: {
    type: 'website',
    locale: 'es_DO',
    url: '/',
    siteName: 'OKLA',
    title: 'OKLA - Marketplace de Vehículos #1 en República Dominicana',
    description:
      'Encuentra el vehículo perfecto o vende el tuyo en OKLA. El marketplace de autos más grande de República Dominicana.',
    images: [
      {
        url: '/og-image.jpg',
        width: 1200,
        height: 630,
        alt: 'OKLA - Marketplace de Vehículos',
      },
    ],
  },
  twitter: {
    card: 'summary_large_image',
    title: 'OKLA - Marketplace de Vehículos',
    description: 'El marketplace de autos #1 de República Dominicana',
    images: ['/og-image.jpg'],
    creator: '@okla',
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      'max-video-preview': -1,
      'max-image-preview': 'large',
      'max-snippet': -1,
    },
  },
  icons: {
    icon: '/favicon.ico',
    shortcut: '/favicon-16x16.png',
    apple: '/apple-touch-icon.png',
  },
  manifest: '/site.webmanifest',
};

export const viewport: Viewport = {
  themeColor: '#00A870',
  width: 'device-width',
  initialScale: 1,
  maximumScale: 5,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es-DO" className={inter.variable}>
      <body className="min-h-screen bg-white antialiased">
        <Providers>
          <div className="flex min-h-screen flex-col">
            <Navbar />
            <main className="flex-1">{children}</main>
            <Footer />
          </div>
          <Toaster
            position="top-right"
            toastOptions={{
              style: {
                background: 'white',
                border: '1px solid #E5E7EB',
                borderRadius: '0.75rem',
              },
            }}
          />
        </Providers>
      </body>
    </html>
  );
}

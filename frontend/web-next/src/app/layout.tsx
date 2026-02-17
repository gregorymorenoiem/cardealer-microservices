import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import { Providers } from './providers';
import { Toaster } from 'sonner';
import { GoogleAnalytics } from '@/components/analytics/google-analytics';
import { SiteJsonLd } from '@/lib/seo';
import { PWAComponents } from '@/components/pwa/pwa-wrapper';
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
  manifest: '/manifest.json',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'default',
    title: 'OKLA',
  },
};

export const viewport: Viewport = {
  themeColor: '#00A870',
  width: 'device-width',
  initialScale: 1,
  maximumScale: 5,
};

// Script to apply theme before hydration (prevents flash of wrong theme)
// Default is 'light' - users must explicitly choose dark mode
const themeScript = `
  (function() {
    try {
      const stored = localStorage.getItem('okla-theme');
      // Default to 'light' instead of 'system' - OKLA's brand is light theme
      const theme = stored || 'light';
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      const isDark = theme === 'dark' || (theme === 'system' && prefersDark);
      if (isDark) {
        document.documentElement.classList.add('dark');
      } else {
        document.documentElement.classList.remove('dark');
      }
    } catch (e) {}
  })()
`;

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html
      lang="es-DO"
      className={inter.variable}
      suppressHydrationWarning
      data-scroll-behavior="smooth"
    >
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
        <meta httpEquiv="Permissions-Policy" content="camera=*, microphone=*" />
        <SiteJsonLd />
        <GoogleAnalytics />
      </head>
      <body
        className="bg-background text-foreground min-h-screen antialiased"
        suppressHydrationWarning
      >
        <Providers>
          <PWAComponents>
            {children}
            <Toaster
              position="top-right"
              toastOptions={{
                classNames: {
                  toast: 'bg-card text-card-foreground border-border',
                },
              }}
            />
          </PWAComponents>
        </Providers>
      </body>
    </html>
  );
}

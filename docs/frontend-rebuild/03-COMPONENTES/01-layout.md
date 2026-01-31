# 🎨 Layout Principal y Navegación

> **Tiempo estimado:** 45 minutos
> **Prerrequisitos:** Next.js configurado, shadcn/ui instalado, design tokens

---

## 📋 OBJETIVO

Implementar el layout principal de la aplicación:

- Layout raíz con providers
- Header/Navbar responsive
- Footer
- Sidebar para dashboard
- Mobile navigation
- Breadcrumbs
- Skip links para accesibilidad

---

## 🔧 PASO 1: Layout Raíz

### Root Layout

```typescript
// filepath: src/app/layout.tsx
import type { Metadata, Viewport } from "next";
import { Inter } from "next/font/google";
import { Toaster } from "sonner";
import { Providers } from "@/components/providers/Providers";
import { SkipLink } from "@/components/ui/SkipLink";
import "@/styles/globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-inter",
});

export const metadata: Metadata = {
  metadataBase: new URL("https://okla.com.do"),
  title: {
    default: "OKLA | Compra y Vende Vehículos en República Dominicana",
    template: "%s | OKLA",
  },
  description:
    "El marketplace #1 de vehículos en RD. Encuentra carros nuevos y usados de dealers verificados y vendedores particulares.",
  keywords: [
    "carros",
    "vehículos",
    "autos",
    "república dominicana",
    "comprar carro",
    "vender carro",
    "dealers",
  ],
  authors: [{ name: "OKLA" }],
  creator: "OKLA",
  publisher: "OKLA",
  openGraph: {
    type: "website",
    locale: "es_DO",
    url: "https://okla.com.do",
    siteName: "OKLA",
    title: "OKLA | Compra y Vende Vehículos en República Dominicana",
    description:
      "El marketplace #1 de vehículos en RD. Encuentra carros nuevos y usados.",
    images: [
      {
        url: "/og-image.jpg",
        width: 1200,
        height: 630,
        alt: "OKLA - Marketplace de Vehículos",
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title: "OKLA | Compra y Vende Vehículos",
    description: "El marketplace #1 de vehículos en República Dominicana",
    images: ["/og-image.jpg"],
    creator: "@okla_rd",
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      "max-video-preview": -1,
      "max-image-preview": "large",
      "max-snippet": -1,
    },
  },
  verification: {
    google: "google-verification-code",
  },
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 5,
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#ffffff" },
    { media: "(prefers-color-scheme: dark)", color: "#0f172a" },
  ],
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="es" className={inter.variable} suppressHydrationWarning>
      <body className="min-h-screen bg-gray-50 font-sans antialiased">
        <Providers>
          <SkipLink />
          {children}
          <Toaster
            position="top-right"
            expand
            richColors
            closeButton
            toastOptions={{
              duration: 4000,
            }}
          />
        </Providers>
      </body>
    </html>
  );
}
```

### Providers

```typescript
// filepath: src/components/providers/Providers.tsx
"use client";

import * as React from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { SessionProvider } from "next-auth/react";
import { ThemeProvider } from "next-themes";
import { queryClient } from "@/lib/query-client";

interface ProvidersProps {
  children: React.ReactNode;
}

export function Providers({ children }: ProvidersProps) {
  return (
    <SessionProvider>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider
          attribute="class"
          defaultTheme="light"
          enableSystem
          disableTransitionOnChange
        >
          {children}
        </ThemeProvider>
        {process.env.NODE_ENV === "development" && (
          <ReactQueryDevtools initialIsOpen={false} />
        )}
      </QueryClientProvider>
    </SessionProvider>
  );
}
```

---

## 🔧 PASO 2: Main Layout (Público)

### Layout con Header y Footer

```typescript
// filepath: src/components/layouts/MainLayout.tsx
import { Header } from "@/components/navigation/Header";
import { Footer } from "@/components/navigation/Footer";
import { MobileNav } from "@/components/navigation/MobileNav";

interface MainLayoutProps {
  children: React.ReactNode;
  showFooter?: boolean;
}

export function MainLayout({ children, showFooter = true }: MainLayoutProps) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main id="main-content" className="flex-1">
        {children}
      </main>
      {showFooter && <Footer />}
      <MobileNav />
    </div>
  );
}
```

### App Group Layout

```typescript
// filepath: src/app/(main)/layout.tsx
import { MainLayout } from "@/components/layouts/MainLayout";

export default function MainGroupLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <MainLayout>{children}</MainLayout>;
}
```

---

## 🔧 PASO 3: Header/Navbar

### Header Component

```typescript
// filepath: src/components/navigation/Header.tsx
"use client";

import * as React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import { Menu, Search, Heart, Bell, User, Plus, X } from "lucide-react";
import { Button } from "@/components/ui/Button";
import { Logo } from "@/components/brand/Logo";
import { SearchBar } from "@/components/search/SearchBar";
import { UserMenu } from "@/components/navigation/UserMenu";
import { NotificationsMenu } from "@/components/navigation/NotificationsMenu";
import { cn } from "@/lib/utils";

const NAV_LINKS = [
  { href: "/vehiculos", label: "Vehículos" },
  { href: "/marcas", label: "Marcas" },
  { href: "/dealers", label: "Dealers" },
  { href: "/noticias", label: "Noticias" },
] as const;

export function Header() {
  const pathname = usePathname();
  const { data: session, status } = useSession();
  const [isSearchOpen, setIsSearchOpen] = React.useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = React.useState(false);

  const isLoading = status === "loading";
  const isAuthenticated = !!session?.user;

  return (
    <header className="sticky top-0 z-header w-full border-b border-gray-200 bg-white">
      <nav className="container" aria-label="Navegación principal">
        <div className="flex h-16 items-center justify-between gap-4">
          {/* Logo */}
          <Link
            href="/"
            className="flex-shrink-0 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 rounded-md"
            aria-label="Ir al inicio"
          >
            <Logo className="h-8 w-auto" />
          </Link>

          {/* Desktop Navigation */}
          <div className="hidden lg:flex lg:items-center lg:gap-8">
            {NAV_LINKS.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={cn(
                  "text-sm font-medium transition-colors hover:text-primary-600",
                  "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 rounded-md px-1",
                  pathname === link.href || pathname.startsWith(link.href + "/")
                    ? "text-primary-600"
                    : "text-gray-700"
                )}
              >
                {link.label}
              </Link>
            ))}
          </div>

          {/* Search Bar - Desktop */}
          <div className="hidden md:block flex-1 max-w-md mx-4">
            <SearchBar />
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2">
            {/* Mobile Search Toggle */}
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setIsSearchOpen(!isSearchOpen)}
              aria-label={isSearchOpen ? "Cerrar búsqueda" : "Abrir búsqueda"}
            >
              {isSearchOpen ? <X size={20} /> : <Search size={20} />}
            </Button>

            {isLoading ? (
              // Loading skeleton
              <div className="flex items-center gap-2">
                <div className="h-9 w-9 animate-pulse rounded-full bg-gray-200" />
                <div className="h-9 w-24 animate-pulse rounded-md bg-gray-200" />
              </div>
            ) : isAuthenticated ? (
              // Authenticated user
              <>
                <Link href="/favoritos" className="hidden sm:block">
                  <Button
                    variant="ghost"
                    size="icon"
                    aria-label="Ver favoritos"
                  >
                    <Heart size={20} />
                  </Button>
                </Link>

                <NotificationsMenu />

                <Link href="/publicar">
                  <Button size="sm" className="hidden sm:flex">
                    <Plus size={16} className="mr-1" />
                    Publicar
                  </Button>
                </Link>

                <UserMenu user={session.user} />
              </>
            ) : (
              // Not authenticated
              <>
                <Link href="/login">
                  <Button variant="ghost" size="sm">
                    Iniciar Sesión
                  </Button>
                </Link>
                <Link href="/registro" className="hidden sm:block">
                  <Button size="sm">Registrarse</Button>
                </Link>
              </>
            )}

            {/* Mobile Menu Toggle */}
            <Button
              variant="ghost"
              size="icon"
              className="lg:hidden"
              onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
              aria-label={isMobileMenuOpen ? "Cerrar menú" : "Abrir menú"}
              aria-expanded={isMobileMenuOpen}
            >
              {isMobileMenuOpen ? <X size={20} /> : <Menu size={20} />}
            </Button>
          </div>
        </div>

        {/* Mobile Search - Expandable */}
        {isSearchOpen && (
          <div className="py-3 md:hidden">
            <SearchBar autoFocus onClose={() => setIsSearchOpen(false)} />
          </div>
        )}

        {/* Mobile Navigation Menu */}
        {isMobileMenuOpen && (
          <MobileMenu
            isOpen={isMobileMenuOpen}
            onClose={() => setIsMobileMenuOpen(false)}
            isAuthenticated={isAuthenticated}
          />
        )}
      </nav>
    </header>
  );
}

interface MobileMenuProps {
  isOpen: boolean;
  onClose: () => void;
  isAuthenticated: boolean;
}

function MobileMenu({ isOpen, onClose, isAuthenticated }: MobileMenuProps) {
  const pathname = usePathname();

  React.useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = "hidden";
    }
    return () => {
      document.body.style.overflow = "";
    };
  }, [isOpen]);

  return (
    <div className="lg:hidden border-t border-gray-200 bg-white py-4">
      <div className="flex flex-col gap-2">
        {NAV_LINKS.map((link) => (
          <Link
            key={link.href}
            href={link.href}
            onClick={onClose}
            className={cn(
              "px-3 py-2 text-base font-medium rounded-md transition-colors",
              pathname === link.href || pathname.startsWith(link.href + "/")
                ? "bg-primary-50 text-primary-600"
                : "text-gray-700 hover:bg-gray-50"
            )}
          >
            {link.label}
          </Link>
        ))}

        {isAuthenticated && (
          <>
            <hr className="my-2" />
            <Link
              href="/favoritos"
              onClick={onClose}
              className="px-3 py-2 text-base font-medium text-gray-700 hover:bg-gray-50 rounded-md"
            >
              Mis Favoritos
            </Link>
            <Link
              href="/publicar"
              onClick={onClose}
              className="px-3 py-2 text-base font-medium text-gray-700 hover:bg-gray-50 rounded-md"
            >
              Publicar Vehículo
            </Link>
          </>
        )}

        {!isAuthenticated && (
          <>
            <hr className="my-2" />
            <Link
              href="/login"
              onClick={onClose}
              className="px-3 py-2 text-base font-medium text-gray-700 hover:bg-gray-50 rounded-md"
            >
              Iniciar Sesión
            </Link>
            <Link
              href="/registro"
              onClick={onClose}
              className="px-3 py-2 text-base font-medium text-primary-600 hover:bg-primary-50 rounded-md"
            >
              Registrarse
            </Link>
          </>
        )}
      </div>
    </div>
  );
}
```

### User Menu

```typescript
// filepath: src/components/navigation/UserMenu.tsx
"use client";

import * as React from "react";
import Link from "next/link";
import { signOut } from "next-auth/react";
import {
  User,
  Settings,
  Car,
  Heart,
  CreditCard,
  LogOut,
  HelpCircle,
  Building2,
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/DropdownMenu";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/Avatar";
import { Button } from "@/components/ui/Button";

interface UserMenuProps {
  user: {
    id: string;
    email: string;
    name?: string | null;
    image?: string | null;
    role?: string;
  };
}

export function UserMenu({ user }: UserMenuProps) {
  const initials = user.name
    ? user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2)
    : user.email[0].toUpperCase();

  const isDealer = user.role === "dealer";
  const isAdmin = user.role === "admin";

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button
          variant="ghost"
          className="relative h-9 w-9 rounded-full"
          aria-label="Menú de usuario"
        >
          <Avatar className="h-9 w-9">
            <AvatarImage src={user.image ?? undefined} alt={user.name ?? ""} />
            <AvatarFallback className="bg-primary-100 text-primary-700">
              {initials}
            </AvatarFallback>
          </Avatar>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent className="w-56" align="end" forceMount>
        <DropdownMenuLabel className="font-normal">
          <div className="flex flex-col space-y-1">
            <p className="text-sm font-medium leading-none">{user.name}</p>
            <p className="text-xs leading-none text-gray-500">{user.email}</p>
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />

        <DropdownMenuGroup>
          <DropdownMenuItem asChild>
            <Link href="/perfil" className="cursor-pointer">
              <User className="mr-2 h-4 w-4" />
              Mi Perfil
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem asChild>
            <Link href="/mis-vehiculos" className="cursor-pointer">
              <Car className="mr-2 h-4 w-4" />
              Mis Vehículos
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem asChild>
            <Link href="/favoritos" className="cursor-pointer">
              <Heart className="mr-2 h-4 w-4" />
              Favoritos
            </Link>
          </DropdownMenuItem>
        </DropdownMenuGroup>

        {isDealer && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuItem asChild>
                <Link href="/dealer/dashboard" className="cursor-pointer">
                  <Building2 className="mr-2 h-4 w-4" />
                  Dashboard Dealer
                </Link>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </>
        )}

        {isAdmin && (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuGroup>
              <DropdownMenuItem asChild>
                <Link href="/admin" className="cursor-pointer">
                  <Settings className="mr-2 h-4 w-4" />
                  Panel de Admin
                </Link>
              </DropdownMenuItem>
            </DropdownMenuGroup>
          </>
        )}

        <DropdownMenuSeparator />

        <DropdownMenuGroup>
          <DropdownMenuItem asChild>
            <Link href="/suscripcion" className="cursor-pointer">
              <CreditCard className="mr-2 h-4 w-4" />
              Suscripción
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem asChild>
            <Link href="/configuracion" className="cursor-pointer">
              <Settings className="mr-2 h-4 w-4" />
              Configuración
            </Link>
          </DropdownMenuItem>
          <DropdownMenuItem asChild>
            <Link href="/soporte" className="cursor-pointer">
              <HelpCircle className="mr-2 h-4 w-4" />
              Ayuda
            </Link>
          </DropdownMenuItem>
        </DropdownMenuGroup>

        <DropdownMenuSeparator />

        <DropdownMenuItem
          className="cursor-pointer text-red-600 focus:text-red-600"
          onClick={() => signOut({ callbackUrl: "/" })}
        >
          <LogOut className="mr-2 h-4 w-4" />
          Cerrar Sesión
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
```

---

## 🔧 PASO 4: Footer

```typescript
// filepath: src/components/navigation/Footer.tsx
import Link from "next/link";
import { Logo } from "@/components/brand/Logo";
import { Facebook, Instagram, Twitter, Youtube, Mail, Phone, MapPin } from "lucide-react";

const FOOTER_LINKS = {
  vehiculos: {
    title: "Vehículos",
    links: [
      { href: "/vehiculos?condicion=nuevo", label: "Nuevos" },
      { href: "/vehiculos?condicion=usado", label: "Usados" },
      { href: "/vehiculos?tipo=suv", label: "SUVs" },
      { href: "/vehiculos?tipo=sedan", label: "Sedanes" },
      { href: "/vehiculos?tipo=pickup", label: "Pickups" },
    ],
  },
  marcas: {
    title: "Marcas Populares",
    links: [
      { href: "/marcas/toyota", label: "Toyota" },
      { href: "/marcas/honda", label: "Honda" },
      { href: "/marcas/hyundai", label: "Hyundai" },
      { href: "/marcas/kia", label: "Kia" },
      { href: "/marcas/nissan", label: "Nissan" },
    ],
  },
  compania: {
    title: "Compañía",
    links: [
      { href: "/nosotros", label: "Sobre Nosotros" },
      { href: "/contacto", label: "Contacto" },
      { href: "/carreras", label: "Trabaja con Nosotros" },
      { href: "/prensa", label: "Prensa" },
    ],
  },
  legal: {
    title: "Legal",
    links: [
      { href: "/terminos", label: "Términos de Uso" },
      { href: "/privacidad", label: "Privacidad" },
      { href: "/cookies", label: "Cookies" },
    ],
  },
} as const;

const SOCIAL_LINKS = [
  { href: "https://facebook.com/okla.rd", icon: Facebook, label: "Facebook" },
  { href: "https://instagram.com/okla.rd", icon: Instagram, label: "Instagram" },
  { href: "https://twitter.com/okla_rd", icon: Twitter, label: "Twitter" },
  { href: "https://youtube.com/@oklard", icon: Youtube, label: "YouTube" },
];

export function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gray-900 text-gray-300" role="contentinfo">
      <div className="container py-12 md:py-16">
        {/* Main Footer Content */}
        <div className="grid grid-cols-2 gap-8 md:grid-cols-3 lg:grid-cols-6">
          {/* Brand Column */}
          <div className="col-span-2 md:col-span-3 lg:col-span-2">
            <Link href="/" aria-label="Ir al inicio">
              <Logo variant="white" className="h-8 w-auto mb-4" />
            </Link>
            <p className="text-sm text-gray-400 mb-6 max-w-sm">
              El marketplace #1 de vehículos en República Dominicana. Conectamos
              compradores y vendedores de manera segura y transparente.
            </p>

            {/* Contact Info */}
            <div className="space-y-2 text-sm">
              <a
                href="tel:+18095551234"
                className="flex items-center gap-2 hover:text-white transition-colors"
              >
                <Phone size={16} />
                (809) 555-1234
              </a>
              <a
                href="mailto:info@okla.com.do"
                className="flex items-center gap-2 hover:text-white transition-colors"
              >
                <Mail size={16} />
                info@okla.com.do
              </a>
              <div className="flex items-center gap-2">
                <MapPin size={16} />
                Santo Domingo, República Dominicana
              </div>
            </div>

            {/* Social Links */}
            <div className="flex gap-4 mt-6">
              {SOCIAL_LINKS.map((social) => (
                <a
                  key={social.href}
                  href={social.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-gray-400 hover:text-white transition-colors"
                  aria-label={social.label}
                >
                  <social.icon size={20} />
                </a>
              ))}
            </div>
          </div>

          {/* Link Columns */}
          {Object.entries(FOOTER_LINKS).map(([key, section]) => (
            <div key={key}>
              <h3 className="font-semibold text-white mb-4">{section.title}</h3>
              <ul className="space-y-2">
                {section.links.map((link) => (
                  <li key={link.href}>
                    <Link
                      href={link.href}
                      className="text-sm text-gray-400 hover:text-white transition-colors"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom Bar */}
        <div className="mt-12 pt-8 border-t border-gray-800">
          <div className="flex flex-col md:flex-row justify-between items-center gap-4">
            <p className="text-sm text-gray-500">
              © {currentYear} OKLA. Todos los derechos reservados.
            </p>
            <div className="flex gap-6 text-sm text-gray-500">
              <Link href="/terminos" className="hover:text-white transition-colors">
                Términos
              </Link>
              <Link href="/privacidad" className="hover:text-white transition-colors">
                Privacidad
              </Link>
              <Link href="/sitemap.xml" className="hover:text-white transition-colors">
                Sitemap
              </Link>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
}
```

---

## 🔧 PASO 5: Mobile Bottom Navigation

```typescript
// filepath: src/components/navigation/MobileNav.tsx
"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import { Home, Search, Heart, PlusCircle, User } from "lucide-react";
import { cn } from "@/lib/utils";

const NAV_ITEMS = [
  { href: "/", icon: Home, label: "Inicio" },
  { href: "/vehiculos", icon: Search, label: "Buscar" },
  { href: "/publicar", icon: PlusCircle, label: "Publicar", requiresAuth: true },
  { href: "/favoritos", icon: Heart, label: "Favoritos", requiresAuth: true },
  { href: "/perfil", icon: User, label: "Perfil", requiresAuth: true },
] as const;

export function MobileNav() {
  const pathname = usePathname();
  const { data: session, status } = useSession();

  // Don't show on auth pages
  if (pathname.startsWith("/login") || pathname.startsWith("/registro")) {
    return null;
  }

  return (
    <nav
      className="fixed bottom-0 left-0 right-0 z-mobile-nav bg-white border-t border-gray-200 lg:hidden"
      aria-label="Navegación móvil"
    >
      <ul className="flex items-center justify-around h-16 max-w-md mx-auto">
        {NAV_ITEMS.map((item) => {
          // Skip auth-required items for unauthenticated users
          if (item.requiresAuth && status !== "authenticated") {
            if (item.label === "Perfil") {
              // Replace with login
              return (
                <li key="login">
                  <Link
                    href="/login"
                    className="flex flex-col items-center gap-1 px-4 py-2 text-gray-500"
                  >
                    <User size={20} />
                    <span className="text-xs">Entrar</span>
                  </Link>
                </li>
              );
            }
            return null;
          }

          const isActive =
            item.href === "/"
              ? pathname === "/"
              : pathname.startsWith(item.href);

          return (
            <li key={item.href}>
              <Link
                href={item.href}
                className={cn(
                  "flex flex-col items-center gap-1 px-4 py-2 rounded-lg transition-colors",
                  isActive
                    ? "text-primary-600"
                    : "text-gray-500 hover:text-gray-700"
                )}
                aria-current={isActive ? "page" : undefined}
              >
                <item.icon size={20} />
                <span className="text-xs font-medium">{item.label}</span>
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
```

---

## 🔧 PASO 6: Dashboard Layout

```typescript
// filepath: src/components/layouts/DashboardLayout.tsx
"use client";

import * as React from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useSession } from "next-auth/react";
import {
  LayoutDashboard,
  Car,
  Heart,
  MessageSquare,
  Bell,
  Settings,
  HelpCircle,
  Menu,
  X,
  CreditCard,
  BarChart3,
} from "lucide-react";
import { Logo } from "@/components/brand/Logo";
import { Button } from "@/components/ui/Button";
import { UserMenu } from "@/components/navigation/UserMenu";
import { cn } from "@/lib/utils";

const SIDEBAR_ITEMS = [
  { href: "/dashboard", icon: LayoutDashboard, label: "Dashboard" },
  { href: "/dashboard/vehiculos", icon: Car, label: "Mis Vehículos" },
  { href: "/dashboard/favoritos", icon: Heart, label: "Favoritos" },
  { href: "/dashboard/mensajes", icon: MessageSquare, label: "Mensajes" },
  { href: "/dashboard/estadisticas", icon: BarChart3, label: "Estadísticas" },
  { href: "/dashboard/notificaciones", icon: Bell, label: "Notificaciones" },
  { href: "/dashboard/suscripcion", icon: CreditCard, label: "Suscripción" },
  { href: "/dashboard/configuracion", icon: Settings, label: "Configuración" },
  { href: "/soporte", icon: HelpCircle, label: "Ayuda" },
] as const;

interface DashboardLayoutProps {
  children: React.ReactNode;
}

export function DashboardLayout({ children }: DashboardLayoutProps) {
  const pathname = usePathname();
  const { data: session } = useSession();
  const [sidebarOpen, setSidebarOpen] = React.useState(false);

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Mobile Header */}
      <header className="sticky top-0 z-header flex h-16 items-center gap-4 border-b bg-white px-4 lg:hidden">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setSidebarOpen(true)}
          aria-label="Abrir menú"
        >
          <Menu size={20} />
        </Button>
        <Logo className="h-6" />
        <div className="ml-auto">
          {session?.user && <UserMenu user={session.user} />}
        </div>
      </header>

      {/* Sidebar Overlay (Mobile) */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-modal bg-black/50 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          "fixed inset-y-0 left-0 z-sidebar w-64 bg-white border-r border-gray-200 transition-transform lg:translate-x-0",
          sidebarOpen ? "translate-x-0" : "-translate-x-full"
        )}
      >
        {/* Sidebar Header */}
        <div className="flex h-16 items-center justify-between px-4 border-b">
          <Link href="/">
            <Logo className="h-6" />
          </Link>
          <Button
            variant="ghost"
            size="icon"
            className="lg:hidden"
            onClick={() => setSidebarOpen(false)}
            aria-label="Cerrar menú"
          >
            <X size={20} />
          </Button>
        </div>

        {/* Navigation */}
        <nav className="p-4" aria-label="Dashboard navigation">
          <ul className="space-y-1">
            {SIDEBAR_ITEMS.map((item) => {
              const isActive =
                item.href === "/dashboard"
                  ? pathname === "/dashboard"
                  : pathname.startsWith(item.href);

              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    onClick={() => setSidebarOpen(false)}
                    className={cn(
                      "flex items-center gap-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors",
                      isActive
                        ? "bg-primary-50 text-primary-700"
                        : "text-gray-700 hover:bg-gray-100"
                    )}
                    aria-current={isActive ? "page" : undefined}
                  >
                    <item.icon size={18} />
                    {item.label}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>

        {/* User Info (Desktop) */}
        <div className="absolute bottom-0 left-0 right-0 p-4 border-t hidden lg:block">
          {session?.user && (
            <div className="flex items-center gap-3">
              <UserMenu user={session.user} />
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium truncate">
                  {session.user.name}
                </p>
                <p className="text-xs text-gray-500 truncate">
                  {session.user.email}
                </p>
              </div>
            </div>
          )}
        </div>
      </aside>

      {/* Main Content */}
      <main
        id="main-content"
        className="lg:pl-64 min-h-screen"
      >
        <div className="p-4 md:p-6 lg:p-8">
          {children}
        </div>
      </main>
    </div>
  );
}
```

---

## 🔧 PASO 7: Skip Link y Breadcrumbs

### Skip Link

```typescript
// filepath: src/components/ui/SkipLink.tsx
export function SkipLink() {
  return (
    <a
      href="#main-content"
      className="sr-only focus:not-sr-only focus:fixed focus:top-4 focus:left-4 focus:z-[100] focus:bg-primary-600 focus:text-white focus:px-4 focus:py-2 focus:rounded-md focus:outline-none"
    >
      Saltar al contenido principal
    </a>
  );
}
```

### Breadcrumbs

```typescript
// filepath: src/components/navigation/Breadcrumbs.tsx
import Link from "next/link";
import { ChevronRight, Home } from "lucide-react";
import { cn } from "@/lib/utils";

export interface BreadcrumbItem {
  label: string;
  href?: string;
}

interface BreadcrumbsProps {
  items: BreadcrumbItem[];
  className?: string;
}

export function Breadcrumbs({ items, className }: BreadcrumbsProps) {
  return (
    <nav
      aria-label="Breadcrumb"
      className={cn("text-sm text-gray-600", className)}
    >
      <ol className="flex items-center flex-wrap gap-1">
        <li>
          <Link
            href="/"
            className="flex items-center hover:text-primary-600 transition-colors"
            aria-label="Inicio"
          >
            <Home size={14} />
          </Link>
        </li>

        {items.map((item, index) => {
          const isLast = index === items.length - 1;

          return (
            <li key={index} className="flex items-center gap-1">
              <ChevronRight size={14} className="text-gray-400" />
              {item.href && !isLast ? (
                <Link
                  href={item.href}
                  className="hover:text-primary-600 transition-colors"
                >
                  {item.label}
                </Link>
              ) : (
                <span
                  className={cn(isLast && "text-gray-900 font-medium")}
                  aria-current={isLast ? "page" : undefined}
                >
                  {item.label}
                </span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
```

---

## ✅ VALIDACIÓN

### Tests de Layout

```typescript
// filepath: __tests__/components/layouts/MainLayout.test.tsx
import { describe, it, expect } from "vitest";
import { screen } from "@testing-library/react";
import { render } from "@tests/utils/test-utils";
import { MainLayout } from "@/components/layouts/MainLayout";

describe("MainLayout", () => {
  it("renders header and footer", () => {
    render(
      <MainLayout>
        <div>Content</div>
      </MainLayout>
    );

    expect(screen.getByRole("banner")).toBeInTheDocument(); // header
    expect(screen.getByRole("contentinfo")).toBeInTheDocument(); // footer
    expect(screen.getByRole("main")).toBeInTheDocument();
  });

  it("hides footer when showFooter is false", () => {
    render(
      <MainLayout showFooter={false}>
        <div>Content</div>
      </MainLayout>
    );

    expect(screen.queryByRole("contentinfo")).not.toBeInTheDocument();
  });
});
```

### Ejecutar verificación

```bash
# Tests
pnpm test components/layouts

# Verificar rendering
pnpm dev
# Navegar a localhost:3000 y verificar:
# - Header visible con logo y navegación
# - Footer con links y redes sociales
# - Mobile nav en viewport pequeño
# - Responsive design
```

---

## 📊 RESUMEN

| Componente      | Archivo                       | Función                    |
| --------------- | ----------------------------- | -------------------------- |
| RootLayout      | `app/layout.tsx`              | Metadata, fonts, providers |
| MainLayout      | `layouts/MainLayout.tsx`      | Header + Footer + Main     |
| DashboardLayout | `layouts/DashboardLayout.tsx` | Sidebar + Main             |
| Header          | `navigation/Header.tsx`       | Navbar responsive          |
| Footer          | `navigation/Footer.tsx`       | Links + Contact            |
| MobileNav       | `navigation/MobileNav.tsx`    | Bottom navigation          |
| UserMenu        | `navigation/UserMenu.tsx`     | Dropdown del usuario       |
| Breadcrumbs     | `navigation/Breadcrumbs.tsx`  | Navegación secundaria      |
| SkipLink        | `ui/SkipLink.tsx`             | Accesibilidad              |

---

## ➡️ SIGUIENTE PASO

Continuar con: `docs/frontend-rebuild/03-COMPONENTES/02-formularios.md`

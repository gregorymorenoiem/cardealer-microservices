'use client';

import * as React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { motion, AnimatePresence } from 'framer-motion';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import {
  Menu,
  X,
  Heart,
  User,
  Car,
  Bell,
  MessageSquare,
  LogOut,
  Plus,
  Building2,
} from 'lucide-react';

// =============================================================================
// TYPES
// =============================================================================

interface NavLink {
  href: string;
  label: string;
  icon?: React.ReactNode;
}

// =============================================================================
// NAVIGATION DATA
// =============================================================================

/**
 * Main navigation links - Optimized for República Dominicana market
 * Structure: Comprar | Vender | Dealers
 * - Simple, clear actions for buyers and sellers
 * - Dealers link for B2B monetization
 * - Search removed from navbar (exists in hero section)
 */
const mainNavLinks: NavLink[] = [
  { href: '/vehiculos', label: 'Comprar', icon: <Car className="h-4 w-4" /> },
  { href: '/vender', label: 'Vender', icon: <Plus className="h-4 w-4" /> },
  { href: '/dealers', label: 'Dealers', icon: <Building2 className="h-4 w-4" /> },
];

const userNavLinks: NavLink[] = [
  { href: '/cuenta/favoritos', label: 'Favoritos', icon: <Heart className="h-4 w-4" /> },
  { href: '/cuenta/mensajes', label: 'Mensajes', icon: <MessageSquare className="h-4 w-4" /> },
  { href: '/cuenta/notificaciones', label: 'Alertas', icon: <Bell className="h-4 w-4" /> },
];

// =============================================================================
// LOGO COMPONENT
// =============================================================================

function Logo() {
  return (
    <Link href="/" className="group flex items-center gap-2.5">
      <div className="relative">
        <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-[#00A870] to-[#009663] shadow-lg shadow-[#00A870]/20 transition-all group-hover:shadow-xl group-hover:shadow-[#00A870]/30">
          <span className="text-xl font-bold text-white">O</span>
        </div>
        {/* Subtle glow effect */}
        <div className="absolute inset-0 rounded-xl bg-[#00A870] opacity-0 blur-lg transition-opacity group-hover:opacity-30" />
      </div>
      <span className="text-2xl font-bold tracking-tight text-slate-900">OKLA</span>
    </Link>
  );
}

// =============================================================================
// DESKTOP NAVIGATION
// =============================================================================

interface DesktopNavProps {
  pathname: string;
  isAuthenticated: boolean;
}

function DesktopNav({ pathname, isAuthenticated }: DesktopNavProps) {
  return (
    <div className="hidden items-center gap-1 lg:flex">
      {/* Main Links - Comprar | Vender | Dealers */}
      {mainNavLinks.map(link => (
        <Link
          key={link.href}
          href={link.href}
          className={cn(
            'relative flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium tracking-wide transition-colors',
            pathname === link.href || pathname.startsWith(link.href)
              ? 'text-[#00A870]'
              : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
          )}
        >
          {link.icon}
          {link.label}
          {(pathname === link.href || pathname.startsWith(link.href)) && (
            <motion.div
              layoutId="navbar-indicator"
              className="absolute right-2 bottom-0 left-2 h-0.5 rounded-full bg-[#00A870]"
              transition={{ type: 'spring', bounce: 0.2, duration: 0.6 }}
            />
          )}
        </Link>
      ))}

      {/* User Links when authenticated */}
      {isAuthenticated && (
        <>
          <div className="mx-2 h-6 w-px bg-slate-200" />
          {userNavLinks.map(link => (
            <Link
              key={link.href}
              href={link.href}
              className={cn(
                'flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                pathname === link.href
                  ? 'bg-[#00A870]/5 text-[#00A870]'
                  : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900'
              )}
            >
              {link.icon}
            </Link>
          ))}
        </>
      )}
    </div>
  );
}

// =============================================================================
// RIGHT SIDE ACTIONS
// =============================================================================

interface RightActionsProps {
  isAuthenticated: boolean;
  onMobileMenuToggle: () => void;
  isMobileMenuOpen: boolean;
}

function RightActions({
  isAuthenticated,
  onMobileMenuToggle,
  isMobileMenuOpen,
}: RightActionsProps) {
  const [isUserMenuOpen, setIsUserMenuOpen] = React.useState(false);
  const userMenuRef = React.useRef<HTMLDivElement>(null);

  // Close dropdown when clicking outside
  React.useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (userMenuRef.current && !userMenuRef.current.contains(event.target as Node)) {
        setIsUserMenuOpen(false);
      }
    }
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="flex items-center gap-2 sm:gap-3">
      {/* Auth Button - OKLA Theme (only when not authenticated) */}
      {!isAuthenticated && (
        <Button
          asChild
          size="sm"
          className="hidden gap-1.5 rounded-full bg-[#00A870] px-5 py-2.5 text-sm font-semibold shadow-md transition-all hover:bg-[#009663] hover:shadow-lg sm:flex"
        >
          <Link href="/login">
            <span>Iniciar Sesión / Registrarse</span>
          </Link>
        </Button>
      )}

      {/* User Menu with Dropdown - Desktop (only when authenticated) */}
      {isAuthenticated && (
        <div className="relative hidden lg:block" ref={userMenuRef}>
          <button
            onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
            className={cn(
              'flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all',
              isUserMenuOpen
                ? 'border-[#00A870] bg-[#00A870]/5'
                : 'border-gray-200 bg-white hover:border-gray-300 hover:bg-gray-50'
            )}
            aria-label="Menú de usuario"
            aria-expanded={isUserMenuOpen}
          >
            <User className={cn('h-5 w-5', isUserMenuOpen ? 'text-[#00A870]' : 'text-gray-600')} />
          </button>

          {/* Dropdown Menu */}
          <AnimatePresence>
            {isUserMenuOpen && (
              <motion.div
                initial={{ opacity: 0, scale: 0.95, y: -10 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.95, y: -10 }}
                transition={{ duration: 0.15 }}
                className="absolute top-full right-0 z-50 mt-2 w-56 origin-top-right rounded-xl border border-gray-100 bg-white p-2 shadow-xl"
              >
                {/* Authenticated Menu */}
                <div className="mb-2 border-b border-gray-100 px-3 pb-3">
                  <p className="text-sm font-semibold text-gray-900">Mi Cuenta</p>
                  <p className="text-xs text-gray-500">usuario@email.com</p>
                </div>
                <Link
                  href="/cuenta"
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
                  onClick={() => setIsUserMenuOpen(false)}
                >
                  <User className="h-4 w-4 text-gray-400" />
                  Mi Perfil
                </Link>
                <Link
                  href="/cuenta/mis-vehiculos"
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
                  onClick={() => setIsUserMenuOpen(false)}
                >
                  <Car className="h-4 w-4 text-gray-400" />
                  Mis Vehículos
                </Link>
                <Link
                  href="/cuenta/favoritos"
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
                  onClick={() => setIsUserMenuOpen(false)}
                >
                  <Heart className="h-4 w-4 text-gray-400" />
                  Favoritos
                </Link>
                <Link
                  href="/cuenta/mensajes"
                  className="flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50"
                  onClick={() => setIsUserMenuOpen(false)}
                >
                  <MessageSquare className="h-4 w-4 text-gray-400" />
                  Mensajes
                </Link>
                <div className="my-2 border-t border-gray-100" />
                <button
                  className="flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium text-red-600 transition-colors hover:bg-red-50"
                  onClick={() => setIsUserMenuOpen(false)}
                >
                  <LogOut className="h-4 w-4" />
                  Cerrar Sesión
                </button>
              </motion.div>
            )}
          </AnimatePresence>
        </div>
      )}

      {/* Mobile Menu Button */}
      <button
        className="flex h-10 w-10 items-center justify-center rounded-xl text-gray-600 transition-colors hover:bg-gray-100 lg:hidden"
        onClick={onMobileMenuToggle}
        aria-label={isMobileMenuOpen ? 'Cerrar menú' : 'Abrir menú'}
        aria-expanded={isMobileMenuOpen}
      >
        <AnimatePresence mode="wait">
          {isMobileMenuOpen ? (
            <motion.div
              key="close"
              initial={{ rotate: -90, opacity: 0 }}
              animate={{ rotate: 0, opacity: 1 }}
              exit={{ rotate: 90, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              <X className="h-6 w-6" />
            </motion.div>
          ) : (
            <motion.div
              key="menu"
              initial={{ rotate: 90, opacity: 0 }}
              animate={{ rotate: 0, opacity: 1 }}
              exit={{ rotate: -90, opacity: 0 }}
              transition={{ duration: 0.2 }}
            >
              <Menu className="h-6 w-6" />
            </motion.div>
          )}
        </AnimatePresence>
      </button>
    </div>
  );
}

// =============================================================================
// MOBILE MENU
// =============================================================================

interface MobileMenuProps {
  isOpen: boolean;
  pathname: string;
  isAuthenticated: boolean;
}

function MobileMenu({ isOpen, pathname, isAuthenticated }: MobileMenuProps) {
  return (
    <AnimatePresence>
      {isOpen && (
        <motion.div
          initial={{ opacity: 0, height: 0 }}
          animate={{ opacity: 1, height: 'auto' }}
          exit={{ opacity: 0, height: 0 }}
          transition={{ duration: 0.3, ease: 'easeInOut' }}
          className="overflow-hidden border-t border-gray-100 bg-white lg:hidden"
        >
          <div className="space-y-2 px-4 py-4">
            {/* Main Navigation - Comprar | Vender | Dealers */}
            {mainNavLinks.map(link => (
              <Link
                key={link.href}
                href={link.href}
                className={cn(
                  'flex items-center gap-3 rounded-xl px-4 py-3 text-base font-medium transition-colors',
                  pathname === link.href || pathname.startsWith(link.href)
                    ? 'bg-[#00A870]/10 text-[#00A870]'
                    : 'text-gray-700 hover:bg-gray-50'
                )}
              >
                {link.icon}
                {link.label}
              </Link>
            ))}

            {/* User Links */}
            {isAuthenticated && (
              <>
                <div className="my-3 border-t border-gray-100" />
                {userNavLinks.map(link => (
                  <Link
                    key={link.href}
                    href={link.href}
                    className={cn(
                      'flex items-center gap-3 rounded-xl px-4 py-3 text-base font-medium transition-colors',
                      pathname === link.href
                        ? 'bg-[#00A870]/10 text-[#00A870]'
                        : 'text-gray-700 hover:bg-gray-50'
                    )}
                  >
                    {link.icon}
                    {link.label}
                  </Link>
                ))}
              </>
            )}

            {/* Divider */}
            <div className="my-3 border-t border-gray-100" />

            {/* Auth Section */}
            {!isAuthenticated && (
              <div className="grid grid-cols-2 gap-3 pt-2">
                <Link
                  href="/login"
                  className="flex items-center justify-center rounded-xl border border-gray-200 px-4 py-3 font-medium text-gray-700 hover:bg-gray-50"
                >
                  Iniciar Sesión
                </Link>
                <Link
                  href="/registro"
                  className="flex items-center justify-center rounded-xl bg-gray-900 px-4 py-3 font-medium text-white hover:bg-gray-800"
                >
                  Registrarse
                </Link>
              </div>
            )}

            {isAuthenticated && (
              <button className="flex w-full items-center justify-center gap-2 rounded-xl px-4 py-3 font-medium text-red-600 transition-colors hover:bg-red-50">
                <LogOut className="h-5 w-5" />
                Cerrar Sesión
              </button>
            )}
          </div>
        </motion.div>
      )}
    </AnimatePresence>
  );
}

// =============================================================================
// MAIN NAVBAR COMPONENT
// =============================================================================

export function Navbar() {
  const pathname = usePathname();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = React.useState(false);
  const [isScrolled, setIsScrolled] = React.useState(false);

  // TODO: Replace with actual auth state
  const isAuthenticated = false;

  // Handle scroll effect
  React.useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 10);
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  // Close mobile menu on route change
  React.useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [pathname]);

  // Prevent body scroll when mobile menu is open
  React.useEffect(() => {
    if (isMobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [isMobileMenuOpen]);

  return (
    <header
      className={cn(
        'sticky top-0 z-50 w-full transition-all duration-300',
        isScrolled ? 'border-b border-gray-100 bg-white/95 shadow-sm backdrop-blur-md' : 'bg-white'
      )}
    >
      <nav className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:h-18 lg:px-8">
        {/* Left - Logo + Navigation together */}
        <div className="flex items-center gap-8">
          <Logo />
          <DesktopNav pathname={pathname} isAuthenticated={isAuthenticated} />
        </div>

        {/* Right - Actions */}
        <RightActions
          isAuthenticated={isAuthenticated}
          onMobileMenuToggle={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
          isMobileMenuOpen={isMobileMenuOpen}
        />
      </nav>

      <MobileMenu isOpen={isMobileMenuOpen} pathname={pathname} isAuthenticated={isAuthenticated} />
    </header>
  );
}

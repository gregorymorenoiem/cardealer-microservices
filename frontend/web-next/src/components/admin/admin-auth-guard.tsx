/**
 * Admin Auth Guard
 *
 * Client component that protects admin routes.
 * Verifies user is authenticated AND has admin role.
 * Includes admin-specific header and sidebar.
 */

'use client';

import * as React from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import {
  Loader2,
  ShieldAlert,
  LogOut,
  Bell,
  Menu,
  X,
  ChevronDown,
  Home,
  Search,
  PanelLeftClose,
  PanelLeft,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useAuth } from '@/hooks/use-auth';
import { useSiteConfig } from '@/providers/site-config-provider';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip';
import {
  ADMIN_MAIN_NAVIGATION,
  ADMIN_MANAGEMENT_NAVIGATION,
  ADMIN_SYSTEM_NAVIGATION,
} from '@/config/navigation';

// =============================================================================
// BADGE TYPES & COLORS
// =============================================================================

type BadgeType = 'urgent' | 'warning' | 'info';

interface BadgeConfig {
  count: number;
  type: BadgeType;
}

const badgeColors: Record<BadgeType, string> = {
  urgent: 'bg-red-500',
  warning: 'bg-amber-500',
  info: 'bg-blue-500',
};

// =============================================================================
// SIDEBAR COLLAPSED STATE PERSISTENCE
// =============================================================================

const SIDEBAR_COLLAPSED_KEY = 'okla_admin_sidebar_collapsed';

function getSidebarCollapsed(): boolean {
  if (typeof window === 'undefined') return false;
  return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === 'true';
}

function setSidebarCollapsed(collapsed: boolean): void {
  if (typeof window === 'undefined') return;
  localStorage.setItem(SIDEBAR_COLLAPSED_KEY, collapsed ? 'true' : 'false');
}

// =============================================================================
// LOADING STATE
// =============================================================================

function AdminLoadingState() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-900">
      <div className="text-center">
        <Loader2 className="mx-auto h-12 w-12 animate-spin text-primary" />
        <p className="mt-4 text-slate-300">Verificando permisos...</p>
      </div>
    </div>
  );
}

// =============================================================================
// ACCESS DENIED STATE
// =============================================================================

function AdminAccessDenied() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-900">
      <div className="mx-4 max-w-md text-center">
        <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-red-500/10">
          <ShieldAlert className="h-10 w-10 text-red-500" />
        </div>
        <h1 className="mb-2 text-2xl font-bold text-white">Acceso Denegado</h1>
        <p className="mb-6 text-slate-400">
          No tienes permisos para acceder al panel de administración. Esta área está restringida
          solo para administradores.
        </p>
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <Link href="/">
            <Button variant="outline" className="w-full sm:w-auto">
              Ir al Inicio
            </Button>
          </Link>
          <Link href="/cuenta">
            <Button className="w-full bg-primary hover:bg-primary/90 sm:w-auto">
              Mi Cuenta
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// ADMIN HEADER
// =============================================================================

interface AdminHeaderProps {
  user: {
    firstName?: string;
    lastName?: string;
    email: string;
  };
  onLogout: () => void;
  onMenuToggle: () => void;
  isMobileMenuOpen: boolean;
}

function AdminHeader({ user, onLogout, onMenuToggle, isMobileMenuOpen }: AdminHeaderProps) {
  const siteConfig = useSiteConfig();
  const initials =
    user.firstName && user.lastName
      ? `${user.firstName[0]}${user.lastName[0]}`
      : user.email[0].toUpperCase();

  const displayName =
    user.firstName && user.lastName ? `${user.firstName} ${user.lastName}` : user.email;

  return (
    <header className="bg-card sticky top-0 z-50 h-16 shadow-[0_1px_3px_rgba(0,0,0,0.05)]">
      <div className="flex h-full items-center justify-between px-4">
        {/* Left section */}
        <div className="flex items-center gap-4">
          {/* Mobile menu button */}
          <button
            type="button"
            onClick={onMenuToggle}
            className="text-muted-foreground hover:text-foreground lg:hidden"
          >
            {isMobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
          </button>

          {/* Logo */}
          <Link href="/admin" className="flex items-center gap-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary/100">
              <span className="text-lg font-bold text-white">{siteConfig.siteName.charAt(0)}</span>
            </div>
            <div className="hidden sm:block">
              <span className="text-xl font-bold">{siteConfig.siteName}</span>
              <span className="text-muted-foreground ml-2 text-sm">Admin</span>
            </div>
          </Link>
        </div>

        {/* Center - Search */}
        <div className="mx-8 hidden max-w-md flex-1 md:block">
          <div className="relative">
            <Search className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
            <input
              type="text"
              placeholder="Buscar usuarios, vehículos, dealers..."
              className="bg-muted/50 focus:bg-muted placeholder:text-muted-foreground h-9 w-full rounded-lg border-0 pr-4 pl-10 text-sm focus:ring-2 focus:ring-primary/20 focus:outline-none"
            />
          </div>
        </div>

        {/* Right section */}
        <div className="flex items-center gap-2">
          {/* Home link */}
          <Link href="/">
            <Button
              variant="ghost"
              size="icon"
              className="text-muted-foreground hover:text-foreground"
            >
              <Home className="h-5 w-5" />
            </Button>
          </Link>

          {/* Notifications */}
          <Button
            variant="ghost"
            size="icon"
            className="text-muted-foreground hover:text-foreground relative"
          >
            <Bell className="h-5 w-5" />
            {/* Badge will be shown when there are real notifications from API */}
          </Button>

          {/* User dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="flex items-center gap-2 pr-3 pl-2">
                <Avatar className="h-8 w-8">
                  <AvatarFallback className="bg-primary/100 text-sm text-white">
                    {initials}
                  </AvatarFallback>
                </Avatar>
                <span className="hidden text-sm font-medium sm:block">{displayName}</span>
                <ChevronDown className="text-muted-foreground h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuLabel>
                <div className="flex flex-col">
                  <span>{displayName}</span>
                  <span className="text-muted-foreground text-xs font-normal">{user.email}</span>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem asChild>
                <Link href="/cuenta/perfil">Mi Perfil</Link>
              </DropdownMenuItem>
              <DropdownMenuItem asChild>
                <Link href="/admin/configuracion">Configuración</Link>
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={onLogout} className="text-red-600 focus:text-red-600">
                <LogOut className="mr-2 h-4 w-4" />
                Cerrar Sesión
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      </div>
    </header>
  );
}

// =============================================================================
// SIDEBAR COMPONENTS
// =============================================================================

interface SidebarLinkProps {
  href: string;
  label: string;
  icon: React.ElementType;
  isActive: boolean;
  badge?: BadgeConfig;
  isCollapsed?: boolean;
}

function SidebarLink({ href, label, icon: Icon, isActive, badge, isCollapsed }: SidebarLinkProps) {
  const linkContent = (
    <Link
      href={href}
      className={cn(
        'group relative flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200',
        isActive
          ? 'bg-primary/100/10 text-primary dark:text-primary/80'
          : 'text-muted-foreground hover:bg-muted hover:text-foreground',
        isCollapsed && 'justify-center px-2'
      )}
    >
      {/* Active indicator bar */}
      {isActive && (
        <span className="absolute top-1/2 left-0 h-6 w-1 -translate-y-1/2 rounded-r-full bg-primary/100" />
      )}

      <Icon
        className={cn(
          'h-5 w-5 flex-shrink-0 transition-colors duration-200',
          isActive
            ? 'text-primary dark:text-primary/80'
            : 'text-muted-foreground group-hover:text-foreground'
        )}
      />

      {!isCollapsed && (
        <>
          <span className="flex-1">{label}</span>

          {/* Badge with type-based colors */}
          {badge && badge.count > 0 && (
            <span
              className={cn(
                'flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[10px] font-bold text-white',
                badgeColors[badge.type]
              )}
            >
              {badge.count > 99 ? '99+' : badge.count}
            </span>
          )}
        </>
      )}

      {/* Badge indicator when collapsed */}
      {isCollapsed && badge && badge.count > 0 && (
        <span
          className={cn(
            'absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full text-[8px] font-bold text-white',
            badgeColors[badge.type]
          )}
        >
          {badge.count > 9 ? '9+' : badge.count}
        </span>
      )}
    </Link>
  );

  if (isCollapsed) {
    return (
      <Tooltip delayDuration={0}>
        <TooltipTrigger asChild>{linkContent}</TooltipTrigger>
        <TooltipContent side="right" className="flex items-center gap-2">
          {label}
          {badge && badge.count > 0 && (
            <span
              className={cn(
                'flex h-5 min-w-5 items-center justify-center rounded-full px-1.5 text-[10px] font-bold text-white',
                badgeColors[badge.type]
              )}
            >
              {badge.count}
            </span>
          )}
        </TooltipContent>
      </Tooltip>
    );
  }

  return linkContent;
}

interface SidebarSectionProps {
  title: string;
  children: React.ReactNode;
  isCollapsed?: boolean;
}

function SidebarSection({ title, children, isCollapsed }: SidebarSectionProps) {
  return (
    <div className="mt-6 first:mt-0">
      {!isCollapsed && (
        <p className="text-muted-foreground/70 mb-2 px-3 text-[11px] font-semibold tracking-wider uppercase">
          {title}
        </p>
      )}
      {isCollapsed && <div className="bg-border mx-2 my-3 h-px" />}
      <div className="space-y-1">{children}</div>
    </div>
  );
}

// =============================================================================
// ADMIN LAYOUT CONTENT
// =============================================================================

interface AdminLayoutContentProps {
  children: React.ReactNode;
}

function AdminLayoutContent({ children }: AdminLayoutContentProps) {
  const { user, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = React.useState(false);
  const [isCollapsed, setIsCollapsed] = React.useState(false);

  // Load collapsed state from localStorage on mount
  React.useEffect(() => {
    setIsCollapsed(getSidebarCollapsed());
  }, []);

  const handleToggleCollapse = () => {
    const newState = !isCollapsed;
    setIsCollapsed(newState);
    setSidebarCollapsed(newState);
  };

  const handleLogout = async () => {
    await logout();
    router.push('/login');
  };

  // Check if a path is active
  const isActivePath = (href: string) => {
    if (href === '/admin') {
      return pathname === '/admin';
    }
    return pathname.startsWith(href);
  };

  // Close mobile menu on route change
  React.useEffect(() => {
    setIsMobileMenuOpen(false);
  }, [pathname]);

  const sidebarContent = (
    <TooltipProvider>
      {/* Main Navigation */}
      <SidebarSection title="Principal" isCollapsed={isCollapsed}>
        {ADMIN_MAIN_NAVIGATION.map(link => (
          <SidebarLink
            key={link.href}
            href={link.href}
            label={link.label}
            icon={link.icon}
            isActive={isActivePath(link.href)}
            isCollapsed={isCollapsed}
          />
        ))}
      </SidebarSection>

      {/* Management Section */}
      <SidebarSection title="Gestión" isCollapsed={isCollapsed}>
        {ADMIN_MANAGEMENT_NAVIGATION.map(link => (
          <SidebarLink
            key={link.href}
            href={link.href}
            label={link.label}
            icon={link.icon}
            isActive={isActivePath(link.href)}
            isCollapsed={isCollapsed}
          />
        ))}
      </SidebarSection>

      {/* System Section */}
      <SidebarSection title="Sistema" isCollapsed={isCollapsed}>
        {ADMIN_SYSTEM_NAVIGATION.map(link => (
          <SidebarLink
            key={link.href}
            href={link.href}
            label={link.label}
            icon={link.icon}
            isActive={isActivePath(link.href)}
            isCollapsed={isCollapsed}
          />
        ))}
      </SidebarSection>

      {/* Collapse toggle button */}
      <div className="border-border mt-auto border-t pt-4">
        <Tooltip delayDuration={0}>
          <TooltipTrigger asChild>
            <button
              onClick={handleToggleCollapse}
              className={cn(
                'flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-all duration-200',
                'text-muted-foreground hover:bg-muted hover:text-foreground',
                isCollapsed && 'justify-center px-2'
              )}
            >
              {isCollapsed ? (
                <PanelLeft className="h-5 w-5" />
              ) : (
                <>
                  <PanelLeftClose className="h-5 w-5" />
                  <span>Colapsar menú</span>
                </>
              )}
            </button>
          </TooltipTrigger>
          {isCollapsed && <TooltipContent side="right">Expandir menú</TooltipContent>}
        </Tooltip>
      </div>
    </TooltipProvider>
  );

  return (
    <div className="flex min-h-screen flex-col">
      {/* Admin Header */}
      <AdminHeader
        user={{
          firstName: user?.firstName,
          lastName: user?.lastName,
          email: user?.email || '',
        }}
        onLogout={handleLogout}
        onMenuToggle={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
        isMobileMenuOpen={isMobileMenuOpen}
      />

      <div className="flex h-[calc(100vh-4rem)]">
        {/* Desktop Sidebar */}
        <aside
          className={cn(
            'bg-card hidden flex-shrink-0 shadow-[1px_0_3px_rgba(0,0,0,0.05)] transition-all duration-300 lg:block',
            isCollapsed ? 'w-16' : 'w-64'
          )}
        >
          <nav className="flex h-full flex-col px-2 py-4">{sidebarContent}</nav>
        </aside>

        {/* Mobile Sidebar Overlay */}
        {isMobileMenuOpen && (
          <div
            className="fixed inset-0 z-40 bg-black/50 lg:hidden"
            onClick={() => setIsMobileMenuOpen(false)}
          />
        )}

        {/* Mobile Sidebar - Always expanded on mobile */}
        <aside
          className={cn(
            'bg-card fixed top-16 left-0 z-50 h-[calc(100vh-4rem)] w-64 transform shadow-lg transition-transform duration-300 lg:hidden',
            isMobileMenuOpen ? 'translate-x-0' : '-translate-x-full'
          )}
        >
          <nav className="flex h-full flex-col px-3 py-4">
            <TooltipProvider>
              {/* Main Navigation */}
              <SidebarSection title="Principal">
                {ADMIN_MAIN_NAVIGATION.map(link => (
                  <SidebarLink
                    key={link.href}
                    href={link.href}
                    label={link.label}
                    icon={link.icon}
                    isActive={isActivePath(link.href)}
                  />
                ))}
              </SidebarSection>

              {/* Management Section */}
              <SidebarSection title="Gestión">
                {ADMIN_MANAGEMENT_NAVIGATION.map(link => (
                  <SidebarLink
                    key={link.href}
                    href={link.href}
                    label={link.label}
                    icon={link.icon}
                    isActive={isActivePath(link.href)}
                  />
                ))}
              </SidebarSection>

              {/* System Section */}
              <SidebarSection title="Sistema">
                {ADMIN_SYSTEM_NAVIGATION.map(link => (
                  <SidebarLink
                    key={link.href}
                    href={link.href}
                    label={link.label}
                    icon={link.icon}
                    isActive={isActivePath(link.href)}
                  />
                ))}
              </SidebarSection>
            </TooltipProvider>
          </nav>
        </aside>

        {/* Main Content - Solo este tiene scroll */}
        <main className="bg-muted/30 flex-1 overflow-y-auto">
          <div className="p-4 lg:p-6">{children}</div>
        </main>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN GUARD COMPONENT
// =============================================================================

interface AdminAuthGuardProps {
  children: React.ReactNode;
}

/**
 * Admin Auth Guard - Protects admin routes
 */
export function AdminAuthGuard({ children }: AdminAuthGuardProps) {
  const { user, isLoading, isAuthenticated } = useAuth();
  const router = useRouter();

  // Redirect to login if not authenticated
  React.useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login?redirect=/admin');
    }
  }, [isLoading, isAuthenticated, router]);

  // Show loading while checking auth
  if (isLoading) {
    return <AdminLoadingState />;
  }

  // Show loading while redirecting
  if (!isAuthenticated) {
    return <AdminLoadingState />;
  }

  // Check admin role — admin and platform_employee can access
  if (user?.accountType !== 'admin' && user?.accountType !== 'platform_employee') {
    return <AdminAccessDenied />;
  }

  // User is authenticated and is admin
  return <AdminLayoutContent>{children}</AdminLayoutContent>;
}

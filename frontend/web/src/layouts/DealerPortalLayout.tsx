/**
 * DealerPortalLayout - Portal unificado para Dealers
 *
 * Usa el mismo tema visual que HomePage y Vehículos
 * Incluye DealerNavbar + Sidebar colapsable + Contenido
 */

import type { ReactNode } from 'react';
import { useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { usePermissions } from '@/hooks/usePermissions';
import {
  FiMenu,
  FiX,
  FiHome,
  FiGrid,
  FiTruck,
  FiUsers,
  FiTarget,
  FiBarChart2,
  FiSettings,
  FiDollarSign,
  FiMessageSquare,
  FiFileText,
  FiBell,
  FiLogOut,
  FiChevronDown,
  FiChevronRight,
  FiPlusCircle,
  FiPackage,
  FiTrendingUp,
  FiCalendar,
  FiStar,
  FiSearch,
  FiHelpCircle,
  FiExternalLink,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { Crown, Building2 } from 'lucide-react';
import NotificationDropdown from '@/components/organisms/NotificationDropdown';

interface DealerPortalLayoutProps {
  children: ReactNode;
}

// Tipo para items del menú
interface MenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  href?: string;
  badge?: string | number;
  badgeColor?: string;
  children?: MenuItem[];
  requiredPlan?: string[];
}

const DealerPortalLayout = ({ children }: DealerPortalLayoutProps) => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();
  const { dealerPlan, usage, limits, getUsagePercentage, hasReachedLimit } = usePermissions();

  const [isSidebarOpen, setIsSidebarOpen] = useState(true);
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);
  const [expandedSections, setExpandedSections] = useState<string[]>(['dashboard']);
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);

  // Cerrar menús al cambiar de ruta
  useEffect(() => {
    setIsMobileSidebarOpen(false);
    setIsUserMenuOpen(false);
  }, [location.pathname]);

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  // Menú principal del dealer
  const menuItems: MenuItem[] = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: FiGrid,
      children: [
        { id: 'overview', label: 'Resumen General', icon: FiHome, href: '/dealer/dashboard' },
        { id: 'sales', label: 'Ventas del Mes', icon: FiTrendingUp, href: '/dealer/sales' },
        {
          id: 'alerts',
          label: 'Alertas',
          icon: FiBell,
          href: '/dealer/alerts',
          badge: 3,
          badgeColor: 'bg-red-500',
        },
      ],
    },
    {
      id: 'inventory',
      label: 'Inventario',
      icon: FaCar,
      children: [
        { id: 'vehicles', label: 'Mis Vehículos', icon: FaCar, href: '/dealer/inventory' },
        {
          id: 'add-vehicle',
          label: 'Agregar Vehículo',
          icon: FiPlusCircle,
          href: '/dealer/inventory/new',
        },
        { id: 'listings', label: 'Publicaciones', icon: FiPackage, href: '/dealer/listings' },
      ],
    },
    {
      id: 'leads',
      label: 'Leads & CRM',
      icon: FiTarget,
      children: [
        { id: 'inquiries', label: 'Consultas', icon: FiMessageSquare, href: '/dealer/inquiries' },
        {
          id: 'crm',
          label: 'Pipeline de Ventas',
          icon: FiTrendingUp,
          href: '/dealer/crm',
          requiredPlan: ['pro', 'enterprise'],
        },
      ],
    },
    {
      id: 'analytics',
      label: 'Analytics',
      icon: FiBarChart2,
      children: [
        {
          id: 'basic-analytics',
          label: 'Estadísticas',
          icon: FiBarChart2,
          href: '/dealer/analytics',
        },
        {
          id: 'advanced',
          label: 'Avanzado',
          icon: FiTrendingUp,
          href: '/dealer/analytics/advanced',
          requiredPlan: ['pro', 'enterprise'],
        },
        {
          id: 'market-analysis',
          label: 'Análisis de Mercado',
          icon: FiTarget,
          href: '/dealer/market-analysis',
          requiredPlan: ['pro', 'enterprise'],
        },
      ],
    },
    {
      id: 'billing',
      label: 'Facturación',
      icon: FiDollarSign,
      children: [
        { id: 'invoices', label: 'Facturas', icon: FiFileText, href: '/dealer/billing' },
        { id: 'plans', label: 'Mi Plan', icon: Crown, href: '/dealer/plans' },
      ],
    },
    {
      id: 'settings',
      label: 'Configuración',
      icon: FiSettings,
      children: [
        { id: 'profile', label: 'Perfil Público', icon: Building2, href: '/dealer/profile/edit' },
        { id: 'settings', label: 'Configuración', icon: FiSettings, href: '/dealer/settings' },
      ],
    },
  ];

  const toggleSection = (sectionId: string) => {
    setExpandedSections((prev) =>
      prev.includes(sectionId) ? prev.filter((id) => id !== sectionId) : [...prev, sectionId]
    );
  };

  const isActive = (href: string) =>
    location.pathname === href || location.pathname.startsWith(href + '/');

  const isSectionActive = (item: MenuItem) => {
    if (item.href && isActive(item.href)) return true;
    if (item.children) {
      return item.children.some((child) => child.href && isActive(child.href));
    }
    return false;
  };

  const getPlanBadgeColor = () => {
    switch (dealerPlan) {
      case 'enterprise':
        return 'bg-gradient-to-r from-purple-500 to-pink-500 text-white';
      case 'pro':
        return 'bg-gradient-to-r from-blue-500 to-cyan-500 text-white';
      case 'basic':
        return 'bg-gradient-to-r from-emerald-500 to-teal-500 text-white';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  const renderMenuItem = (item: MenuItem, isChild = false) => {
    const Icon = item.icon;
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedSections.includes(item.id);
    const active = item.href ? isActive(item.href) : false;
    const sectionActive = isSectionActive(item);

    // Verificar si el item requiere un plan específico
    const isLocked = item.requiredPlan && dealerPlan && !item.requiredPlan.includes(dealerPlan);

    if (hasChildren) {
      return (
        <div key={item.id} className="space-y-1">
          <button
            onClick={() => toggleSection(item.id)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl transition-all duration-200 ${
              sectionActive
                ? 'bg-blue-50 text-blue-700'
                : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
            }`}
          >
            <Icon className="w-5 h-5" />
            {isSidebarOpen && (
              <>
                <span className="flex-1 font-medium text-left text-sm">{item.label}</span>
                {item.badge && (
                  <span
                    className={`px-2 py-0.5 text-xs rounded-full text-white ${item.badgeColor || 'bg-blue-500'}`}
                  >
                    {item.badge}
                  </span>
                )}
                {isExpanded ? (
                  <FiChevronDown className="w-4 h-4 text-gray-400" />
                ) : (
                  <FiChevronRight className="w-4 h-4 text-gray-400" />
                )}
              </>
            )}
          </button>

          {isExpanded && isSidebarOpen && (
            <div className="ml-3 pl-4 border-l-2 border-gray-100 space-y-0.5">
              {item.children!.map((child) => renderMenuItem(child, true))}
            </div>
          )}
        </div>
      );
    }

    if (isLocked) {
      return (
        <div
          key={item.id}
          className={`flex items-center gap-3 px-3 py-2 rounded-lg text-gray-400 cursor-not-allowed ${isChild ? 'text-sm' : ''}`}
          title="Disponible en planes superiores"
        >
          <Icon className="w-4 h-4" />
          {isSidebarOpen && (
            <>
              <span className="flex-1">{item.label}</span>
              <Crown className="w-4 h-4 text-amber-400" />
            </>
          )}
        </div>
      );
    }

    return (
      <Link
        key={item.id}
        to={item.href || '#'}
        className={`flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-200 ${isChild ? 'text-sm' : ''} ${
          active
            ? 'bg-gradient-to-r from-blue-600 to-blue-500 text-white shadow-lg shadow-blue-600/30'
            : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'
        }`}
      >
        <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
        {isSidebarOpen && (
          <>
            <span className="flex-1">{item.label}</span>
            {item.badge && (
              <span
                className={`px-2 py-0.5 text-xs rounded-full text-white ${item.badgeColor || 'bg-blue-500'}`}
              >
                {item.badge}
              </span>
            )}
          </>
        )}
      </Link>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* ==================== TOP NAVBAR ==================== */}
      <nav className="fixed top-0 left-0 right-0 z-50 bg-white border-b border-gray-200 shadow-sm">
        <div className="flex items-center justify-between h-16 px-4 lg:px-6">
          {/* Left: Logo + Toggle + Breadcrumb */}
          <div className="flex items-center gap-4">
            {/* Mobile menu toggle */}
            <button
              onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
              className="lg:hidden p-2 rounded-xl hover:bg-gray-100"
            >
              {isMobileSidebarOpen ? <FiX className="w-6 h-6" /> : <FiMenu className="w-6 h-6" />}
            </button>

            {/* Desktop sidebar toggle */}
            <button
              onClick={() => setIsSidebarOpen(!isSidebarOpen)}
              className="hidden lg:flex p-2 rounded-xl hover:bg-gray-100"
            >
              <FiMenu className="w-5 h-5 text-gray-600" />
            </button>

            {/* Logo */}
            <Link to="/" className="flex items-center gap-3 group">
              <div className="w-10 h-10 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-xl flex items-center justify-center shadow-lg group-hover:shadow-xl group-hover:scale-105 transition-all duration-300">
                <span className="text-white font-bold text-xl">O</span>
              </div>
              <div className="hidden sm:flex flex-col">
                <span className="text-xl font-bold bg-gradient-to-r from-gray-900 via-gray-800 to-gray-700 bg-clip-text text-transparent leading-tight">
                  Okla
                </span>
                <span className="text-[10px] text-emerald-600 -mt-0.5 tracking-widest uppercase font-semibold">
                  Portal Dealer
                </span>
              </div>
            </Link>

            {/* Plan Badge */}
            <div
              className={`hidden md:flex items-center gap-1.5 px-3 py-1.5 rounded-full text-xs font-semibold shadow-sm ${getPlanBadgeColor()}`}
            >
              <Crown className="w-3.5 h-3.5" />
              <span className="capitalize">{dealerPlan || 'Free'}</span>
            </div>
          </div>

          {/* Center: Quick Search */}
          <div className="hidden lg:flex flex-1 max-w-md mx-8">
            <div className="relative w-full">
              <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar vehículos, leads, facturas..."
                className="w-full pl-10 pr-4 py-2.5 bg-gray-50 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400 transition-all"
              />
            </div>
          </div>

          {/* Right: Actions + User */}
          <div className="flex items-center gap-3">
            {/* Quick Actions */}
            <Link
              to="/dealer/inventory/new"
              className="hidden sm:flex items-center gap-2 px-4 py-2.5 bg-gradient-to-r from-emerald-500 via-emerald-600 to-teal-500 text-white rounded-xl font-semibold text-sm shadow-lg hover:scale-105 transition-all duration-200"
            >
              <FiPlusCircle className="w-4 h-4" />
              <span>Publicar</span>
            </Link>

            {/* Notifications */}
            <NotificationDropdown />

            {/* Help */}
            <button className="hidden sm:flex p-2.5 rounded-xl hover:bg-gray-100 transition-colors">
              <FiHelpCircle className="w-5 h-5 text-gray-600" />
            </button>

            {/* Go to Marketplace */}
            <Link
              to="/"
              className="hidden md:flex items-center gap-2 px-3 py-2 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded-xl transition-colors"
            >
              <FiExternalLink className="w-4 h-4" />
              <span>Marketplace</span>
            </Link>

            {/* User Menu */}
            <div className="relative">
              <button
                onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
                className="flex items-center gap-3 px-3 py-2 rounded-xl hover:bg-gray-50 transition-all duration-200 border border-gray-200 hover:border-gray-300"
              >
                <div className="w-9 h-9 bg-gradient-to-br from-blue-600 via-blue-500 to-emerald-500 rounded-full flex items-center justify-center shadow-md">
                  <span className="text-white text-sm font-bold">
                    {(user?.name || user?.email || 'D').charAt(0).toUpperCase()}
                  </span>
                </div>
                <div className="hidden lg:block text-left">
                  <p className="text-sm font-semibold text-gray-900 leading-tight truncate max-w-[120px]">
                    {user?.name || 'Dealer'}
                  </p>
                  <p className="text-xs text-emerald-600 font-medium">Portal Dealer</p>
                </div>
                <FiChevronDown
                  className={`w-4 h-4 text-gray-400 transition-transform duration-200 ${isUserMenuOpen ? 'rotate-180' : ''}`}
                />
              </button>

              {/* User Dropdown */}
              {isUserMenuOpen && (
                <>
                  <div className="fixed inset-0 z-10" onClick={() => setIsUserMenuOpen(false)} />
                  <div className="absolute right-0 mt-3 w-64 bg-white rounded-xl shadow-2xl border border-gray-200/50 py-2 z-20 animate-in fade-in slide-in-from-top-2 duration-200">
                    <div className="px-4 py-3 border-b border-gray-100">
                      <p className="font-semibold text-gray-900">{user?.name || user?.email}</p>
                      <p className="text-xs text-gray-500 truncate">{user?.email}</p>
                    </div>

                    <div className="py-2">
                      <Link
                        to="/dealer/profile/edit"
                        onClick={() => setIsUserMenuOpen(false)}
                        className="flex items-center gap-3 px-4 py-2.5 hover:bg-gray-50 transition-colors"
                      >
                        <Building2 className="w-4 h-4 text-gray-500" />
                        <span className="text-sm text-gray-700">Perfil del Dealer</span>
                      </Link>
                      <Link
                        to="/dealer/settings"
                        onClick={() => setIsUserMenuOpen(false)}
                        className="flex items-center gap-3 px-4 py-2.5 hover:bg-gray-50 transition-colors"
                      >
                        <FiSettings className="w-4 h-4 text-gray-500" />
                        <span className="text-sm text-gray-700">Configuración</span>
                      </Link>
                      <Link
                        to="/dealer/plans"
                        onClick={() => setIsUserMenuOpen(false)}
                        className="flex items-center gap-3 px-4 py-2.5 hover:bg-gray-50 transition-colors"
                      >
                        <Crown className="w-4 h-4 text-amber-500" />
                        <span className="text-sm text-gray-700">Actualizar Plan</span>
                      </Link>
                    </div>

                    <div className="border-t border-gray-200 pt-2">
                      <button
                        onClick={handleLogout}
                        className="w-full flex items-center justify-center gap-2 px-4 py-3 text-red-600 hover:bg-red-50 transition-all font-medium"
                      >
                        <FiLogOut className="w-4 h-4" />
                        <span>Cerrar sesión</span>
                      </button>
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </nav>

      {/* ==================== SIDEBAR ==================== */}
      {/* Mobile Overlay */}
      {isMobileSidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 lg:hidden"
          onClick={() => setIsMobileSidebarOpen(false)}
        />
      )}

      {/* Sidebar */}
      <aside
        className={`fixed top-16 left-0 z-40 h-[calc(100vh-4rem)] bg-white border-r border-gray-200 transition-all duration-300 flex flex-col ${
          isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full lg:translate-x-0'
        } ${isSidebarOpen ? 'w-64' : 'w-20'}`}
      >
        {/* Menu Items */}
        <div className="flex-1 overflow-y-auto p-4 space-y-1">
          {menuItems.map((item) => renderMenuItem(item))}
        </div>

        {/* Usage Stats */}
        {isSidebarOpen && (
          <div className="p-4 border-t border-gray-100">
            <div className="space-y-3">
              <div>
                <div className="flex justify-between text-xs text-gray-600 mb-1">
                  <span>Publicaciones</span>
                  <span>
                    {usage?.listings || 0}/
                    {limits?.maxListings === Infinity ? '∞' : limits?.maxListings || 10}
                  </span>
                </div>
                <div className="w-full h-1.5 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all ${
                      hasReachedLimit?.('listings')
                        ? 'bg-red-500'
                        : (getUsagePercentage?.('listings') || 0) >= 80
                          ? 'bg-amber-500'
                          : 'bg-blue-500'
                    }`}
                    style={{ width: `${Math.min(100, getUsagePercentage?.('listings') || 0)}%` }}
                  />
                </div>
              </div>
              <div>
                <div className="flex justify-between text-xs text-gray-600 mb-1">
                  <span>Destacados</span>
                  <span>
                    {usage?.featuredListings || 0}/{limits?.maxFeaturedListings || 3}
                  </span>
                </div>
                <div className="w-full h-1.5 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full transition-all ${
                      hasReachedLimit?.('featured')
                        ? 'bg-red-500'
                        : (getUsagePercentage?.('featured') || 0) >= 80
                          ? 'bg-amber-500'
                          : 'bg-blue-500'
                    }`}
                    style={{ width: `${Math.min(100, getUsagePercentage?.('featured') || 0)}%` }}
                  />
                </div>
              </div>
            </div>

            {/* Upgrade CTA */}
            {(dealerPlan === 'free' || dealerPlan === 'basic') && (
              <Link
                to="/dealer/plans"
                className="mt-4 block bg-gradient-to-r from-blue-600 to-emerald-500 text-white rounded-xl p-4 text-center hover:shadow-lg transition-all"
              >
                <Crown className="h-5 w-5 mx-auto mb-2" />
                <p className="text-sm font-semibold">Actualiza tu Plan</p>
                <p className="text-xs text-blue-100 mt-1">Desbloquea más funciones</p>
              </Link>
            )}
          </div>
        )}
      </aside>

      {/* ==================== MAIN CONTENT ==================== */}
      <main
        className={`pt-16 transition-all duration-300 ${isSidebarOpen ? 'lg:ml-64' : 'lg:ml-20'}`}
      >
        <div className="min-h-[calc(100vh-4rem)]">{children}</div>
      </main>
    </div>
  );
};

export default DealerPortalLayout;

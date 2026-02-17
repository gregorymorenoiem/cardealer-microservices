/**
 * User Sidebar - Dashboard para usuarios individuales
 *
 * Sidebar con navegación para compradores y vendedores individuales
 * - Compradores: Favoritos, Alertas, Mensajes, Perfil
 * - Vendedores: Mis publicaciones, Leads, Estadísticas, Perfil
 */

import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Home,
  Heart,
  Bell,
  MessageSquare,
  Search,
  Car,
  Plus,
  Clock,
  Eye,
  TrendingUp,
  Users,
  Star,
  Settings,
  User,
  Shield,
  Smartphone,
  CreditCard,
  FileText,
  HelpCircle,
  ChevronDown,
  ChevronRight,
  ListFilter,
  BarChart3,
} from 'lucide-react';
import { useAuthStore } from '@/store/authStore';

interface MenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path?: string;
  exact?: boolean;
  children?: MenuItem[];
  badge?: string | number;
  badgeColor?: string;
  requiresSeller?: boolean; // Solo para vendedores
  requiresBuyer?: boolean; // Solo para compradores (todos por defecto)
}

interface UserSidebarProps {
  userType?: 'buyer' | 'seller' | 'both'; // Tipo de usuario individual
}

const UserSidebar = ({ userType = 'both' }: UserSidebarProps) => {
  const location = useLocation();
  const [expandedSections, setExpandedSections] = useState<string[]>(['dashboard']);
  const { user } = useAuthStore();

  // Determinar si el usuario es vendedor (tiene publicaciones o eligió vender)
  const isSeller = userType === 'seller' || userType === 'both';
  const isBuyer = userType === 'buyer' || userType === 'both';

  const menuItems: MenuItem[] = [
    // Dashboard - Para todos
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: LayoutDashboard,
      children: [
        {
          id: 'dashboard-home',
          label: 'Mi Resumen',
          icon: Home,
          path: '/dashboard',
          exact: true,
        },
        {
          id: 'dashboard-activity',
          label: 'Actividad Reciente',
          icon: TrendingUp,
          path: '/dashboard/activity',
        },
      ],
    },

    // Búsqueda y Favoritos - Para compradores
    {
      id: 'search',
      label: 'Búsqueda',
      icon: Search,
      requiresBuyer: true,
      children: [
        {
          id: 'search-saved',
          label: 'Búsquedas Guardadas',
          icon: ListFilter,
          path: '/search/saved',
        },
        {
          id: 'search-history',
          label: 'Historial de Búsqueda',
          icon: Clock,
          path: '/search/history',
        },
      ],
    },
    {
      id: 'favorites',
      label: 'Favoritos',
      icon: Heart,
      requiresBuyer: true,
      children: [
        {
          id: 'favorites-all',
          label: 'Mis Favoritos',
          icon: Heart,
          path: '/favorites',
          badge: 12,
          badgeColor: 'bg-pink-500',
        },
        {
          id: 'favorites-compare',
          label: 'Comparar Vehículos',
          icon: BarChart3,
          path: '/favorites/compare',
        },
      ],
    },

    // Alertas - Para todos
    {
      id: 'alerts',
      label: 'Alertas',
      icon: Bell,
      children: [
        {
          id: 'alerts-price',
          label: 'Alertas de Precio',
          icon: TrendingUp,
          path: '/alerts/price',
          badge: 3,
          badgeColor: 'bg-blue-500',
        },
        {
          id: 'alerts-new',
          label: 'Nuevos Vehículos',
          icon: Car,
          path: '/alerts/new-vehicles',
        },
        {
          id: 'alerts-config',
          label: 'Configurar Alertas',
          icon: Settings,
          path: '/alerts/settings',
        },
      ],
    },

    // Mis Publicaciones - Solo para vendedores
    {
      id: 'listings',
      label: 'Mis Publicaciones',
      icon: Car,
      requiresSeller: true,
      children: [
        {
          id: 'listings-all',
          label: 'Todas mis Publicaciones',
          icon: Car,
          path: '/my-listings',
        },
        {
          id: 'listings-new',
          label: 'Nueva Publicación',
          icon: Plus,
          path: '/my-listings/new',
        },
        {
          id: 'listings-pending',
          label: 'Pendientes de Aprobación',
          icon: Clock,
          path: '/my-listings/pending',
        },
        {
          id: 'listings-stats',
          label: 'Estadísticas',
          icon: BarChart3,
          path: '/my-listings/stats',
        },
      ],
    },

    // Leads/Consultas - Solo para vendedores
    {
      id: 'leads',
      label: 'Consultas Recibidas',
      icon: Users,
      requiresSeller: true,
      children: [
        {
          id: 'leads-all',
          label: 'Todas las Consultas',
          icon: Users,
          path: '/leads',
          badge: 5,
          badgeColor: 'bg-green-500',
        },
        {
          id: 'leads-unread',
          label: 'Sin Responder',
          icon: MessageSquare,
          path: '/leads/unread',
        },
      ],
    },

    // Analytics - Solo para vendedores
    {
      id: 'analytics',
      label: 'Estadísticas',
      icon: BarChart3,
      requiresSeller: true,
      children: [
        {
          id: 'analytics-views',
          label: 'Vistas de Mis Vehículos',
          icon: Eye,
          path: '/analytics/views',
        },
        {
          id: 'analytics-leads',
          label: 'Leads Generados',
          icon: Users,
          path: '/analytics/leads',
        },
        {
          id: 'analytics-performance',
          label: 'Rendimiento',
          icon: TrendingUp,
          path: '/analytics/performance',
        },
      ],
    },

    // Mensajes - Para todos
    {
      id: 'messages',
      label: 'Mensajes',
      icon: MessageSquare,
      children: [
        {
          id: 'messages-inbox',
          label: 'Bandeja de Entrada',
          icon: MessageSquare,
          path: '/messages',
          badge: 8,
          badgeColor: 'bg-blue-500',
        },
        {
          id: 'messages-sent',
          label: 'Enviados',
          icon: FileText,
          path: '/messages/sent',
        },
      ],
    },

    // Reseñas - Para vendedores
    {
      id: 'reviews',
      label: 'Mis Reseñas',
      icon: Star,
      requiresSeller: true,
      children: [
        {
          id: 'reviews-received',
          label: 'Reseñas Recibidas',
          icon: Star,
          path: '/reviews',
        },
        {
          id: 'reviews-respond',
          label: 'Responder Reseñas',
          icon: MessageSquare,
          path: '/reviews/respond',
        },
      ],
    },

    // Configuración - Para todos
    {
      id: 'settings',
      label: 'Configuración',
      icon: Settings,
      children: [
        {
          id: 'settings-profile',
          label: 'Mi Perfil',
          icon: User,
          path: '/profile',
        },
        {
          id: 'settings-security',
          label: 'Seguridad',
          icon: Shield,
          path: '/settings/security',
        },
        {
          id: 'settings-notifications',
          label: 'Notificaciones',
          icon: Bell,
          path: '/settings/notifications',
        },
        {
          id: 'settings-contact',
          label: 'Datos de Contacto',
          icon: Smartphone,
          path: '/settings/contact',
        },
        {
          id: 'settings-payment',
          label: 'Métodos de Pago',
          icon: CreditCard,
          path: '/settings/payment',
        },
      ],
    },

    // Ayuda - Para todos
    {
      id: 'help',
      label: 'Ayuda',
      icon: HelpCircle,
      children: [
        {
          id: 'help-faq',
          label: 'Preguntas Frecuentes',
          icon: HelpCircle,
          path: '/help/faq',
        },
        {
          id: 'help-contact',
          label: 'Contactar Soporte',
          icon: MessageSquare,
          path: '/help/contact',
        },
      ],
    },
  ];

  // Filtrar menús según tipo de usuario
  const filteredMenuItems = menuItems.filter((item) => {
    if (item.requiresSeller && !isSeller) return false;
    if (item.requiresBuyer && !isBuyer) return false;
    return true;
  });

  const toggleSection = (sectionId: string) => {
    setExpandedSections((prev) =>
      prev.includes(sectionId) ? prev.filter((id) => id !== sectionId) : [...prev, sectionId]
    );
  };

  const isActive = (path: string, exact?: boolean) => {
    if (exact) {
      return location.pathname === path;
    }
    return location.pathname.startsWith(path);
  };

  const isSectionActive = (item: MenuItem) => {
    if (item.path && isActive(item.path, item.exact)) return true;
    if (item.children) {
      return item.children.some((child) => child.path && isActive(child.path, child.exact));
    }
    return false;
  };

  const renderMenuItem = (item: MenuItem, isChild = false) => {
    const Icon = item.icon;
    const hasChildren = item.children && item.children.length > 0;
    const isExpanded = expandedSections.includes(item.id);
    const active = item.path ? isActive(item.path, item.exact) : false;
    const sectionActive = isSectionActive(item);

    if (hasChildren) {
      return (
        <div key={item.id} className="space-y-1">
          <button
            onClick={() => toggleSection(item.id)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${
              sectionActive ? 'bg-emerald-50 text-emerald-700' : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="flex-1 font-medium text-left">{item.label}</span>
            {item.badge && (
              <span
                className={`px-2 py-0.5 text-xs rounded-full text-white ${
                  item.badgeColor || 'bg-emerald-500'
                }`}
              >
                {item.badge}
              </span>
            )}
            {isExpanded ? (
              <ChevronDown className="w-4 h-4 text-gray-400" />
            ) : (
              <ChevronRight className="w-4 h-4 text-gray-400" />
            )}
          </button>

          {isExpanded && (
            <div className="ml-2 pl-4 border-l-2 border-gray-100 space-y-0.5">
              {item.children!.map((child) => renderMenuItem(child, true))}
            </div>
          )}
        </div>
      );
    }

    return (
      <Link
        key={item.id}
        to={item.path || '#'}
        className={`flex items-center gap-3 px-3 py-2 rounded-lg transition-colors ${
          isChild ? 'text-sm' : ''
        } ${
          active
            ? 'bg-emerald-600 text-white'
            : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
        }`}
      >
        <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
        <span className="flex-1">{item.label}</span>
        {item.badge && (
          <span
            className={`px-2 py-0.5 text-xs rounded-full text-white ${
              item.badgeColor || 'bg-emerald-500'
            }`}
          >
            {item.badge}
          </span>
        )}
      </Link>
    );
  };

  return (
    <nav className="space-y-1 overflow-y-auto max-h-[calc(100vh-200px)] pr-2">
      {/* User Info Header */}
      <div className="mb-4 p-3 bg-gradient-to-r from-emerald-50 to-teal-50 rounded-lg border border-emerald-100">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-emerald-600 flex items-center justify-center text-white font-semibold">
            {user?.firstName?.[0] || user?.email?.[0] || 'U'}
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-medium text-gray-900 truncate">
              {user?.firstName} {user?.lastName}
            </p>
            <p className="text-xs text-gray-500 truncate">
              {isSeller && isBuyer
                ? 'Comprador & Vendedor'
                : isSeller
                  ? 'Vendedor Individual'
                  : 'Comprador'}
            </p>
          </div>
        </div>
      </div>

      {/* Menu Items */}
      {filteredMenuItems.map((item) => renderMenuItem(item))}

      {/* CTA para convertirse en Dealer */}
      {isSeller && (
        <div className="mt-6 p-4 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg border border-blue-200">
          <h4 className="font-medium text-blue-900 mb-1">¿Vendes más de 5 vehículos al mes?</h4>
          <p className="text-xs text-blue-700 mb-3">
            Conviértete en Dealer y accede a herramientas profesionales.
          </p>
          <Link
            to="/dealer/landing"
            className="block w-full text-center px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
          >
            Conocer planes de Dealer
          </Link>
        </div>
      )}
    </nav>
  );
};

export default UserSidebar;

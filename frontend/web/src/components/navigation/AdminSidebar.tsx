/**
 * Admin Sidebar - Menú Optimizado
 *
 * Sidebar con navegación jerárquica para el portal de administrador
 * Incluye gestión de dealers, publicidad, analytics y sistema
 */

import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  LayoutDashboard,
  Building2,
  FileStack,
  TrendingUp,
  DollarSign,
  AlertCircle,
  Users,
  Plus,
  Edit,
  Power,
  CreditCard,
  ShoppingBag,
  Clock,
  AlertTriangle,
  Star,
  FileText,
  Shield,
  Ban,
  ListX,
  Receipt,
  Wallet,
  Tag,
  Megaphone,
  Image,
  Mail,
  Settings as SettingsIcon,
  Percent,
  Gift,
  BarChart3,
  Trophy,
  Target,
  PieChart,
  Activity,
  Search,
  MousePointer,
  UserCheck,
  CalendarClock,
  Gauge,
  LineChart,
  TrendingDown,
  Settings,
  ClipboardList,
  Bell,
  ChevronDown,
  ChevronRight,
  Key,
  Gem,
  Eye,
  Globe,
} from 'lucide-react';

interface MenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path?: string;
  exact?: boolean;
  children?: MenuItem[];
  badge?: string | number;
  badgeColor?: string;
  highlight?: boolean;
}

const AdminSidebar = () => {
  const location = useLocation();
  const [expandedSections, setExpandedSections] = useState<string[]>(['dashboard', 'advertising']);

  const menuItems: MenuItem[] = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: LayoutDashboard,
      children: [
        {
          id: 'dashboard-home',
          label: 'Resumen general',
          icon: LayoutDashboard,
          path: '/admin',
          exact: true,
        },
        {
          id: 'dashboard-dealers',
          label: 'Dealers activos',
          icon: Building2,
          path: '/admin/dealers-active',
        },
        {
          id: 'dashboard-listings',
          label: 'Publicaciones activas',
          icon: FileStack,
          path: '/admin/listings-summary',
        },
        {
          id: 'dashboard-leads',
          label: 'Leads generados',
          icon: Users,
          path: '/admin/leads-summary',
        },
        {
          id: 'dashboard-subscriptions',
          label: 'Ingresos suscripciones',
          icon: CreditCard,
          path: '/admin/subscription-revenue',
        },
        {
          id: 'dashboard-advertising',
          label: 'Ingresos publicidad',
          icon: DollarSign,
          path: '/admin/advertising-revenue',
          badge: '💰',
          highlight: true,
        },
        {
          id: 'dashboard-alerts',
          label: 'Alertas críticas',
          icon: AlertCircle,
          path: '/admin/alerts',
          badge: 5,
          badgeColor: 'bg-red-500',
        },
      ],
    },
    {
      id: 'dealers',
      label: 'Dealers (Clientes)',
      icon: Building2,
      children: [
        {
          id: 'dealers-list',
          label: 'Listado de dealers',
          icon: Building2,
          path: '/admin/dealers',
        },
        { id: 'dealers-new', label: 'Crear dealer', icon: Plus, path: '/admin/dealers/new' },
        { id: 'dealers-edit', label: 'Editar dealer', icon: Edit, path: '/admin/dealers/edit' },
        {
          id: 'dealers-toggle',
          label: 'Activar/Desactivar',
          icon: Power,
          path: '/admin/dealers/status',
        },
        {
          id: 'dealers-plans',
          label: 'Plan de suscripción',
          icon: CreditCard,
          path: '/admin/dealers/plans',
        },
      ],
    },
    {
      id: 'marketplace',
      label: 'Marketplace Público',
      icon: ShoppingBag,
      children: [
        {
          id: 'marketplace-pending',
          label: 'Publicaciones pendientes',
          icon: Clock,
          path: '/admin/pending',
          badge: 12,
          badgeColor: 'bg-amber-500',
        },
        {
          id: 'marketplace-reported',
          label: 'Publicaciones reportadas',
          icon: AlertTriangle,
          path: '/admin/reported',
        },
        {
          id: 'marketplace-featured',
          label: 'Publicaciones destacadas',
          icon: Star,
          path: '/admin/featured',
        },
        {
          id: 'marketplace-rules',
          label: 'Reglas de publicación',
          icon: FileText,
          path: '/admin/rules',
        },
      ],
    },
    {
      id: 'moderation',
      label: 'Moderación y Seguridad',
      icon: Shield,
      children: [
        {
          id: 'moderation-reports',
          label: 'Reportes de usuarios',
          icon: AlertTriangle,
          path: '/admin/reports',
        },
        { id: 'moderation-block', label: 'Bloqueo de contenido', icon: Ban, path: '/admin/blocks' },
        { id: 'moderation-blacklist', label: 'Lista negra', icon: ListX, path: '/admin/blacklist' },
      ],
    },
    {
      id: 'billing',
      label: 'Facturación SaaS',
      icon: Receipt,
      children: [
        {
          id: 'billing-subscriptions',
          label: 'Suscripciones activas',
          icon: CreditCard,
          path: '/admin/billing/subscriptions',
        },
        {
          id: 'billing-invoices',
          label: 'Facturas a dealers',
          icon: Receipt,
          path: '/admin/billing/invoices',
        },
        {
          id: 'billing-payments',
          label: 'Pagos recibidos',
          icon: Wallet,
          path: '/admin/billing/payments',
        },
        { id: 'billing-plans', label: 'Planes y precios', icon: Tag, path: '/admin/billing/plans' },
      ],
    },
    {
      id: 'advertising',
      label: 'Publicidad de Plataforma',
      icon: Gem,
      badge: '🏆 PRINCIPAL',
      highlight: true,
      children: [
        {
          id: 'ad-products',
          label: 'Productos Publicitarios',
          icon: Megaphone,
          path: '/admin/advertising/products',
        },
        {
          id: 'ad-featured',
          label: 'Destacados en Home',
          icon: Star,
          path: '/admin/advertising/featured',
        },
        {
          id: 'ad-sponsored',
          label: 'Publicaciones Patrocinadas',
          icon: Search,
          path: '/admin/advertising/sponsored',
        },
        {
          id: 'ad-banners',
          label: 'Banners promocionales',
          icon: Image,
          path: '/admin/advertising/banners',
        },
        {
          id: 'ad-email',
          label: 'Email marketing masivo',
          icon: Mail,
          path: '/admin/advertising/email',
        },
        {
          id: 'ad-campaigns',
          label: 'Campañas Activas',
          icon: Megaphone,
          path: '/admin/advertising/campaigns',
        },
        {
          id: 'ad-pricing',
          label: 'Configuración de Precios',
          icon: Tag,
          path: '/admin/advertising/pricing',
        },
        {
          id: 'ad-discounts',
          label: 'Descuentos por volumen',
          icon: Percent,
          path: '/admin/advertising/discounts',
        },
        {
          id: 'ad-offers',
          label: 'Ofertas especiales',
          icon: Gift,
          path: '/admin/advertising/offers',
        },
        {
          id: 'ad-reports',
          label: 'Reportes de Publicidad',
          icon: BarChart3,
          path: '/admin/advertising/reports',
        },
        {
          id: 'ad-top-dealers',
          label: 'Dealers top (más gasto)',
          icon: Trophy,
          path: '/admin/advertising/top-dealers',
        },
        {
          id: 'ad-conversion',
          label: 'Conversión por campaña',
          icon: Target,
          path: '/admin/advertising/conversion',
        },
        {
          id: 'ad-roi',
          label: 'ROI promedio dealers',
          icon: PieChart,
          path: '/admin/advertising/roi',
        },
      ],
    },
    {
      id: 'analytics',
      label: 'Analítica y BI',
      icon: Activity,
      children: [
        {
          id: 'analytics-traffic',
          label: 'Tráfico del marketplace',
          icon: Globe,
          path: '/admin/analytics/traffic',
        },
        {
          id: 'analytics-vehicles',
          label: 'Vehículos más vistos',
          icon: Eye,
          path: '/admin/analytics/top-vehicles',
        },
        {
          id: 'analytics-searches',
          label: 'Búsquedas populares',
          icon: Search,
          path: '/admin/analytics/searches',
        },
        {
          id: 'analytics-conversion',
          label: 'Conversión visitas→leads',
          icon: MousePointer,
          path: '/admin/analytics/conversion',
        },
        {
          id: 'analytics-dealers',
          label: 'Comportamiento Dealers',
          icon: UserCheck,
          path: '/admin/analytics/dealer-behavior',
        },
        {
          id: 'analytics-usage',
          label: 'Uso de herramientas',
          icon: Gauge,
          path: '/admin/analytics/usage',
        },
        {
          id: 'analytics-frequency',
          label: 'Frecuencia publicación',
          icon: CalendarClock,
          path: '/admin/analytics/frequency',
        },
        {
          id: 'analytics-adoption',
          label: 'Adopción publicidad',
          icon: TrendingUp,
          path: '/admin/analytics/adoption',
        },
        {
          id: 'analytics-revenue',
          label: 'Rentabilidad',
          icon: LineChart,
          path: '/admin/analytics/revenue',
        },
        {
          id: 'analytics-cac-ltv',
          label: 'CAC vs LTV',
          icon: TrendingDown,
          path: '/admin/analytics/cac-ltv',
        },
        {
          id: 'analytics-projection',
          label: 'Proyección crecimiento',
          icon: TrendingUp,
          path: '/admin/analytics/projection',
        },
      ],
    },
    {
      id: 'system',
      label: 'Sistema',
      icon: Settings,
      children: [
        {
          id: 'system-config',
          label: 'Configuración general',
          icon: SettingsIcon,
          path: '/admin/settings',
        },
        {
          id: 'system-audit',
          label: 'Auditoría de acciones',
          icon: ClipboardList,
          path: '/admin/audit',
        },
        {
          id: 'system-notifications',
          label: 'Notificaciones globales',
          icon: Bell,
          path: '/admin/notifications',
        },
      ],
    },
    {
      id: 'rbac',
      label: 'Roles y Permisos (RBAC)',
      icon: Shield,
      badge: '🔐',
      children: [
        { id: 'rbac-roles', label: 'Gestión de Roles', icon: Shield, path: '/admin/roles' },
        {
          id: 'rbac-permissions',
          label: 'Gestión de Permisos',
          icon: Key,
          path: '/admin/permissions',
        },
      ],
    },
  ];

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
              item.highlight
                ? sectionActive
                  ? 'bg-gradient-to-r from-amber-100 to-orange-100 text-amber-800 ring-1 ring-amber-300'
                  : 'bg-gradient-to-r from-amber-50 to-orange-50 text-amber-700 hover:from-amber-100 hover:to-orange-100'
                : sectionActive
                  ? 'bg-indigo-50 text-indigo-700'
                  : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <Icon className={`w-5 h-5 ${item.highlight ? 'text-amber-600' : ''}`} />
            <span className="flex-1 font-medium text-left">{item.label}</span>
            {item.badge && (
              <span
                className={`px-2 py-0.5 text-xs rounded-full ${
                  typeof item.badge === 'string' && item.badge.includes('🏆')
                    ? 'bg-gradient-to-r from-amber-400 to-orange-400 text-white font-semibold'
                    : item.badgeColor
                      ? `${item.badgeColor} text-white`
                      : 'bg-indigo-100 text-indigo-700'
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
            <div
              className={`ml-2 pl-4 border-l-2 space-y-0.5 ${
                item.highlight ? 'border-amber-200' : 'border-gray-100'
              }`}
            >
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
            ? 'bg-indigo-600 text-white'
            : item.highlight
              ? 'text-amber-700 hover:bg-amber-50'
              : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
        }`}
      >
        <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
        <span className="flex-1">{item.label}</span>
        {item.badge && (
          <span
            className={`px-2 py-0.5 text-xs rounded-full text-white ${
              item.badgeColor || 'bg-indigo-500'
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
      {menuItems.map((item) => renderMenuItem(item))}
    </nav>
  );
};

export default AdminSidebar;

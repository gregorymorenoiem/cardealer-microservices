/**
 * Dealer Sidebar - Menú Optimizado con Control de Acceso por Rol
 *
 * Sidebar con navegación jerárquica para el portal del dealer
 * Incluye todas las secciones del negocio con submenús colapsables
 * Filtrado por dealerRole: owner, manager, sales_manager, inventory_manager, salesperson, viewer
 */

import { useState } from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  Home,
  LayoutDashboard,
  TrendingUp,
  Package,
  Users as UsersIcon,
  AlertCircle,
  FileStack,
  Receipt,
  FileText,
  CreditCard,
  AlertTriangle,
  FileCheck,
  Settings as SettingsIcon,
  Car,
  Plus,
  Tag,
  DollarSign,
  ShoppingBag,
  Clock,
  Store,
  Users,
  CalendarCheck,
  UserCheck,
  Megaphone,
  Star,
  Search,
  Image,
  Mail,
  Crown,
  BarChart3,
  Calculator,
  Table,
  Percent,
  Building,
  User,
  Shield,
  Smartphone,
  ChevronDown,
  ChevronRight,
  Sparkles,
  Lock,
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';
import { useAuthStore } from '@/store/authStore';
import { DealerRole } from '@/shared/types';

interface MenuItem {
  id: string;
  label: string;
  icon: React.ElementType;
  path?: string;
  exact?: boolean;
  children?: MenuItem[];
  available?: boolean;
  badge?: string | number;
  badgeColor?: string;
  // Roles que pueden ver este item (si no se especifica, todos pueden verlo)
  allowedRoles?: DealerRole[];
}

// Roles con acceso a cada sección
const ROLE_ACCESS = {
  // Todos los roles
  ALL: [
    DealerRole.OWNER,
    DealerRole.MANAGER,
    DealerRole.SALES_MANAGER,
    DealerRole.INVENTORY_MANAGER,
    DealerRole.SALESPERSON,
    DealerRole.VIEWER,
  ],
  // Solo owner y manager
  MANAGEMENT: [DealerRole.OWNER, DealerRole.MANAGER],
  // Gestión de inventario
  INVENTORY: [
    DealerRole.OWNER,
    DealerRole.MANAGER,
    DealerRole.INVENTORY_MANAGER,
    DealerRole.SALESPERSON,
  ],
  // Gestión de leads/ventas
  SALES: [DealerRole.OWNER, DealerRole.MANAGER, DealerRole.SALES_MANAGER, DealerRole.SALESPERSON],
  // Suscripción y facturación
  BILLING: [DealerRole.OWNER, DealerRole.MANAGER],
  // Configuración
  SETTINGS: [DealerRole.OWNER, DealerRole.MANAGER],
  // Publicidad
  ADVERTISING: [DealerRole.OWNER, DealerRole.MANAGER, DealerRole.SALES_MANAGER],
  // Analytics
  ANALYTICS: [
    DealerRole.OWNER,
    DealerRole.MANAGER,
    DealerRole.SALES_MANAGER,
    DealerRole.INVENTORY_MANAGER,
  ],
};

const DealerSidebar = () => {
  const location = useLocation();
  const [expandedSections, setExpandedSections] = useState<string[]>(['dashboard']);
  const { portalAccess } = usePermissions();
  const { user } = useAuthStore();

  // Obtener el rol del dealer del usuario actual
  const dealerRole = (user?.dealerRole as DealerRole) || DealerRole.VIEWER;

  // Verificar si el usuario tiene acceso a un item
  const hasRoleAccess = (allowedRoles?: DealerRole[]) => {
    if (!allowedRoles) return true; // Si no se especifican roles, todos tienen acceso
    return allowedRoles.includes(dealerRole);
  };

  const menuItems: MenuItem[] = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: LayoutDashboard,
      allowedRoles: ROLE_ACCESS.ALL,
      children: [
        {
          id: 'dashboard-home',
          label: 'Resumen del negocio',
          icon: Home,
          path: '/dealer',
          exact: true,
        },
        {
          id: 'dashboard-sales',
          label: 'Ventas del mes',
          icon: TrendingUp,
          path: '/dealer/sales',
          allowedRoles: ROLE_ACCESS.SALES,
        },
        {
          id: 'dashboard-inventory',
          label: 'Inventario disponible',
          icon: Package,
          path: '/dealer/inventory-summary',
          allowedRoles: ROLE_ACCESS.INVENTORY,
        },
        {
          id: 'dashboard-leads',
          label: 'Leads recientes',
          icon: UsersIcon,
          path: '/dealer/leads-recent',
          allowedRoles: ROLE_ACCESS.SALES,
        },
        {
          id: 'dashboard-publications',
          label: 'Publicaciones activas',
          icon: FileStack,
          path: '/dealer/active-listings',
        },
        {
          id: 'dashboard-alerts',
          label: 'Alertas',
          icon: AlertCircle,
          path: '/dealer/alerts',
          badge: 3,
          badgeColor: 'bg-red-500',
        },
      ],
    },
    // NOTA: Facturación & NCF (DGII) es para Administradores de OKLA, NO para dealers.
    // Los dealers tienen acceso a "Mi Suscripción" para ver sus pagos como CLIENTES de OKLA.
    {
      id: 'subscription',
      label: 'Mi Suscripción',
      icon: CreditCard,
      allowedRoles: ROLE_ACCESS.BILLING,
      children: [
        {
          id: 'subscription-plan',
          label: 'Mi Plan Actual',
          icon: Crown,
          path: '/dealer/subscription',
        },
        {
          id: 'subscription-invoices',
          label: 'Mis Facturas',
          icon: FileText,
          path: '/dealer/subscription/invoices',
        },
        {
          id: 'subscription-payment',
          label: 'Método de Pago',
          icon: CreditCard,
          path: '/dealer/subscription/payment-method',
        },
        {
          id: 'subscription-upgrade',
          label: 'Cambiar Plan',
          icon: TrendingUp,
          path: '/dealer/subscription/upgrade',
        },
      ],
    },
    {
      id: 'inventory',
      allowedRoles: ROLE_ACCESS.INVENTORY,
      label: 'Inventario de Vehículos',
      icon: Car,
      children: [
        {
          id: 'inventory-list',
          label: 'Listado de vehículos',
          icon: Car,
          path: '/dealer/inventory',
        },
        {
          id: 'inventory-new',
          label: 'Agregar vehículo',
          icon: Plus,
          path: '/dealer/inventory/new',
        },
        { id: 'inventory-status', label: 'Estados', icon: Tag, path: '/dealer/inventory/status' },
        {
          id: 'inventory-costs',
          label: 'Costos & precios',
          icon: DollarSign,
          path: '/dealer/inventory/costs',
        },
      ],
    },
    {
      id: 'marketplace',
      label: 'Publicación en Marketplace',
      icon: ShoppingBag,
      allowedRoles: ROLE_ACCESS.INVENTORY,
      children: [
        {
          id: 'marketplace-active',
          label: 'Publicaciones activas',
          icon: FileStack,
          path: '/dealer/listings',
        },
        {
          id: 'marketplace-pending',
          label: 'Publicaciones pendientes',
          icon: Clock,
          path: '/dealer/listings/pending',
        },
        {
          id: 'marketplace-new',
          label: 'Nueva publicación',
          icon: Plus,
          path: '/dealer/listings/new',
        },
        {
          id: 'marketplace-config',
          label: 'Configuración de tienda',
          icon: Store,
          path: '/dealer/store-settings',
          allowedRoles: ROLE_ACCESS.MANAGEMENT,
        },
      ],
    },
    {
      id: 'crm',
      label: 'CRM / Leads',
      icon: Users,
      available: portalAccess.crm,
      allowedRoles: ROLE_ACCESS.SALES,
      children: [
        { id: 'crm-all', label: 'Todos los leads', icon: Users, path: '/dealer/crm' },
        {
          id: 'crm-pipeline',
          label: 'Pipeline de ventas',
          icon: TrendingUp,
          path: '/dealer/crm/pipeline',
        },
        {
          id: 'crm-calendar',
          label: 'Calendario de seguimiento',
          icon: CalendarCheck,
          path: '/dealer/crm/calendar',
        },
        {
          id: 'crm-assign',
          label: 'Asignación a vendedores',
          icon: UserCheck,
          path: '/dealer/crm/assignments',
          allowedRoles: ROLE_ACCESS.MANAGEMENT,
        },
      ],
    },
    {
      id: 'advertising',
      label: 'Publicidad y Promociones',
      icon: Megaphone,
      badge: '💰',
      allowedRoles: ROLE_ACCESS.ADVERTISING,
      children: [
        {
          id: 'advertising-products',
          label: 'Productos Disponibles',
          icon: Sparkles,
          path: '/dealer/advertising/products',
        },
        {
          id: 'advertising-featured',
          label: 'Destacado en Home',
          icon: Star,
          path: '/dealer/advertising/featured',
        },
        {
          id: 'advertising-sponsored',
          label: 'Publicación Patrocinada',
          icon: Search,
          path: '/dealer/advertising/sponsored',
        },
        {
          id: 'advertising-banner',
          label: 'Banner promocional',
          icon: Image,
          path: '/dealer/advertising/banners',
        },
        {
          id: 'advertising-email',
          label: 'Email marketing',
          icon: Mail,
          path: '/dealer/advertising/email',
        },
        {
          id: 'advertising-premium',
          label: 'Paquete Premium',
          icon: Crown,
          path: '/dealer/advertising/premium',
        },
        {
          id: 'advertising-campaigns',
          label: 'Mis Campañas Activas',
          icon: Megaphone,
          path: '/dealer/advertising/campaigns',
        },
        {
          id: 'advertising-history',
          label: 'Historial de Promociones',
          icon: FileText,
          path: '/dealer/advertising/history',
        },
        {
          id: 'advertising-roi',
          label: 'Resultados y ROI',
          icon: BarChart3,
          path: '/dealer/advertising/results',
        },
      ],
    },
    {
      id: 'financing',
      label: 'Financiamiento y Seguros',
      icon: Calculator,
      available: portalAccess.finance,
      allowedRoles: ROLE_ACCESS.SALES,
      children: [
        {
          id: 'financing-simulator',
          label: 'Simulador de financiamiento',
          icon: Calculator,
          path: '/dealer/financing/simulator',
        },
        {
          id: 'financing-amortization',
          label: 'Tabla de amortización',
          icon: Table,
          path: '/dealer/financing/amortization',
        },
        {
          id: 'financing-operations',
          label: 'Operaciones activas',
          icon: FileText,
          path: '/dealer/financing/operations',
        },
        {
          id: 'financing-commissions',
          label: 'Comisiones generadas',
          icon: Percent,
          path: '/dealer/financing/commissions',
          allowedRoles: ROLE_ACCESS.MANAGEMENT,
        },
      ],
    },
    {
      id: 'settings',
      label: 'Configuración',
      icon: SettingsIcon,
      allowedRoles: ROLE_ACCESS.SETTINGS,
      children: [
        {
          id: 'settings-profile',
          label: 'Perfil del dealer',
          icon: Building,
          path: '/dealer/settings',
        },
        { id: 'settings-users', label: 'Usuarios y roles', icon: User, path: '/dealer/employees' },
        {
          id: 'settings-preferences',
          label: 'Preferencias del sistema',
          icon: SettingsIcon,
          path: '/dealer/settings/preferences',
        },
        {
          id: 'settings-channels',
          label: 'Canales de contacto',
          icon: Smartphone,
          path: '/dealer/settings/channels',
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
    const isAvailable = item.available !== false;
    const hasAccess = hasRoleAccess(item.allowedRoles);

    // Si no tiene acceso por rol, mostrar bloqueado
    if (!hasAccess) {
      return (
        <div
          key={item.id}
          className={`flex items-center gap-3 px-3 py-2 rounded-lg text-gray-400 cursor-not-allowed opacity-50 ${
            isChild ? 'ml-4 text-sm' : ''
          }`}
          title="No tienes permisos para acceder a esta sección"
        >
          <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
          <span className="flex-1">{item.label}</span>
          <Lock className="w-4 h-4" />
        </div>
      );
    }

    if (!isAvailable) {
      return (
        <div
          key={item.id}
          className={`flex items-center gap-3 px-3 py-2 rounded-lg text-gray-400 cursor-not-allowed opacity-50 ${
            isChild ? 'ml-4 text-sm' : ''
          }`}
          title="Disponible en planes superiores"
        >
          <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
          <span className="flex-1">{item.label}</span>
          <Shield className="w-4 h-4" />
        </div>
      );
    }

    if (hasChildren) {
      // Filtrar children por rol
      const accessibleChildren = item.children!.filter((child) =>
        hasRoleAccess(child.allowedRoles)
      );

      if (accessibleChildren.length === 0) return null;

      return (
        <div key={item.id} className="space-y-1">
          <button
            onClick={() => toggleSection(item.id)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${
              sectionActive ? 'bg-blue-50 text-blue-700' : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="flex-1 font-medium text-left">{item.label}</span>
            {item.badge && (
              <span
                className={`px-2 py-0.5 text-xs rounded-full ${
                  item.badgeColor || 'bg-blue-100 text-blue-700'
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
              {accessibleChildren.map((child) => renderMenuItem(child, true))}
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
          active ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
        }`}
      >
        <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
        <span className="flex-1">{item.label}</span>
        {item.badge && (
          <span
            className={`px-2 py-0.5 text-xs rounded-full text-white ${
              item.badgeColor || 'bg-blue-500'
            }`}
          >
            {item.badge}
          </span>
        )}
      </Link>
    );
  };

  // Filtrar menu items por rol del usuario
  const accessibleMenuItems = menuItems.filter((item) => hasRoleAccess(item.allowedRoles));

  return (
    <nav className="space-y-1 overflow-y-auto max-h-[calc(100vh-200px)] pr-2">
      {/* Role Indicator */}
      <div className="mb-4 p-3 bg-gradient-to-r from-blue-50 to-indigo-50 rounded-lg border border-blue-100">
        <div className="flex items-center gap-2">
          <Shield className="w-4 h-4 text-blue-600" />
          <span className="text-sm font-medium text-blue-900">
            Rol: {dealerRole.charAt(0).toUpperCase() + dealerRole.slice(1).replace('_', ' ')}
          </span>
        </div>
      </div>

      {accessibleMenuItems.map((item) => renderMenuItem(item))}
    </nav>
  );
};

export default DealerSidebar;

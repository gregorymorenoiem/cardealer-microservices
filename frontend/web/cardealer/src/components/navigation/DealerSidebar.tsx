/**
 * Dealer Sidebar - Menú Optimizado
 * 
 * Sidebar con navegación jerárquica para el portal del dealer
 * Incluye todas las secciones del negocio con submenús colapsables
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
  FileEdit,
  Store,
  Phone,
  Users,
  UserPlus,
  CalendarCheck,
  UserCheck,
  Megaphone,
  Star,
  Search,
  Image,
  Mail,
  Crown,
  BarChart3,
  MousePointer,
  Target,
  Calculator,
  Table,
  Download,
  Percent,
  Building,
  User,
  Shield,
  Smartphone,
  ChevronDown,
  ChevronRight,
  Sparkles
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';

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
}

const DealerSidebar = () => {
  const location = useLocation();
  const [expandedSections, setExpandedSections] = useState<string[]>(['dashboard']);
  const { portalAccess, dealerPlan } = usePermissions();

  const menuItems: MenuItem[] = [
    {
      id: 'dashboard',
      label: 'Dashboard',
      icon: LayoutDashboard,
      children: [
        { id: 'dashboard-home', label: 'Resumen del negocio', icon: Home, path: '/dealer', exact: true },
        { id: 'dashboard-sales', label: 'Ventas del mes', icon: TrendingUp, path: '/dealer/sales' },
        { id: 'dashboard-inventory', label: 'Inventario disponible', icon: Package, path: '/dealer/inventory-summary' },
        { id: 'dashboard-leads', label: 'Leads recientes', icon: UsersIcon, path: '/dealer/leads-recent' },
        { id: 'dashboard-publications', label: 'Publicaciones activas', icon: FileStack, path: '/dealer/active-listings' },
        { id: 'dashboard-alerts', label: 'Alertas', icon: AlertCircle, path: '/dealer/alerts', badge: 3, badgeColor: 'bg-red-500' },
      ]
    },
    {
      id: 'invoicing',
      label: 'Facturación & NCF',
      icon: Receipt,
      available: portalAccess.invoicing,
      children: [
        { id: 'invoicing-new', label: 'Nueva factura', icon: Plus, path: '/dealer/invoicing/new' },
        { id: 'invoicing-list', label: 'Facturas emitidas', icon: FileText, path: '/dealer/invoicing' },
        { id: 'invoicing-notes', label: 'Notas de crédito/débito', icon: CreditCard, path: '/dealer/invoicing/notes' },
        { id: 'invoicing-void', label: 'Anulación de comprobantes', icon: AlertTriangle, path: '/dealer/invoicing/void' },
        { id: 'invoicing-dgii', label: 'Reportes DGII (607, 608)', icon: FileCheck, path: '/dealer/invoicing/dgii-reports' },
        { id: 'invoicing-config', label: 'Configuración fiscal', icon: SettingsIcon, path: '/dealer/invoicing/settings' },
      ]
    },
    {
      id: 'inventory',
      label: 'Inventario de Vehículos',
      icon: Car,
      children: [
        { id: 'inventory-list', label: 'Listado de vehículos', icon: Car, path: '/dealer/inventory' },
        { id: 'inventory-new', label: 'Agregar vehículo', icon: Plus, path: '/dealer/inventory/new' },
        { id: 'inventory-status', label: 'Estados', icon: Tag, path: '/dealer/inventory/status' },
        { id: 'inventory-costs', label: 'Costos & precios', icon: DollarSign, path: '/dealer/inventory/costs' },
      ]
    },
    {
      id: 'marketplace',
      label: 'Publicación en Marketplace',
      icon: ShoppingBag,
      children: [
        { id: 'marketplace-active', label: 'Publicaciones activas', icon: FileStack, path: '/dealer/listings' },
        { id: 'marketplace-pending', label: 'Publicaciones pendientes', icon: Clock, path: '/dealer/listings/pending' },
        { id: 'marketplace-new', label: 'Nueva publicación', icon: Plus, path: '/dealer/listings/new' },
        { id: 'marketplace-config', label: 'Configuración de tienda', icon: Store, path: '/dealer/store-settings' },
      ]
    },
    {
      id: 'crm',
      label: 'CRM / Leads',
      icon: Users,
      available: portalAccess.crm,
      children: [
        { id: 'crm-all', label: 'Todos los leads', icon: Users, path: '/dealer/crm' },
        { id: 'crm-pipeline', label: 'Pipeline de ventas', icon: TrendingUp, path: '/dealer/crm/pipeline' },
        { id: 'crm-calendar', label: 'Calendario de seguimiento', icon: CalendarCheck, path: '/dealer/crm/calendar' },
        { id: 'crm-assign', label: 'Asignación a vendedores', icon: UserCheck, path: '/dealer/crm/assignments' },
      ]
    },
    {
      id: 'advertising',
      label: 'Publicidad y Promociones',
      icon: Megaphone,
      badge: '💰',
      children: [
        { 
          id: 'advertising-products', 
          label: 'Productos Disponibles', 
          icon: Sparkles, 
          path: '/dealer/advertising/products',
        },
        { id: 'advertising-featured', label: 'Destacado en Home', icon: Star, path: '/dealer/advertising/featured' },
        { id: 'advertising-sponsored', label: 'Publicación Patrocinada', icon: Search, path: '/dealer/advertising/sponsored' },
        { id: 'advertising-banner', label: 'Banner promocional', icon: Image, path: '/dealer/advertising/banners' },
        { id: 'advertising-email', label: 'Email marketing', icon: Mail, path: '/dealer/advertising/email' },
        { id: 'advertising-premium', label: 'Paquete Premium', icon: Crown, path: '/dealer/advertising/premium' },
        { id: 'advertising-campaigns', label: 'Mis Campañas Activas', icon: Megaphone, path: '/dealer/advertising/campaigns' },
        { id: 'advertising-history', label: 'Historial de Promociones', icon: FileText, path: '/dealer/advertising/history' },
        { id: 'advertising-roi', label: 'Resultados y ROI', icon: BarChart3, path: '/dealer/advertising/results' },
      ]
    },
    {
      id: 'financing',
      label: 'Financiamiento y Seguros',
      icon: Calculator,
      available: portalAccess.finance,
      children: [
        { id: 'financing-simulator', label: 'Simulador de financiamiento', icon: Calculator, path: '/dealer/financing/simulator' },
        { id: 'financing-amortization', label: 'Tabla de amortización', icon: Table, path: '/dealer/financing/amortization' },
        { id: 'financing-operations', label: 'Operaciones activas', icon: FileText, path: '/dealer/financing/operations' },
        { id: 'financing-commissions', label: 'Comisiones generadas', icon: Percent, path: '/dealer/financing/commissions' },
      ]
    },
    {
      id: 'settings',
      label: 'Configuración',
      icon: SettingsIcon,
      children: [
        { id: 'settings-profile', label: 'Perfil del dealer', icon: Building, path: '/dealer/settings' },
        { id: 'settings-users', label: 'Usuarios y roles', icon: User, path: '/dealer/settings/users' },
        { id: 'settings-preferences', label: 'Preferencias del sistema', icon: SettingsIcon, path: '/dealer/settings/preferences' },
        { id: 'settings-channels', label: 'Canales de contacto', icon: Smartphone, path: '/dealer/settings/channels' },
      ]
    },
  ];

  const toggleSection = (sectionId: string) => {
    setExpandedSections(prev => 
      prev.includes(sectionId) 
        ? prev.filter(id => id !== sectionId)
        : [...prev, sectionId]
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
      return item.children.some(child => child.path && isActive(child.path, child.exact));
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
      return (
        <div key={item.id} className="space-y-1">
          <button
            onClick={() => toggleSection(item.id)}
            className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors ${
              sectionActive
                ? 'bg-blue-50 text-blue-700'
                : 'text-gray-700 hover:bg-gray-100'
            }`}
          >
            <Icon className="w-5 h-5" />
            <span className="flex-1 font-medium text-left">{item.label}</span>
            {item.badge && (
              <span className={`px-2 py-0.5 text-xs rounded-full ${
                item.badgeColor || 'bg-blue-100 text-blue-700'
              }`}>
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
              {item.children!.map(child => renderMenuItem(child, true))}
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
            ? 'bg-blue-600 text-white'
            : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
        }`}
      >
        <Icon className={isChild ? 'w-4 h-4' : 'w-5 h-5'} />
        <span className="flex-1">{item.label}</span>
        {item.badge && (
          <span className={`px-2 py-0.5 text-xs rounded-full text-white ${
            item.badgeColor || 'bg-blue-500'
          }`}>
            {item.badge}
          </span>
        )}
      </Link>
    );
  };

  return (
    <nav className="space-y-1 pr-2 min-h-full">
      {menuItems.map(item => renderMenuItem(item))}
    </nav>
  );
};

export default DealerSidebar;

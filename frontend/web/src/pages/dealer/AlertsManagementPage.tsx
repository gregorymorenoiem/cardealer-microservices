import { useState, useCallback } from 'react';
import {
  FiBell,
  FiAlertTriangle,
  FiAlertCircle,
  FiInfo,
  FiCheck,
  FiCheckCircle,
  FiX,
  FiRefreshCw,
  FiTrash2,
  FiEye,
  FiClock,
  FiTrendingDown,
  FiDollarSign,
  FiPackage,
  FiUsers,
  FiSettings,
  FiChevronRight,
} from 'react-icons/fi';
import MainLayout from '../../layouts/MainLayout';

// ============================================
// TYPES
// ============================================

type AlertSeverity = 'info' | 'warning' | 'critical';
type AlertStatus = 'unread' | 'read' | 'acted' | 'dismissed';
type AlertType =
  | 'slow_inventory'
  | 'price_drop_needed'
  | 'low_engagement'
  | 'lead_response_slow'
  | 'competitor_price'
  | 'inventory_low'
  | 'performance_drop'
  | 'payment_due'
  | 'review_needed'
  | 'opportunity';

interface DealerAlert {
  id: string;
  type: AlertType;
  severity: AlertSeverity;
  status: AlertStatus;
  title: string;
  message: string;
  vehicleId?: string;
  vehicleTitle?: string;
  actionUrl?: string;
  actionLabel?: string;
  createdAt: string;
  readAt?: string;
  expiresAt?: string;
  metadata?: Record<string, any>;
}

interface AlertSummary {
  total: number;
  unread: number;
  critical: number;
  warning: number;
  info: number;
  byType: Record<AlertType, number>;
}

// ============================================
// MOCK DATA
// ============================================

const mockAlerts: DealerAlert[] = [
  {
    id: '1',
    type: 'slow_inventory',
    severity: 'critical',
    status: 'unread',
    title: 'Inventario Lento - Acción Requerida',
    message:
      'Tienes 5 vehículos con más de 60 días en inventario. Considera ajustar precios para acelerar la venta.',
    actionUrl: '/inventory?filter=aging',
    actionLabel: 'Ver Inventario',
    createdAt: new Date(Date.now() - 3600000).toISOString(),
    expiresAt: new Date(Date.now() + 86400000 * 7).toISOString(),
    metadata: { vehicleCount: 5, avgDays: 78 },
  },
  {
    id: '2',
    type: 'price_drop_needed',
    severity: 'warning',
    status: 'unread',
    title: 'Ajuste de Precio Sugerido',
    message:
      'El Toyota Camry 2022 está 15% por encima del precio de mercado y tiene bajo engagement.',
    vehicleId: 'v123',
    vehicleTitle: 'Toyota Camry 2022',
    actionUrl: '/vehicles/v123/edit',
    actionLabel: 'Editar Precio',
    createdAt: new Date(Date.now() - 7200000).toISOString(),
    metadata: { currentPrice: 32000, suggestedPrice: 27500, marketAvg: 27200 },
  },
  {
    id: '3',
    type: 'lead_response_slow',
    severity: 'critical',
    status: 'unread',
    title: 'Leads Sin Responder',
    message:
      'Tienes 3 leads calientes esperando respuesta por más de 2 horas. El tiempo de respuesta afecta la conversión.',
    actionUrl: '/leads?filter=pending',
    actionLabel: 'Ver Leads',
    createdAt: new Date(Date.now() - 1800000).toISOString(),
    metadata: { pendingLeads: 3, avgWaitTime: 145 },
  },
  {
    id: '4',
    type: 'competitor_price',
    severity: 'warning',
    status: 'read',
    title: 'Competencia con Mejor Precio',
    message: 'Un dealer cercano tiene un Honda Accord 2023 similar a $28,500 (tú: $31,000).',
    vehicleId: 'v456',
    vehicleTitle: 'Honda Accord 2023',
    actionUrl: '/vehicles/v456/edit',
    actionLabel: 'Revisar',
    createdAt: new Date(Date.now() - 86400000).toISOString(),
    readAt: new Date(Date.now() - 43200000).toISOString(),
    metadata: { yourPrice: 31000, competitorPrice: 28500, difference: 8.1 },
  },
  {
    id: '5',
    type: 'low_engagement',
    severity: 'warning',
    status: 'unread',
    title: 'Bajo Engagement en Listados',
    message: '8 de tus vehículos tienen menos de 50 vistas en los últimos 7 días.',
    actionUrl: '/analytics/performance',
    actionLabel: 'Ver Análisis',
    createdAt: new Date(Date.now() - 172800000).toISOString(),
    metadata: { vehicleCount: 8, avgViews: 23 },
  },
  {
    id: '6',
    type: 'opportunity',
    severity: 'info',
    status: 'unread',
    title: 'Oportunidad de Venta',
    message: 'Alta demanda de SUVs en tu zona. Considera agregar más inventario de esta categoría.',
    actionUrl: '/market-insights',
    actionLabel: 'Ver Tendencias',
    createdAt: new Date(Date.now() - 259200000).toISOString(),
    metadata: { demand: 'high', category: 'SUV', yourInventory: 3 },
  },
  {
    id: '7',
    type: 'payment_due',
    severity: 'info',
    status: 'read',
    title: 'Próximo Pago de Suscripción',
    message: 'Tu suscripción Professional se renovará el 15 de febrero por RD$5,900.',
    actionUrl: '/billing',
    actionLabel: 'Ver Facturación',
    createdAt: new Date(Date.now() - 345600000).toISOString(),
    readAt: new Date(Date.now() - 259200000).toISOString(),
  },
  {
    id: '8',
    type: 'performance_drop',
    severity: 'warning',
    status: 'acted',
    title: 'Caída en Rendimiento',
    message: 'Tus vistas han bajado 25% comparado con la semana pasada.',
    actionUrl: '/analytics/overview',
    actionLabel: 'Ver Dashboard',
    createdAt: new Date(Date.now() - 432000000).toISOString(),
    metadata: { currentViews: 1250, previousViews: 1680, changePercent: -25.6 },
  },
];

// ============================================
// HELPER FUNCTIONS
// ============================================

const getSeverityStyles = (severity: AlertSeverity) => {
  switch (severity) {
    case 'critical':
      return {
        bg: 'bg-red-50',
        border: 'border-red-200',
        icon: 'text-red-600',
        badge: 'bg-red-100 text-red-700',
      };
    case 'warning':
      return {
        bg: 'bg-amber-50',
        border: 'border-amber-200',
        icon: 'text-amber-600',
        badge: 'bg-amber-100 text-amber-700',
      };
    case 'info':
      return {
        bg: 'bg-blue-50',
        border: 'border-blue-200',
        icon: 'text-blue-600',
        badge: 'bg-blue-100 text-blue-700',
      };
  }
};

const getSeverityIcon = (severity: AlertSeverity) => {
  switch (severity) {
    case 'critical':
      return <FiAlertCircle className="w-5 h-5" />;
    case 'warning':
      return <FiAlertTriangle className="w-5 h-5" />;
    case 'info':
      return <FiInfo className="w-5 h-5" />;
  }
};

const getTypeIcon = (type: AlertType) => {
  const icons: Record<AlertType, React.ReactNode> = {
    slow_inventory: <FiClock className="w-4 h-4" />,
    price_drop_needed: <FiTrendingDown className="w-4 h-4" />,
    low_engagement: <FiEye className="w-4 h-4" />,
    lead_response_slow: <FiUsers className="w-4 h-4" />,
    competitor_price: <FiDollarSign className="w-4 h-4" />,
    inventory_low: <FiPackage className="w-4 h-4" />,
    performance_drop: <FiTrendingDown className="w-4 h-4" />,
    payment_due: <FiDollarSign className="w-4 h-4" />,
    review_needed: <FiCheckCircle className="w-4 h-4" />,
    opportunity: <FiInfo className="w-4 h-4" />,
  };
  return icons[type] || <FiBell className="w-4 h-4" />;
};

const getTimeAgo = (dateString: string) => {
  const date = new Date(dateString);
  const now = new Date();
  const diffMs = now.getTime() - date.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 60) return `Hace ${diffMins} min`;
  if (diffHours < 24) return `Hace ${diffHours}h`;
  if (diffDays < 7) return `Hace ${diffDays}d`;
  return date.toLocaleDateString('es-DO');
};

// ============================================
// COMPONENTS
// ============================================

const SummaryCard = ({
  title,
  value,
  icon,
  color,
}: {
  title: string;
  value: number;
  icon: React.ReactNode;
  color: string;
}) => {
  const colorClasses: Record<string, string> = {
    red: 'bg-red-50 text-red-600',
    amber: 'bg-amber-50 text-amber-600',
    blue: 'bg-blue-50 text-blue-600',
    green: 'bg-green-50 text-green-600',
    gray: 'bg-gray-50 text-gray-600',
  };

  return (
    <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
      <div className={`inline-flex p-3 rounded-lg ${colorClasses[color]}`}>{icon}</div>
      <div className="mt-3">
        <p className="text-2xl font-bold text-gray-900">{value}</p>
        <p className="text-sm text-gray-500">{title}</p>
      </div>
    </div>
  );
};

interface AlertCardProps {
  alert: DealerAlert;
  onMarkAsRead: (id: string) => void;
  onDismiss: (id: string) => void;
  onAction: (id: string) => void;
}

const AlertCard = ({ alert, onMarkAsRead, onDismiss, onAction }: AlertCardProps) => {
  const styles = getSeverityStyles(alert.severity);
  const isUnread = alert.status === 'unread';

  return (
    <div
      className={`
      relative rounded-xl border p-5 transition-all
      ${styles.bg} ${styles.border}
      ${isUnread ? 'shadow-md' : 'opacity-80'}
    `}
    >
      {/* Status indicator */}
      {isUnread && <span className="absolute top-4 right-4 w-2.5 h-2.5 bg-blue-500 rounded-full" />}

      <div className="flex items-start gap-4">
        {/* Icon */}
        <div className={`p-2.5 rounded-lg ${styles.badge}`}>{getSeverityIcon(alert.severity)}</div>

        {/* Content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 mb-1">
            <span
              className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-medium ${styles.badge}`}
            >
              {getTypeIcon(alert.type)}
              {alert.severity.charAt(0).toUpperCase() + alert.severity.slice(1)}
            </span>
            <span className="text-xs text-gray-500">{getTimeAgo(alert.createdAt)}</span>
          </div>

          <h3 className="text-base font-semibold text-gray-900 mb-1">{alert.title}</h3>

          <p className="text-sm text-gray-600 mb-3">{alert.message}</p>

          {/* Vehicle info if applicable */}
          {alert.vehicleTitle && (
            <p className="text-xs text-gray-500 mb-3">
              Vehículo: <span className="font-medium">{alert.vehicleTitle}</span>
            </p>
          )}

          {/* Metadata */}
          {alert.metadata && Object.keys(alert.metadata).length > 0 && (
            <div className="flex flex-wrap gap-2 mb-3">
              {alert.metadata.vehicleCount && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-white rounded text-xs text-gray-600">
                  <FiPackage className="w-3 h-3" />
                  {alert.metadata.vehicleCount} vehículos
                </span>
              )}
              {alert.metadata.avgDays && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-white rounded text-xs text-gray-600">
                  <FiClock className="w-3 h-3" />~{alert.metadata.avgDays} días promedio
                </span>
              )}
              {alert.metadata.suggestedPrice && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-green-100 rounded text-xs text-green-700">
                  <FiDollarSign className="w-3 h-3" />
                  Sugerido: ${alert.metadata.suggestedPrice.toLocaleString()}
                </span>
              )}
              {alert.metadata.difference && (
                <span className="inline-flex items-center gap-1 px-2 py-1 bg-red-100 rounded text-xs text-red-700">
                  <FiTrendingDown className="w-3 h-3" />-{alert.metadata.difference}%
                </span>
              )}
            </div>
          )}

          {/* Actions */}
          <div className="flex items-center gap-2">
            {alert.actionUrl && (
              <a
                href={alert.actionUrl}
                onClick={() => onAction(alert.id)}
                className="inline-flex items-center gap-1 px-3 py-1.5 bg-gray-900 text-white text-sm font-medium rounded-lg hover:bg-gray-800 transition-colors"
              >
                {alert.actionLabel || 'Ver'}
                <FiChevronRight className="w-4 h-4" />
              </a>
            )}

            {isUnread && (
              <button
                onClick={() => onMarkAsRead(alert.id)}
                className="inline-flex items-center gap-1 px-3 py-1.5 text-sm font-medium text-gray-600 hover:text-gray-900 hover:bg-white rounded-lg transition-colors"
              >
                <FiCheck className="w-4 h-4" />
                Marcar leída
              </button>
            )}

            <button
              onClick={() => onDismiss(alert.id)}
              className="inline-flex items-center gap-1 p-1.5 text-gray-400 hover:text-red-600 hover:bg-white rounded-lg transition-colors ml-auto"
              title="Descartar"
            >
              <FiX className="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

// ============================================
// MAIN COMPONENT
// ============================================

type FilterTab = 'all' | 'unread' | 'critical' | 'warning' | 'info';

interface AlertsManagementPageProps {
  dealerId?: string;
}

export const AlertsManagementPage = ({ dealerId = 'demo' }: AlertsManagementPageProps) => {
  const [alerts, setAlerts] = useState<DealerAlert[]>(mockAlerts);
  const [activeTab, setActiveTab] = useState<FilterTab>('all');
  const [isLoading, setIsLoading] = useState(false);

  // Log dealerId for future API integration
  console.debug('Loading alerts for dealer:', dealerId);

  // Calculate summary from current alerts
  const summary: AlertSummary = {
    total: alerts.length,
    unread: alerts.filter((a) => a.status === 'unread').length,
    critical: alerts.filter((a) => a.severity === 'critical').length,
    warning: alerts.filter((a) => a.severity === 'warning').length,
    info: alerts.filter((a) => a.severity === 'info').length,
    byType: alerts.reduce(
      (acc, alert) => {
        acc[alert.type] = (acc[alert.type] || 0) + 1;
        return acc;
      },
      {} as Record<AlertType, number>
    ),
  };

  const handleRefresh = useCallback(() => {
    setIsLoading(true);
    setTimeout(() => setIsLoading(false), 1000);
  }, []);

  const handleMarkAsRead = useCallback((id: string) => {
    setAlerts((prev) =>
      prev.map((a) =>
        a.id === id ? { ...a, status: 'read' as AlertStatus, readAt: new Date().toISOString() } : a
      )
    );
  }, []);

  const handleDismiss = useCallback((id: string) => {
    setAlerts((prev) => prev.filter((a) => a.id !== id));
  }, []);

  const handleAction = useCallback(
    (id: string) => {
      handleMarkAsRead(id);
      setAlerts((prev) =>
        prev.map((a) => (a.id === id ? { ...a, status: 'acted' as AlertStatus } : a))
      );
    },
    [handleMarkAsRead]
  );

  const handleMarkAllAsRead = useCallback(() => {
    setAlerts((prev) =>
      prev.map((a) =>
        a.status === 'unread'
          ? { ...a, status: 'read' as AlertStatus, readAt: new Date().toISOString() }
          : a
      )
    );
  }, []);

  const handleDismissAll = useCallback(() => {
    if (confirm('¿Estás seguro de descartar todas las alertas?')) {
      setAlerts([]);
    }
  }, []);

  // Filter alerts based on active tab
  const filteredAlerts = alerts.filter((alert) => {
    switch (activeTab) {
      case 'unread':
        return alert.status === 'unread';
      case 'critical':
        return alert.severity === 'critical';
      case 'warning':
        return alert.severity === 'warning';
      case 'info':
        return alert.severity === 'info';
      default:
        return true;
    }
  });

  const tabs: { id: FilterTab; label: string; count: number }[] = [
    { id: 'all', label: 'Todas', count: summary.total },
    { id: 'unread', label: 'Sin leer', count: summary.unread },
    { id: 'critical', label: 'Críticas', count: summary.critical },
    { id: 'warning', label: 'Advertencias', count: summary.warning },
    { id: 'info', label: 'Información', count: summary.info },
  ];

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        {/* Header */}
        <div className="bg-white border-b border-gray-200">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
            <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 flex items-center gap-2">
                  <FiBell className="w-7 h-7 text-amber-600" />
                  Alertas
                  {summary.unread > 0 && (
                    <span className="inline-flex items-center justify-center px-2.5 py-0.5 rounded-full text-sm font-bold bg-red-500 text-white">
                      {summary.unread}
                    </span>
                  )}
                </h1>
                <p className="text-sm text-gray-500 mt-1">
                  Notificaciones y alertas importantes para tu negocio
                </p>
              </div>

              <div className="flex items-center gap-3">
                <button
                  onClick={handleRefresh}
                  disabled={isLoading}
                  className="p-2 text-gray-500 hover:text-gray-700 hover:bg-gray-100 rounded-lg"
                >
                  <FiRefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
                </button>

                {summary.unread > 0 && (
                  <button
                    onClick={handleMarkAllAsRead}
                    className="flex items-center gap-2 px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200"
                  >
                    <FiCheck className="w-4 h-4" />
                    Marcar todo leído
                  </button>
                )}

                <button
                  onClick={handleDismissAll}
                  className="flex items-center gap-2 px-4 py-2 text-red-600 bg-red-50 rounded-lg hover:bg-red-100"
                >
                  <FiTrash2 className="w-4 h-4" />
                  Limpiar
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          {/* Summary Cards */}
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 mb-8">
            <SummaryCard
              title="Total Alertas"
              value={summary.total}
              icon={<FiBell className="w-6 h-6" />}
              color="gray"
            />
            <SummaryCard
              title="Sin Leer"
              value={summary.unread}
              icon={<FiEye className="w-6 h-6" />}
              color="blue"
            />
            <SummaryCard
              title="Críticas"
              value={summary.critical}
              icon={<FiAlertCircle className="w-6 h-6" />}
              color="red"
            />
            <SummaryCard
              title="Advertencias"
              value={summary.warning}
              icon={<FiAlertTriangle className="w-6 h-6" />}
              color="amber"
            />
            <SummaryCard
              title="Información"
              value={summary.info}
              icon={<FiInfo className="w-6 h-6" />}
              color="blue"
            />
          </div>

          {/* Tabs */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 mb-6">
            <div className="flex overflow-x-auto">
              {tabs.map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`
                    flex items-center gap-2 px-6 py-4 text-sm font-medium whitespace-nowrap border-b-2 transition-colors
                    ${
                      activeTab === tab.id
                        ? 'border-blue-500 text-blue-600'
                        : 'border-transparent text-gray-500 hover:text-gray-700'
                    }
                  `}
                >
                  {tab.label}
                  <span
                    className={`
                    px-2 py-0.5 rounded-full text-xs font-bold
                    ${activeTab === tab.id ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500'}
                  `}
                  >
                    {tab.count}
                  </span>
                </button>
              ))}
            </div>
          </div>

          {/* Alerts List */}
          <div className="space-y-4">
            {filteredAlerts.length === 0 ? (
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-12 text-center">
                <FiBell className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                <h3 className="text-lg font-medium text-gray-900 mb-2">No hay alertas</h3>
                <p className="text-sm text-gray-500">
                  {activeTab === 'all'
                    ? 'No tienes alertas pendientes. ¡Todo está en orden!'
                    : `No hay alertas en la categoría "${tabs.find((t) => t.id === activeTab)?.label}"`}
                </p>
              </div>
            ) : (
              filteredAlerts.map((alert) => (
                <AlertCard
                  key={alert.id}
                  alert={alert}
                  onMarkAsRead={handleMarkAsRead}
                  onDismiss={handleDismiss}
                  onAction={handleAction}
                />
              ))
            )}
          </div>

          {/* Settings Link */}
          <div className="mt-8 bg-white rounded-xl shadow-sm border border-gray-100 p-6">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">Configuración de Alertas</h3>
                <p className="text-sm text-gray-500 mt-1">
                  Personaliza qué alertas recibir y cómo recibirlas
                </p>
              </div>
              <a
                href="/settings/notifications"
                className="flex items-center gap-2 px-4 py-2 text-blue-600 hover:bg-blue-50 rounded-lg"
              >
                <FiSettings className="w-4 h-4" />
                Configurar
                <FiChevronRight className="w-4 h-4" />
              </a>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default AlertsManagementPage;

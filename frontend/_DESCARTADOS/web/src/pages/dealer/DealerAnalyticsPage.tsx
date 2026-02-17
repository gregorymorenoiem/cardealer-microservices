/**
 * DealerAnalyticsPage - Analytics del Dealer con nuevo layout
 */

import { useState, useEffect } from 'react';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiTrendingUp,
  FiTrendingDown,
  FiEye,
  FiTarget,
  FiMessageSquare,
  FiDollarSign,
  FiCalendar,
  FiArrowRight,
  FiDownload,
} from 'react-icons/fi';
import { FaCar } from 'react-icons/fa';
import { useAuthStore } from '@/store/authStore';
import useDealerAnalytics from '@/hooks/useDealerAnalytics';

export default function DealerAnalyticsPage() {
  const user = useAuthStore((state) => state.user);
  const [dateRange, setDateRange] = useState({
    fromDate: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000), // 30 days ago
    toDate: new Date(),
  });

  // Use the custom hook to fetch real data
  const { dashboardSummary, quickStats, conversionFunnel, isLoading, error, refreshData } =
    useDealerAnalytics({
      dealerId: user?.dealerId || 'a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', // Use test dealer ID if not available
      fromDate: dateRange.fromDate,
      toDate: dateRange.toDate,
    });

  // Handle date range change
  const handleDateRangeChange = (days: number) => {
    setDateRange({
      fromDate: new Date(Date.now() - days * 24 * 60 * 60 * 1000),
      toDate: new Date(),
    });
  };

  if (isLoading) {
    return (
      <DealerPortalLayout>
        <div className="flex items-center justify-center h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </DealerPortalLayout>
    );
  }

  if (error) {
    return (
      <DealerPortalLayout>
        <div className="p-8 bg-red-50 border border-red-200 rounded-lg">
          <p className="text-red-600 font-medium">{error}</p>
          <button
            onClick={refreshData}
            className="mt-4 px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700"
          >
            Reintentar
          </button>
        </div>
      </DealerPortalLayout>
    );
  }
  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 space-y-6">
        {/* Header */}
        <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900">Analytics</h1>
            <p className="text-gray-500 mt-1">Analiza el rendimiento de tu negocio</p>
          </div>
          <div className="flex items-center gap-3">
            <select
              className="px-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              onChange={(e) => handleDateRangeChange(Number(e.target.value))}
            >
              <option value="30">Últimos 30 días</option>
              <option value="7">Últimos 7 días</option>
              <option value="90">Últimos 90 días</option>
              <option value="365">Este año</option>
            </select>
            <button className="flex items-center gap-2 px-4 py-2.5 border border-gray-200 rounded-xl text-gray-700 hover:bg-gray-50">
              <FiDownload className="w-4 h-4" />
              <span>Exportar</span>
            </button>
          </div>
        </div>

        {/* Stats Overview */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 lg:gap-6">
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-purple-100 rounded-xl">
                <FiEye className="w-6 h-6 text-purple-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                {quickStats?.viewsGrowth ? `+${quickStats.viewsGrowth.toFixed(1)}%` : '+0%'}
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">
              {quickStats?.totalViews?.toLocaleString() || '0'}
            </p>
            <p className="text-sm text-gray-500">Vistas Totales</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-red-100 rounded-xl">
                <FiTarget className="w-6 h-6 text-red-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                {quickStats?.leadsGrowth ? `+${quickStats.leadsGrowth.toFixed(1)}%` : '+0%'}
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">
              {quickStats?.totalLeads?.toLocaleString() || '0'}
            </p>
            <p className="text-sm text-gray-500">Leads Generados</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-emerald-100 rounded-xl">
                <FiMessageSquare className="w-6 h-6 text-emerald-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                {quickStats?.contactsGrowth ? `+${quickStats.contactsGrowth.toFixed(1)}%` : '+0%'}
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">
              {quickStats?.totalContacts?.toLocaleString() || '0'}
            </p>
            <p className="text-sm text-gray-500">Consultas</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-blue-100 rounded-xl">
                <FaCar className="w-6 h-6 text-blue-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                {quickStats?.salesGrowth ? `+${quickStats.salesGrowth.toFixed(1)}%` : '+0%'}
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">
              {quickStats?.actualSales?.toLocaleString() || '0'}
            </p>
            <p className="text-sm text-gray-500">Ventas del Mes</p>
          </div>
        </div>

        {/* Conversion Funnel */}
        {conversionFunnel && (
          <div className="bg-gradient-to-br from-blue-600 to-indigo-600 rounded-2xl p-6 text-white">
            <h3 className="text-lg font-bold mb-6">Embudo de Conversión</h3>
            <div className="flex items-center justify-between gap-4">
              <div className="flex-1 text-center">
                <p className="text-4xl font-bold mb-2">{conversionFunnel.views.toLocaleString()}</p>
                <p className="text-blue-200 text-sm">Vistas</p>
              </div>
              <FiArrowRight className="w-8 h-8 text-blue-300" />
              <div className="flex-1 text-center">
                <p className="text-4xl font-bold mb-2">
                  {conversionFunnel.inquiries.toLocaleString()}
                </p>
                <p className="text-blue-200 text-sm">Consultas</p>
              </div>
              <FiArrowRight className="w-8 h-8 text-blue-300" />
              <div className="flex-1 text-center">
                <p className="text-4xl font-bold mb-2">{conversionFunnel.leads.toLocaleString()}</p>
                <p className="text-blue-200 text-sm">Leads</p>
              </div>
              <FiArrowRight className="w-8 h-8 text-blue-300" />
              <div className="flex-1 text-center">
                <p className="text-4xl font-bold mb-2">
                  {conversionFunnel.closedSales.toLocaleString()}
                </p>
                <p className="text-blue-200 text-sm">Ventas</p>
              </div>
            </div>
            <div className="mt-6 pt-4 border-t border-white/20">
              <p className="text-center text-blue-200 text-sm">
                Tasa de conversión global:{' '}
                <span className="text-white font-bold">
                  {conversionFunnel.conversionRate.toFixed(2)}%
                </span>
              </p>
            </div>
          </div>
        )}

        {/* Analytics Summary */}
        {dashboardSummary && dashboardSummary.analytics && (
          <div className="grid lg:grid-cols-2 gap-6">
            <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Resumen de Analíticas</h3>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Vistas Totales</span>
                  <span className="font-bold text-gray-900">
                    {(dashboardSummary.analytics.totalViews ?? 0).toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Vistas Únicas</span>
                  <span className="font-bold text-gray-900">
                    {(dashboardSummary.analytics.uniqueViews ?? 0).toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Tiempo Promedio</span>
                  <span className="font-bold text-gray-900">
                    {(dashboardSummary.analytics.averageViewDuration ?? 0).toFixed(1)}s
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Tasa de Conversión</span>
                  <span className="font-bold text-gray-900">
                    {(dashboardSummary.analytics.conversionRate ?? 0).toFixed(2)}%
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Precio Promedio</span>
                  <span className="font-bold text-gray-900">
                    ${(dashboardSummary.analytics.averageVehiclePrice ?? 0).toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Días en Mercado</span>
                  <span className="font-bold text-gray-900">
                    {(dashboardSummary.analytics.averageDaysOnMarket ?? 0).toFixed(0)} días
                  </span>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Inventario</h3>
              <div className="space-y-4">
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Vehículos Activos</span>
                  <span className="font-bold text-gray-900">
                    {dashboardSummary.analytics?.activeListings ?? 0}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Vehículos Vendidos</span>
                  <span className="font-bold text-gray-900">
                    {dashboardSummary.analytics?.soldVehicles ?? 0}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Ingresos Totales</span>
                  <span className="font-bold text-gray-900">
                    ${(dashboardSummary.analytics?.totalRevenue ?? 0).toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Precio Promedio</span>
                  <span className="font-bold text-gray-900">
                    ${(dashboardSummary.analytics?.averageVehiclePrice ?? 0).toLocaleString()}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-gray-600">Días Promedio en Mercado</span>
                  <span className="font-bold text-blue-600">
                    {(dashboardSummary.analytics?.averageDaysOnMarket ?? 0).toFixed(0)} días
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}

/**
 * DealerAnalyticsPage - Analytics del Dealer con nuevo layout
 */

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

// Mock data para gráficos
const mockChartData = {
  views: [120, 150, 180, 220, 195, 280, 320, 290, 350, 380, 420, 450],
  leads: [12, 15, 18, 22, 19, 28, 32, 29, 35, 38, 42, 45],
  months: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
};

const topVehicles = [
  { name: 'Toyota Corolla 2022', views: 450, inquiries: 32, conversion: 12.5 },
  { name: 'Honda Civic 2021', views: 380, inquiries: 28, conversion: 10.2 },
  { name: 'BMW X5 2023', views: 320, inquiries: 45, conversion: 18.5 },
  { name: 'Mercedes C300 2022', views: 280, inquiries: 22, conversion: 8.8 },
];

export default function DealerAnalyticsPage() {
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
            <select className="px-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500/20">
              <option>Últimos 30 días</option>
              <option>Últimos 7 días</option>
              <option>Este mes</option>
              <option>Este año</option>
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
                +23%
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">12,450</p>
            <p className="text-sm text-gray-500">Vistas Totales</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-red-100 rounded-xl">
                <FiTarget className="w-6 h-6 text-red-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                +15%
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">185</p>
            <p className="text-sm text-gray-500">Leads Generados</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-emerald-100 rounded-xl">
                <FiMessageSquare className="w-6 h-6 text-emerald-600" />
              </div>
              <div className="flex items-center gap-1 text-green-600 text-sm font-medium">
                <FiTrendingUp className="w-4 h-4" />
                +8%
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">423</p>
            <p className="text-sm text-gray-500">Consultas</p>
          </div>

          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-4">
              <div className="p-3 bg-blue-100 rounded-xl">
                <FaCar className="w-6 h-6 text-blue-600" />
              </div>
              <div className="flex items-center gap-1 text-red-600 text-sm font-medium">
                <FiTrendingDown className="w-4 h-4" />
                -2%
              </div>
            </div>
            <p className="text-3xl font-bold text-gray-900 mb-1">18</p>
            <p className="text-sm text-gray-500">Ventas del Mes</p>
          </div>
        </div>

        {/* Charts Row */}
        <div className="grid lg:grid-cols-2 gap-6">
          {/* Views Chart */}
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-bold text-gray-900">Vistas por Mes</h3>
              <FiEye className="w-5 h-5 text-gray-400" />
            </div>
            {/* Simple bar visualization */}
            <div className="flex items-end justify-between h-48 gap-2">
              {mockChartData.views.map((value, index) => {
                const height = (value / Math.max(...mockChartData.views)) * 100;
                return (
                  <div key={index} className="flex-1 flex flex-col items-center gap-2">
                    <div
                      className="w-full bg-gradient-to-t from-blue-600 to-blue-400 rounded-t-lg transition-all hover:from-blue-700 hover:to-blue-500"
                      style={{ height: `${height}%` }}
                    />
                    <span className="text-xs text-gray-400">{mockChartData.months[index]}</span>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Leads Chart */}
          <div className="bg-white rounded-2xl p-6 shadow-sm border border-gray-100">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-lg font-bold text-gray-900">Leads por Mes</h3>
              <FiTarget className="w-5 h-5 text-gray-400" />
            </div>
            {/* Simple bar visualization */}
            <div className="flex items-end justify-between h-48 gap-2">
              {mockChartData.leads.map((value, index) => {
                const height = (value / Math.max(...mockChartData.leads)) * 100;
                return (
                  <div key={index} className="flex-1 flex flex-col items-center gap-2">
                    <div
                      className="w-full bg-gradient-to-t from-emerald-600 to-emerald-400 rounded-t-lg transition-all hover:from-emerald-700 hover:to-emerald-500"
                      style={{ height: `${height}%` }}
                    />
                    <span className="text-xs text-gray-400">{mockChartData.months[index]}</span>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Top Vehicles Table */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          <div className="p-6 border-b border-gray-100">
            <h3 className="text-lg font-bold text-gray-900">Vehículos Más Populares</h3>
          </div>
          <table className="w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-left px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                  Vehículo
                </th>
                <th className="text-center px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                  Vistas
                </th>
                <th className="text-center px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                  Consultas
                </th>
                <th className="text-center px-6 py-4 text-xs font-semibold text-gray-500 uppercase">
                  Conversión
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {topVehicles.map((vehicle, index) => (
                <tr key={index} className="hover:bg-gray-50">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <span className="w-8 h-8 bg-blue-100 rounded-lg flex items-center justify-center text-blue-600 font-bold text-sm">
                        {index + 1}
                      </span>
                      <span className="font-medium text-gray-900">{vehicle.name}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-center text-gray-600">
                    {vehicle.views.toLocaleString()}
                  </td>
                  <td className="px-6 py-4 text-center text-gray-600">{vehicle.inquiries}</td>
                  <td className="px-6 py-4 text-center">
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-medium ${
                        vehicle.conversion > 15
                          ? 'bg-green-100 text-green-700'
                          : vehicle.conversion > 10
                            ? 'bg-amber-100 text-amber-700'
                            : 'bg-gray-100 text-gray-600'
                      }`}
                    >
                      {vehicle.conversion}%
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Conversion Funnel */}
        <div className="bg-gradient-to-br from-blue-600 to-indigo-600 rounded-2xl p-6 text-white">
          <h3 className="text-lg font-bold mb-6">Embudo de Conversión</h3>
          <div className="flex items-center justify-between gap-4">
            <div className="flex-1 text-center">
              <p className="text-4xl font-bold mb-2">12,450</p>
              <p className="text-blue-200 text-sm">Vistas</p>
            </div>
            <FiArrowRight className="w-8 h-8 text-blue-300" />
            <div className="flex-1 text-center">
              <p className="text-4xl font-bold mb-2">423</p>
              <p className="text-blue-200 text-sm">Consultas</p>
            </div>
            <FiArrowRight className="w-8 h-8 text-blue-300" />
            <div className="flex-1 text-center">
              <p className="text-4xl font-bold mb-2">185</p>
              <p className="text-blue-200 text-sm">Leads</p>
            </div>
            <FiArrowRight className="w-8 h-8 text-blue-300" />
            <div className="flex-1 text-center">
              <p className="text-4xl font-bold mb-2">18</p>
              <p className="text-blue-200 text-sm">Ventas</p>
            </div>
          </div>
          <div className="mt-6 pt-4 border-t border-white/20">
            <p className="text-center text-blue-200 text-sm">
              Tasa de conversión global: <span className="text-white font-bold">0.14%</span>
            </p>
          </div>
        </div>
      </div>
    </DealerPortalLayout>
  );
}

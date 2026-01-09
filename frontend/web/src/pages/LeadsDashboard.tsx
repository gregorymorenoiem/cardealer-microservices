import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import leadScoringService, {
  LeadDto,
  LeadStatisticsDto,
  LeadTemperature,
  LeadStatus,
} from '@/services/leadScoringService';
import {
  FiSearch,
  FiFilter,
  FiDownload,
  FiChevronRight,
  FiPhone,
  FiMail,
  FiMessageSquare,
} from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';

export const LeadsDashboard = () => {
  const navigate = useNavigate();

  // Estados
  const [leads, setLeads] = useState<LeadDto[]>([]);
  const [statistics, setStatistics] = useState<LeadStatisticsDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filtros
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [temperatureFilter, setTemperatureFilter] = useState<LeadTemperature | null>(null);
  const [statusFilter, setStatusFilter] = useState<LeadStatus | null>(null);
  const [searchTerm, setSearchTerm] = useState('');

  // Paginación
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // TODO: Obtener dealerId del usuario autenticado
  const dealerId = 'your-dealer-id-here'; // Replace with actual dealer ID from auth context

  useEffect(() => {
    loadLeads();
    loadStatistics();
  }, [page, temperatureFilter, statusFilter]);

  const loadLeads = async () => {
    try {
      setIsLoading(true);
      const response = await leadScoringService.getLeadsByDealer(
        dealerId,
        page,
        pageSize,
        temperatureFilter,
        statusFilter,
        searchTerm || null
      );
      setLeads(response.leads);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
    } catch (err: any) {
      setError(err.message || 'Error loading leads');
    } finally {
      setIsLoading(false);
    }
  };

  const loadStatistics = async () => {
    try {
      const stats = await leadScoringService.getLeadStatistics(dealerId);
      setStatistics(stats);
    } catch (err: any) {
      console.error('Error loading statistics:', err);
    }
  };

  const handleSearch = () => {
    setPage(1);
    loadLeads();
  };

  const handleLeadClick = (leadId: string) => {
    navigate(`/dealer/leads/${leadId}`);
  };

  if (isLoading && !leads.length) {
    return (
      <MainLayout>
        <div className="flex justify-center items-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Leads</h1>
          <p className="text-gray-600">
            Gestiona tus leads y maximiza conversiones con scoring automático
          </p>
        </div>

        {/* Statistics Cards */}
        {statistics && (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
            {/* Total Leads */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm mb-1">Total Leads</p>
                  <p className="text-3xl font-bold text-gray-900">{statistics.totalLeads}</p>
                </div>
                <div className="bg-blue-100 rounded-full p-3">
                  <span className="text-2xl">👥</span>
                </div>
              </div>
            </div>

            {/* Hot Leads */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm mb-1">Leads HOT 🔥</p>
                  <p className="text-3xl font-bold text-red-600">{statistics.hotLeads}</p>
                  <p className="text-xs text-gray-500 mt-1">¡Contactar ahora!</p>
                </div>
                <div className="bg-red-100 rounded-full p-3">
                  <span className="text-2xl">🔥</span>
                </div>
              </div>
            </div>

            {/* Average Score */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm mb-1">Score Promedio</p>
                  <p className="text-3xl font-bold text-gray-900">
                    {Math.round(statistics.averageScore)}
                  </p>
                  <p className="text-xs text-gray-500 mt-1">De 100 puntos</p>
                </div>
                <div className="bg-yellow-100 rounded-full p-3">
                  <span className="text-2xl">📊</span>
                </div>
              </div>
            </div>

            {/* Conversion Rate */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 text-sm mb-1">Tasa de Conversión</p>
                  <p className="text-3xl font-bold text-green-600">
                    {statistics.conversionRate.toFixed(1)}%
                  </p>
                  <p className="text-xs text-gray-500 mt-1">
                    {statistics.convertedLeads} convertidos
                  </p>
                </div>
                <div className="bg-green-100 rounded-full p-3">
                  <span className="text-2xl">✅</span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Filters and Search */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Search */}
            <div className="md:col-span-2">
              <div className="relative">
                <FiSearch className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar por nombre, email, teléfono..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            {/* Temperature Filter */}
            <div>
              <select
                value={temperatureFilter || ''}
                onChange={(e) => setTemperatureFilter((e.target.value as LeadTemperature) || null)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Todas las temperaturas</option>
                <option value="Hot">🔥 HOT</option>
                <option value="Warm">🟡 WARM</option>
                <option value="Cold">❄️ COLD</option>
              </select>
            </div>

            {/* Status Filter */}
            <div>
              <select
                value={statusFilter || ''}
                onChange={(e) => setStatusFilter((e.target.value as LeadStatus) || null)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Todos los estados</option>
                <option value="New">Nuevo</option>
                <option value="Contacted">Contactado</option>
                <option value="Qualified">Calificado</option>
                <option value="Nurturing">En seguimiento</option>
                <option value="Negotiating">Negociando</option>
                <option value="Converted">Convertido</option>
              </select>
            </div>
          </div>

          <div className="flex justify-between items-center mt-4">
            <p className="text-sm text-gray-600">
              Mostrando {leads.length} de {totalCount} leads
            </p>
            <button
              onClick={handleSearch}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
            >
              <FiSearch className="w-4 h-4" />
              Buscar
            </button>
          </div>
        </div>

        {/* Leads List */}
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        <div className="bg-white rounded-lg shadow-md overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Usuario
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Vehículo
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Score
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Temp
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Estado
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Última Interacción
                </th>
                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                  Acciones
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {leads.map((lead) => (
                <tr
                  key={lead.id}
                  onClick={() => handleLeadClick(lead.id)}
                  className="hover:bg-gray-50 cursor-pointer transition-colors"
                >
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div>
                      <div className="text-sm font-medium text-gray-900">{lead.userFullName}</div>
                      <div className="text-sm text-gray-500">{lead.userEmail}</div>
                      {lead.userPhone && (
                        <div className="text-xs text-gray-400">{lead.userPhone}</div>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <div className="text-sm text-gray-900">{lead.vehicleTitle}</div>
                    <div className="text-sm text-gray-500">
                      ${lead.vehiclePrice.toLocaleString()}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center">
                      <div className="w-16">
                        <div className="text-sm font-bold text-gray-900">{lead.score}</div>
                        <div className="w-full bg-gray-200 rounded-full h-2">
                          <div
                            className={`h-2 rounded-full ${leadScoringService.getScoreBarClass(lead.score)}`}
                            style={{ width: `${lead.score}%` }}
                          ></div>
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        lead.temperature === 'Hot'
                          ? 'bg-red-100 text-red-800'
                          : lead.temperature === 'Warm'
                            ? 'bg-orange-100 text-orange-800'
                            : 'bg-blue-100 text-blue-800'
                      }`}
                    >
                      {leadScoringService.getTemperatureEmoji(lead.temperature)} {lead.temperature}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                      {lead.status}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {leadScoringService.formatRelativeTime(lead.lastInteractionAt)}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                    <div className="flex items-center justify-end gap-2">
                      {lead.userPhone && (
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            window.open(`tel:${lead.userPhone}`);
                          }}
                          className="text-blue-600 hover:text-blue-900"
                          title="Llamar"
                        >
                          <FiPhone className="w-4 h-4" />
                        </button>
                      )}
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          window.open(`mailto:${lead.userEmail}`);
                        }}
                        className="text-blue-600 hover:text-blue-900"
                        title="Email"
                      >
                        <FiMail className="w-4 h-4" />
                      </button>
                      <FiChevronRight className="w-4 h-4 text-gray-400" />
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {leads.length === 0 && !isLoading && (
            <div className="text-center py-12">
              <p className="text-gray-500 text-lg">No hay leads disponibles</p>
              <p className="text-gray-400 text-sm mt-2">
                Los leads aparecerán aquí cuando los usuarios interactúen con tus vehículos
              </p>
            </div>
          )}
        </div>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex justify-center items-center gap-2 mt-6">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
            >
              Anterior
            </button>
            <span className="px-4 py-2 text-gray-700">
              Página {page} de {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-4 py-2 border border-gray-300 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50"
            >
              Siguiente
            </button>
          </div>
        )}
      </div>
    </MainLayout>
  );
};

export default LeadsDashboard;

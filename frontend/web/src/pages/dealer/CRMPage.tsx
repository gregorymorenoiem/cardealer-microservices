/**
 * CRM Page - Kanban Board
 *
 * Visual pipeline management for deals and leads
 * Drag-and-drop interface for moving deals between stages
 *
 * Updated to use TanStack Query hooks for data fetching
 */

import { useState, useMemo, useEffect } from 'react';
import { Link } from 'react-router-dom';
import {
  Plus,
  Users,
  DollarSign,
  TrendingUp,
  Phone,
  Mail,
  Calendar,
  MoreVertical,
  ChevronDown,
  Filter,
  Search,
  ArrowRight,
  Lock,
  Crown,
  RefreshCw,
  User,
  Briefcase,
  Clock,
  CheckCircle,
  Loader2,
} from 'lucide-react';
import { usePermissions } from '@/hooks/usePermissions';
import {
  useKanbanBoard,
  useRecentLeads,
  useMoveDeal,
  usePipelines,
  useActivities,
} from '@/hooks/useCRM';
import type { Deal, Lead, Stage } from '@/mocks/crmData';

// Deal Card Component
const DealCard = ({
  deal,
  onMove,
}: {
  deal: Deal;
  onMove: (dealId: string, direction: 'prev' | 'next') => void;
}) => {
  const [showMenu, setShowMenu] = useState(false);

  const formatValue = (value: number) => {
    if (value >= 1000000) {
      return `$${(value / 1000000).toFixed(1)}M`;
    }
    return `$${(value / 1000).toFixed(0)}K`;
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-3 shadow-sm hover:shadow-md transition-shadow cursor-grab active:cursor-grabbing">
      <div className="flex items-start justify-between mb-2">
        <h4 className="font-medium text-gray-900 text-sm line-clamp-2">{deal.title}</h4>
        <div className="relative">
          <button
            onClick={() => setShowMenu(!showMenu)}
            className="p-1 text-gray-400 hover:text-gray-600 rounded"
          >
            <MoreVertical className="h-4 w-4" />
          </button>
          {showMenu && (
            <>
              <div className="fixed inset-0" onClick={() => setShowMenu(false)} />
              <div className="absolute right-0 top-6 bg-white rounded-lg shadow-lg border border-gray-200 py-1 z-10 min-w-[120px]">
                <button
                  onClick={() => {
                    onMove(deal.id, 'prev');
                    setShowMenu(false);
                  }}
                  className="w-full px-3 py-1.5 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                >
                  <ArrowRight className="h-4 w-4 rotate-180" />
                  Anterior
                </button>
                <button
                  onClick={() => {
                    onMove(deal.id, 'next');
                    setShowMenu(false);
                  }}
                  className="w-full px-3 py-1.5 text-left text-sm text-gray-700 hover:bg-gray-50 flex items-center gap-2"
                >
                  <ArrowRight className="h-4 w-4" />
                  Siguiente
                </button>
              </div>
            </>
          )}
        </div>
      </div>

      <div className="flex items-center gap-2 mb-2">
        <span className="text-lg font-semibold text-gray-900">{formatValue(deal.value)}</span>
        <span className="text-xs text-gray-500">{deal.currency}</span>
      </div>

      {deal.description && (
        <p className="text-xs text-gray-500 mb-2 line-clamp-2">{deal.description}</p>
      )}

      <div className="flex items-center justify-between text-xs">
        <div className="flex items-center gap-1 text-gray-500">
          <Calendar className="h-3 w-3" />
          {deal.expectedCloseDate
            ? new Date(deal.expectedCloseDate).toLocaleDateString('es-MX', {
                day: 'numeric',
                month: 'short',
              })
            : 'Sin fecha'}
        </div>
        <span
          className="px-1.5 py-0.5 rounded text-xs font-medium"
          style={{
            backgroundColor: `${deal.stageColor}20`,
            color: deal.stageColor,
          }}
        >
          {deal.probability}%
        </span>
      </div>

      {deal.tags.length > 0 && (
        <div className="flex flex-wrap gap-1 mt-2">
          {deal.tags.slice(0, 2).map((tag) => (
            <span key={tag} className="px-1.5 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">
              {tag}
            </span>
          ))}
          {deal.tags.length > 2 && (
            <span className="px-1.5 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">
              +{deal.tags.length - 2}
            </span>
          )}
        </div>
      )}
    </div>
  );
};

// Stage Column Component
const StageColumn = ({
  stage,
  deals,
  onMoveDeal,
}: {
  stage: Stage;
  deals: Deal[];
  onMoveDeal: (dealId: string, direction: 'prev' | 'next') => void;
}) => {
  const totalValue = deals.reduce((sum, d) => sum + d.value, 0);

  const formatValue = (value: number) => {
    if (value >= 1000000) {
      return `$${(value / 1000000).toFixed(1)}M`;
    }
    return `$${(value / 1000).toFixed(0)}K`;
  };

  return (
    <div className="flex-shrink-0 w-72 flex flex-col bg-gray-50 rounded-lg">
      {/* Stage Header */}
      <div className="p-3 rounded-t-lg" style={{ backgroundColor: `${stage.color}15` }}>
        <div className="flex items-center justify-between mb-1">
          <div className="flex items-center gap-2">
            <div className="w-3 h-3 rounded-full" style={{ backgroundColor: stage.color }} />
            <h3 className="font-semibold text-gray-900">{stage.name}</h3>
          </div>
          <span className="text-sm text-gray-500">{deals.length}</span>
        </div>
        <div className="text-sm text-gray-600">{formatValue(totalValue)}</div>
      </div>

      {/* Deals */}
      <div className="flex-1 p-2 space-y-2 overflow-y-auto max-h-[calc(100vh-350px)]">
        {deals.map((deal) => (
          <DealCard key={deal.id} deal={deal} onMove={onMoveDeal} />
        ))}

        {deals.length === 0 && (
          <div className="text-center py-8 text-gray-400">
            <Briefcase className="h-8 w-8 mx-auto mb-2 opacity-50" />
            <p className="text-sm">Sin deals</p>
          </div>
        )}
      </div>

      {/* Add Deal Button */}
      <div className="p-2 border-t border-gray-200">
        <button className="w-full flex items-center justify-center gap-1 py-2 text-sm text-gray-500 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
          <Plus className="h-4 w-4" />
          Agregar deal
        </button>
      </div>
    </div>
  );
};

// Leads Section Component
const LeadsSection = ({ leads }: { leads: Lead[] }) => {
  const getStatusColor = (status: Lead['status']) => {
    const colors: Record<Lead['status'], string> = {
      New: 'bg-blue-100 text-blue-700',
      Contacted: 'bg-purple-100 text-purple-700',
      Qualified: 'bg-green-100 text-green-700',
      Proposal: 'bg-yellow-100 text-yellow-700',
      Negotiation: 'bg-orange-100 text-orange-700',
      Won: 'bg-emerald-100 text-emerald-700',
      Lost: 'bg-red-100 text-red-700',
      Unqualified: 'bg-gray-100 text-gray-700',
    };
    return colors[status];
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-semibold text-gray-900 flex items-center gap-2">
          <Users className="h-5 w-5 text-blue-500" />
          Leads Recientes
        </h3>
        <Link to="/dealer/crm/leads" className="text-sm text-blue-600 hover:text-blue-700">
          Ver todos →
        </Link>
      </div>

      <div className="space-y-3">
        {leads.slice(0, 5).map((lead) => (
          <div
            key={lead.id}
            className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-full bg-gradient-to-br from-blue-400 to-blue-600 flex items-center justify-center text-white font-medium">
                {lead.firstName[0]}
                {lead.lastName[0]}
              </div>
              <div>
                <p className="font-medium text-gray-900">{lead.fullName}</p>
                <p className="text-sm text-gray-500">{lead.company || lead.email}</p>
              </div>
            </div>
            <div className="flex items-center gap-3">
              <div className="text-right">
                <span
                  className={`px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(lead.status)}`}
                >
                  {lead.status}
                </span>
                <p className="text-xs text-gray-500 mt-1">Score: {lead.score}</p>
              </div>
              <div className="flex gap-1">
                <button className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                  <Phone className="h-4 w-4" />
                </button>
                <button className="p-1.5 text-gray-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                  <Mail className="h-4 w-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

// Main CRM Page Component
const CRMPage = () => {
  const { portalAccess, isDealer, isDealerEmployee } = usePermissions();

  const [selectedPipeline, setSelectedPipeline] = useState<string>('');
  const [searchQuery, setSearchQuery] = useState('');

  // TanStack Query hooks for data fetching
  const pipelines = usePipelines();

  // Set selected pipeline to first available when data loads
  useEffect(() => {
    if (pipelines.data && pipelines.data.length > 0 && !selectedPipeline) {
      setSelectedPipeline(pipelines.data[0].id);
    }
  }, [pipelines.data, selectedPipeline]);

  // Only enable kanban when we have a valid pipeline ID
  const kanban = useKanbanBoard(selectedPipeline);
  const recentLeadsQuery = useRecentLeads(5);
  const recentActivitiesQuery = useActivities(undefined, { limit: 6 });
  const moveDealMutation = useMoveDeal();

  // Derived state
  const deals = kanban.deals;
  const leads = recentLeadsQuery.data ?? [];
  const activities = recentActivitiesQuery.data ?? [];
  const isLoading = kanban.isLoading || pipelines.isLoading;

  // Check CRM access - allow for dealers regardless of plan for now
  const hasAccess = portalAccess.crm || isDealer || isDealerEmployee;

  // Get current pipeline from TanStack Query data
  const currentPipeline = useMemo(() => {
    const allPipelines = pipelines.data ?? [];
    return allPipelines.find((p) => p.id === selectedPipeline) || allPipelines[0];
  }, [pipelines.data, selectedPipeline]);

  // Filter deals by search
  const filteredDeals = useMemo(() => {
    return deals.filter((deal) => {
      const matchesSearch =
        searchQuery === '' || deal.title.toLowerCase().includes(searchQuery.toLowerCase());
      return matchesSearch;
    });
  }, [deals, searchQuery]);

  // Pipeline stats
  const pipelineStats = useMemo(() => {
    const pipelineDeals = filteredDeals;
    const openDeals = pipelineDeals.filter((d) => d.status === 'Open');
    const wonDeals = pipelineDeals.filter((d) => d.status === 'Won');

    return {
      totalDeals: pipelineDeals.length,
      totalValue: pipelineDeals.reduce((sum, d) => sum + d.value, 0),
      openValue: openDeals.reduce((sum, d) => sum + d.value, 0),
      wonValue: wonDeals.reduce((sum, d) => sum + d.value, 0),
      avgDealSize:
        pipelineDeals.length > 0
          ? pipelineDeals.reduce((sum, d) => sum + d.value, 0) / pipelineDeals.length
          : 0,
      conversionRate:
        pipelineDeals.length > 0
          ? ((wonDeals.length / pipelineDeals.length) * 100).toFixed(1)
          : '0',
    };
  }, [filteredDeals]);

  if (!hasAccess) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto flex items-center justify-center min-h-[60vh]">
          <div className="text-center max-w-md">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <Lock className="h-8 w-8 text-gray-400" />
            </div>
            <h2 className="text-xl font-bold text-gray-900 mb-2">CRM No Disponible</h2>
            <p className="text-gray-600 mb-6">
              El módulo de CRM está disponible a partir del plan{' '}
              <span className="font-medium text-blue-600">PRO</span>. Actualiza tu plan para acceder
              a gestión de leads, pipeline de ventas y más.
            </p>
            <Link
              to="/dealer/plans"
              className="inline-flex items-center gap-2 px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              <Crown className="h-5 w-5" />
              Actualizar a PRO
            </Link>
          </div>
        </div>
      </div>
    );
  }

  // Get deals by stage
  const getDealsByStage = (stageId: string) => {
    return filteredDeals.filter((d) => d.stageId === stageId);
  };

  // Move deal between stages using TanStack Query mutation
  const moveDeal = (dealId: string, direction: 'prev' | 'next') => {
    if (!currentPipeline) return;

    const deal = deals.find((d) => d.id === dealId);
    if (!deal) return;

    const currentStageIndex = currentPipeline.stages.findIndex((s) => s.id === deal.stageId);
    const newIndex =
      direction === 'next'
        ? Math.min(currentStageIndex + 1, currentPipeline.stages.length - 1)
        : Math.max(currentStageIndex - 1, 0);

    const newStage = currentPipeline.stages[newIndex];

    moveDealMutation.mutate({
      dealId,
      data: { stageId: newStage.id },
    });
  };

  const formatValue = (value: number) => {
    if (value >= 1000000) {
      return `$${(value / 1000000).toFixed(1)}M`;
    }
    return `$${(value / 1000).toFixed(0)}K`;
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 p-6">
        <div className="max-w-7xl mx-auto space-y-6">
          <div className="flex justify-between items-center">
            <div className="h-8 w-48 bg-gray-200 rounded animate-pulse" />
            <div className="h-10 w-40 bg-gray-200 rounded animate-pulse" />
          </div>
          <div className="grid grid-cols-4 gap-4">
            {[1, 2, 3, 4].map((i) => (
              <div key={i} className="h-24 bg-gray-200 rounded-lg animate-pulse" />
            ))}
          </div>
          <div className="flex gap-4 overflow-hidden">
            {[1, 2, 3, 4, 5].map((i) => (
              <div
                key={i}
                className="w-72 h-96 bg-gray-200 rounded-lg animate-pulse flex-shrink-0"
              />
            ))}
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      <div className="max-w-7xl mx-auto space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">CRM</h1>
            <p className="text-gray-600 mt-1">Gestiona tu pipeline de ventas y leads</p>
          </div>
          <div className="flex items-center gap-3">
            <button className="flex items-center gap-2 px-3 py-2 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors">
              <RefreshCw className="h-4 w-4" />
              Actualizar
            </button>
            <button className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors">
              <Plus className="h-5 w-5" />
              Nuevo Deal
            </button>
          </div>
        </div>

        {/* Stats Cards */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-500 text-sm">Deals Totales</span>
              <Briefcase className="h-5 w-5 text-blue-500" />
            </div>
            <p className="text-2xl font-bold text-gray-900 mt-1">{pipelineStats.totalDeals}</p>
            <p className="text-xs text-gray-500">
              {formatValue(pipelineStats.totalValue)} en pipeline
            </p>
          </div>

          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-500 text-sm">Valor Abierto</span>
              <DollarSign className="h-5 w-5 text-green-500" />
            </div>
            <p className="text-2xl font-bold text-gray-900 mt-1">
              {formatValue(pipelineStats.openValue)}
            </p>
            <p className="text-xs text-gray-500">en negociación</p>
          </div>

          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-500 text-sm">Ganados</span>
              <CheckCircle className="h-5 w-5 text-emerald-500" />
            </div>
            <p className="text-2xl font-bold text-gray-900 mt-1">
              {formatValue(pipelineStats.wonValue)}
            </p>
            <p className="text-xs text-gray-500">este mes</p>
          </div>

          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <span className="text-gray-500 text-sm">Conversión</span>
              <TrendingUp className="h-5 w-5 text-purple-500" />
            </div>
            <p className="text-2xl font-bold text-gray-900 mt-1">{pipelineStats.conversionRate}%</p>
            <p className="text-xs text-gray-500">tasa de cierre</p>
          </div>
        </div>

        {/* Pipeline Selector & Search */}
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex flex-col sm:flex-row gap-4">
            {/* Pipeline Selector */}
            <div className="relative">
              <select
                value={selectedPipeline}
                onChange={(e) => setSelectedPipeline(e.target.value)}
                className="appearance-none bg-white border border-gray-300 rounded-lg px-4 py-2 pr-10 font-medium focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              >
                {(pipelines.data || []).map((pipeline) => (
                  <option key={pipeline.id} value={pipeline.id}>
                    {pipeline.name}
                  </option>
                ))}
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400 pointer-events-none" />
            </div>

            {/* Search */}
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
              <input
                type="text"
                placeholder="Buscar deals..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
              />
            </div>

            {/* Filter Button */}
            <button className="flex items-center gap-2 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">
              <Filter className="h-4 w-4" />
              Filtros
            </button>
          </div>
        </div>

        {/* Kanban Board */}
        <div className="overflow-x-auto pb-4">
          <div className="flex gap-4 min-w-max">
            {currentPipeline.stages.map((stage) => (
              <StageColumn
                key={stage.id}
                stage={stage}
                deals={getDealsByStage(stage.id)}
                onMoveDeal={moveDeal}
              />
            ))}
          </div>
        </div>

        {/* Leads Section */}
        <LeadsSection leads={leads} />

        {/* Activities Section */}
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between mb-4">
            <h3 className="font-semibold text-gray-900 flex items-center gap-2">
              <Clock className="h-5 w-5 text-orange-500" />
              Actividades Pendientes
            </h3>
            <Link to="/dealer/crm/activities" className="text-sm text-blue-600 hover:text-blue-700">
              Ver todas →
            </Link>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
            {activities.slice(0, 6).map((activity) => (
              <div key={activity.id} className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                <div
                  className={`p-2 rounded-lg ${
                    activity.type === 'call'
                      ? 'bg-blue-100 text-blue-600'
                      : activity.type === 'email'
                        ? 'bg-green-100 text-green-600'
                        : activity.type === 'meeting'
                          ? 'bg-purple-100 text-purple-600'
                          : 'bg-gray-100 text-gray-600'
                  }`}
                >
                  {activity.type === 'call' && <Phone className="h-4 w-4" />}
                  {activity.type === 'email' && <Mail className="h-4 w-4" />}
                  {activity.type === 'meeting' && <Calendar className="h-4 w-4" />}
                  {activity.type === 'task' && <CheckCircle className="h-4 w-4" />}
                  {activity.type === 'note' && <User className="h-4 w-4" />}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-900 text-sm truncate">{activity.title}</p>
                  {activity.dueDate && (
                    <p className="text-xs text-gray-500">
                      {new Date(activity.dueDate).toLocaleDateString('es-MX', {
                        day: 'numeric',
                        month: 'short',
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </p>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default CRMPage;

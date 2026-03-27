'use client';

import { useState } from 'react';
import {
  useLlmCostBreakdown,
  useLlmModelDistribution,
  useLlmProviderHealth,
  useToggleAggressiveCacheMode,
} from '@/hooks/use-llm-costs';
import {
  DollarSign,
  TrendingUp,
  AlertTriangle,
  Shield,
  Activity,
  Server,
  Bot,
  Brain,
  Zap,
  RefreshCw,
  ExternalLink,
} from 'lucide-react';

// ============================================================
// HELPERS
// ============================================================

function formatUsd(value: number): string {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value);
}

function getStatusColor(status: string): string {
  if (status.includes('AGGRESSIVE') || status.includes('EMERGENCY'))
    return 'bg-red-900/20 border-red-500 text-red-400';
  if (status.includes('CRITICAL')) return 'bg-red-900/10 border-red-400 text-red-300';
  if (status.includes('WARNING')) return 'bg-yellow-900/10 border-yellow-500 text-yellow-300';
  return 'bg-green-900/10 border-green-500 text-green-300';
}

function getProviderIcon(provider: string) {
  switch (provider.toLowerCase()) {
    case 'claude':
      return <Brain className="h-4 w-4" />;
    case 'gemini':
      return <Zap className="h-4 w-4" />;
    case 'llama':
      return <Server className="h-4 w-4" />;
    default:
      return <Activity className="h-4 w-4" />;
  }
}

function getProviderColor(provider: string): string {
  switch (provider.toLowerCase()) {
    case 'claude':
      return 'bg-orange-500/20 text-orange-300 border-orange-500/30';
    case 'gemini':
      return 'bg-blue-500/20 text-blue-300 border-blue-500/30';
    case 'llama':
      return 'bg-purple-500/20 text-purple-300 border-purple-500/30';
    default:
      return 'bg-gray-500/20 text-gray-300 border-gray-500/30';
  }
}

function getAgentIcon(_agent: string) {
  return <Bot className="h-4 w-4" />;
}

// ============================================================
// MAIN PAGE
// ============================================================

export default function LlmCostDashboardPage() {
  const { data: cost, isLoading: costLoading, error: costError } = useLlmCostBreakdown();
  const { data: distribution, isLoading: distLoading } = useLlmModelDistribution();
  const { data: health } = useLlmProviderHealth();
  const toggleAggressive = useToggleAggressiveCacheMode();
  const [showConfirm, setShowConfirm] = useState(false);

  // ── LOADING STATE ────────────────────────────────────────────────
  if (costLoading || distLoading) {
    return (
      <div className="space-y-4 p-6">
        <h1 className="text-2xl font-bold text-white">Costos LLM</h1>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <div
              key={i}
              className="animate-pulse rounded-xl border border-zinc-800 bg-zinc-900 p-6"
            >
              <div className="mb-3 h-4 w-2/3 rounded bg-zinc-700" />
              <div className="h-8 w-1/2 rounded bg-zinc-700" />
            </div>
          ))}
        </div>
      </div>
    );
  }

  // ── ERROR STATE ──────────────────────────────────────────────────
  if (costError || !cost) {
    return (
      <div className="p-6">
        <h1 className="mb-4 text-2xl font-bold text-white">Costos LLM</h1>
        <div className="rounded-xl border border-red-500 bg-red-900/20 p-6 text-red-300">
          <AlertTriangle className="mb-2 h-6 w-6" />
          <p className="font-medium">Error cargando datos de costos</p>
          <p className="mt-1 text-sm text-red-400">
            El endpoint /api/admin/llm-gateway/cost no está disponible. Verifica que el LLM Gateway
            esté configurado.
          </p>
        </div>
      </div>
    );
  }

  const budgetPercent = Math.min(
    (cost.monthlyTotalUsd / (cost.thresholds?.aggressiveCacheUsd || 1)) * 100,
    100
  );

  return (
    <div className="space-y-6 p-6">
      {/* ── HEADER ──────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="flex items-center gap-2 text-2xl font-bold text-white">
            <DollarSign className="h-6 w-6 text-green-400" />
            Monitoreo de Costos LLM
          </h1>
          <p className="mt-1 text-sm text-zinc-400">
            Mes actual: {cost.month} · Actualizado{' '}
            {new Date(cost.generatedAt).toLocaleTimeString('es-DO')}
          </p>
        </div>
        <div className="flex items-center gap-3">
          <a
            href="https://grafana.okla.do/d/llm-cost/llm-cost-overview"
            target="_blank"
            rel="noopener noreferrer"
            className="flex items-center gap-2 rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2 text-sm text-zinc-300 transition-colors hover:border-zinc-500 hover:text-white"
          >
            <ExternalLink className="h-4 w-4" />
            Grafana
          </a>
          <button
            onClick={() => window.location.reload()}
            className="flex items-center gap-2 rounded-lg border border-zinc-700 bg-zinc-800 px-3 py-2 text-sm text-zinc-300 transition-colors hover:border-zinc-500 hover:text-white"
          >
            <RefreshCw className="h-4 w-4" />
            Actualizar
          </button>
        </div>
      </div>

      {/* ── STATUS BANNER ───────────────────────────────────────────── */}
      <div className={`rounded-xl border p-4 ${getStatusColor(cost.status)}`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            {cost.isAggressiveCacheModeActive ? (
              <Shield className="h-5 w-5 text-red-400" />
            ) : (
              <Activity className="h-5 w-5" />
            )}
            <span className="font-medium">{cost.status}</span>
          </div>
          {cost.isAggressiveCacheModeActive && (
            <span className="rounded-full border border-red-500/40 bg-red-500/20 px-3 py-1 text-xs">
              Tráfico: 40% Claude · 40% Gemini · 20% Llama
            </span>
          )}
        </div>
      </div>

      {/* ── STAT CARDS ──────────────────────────────────────────────── */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
        {/* Monthly Total */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <div className="mb-1 flex items-center gap-2 text-sm text-zinc-400">
            <DollarSign className="h-4 w-4" />
            Costo Mensual
          </div>
          <div className="text-3xl font-bold text-white">{formatUsd(cost.monthlyTotalUsd)}</div>
          <div className="mt-2">
            <div className="h-2 w-full overflow-hidden rounded-full bg-zinc-800">
              <div
                className={`h-full rounded-full transition-all ${
                  budgetPercent >= 100
                    ? 'bg-red-500'
                    : budgetPercent >= 71
                      ? 'bg-yellow-500'
                      : 'bg-green-500'
                }`}
                style={{ width: `${budgetPercent}%` }}
              />
            </div>
            <p className="mt-1 text-xs text-zinc-500">
              {budgetPercent.toFixed(0)}% del límite (
              {formatUsd(cost.thresholds?.aggressiveCacheUsd ?? 0)})
            </p>
          </div>
        </div>

        {/* Daily Cost */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <div className="mb-1 flex items-center gap-2 text-sm text-zinc-400">
            <Activity className="h-4 w-4" />
            Costo Hoy
          </div>
          <div className="text-3xl font-bold text-white">{formatUsd(cost.dailyTotalUsd)}</div>
          <p className="mt-2 text-xs text-zinc-500">Acumulado del día actual</p>
        </div>

        {/* Projected */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <div className="mb-1 flex items-center gap-2 text-sm text-zinc-400">
            <TrendingUp className="h-4 w-4" />
            Proyección Mensual
          </div>
          <div
            className={`text-3xl font-bold ${
              cost.projectedMonthlyUsd > (cost.thresholds?.criticalUsd ?? 600)
                ? 'text-red-400'
                : cost.projectedMonthlyUsd > (cost.thresholds?.warningUsd ?? 400)
                  ? 'text-yellow-400'
                  : 'text-white'
            }`}
          >
            {formatUsd(cost.projectedMonthlyUsd)}
          </div>
          <p className="mt-2 text-xs text-zinc-500">Basado en la tasa diaria actual</p>
        </div>

        {/* Requests */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <div className="mb-1 flex items-center gap-2 text-sm text-zinc-400">
            <Zap className="h-4 w-4" />
            Solicitudes Totales
          </div>
          <div className="text-3xl font-bold text-white">
            {distribution?.totalRequests?.toLocaleString('es-DO') ?? '—'}
          </div>
          <p className="mt-2 text-xs text-zinc-500">Desde último reinicio</p>
        </div>
      </div>

      {/* ── TWO-COLUMN: BY AGENT + BY PROVIDER ──────────────────── */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        {/* Cost by Agent */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
            <Bot className="h-5 w-5 text-blue-400" />
            Costo por Agente (Este Mes)
          </h2>
          {Object.keys(cost.byAgent).length === 0 ? (
            <p className="text-sm text-zinc-500">Sin datos de agentes aún</p>
          ) : (
            <div className="space-y-3">
              {Object.entries(cost.byAgent)
                .sort(([, a], [, b]) => b - a)
                .map(([agent, agentCost]) => {
                  const pct =
                    cost.monthlyTotalUsd > 0 ? (agentCost / cost.monthlyTotalUsd) * 100 : 0;
                  return (
                    <div key={agent}>
                      <div className="mb-1 flex items-center justify-between">
                        <div className="flex items-center gap-2 text-sm text-zinc-300">
                          {getAgentIcon(agent)}
                          <span className="capitalize">{agent}</span>
                        </div>
                        <div className="text-sm">
                          <span className="font-medium text-white">{formatUsd(agentCost)}</span>
                          <span className="ml-2 text-zinc-500">({pct.toFixed(1)}%)</span>
                        </div>
                      </div>
                      <div className="h-2 w-full overflow-hidden rounded-full bg-zinc-800">
                        <div
                          className="h-full rounded-full bg-blue-500/60"
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
            </div>
          )}
        </div>

        {/* Cost by Provider/Model */}
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
            <Brain className="h-5 w-5 text-orange-400" />
            Costo por Modelo (Este Mes)
          </h2>
          {Object.keys(cost.byProvider).length === 0 ? (
            <p className="text-sm text-zinc-500">Sin datos de proveedores aún</p>
          ) : (
            <div className="space-y-3">
              {Object.entries(cost.byProvider)
                .sort(([, a], [, b]) => b - a)
                .map(([provider, provCost]) => {
                  const pct =
                    cost.monthlyTotalUsd > 0 ? (provCost / cost.monthlyTotalUsd) * 100 : 0;
                  return (
                    <div key={provider}>
                      <div className="mb-1 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <span
                            className={`inline-flex items-center gap-1.5 rounded-full border px-2 py-0.5 text-xs ${getProviderColor(provider)}`}
                          >
                            {getProviderIcon(provider)}
                            {provider.charAt(0).toUpperCase() + provider.slice(1)}
                          </span>
                        </div>
                        <div className="text-sm">
                          <span className="font-medium text-white">{formatUsd(provCost)}</span>
                          <span className="ml-2 text-zinc-500">({pct.toFixed(1)}%)</span>
                        </div>
                      </div>
                      <div className="h-2 w-full overflow-hidden rounded-full bg-zinc-800">
                        <div
                          className={`h-full rounded-full ${
                            provider === 'claude'
                              ? 'bg-orange-500/60'
                              : provider === 'gemini'
                                ? 'bg-blue-500/60'
                                : 'bg-purple-500/60'
                          }`}
                          style={{ width: `${pct}%` }}
                        />
                      </div>
                    </div>
                  );
                })}
            </div>
          )}
        </div>
      </div>

      {/* ── MODEL DISTRIBUTION ──────────────────────────────────── */}
      {distribution && (
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
            <Activity className="h-5 w-5 text-green-400" />
            Distribución de Solicitudes por Proveedor
          </h2>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            {[
              { name: 'Claude', value: distribution.claude, color: 'orange' },
              { name: 'Gemini', value: distribution.gemini, color: 'blue' },
              { name: 'Llama', value: distribution.llama, color: 'purple' },
              { name: 'Cache', value: distribution.cache, color: 'green' },
            ].map(({ name, value, color }) => (
              <div
                key={name}
                className={`bg-${color}-500/10 border border-${color}-500/20 rounded-xl p-4 text-center`}
              >
                <div className="mb-1 flex items-center justify-center gap-1.5 text-sm text-zinc-400">
                  {getProviderIcon(name.toLowerCase())}
                  {name}
                </div>
                <div className="text-2xl font-bold text-white">{value}%</div>
              </div>
            ))}
          </div>
          <p className="mt-3 text-center text-xs text-zinc-500">{distribution.summary}</p>
        </div>
      )}

      {/* ── PROVIDER HEALTH ─────────────────────────────────────── */}
      {health && (
        <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
          <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
            <Server className="h-5 w-5 text-cyan-400" />
            Estado de Proveedores
          </h2>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-4">
            {Object.entries(health.providers).map(([name, status]) => (
              <div
                key={name}
                className={`rounded-xl border p-4 text-center ${
                  status === 'Healthy'
                    ? 'border-green-500/30 bg-green-900/10 text-green-400'
                    : 'border-red-500/30 bg-red-900/10 text-red-400'
                }`}
              >
                <div className="mb-1 text-sm">{name}</div>
                <div className="text-lg font-bold">
                  {status === 'Healthy' ? '✅ Online' : '❌ Offline'}
                </div>
              </div>
            ))}
          </div>
          <p className="mt-3 text-center text-xs text-zinc-500">
            Último chequeo: {new Date(health.checkedAt).toLocaleTimeString('es-DO')}
          </p>
        </div>
      )}

      {/* ── THRESHOLDS INFO ─────────────────────────────────────── */}
      <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
        <h2 className="mb-4 flex items-center gap-2 text-lg font-semibold text-white">
          <AlertTriangle className="h-5 w-5 text-yellow-400" />
          Umbrales de Alerta Configurados
        </h2>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
          <div className="rounded-xl border border-yellow-500/30 bg-yellow-900/10 p-4">
            <div className="mb-1 text-sm font-medium text-yellow-400">⚠️ Warning</div>
            <div className="text-2xl font-bold text-white">
              {formatUsd(cost.thresholds?.warningUsd ?? 0)}/mes
            </div>
            <p className="mt-1 text-xs text-zinc-500">Alerta al equipo admin</p>
          </div>
          <div className="rounded-xl border border-red-500/30 bg-red-900/10 p-4">
            <div className="mb-1 text-sm font-medium text-red-400">🔴 Crítico</div>
            <div className="text-2xl font-bold text-white">
              {formatUsd(cost.thresholds?.criticalUsd ?? 0)}/mes
            </div>
            <p className="mt-1 text-xs text-zinc-500">Alerta al CTO (&lt;5 min SLA)</p>
          </div>
          <div className="rounded-xl border border-red-600/30 bg-red-900/20 p-4">
            <div className="mb-1 text-sm font-medium text-red-500">🚨 Emergencia</div>
            <div className="text-2xl font-bold text-white">
              {formatUsd(cost.thresholds?.aggressiveCacheUsd ?? 0)}/mes
            </div>
            <p className="mt-1 text-xs text-zinc-500">Modo caché agresivo automático</p>
          </div>
        </div>
      </div>

      {/* ── AGGRESSIVE MODE TOGGLE (CTO) ────────────────────────── */}
      <div className="rounded-xl border border-zinc-800 bg-zinc-900 p-6">
        <div className="flex items-center justify-between">
          <div>
            <h2 className="flex items-center gap-2 text-lg font-semibold text-white">
              <Shield className="h-5 w-5 text-red-400" />
              Control Manual — Modo Caché Agresivo
            </h2>
            <p className="mt-1 text-sm text-zinc-400">
              {cost.isAggressiveCacheModeActive
                ? 'Modo activo: 40% Claude, 40% Gemini Flash, 20% Llama local'
                : 'Modo inactivo: Cascada normal Claude → Gemini → Llama'}
            </p>
          </div>
          <div>
            {!showConfirm ? (
              <button
                onClick={() => setShowConfirm(true)}
                className={`rounded-lg px-4 py-2 text-sm font-medium transition-colors ${
                  cost.isAggressiveCacheModeActive
                    ? 'bg-green-600 text-white hover:bg-green-500'
                    : 'bg-red-600 text-white hover:bg-red-500'
                }`}
              >
                {cost.isAggressiveCacheModeActive ? 'Desactivar' : 'Activar'} Modo Agresivo
              </button>
            ) : (
              <div className="flex items-center gap-2">
                <span className="text-sm text-yellow-400">¿Confirmar?</span>
                <button
                  onClick={() => {
                    toggleAggressive.mutate(!cost.isAggressiveCacheModeActive);
                    setShowConfirm(false);
                  }}
                  disabled={toggleAggressive.isPending}
                  className="rounded-lg bg-red-600 px-3 py-1.5 text-sm text-white hover:bg-red-500"
                >
                  {toggleAggressive.isPending ? 'Procesando...' : 'Sí, confirmar'}
                </button>
                <button
                  onClick={() => setShowConfirm(false)}
                  className="rounded-lg bg-zinc-700 px-3 py-1.5 text-sm text-white hover:bg-zinc-600"
                >
                  Cancelar
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

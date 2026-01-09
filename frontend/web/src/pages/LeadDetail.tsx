import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import leadScoringService from '@/services/leadScoringService';
import type { LeadDto, UpdateLeadStatusDto, LeadStatus } from '@/services/leadScoringService';
import { FiArrowLeft, FiPhone, FiMail, FiEdit, FiSave, FiX } from 'react-icons/fi';
import MainLayout from '@/layouts/MainLayout';

export const LeadDetail = () => {
  const { leadId } = useParams<{ leadId: string }>();
  const navigate = useNavigate();

  const [lead, setLead] = useState<LeadDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Edit mode
  const [isEditing, setIsEditing] = useState(false);
  const [editStatus, setEditStatus] = useState<LeadStatus>('New');
  const [editNotes, setEditNotes] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  const loadLead = useCallback(async () => {
    try {
      setIsLoading(true);
      const data = await leadScoringService.getLeadById(leadId!);
      setLead(data);
      setEditStatus(data.status as LeadStatus);
      setEditNotes(data.dealerNotes || '');
    } catch (err: any) {
      setError(err.message || 'Error loading lead');
    } finally {
      setIsLoading(false);
    }
  }, [leadId]);

  useEffect(() => {
    if (leadId) {
      loadLead();
    }
  }, [leadId, loadLead]);

  const handleSave = async () => {
    if (!lead) return;

    try {
      setIsSaving(true);
      const dto: UpdateLeadStatusDto = {
        status: editStatus,
        dealerNotes: editNotes || null,
      };
      const updated = await leadScoringService.updateLeadStatus(lead.id, dto);
      setLead(updated);
      setIsEditing(false);
    } catch (err: any) {
      alert('Error al guardar: ' + err.message);
    } finally {
      setIsSaving(false);
    }
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="flex justify-center items-center min-h-screen">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
        </div>
      </MainLayout>
    );
  }

  if (error || !lead) {
    return (
      <MainLayout>
        <div className="max-w-7xl mx-auto px-4 py-8">
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg">
            {error || 'Lead no encontrado'}
          </div>
          <button
            onClick={() => navigate('/dealer/leads')}
            className="mt-4 px-4 py-2 text-blue-600 hover:text-blue-800"
          >
            ← Volver a Leads
          </button>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="max-w-7xl mx-auto px-4 py-8">
        {/* Header */}
        <div className="mb-6">
          <button
            onClick={() => navigate('/dealer/leads')}
            className="flex items-center gap-2 text-blue-600 hover:text-blue-800 mb-4"
          >
            <FiArrowLeft /> Volver a Leads
          </button>
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold text-gray-900">{lead.userFullName}</h1>
              <p className="text-gray-600">{lead.userEmail}</p>
            </div>
            <div className="flex items-center gap-3">
              {lead.userPhone && (
                <button
                  onClick={() => window.open(`tel:${lead.userPhone}`)}
                  className="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors flex items-center gap-2"
                >
                  <FiPhone /> Llamar
                </button>
              )}
              <button
                onClick={() => window.open(`mailto:${lead.userEmail}`)}
                className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors flex items-center gap-2"
              >
                <FiMail /> Email
              </button>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Info */}
          <div className="lg:col-span-2 space-y-6">
            {/* Lead Score Card */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Lead Score</h2>

              <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="text-center">
                  <div className="text-4xl font-bold text-gray-900 mb-2">{lead.score}</div>
                  <div className="text-sm text-gray-500">Score Total</div>
                  <div className="w-full bg-gray-200 rounded-full h-2 mt-2">
                    <div
                      className={`h-2 rounded-full ${leadScoringService.getScoreBarClass(lead.score)}`}
                      style={{ width: `${lead.score}%` }}
                    ></div>
                  </div>
                </div>

                <div className="text-center">
                  <span
                    className={`inline-block text-3xl px-4 py-2 rounded-full ${
                      lead.temperature === 'Hot'
                        ? 'bg-red-100 text-red-800'
                        : lead.temperature === 'Warm'
                          ? 'bg-orange-100 text-orange-800'
                          : 'bg-blue-100 text-blue-800'
                    }`}
                  >
                    {leadScoringService.getTemperatureEmoji(lead.temperature)}
                  </span>
                  <div className="text-sm text-gray-500 mt-2">{lead.temperature}</div>
                </div>

                <div className="text-center">
                  <div className="text-4xl font-bold text-green-600 mb-2">
                    {lead.conversionProbability.toFixed(0)}%
                  </div>
                  <div className="text-sm text-gray-500">Probabilidad</div>
                  <div className="text-xs text-gray-400 mt-1">de conversión</div>
                </div>
              </div>

              <div className="grid grid-cols-3 gap-4 pt-4 border-t">
                <div>
                  <div className="text-sm text-gray-500">Engagement</div>
                  <div className="text-2xl font-bold text-gray-900">{lead.engagementScore}</div>
                  <div className="text-xs text-gray-400">de 40</div>
                </div>
                <div>
                  <div className="text-sm text-gray-500">Recency</div>
                  <div className="text-2xl font-bold text-gray-900">{lead.recencyScore}</div>
                  <div className="text-xs text-gray-400">de 30</div>
                </div>
                <div>
                  <div className="text-sm text-gray-500">Intent</div>
                  <div className="text-2xl font-bold text-gray-900">{lead.intentScore}</div>
                  <div className="text-xs text-gray-400">de 30</div>
                </div>
              </div>

              <div className="mt-4 p-4 bg-blue-50 rounded-lg">
                <div className="flex items-start gap-2">
                  <span className="text-2xl">💡</span>
                  <div>
                    <div className="font-semibold text-gray-900">Acción Recomendada</div>
                    <div className="text-sm text-gray-700 mt-1">
                      {leadScoringService.getRecommendedAction(lead)}
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Vehicle Info */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">Vehículo de Interés</h2>
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-lg font-semibold text-gray-900">{lead.vehicleTitle}</div>
                  <div className="text-2xl font-bold text-blue-600 mt-2">
                    ${lead.vehiclePrice.toLocaleString()}
                  </div>
                </div>
                <button
                  onClick={() => navigate(`/vehicles/${lead.vehicleId}`)}
                  className="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
                >
                  Ver Vehículo
                </button>
              </div>
            </div>

            {/* Activity Timeline */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h2 className="text-xl font-bold text-gray-900 mb-4">
                Historial de Actividad ({lead.recentActions.length})
              </h2>

              {lead.recentActions.length === 0 ? (
                <p className="text-gray-500 text-center py-8">No hay actividad registrada</p>
              ) : (
                <div className="space-y-4">
                  {lead.recentActions.map((action) => (
                    <div
                      key={action.id}
                      className="flex items-start gap-4 pb-4 border-b last:border-b-0"
                    >
                      <div className="flex-shrink-0 w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                        <span className="text-lg">
                          {leadScoringService.getActionIcon(action.actionType)}
                        </span>
                      </div>
                      <div className="flex-1">
                        <div className="flex items-center justify-between">
                          <div className="font-medium text-gray-900">
                            {leadScoringService.getActionDescription(action.actionType)}
                          </div>
                          <div className="text-sm text-gray-500">
                            {leadScoringService.formatRelativeTime(action.occurredAt)}
                          </div>
                        </div>
                        <div className="text-sm text-gray-500 mt-1">{action.description}</div>
                        {action.scoreImpact !== 0 && (
                          <div
                            className={`text-xs font-semibold mt-1 ${
                              action.scoreImpact > 0 ? 'text-green-600' : 'text-red-600'
                            }`}
                          >
                            {action.scoreImpact > 0 ? '+' : ''}
                            {action.scoreImpact} puntos
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Sidebar */}
          <div className="space-y-6">
            {/* Stats */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Estadísticas</h3>
              <div className="space-y-3">
                <div className="flex justify-between">
                  <span className="text-gray-600">Vistas</span>
                  <span className="font-semibold">{lead.viewCount}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Contactos</span>
                  <span className="font-semibold">{lead.contactCount}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Favoritos</span>
                  <span className="font-semibold">{lead.favoriteCount}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Shares</span>
                  <span className="font-semibold">{lead.shareCount}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Comparaciones</span>
                  <span className="font-semibold">{lead.comparisonCount}</span>
                </div>
                <div className="pt-3 border-t">
                  <div className="flex items-center justify-between">
                    <span className="text-gray-600">Test Drive</span>
                    {lead.hasScheduledTestDrive ? (
                      <span className="text-green-600 font-semibold">✅ Sí</span>
                    ) : (
                      <span className="text-gray-400">No</span>
                    )}
                  </div>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-gray-600">Financiamiento</span>
                  {lead.hasRequestedFinancing ? (
                    <span className="text-green-600 font-semibold">✅ Sí</span>
                  ) : (
                    <span className="text-gray-400">No</span>
                  )}
                </div>
              </div>
            </div>

            {/* Status & Notes */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-bold text-gray-900">Estado y Notas</h3>
                {!isEditing ? (
                  <button
                    onClick={() => setIsEditing(true)}
                    className="text-blue-600 hover:text-blue-800"
                  >
                    <FiEdit />
                  </button>
                ) : (
                  <div className="flex gap-2">
                    <button
                      onClick={handleSave}
                      disabled={isSaving}
                      className="text-green-600 hover:text-green-800 disabled:opacity-50"
                    >
                      <FiSave />
                    </button>
                    <button
                      onClick={() => {
                        setIsEditing(false);
                        setEditStatus(lead.status as LeadStatus);
                        setEditNotes(lead.dealerNotes || '');
                      }}
                      className="text-red-600 hover:text-red-800"
                    >
                      <FiX />
                    </button>
                  </div>
                )}
              </div>

              {isEditing ? (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Estado</label>
                    <select
                      value={editStatus}
                      onChange={(e) => setEditStatus(e.target.value as LeadStatus)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="New">Nuevo</option>
                      <option value="Contacted">Contactado</option>
                      <option value="Qualified">Calificado</option>
                      <option value="Nurturing">En seguimiento</option>
                      <option value="Negotiating">Negociando</option>
                      <option value="Converted">Convertido</option>
                      <option value="Lost">Perdido</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Notas</label>
                    <textarea
                      value={editNotes}
                      onChange={(e) => setEditNotes(e.target.value)}
                      rows={5}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                      placeholder="Agregar notas sobre este lead..."
                    />
                  </div>
                </div>
              ) : (
                <div className="space-y-4">
                  <div>
                    <div className="text-sm text-gray-500 mb-1">Estado Actual</div>
                    <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-gray-100 text-gray-800">
                      {lead.status}
                    </span>
                  </div>
                  <div>
                    <div className="text-sm text-gray-500 mb-1">Notas del Dealer</div>
                    {lead.dealerNotes ? (
                      <div className="text-sm text-gray-700 whitespace-pre-wrap bg-gray-50 p-3 rounded-lg">
                        {lead.dealerNotes}
                      </div>
                    ) : (
                      <div className="text-sm text-gray-400 italic">Sin notas</div>
                    )}
                  </div>
                </div>
              )}
            </div>

            {/* Timeline */}
            <div className="bg-white rounded-lg shadow-md p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-4">Timeline</h3>
              <div className="space-y-3 text-sm">
                <div>
                  <div className="text-gray-500">Primera interacción</div>
                  <div className="font-medium">
                    {new Date(lead.firstInteractionAt).toLocaleDateString()}
                  </div>
                </div>
                <div>
                  <div className="text-gray-500">Última interacción</div>
                  <div className="font-medium">
                    {leadScoringService.formatRelativeTime(lead.lastInteractionAt)}
                  </div>
                </div>
                {lead.lastContactedAt && (
                  <div>
                    <div className="text-gray-500">Último contacto</div>
                    <div className="font-medium">
                      {new Date(lead.lastContactedAt).toLocaleDateString()}
                    </div>
                  </div>
                )}
                {lead.convertedAt && (
                  <div>
                    <div className="text-gray-500">Convertido</div>
                    <div className="font-medium text-green-600">
                      {new Date(lead.convertedAt).toLocaleDateString()} 🎉
                    </div>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default LeadDetail;

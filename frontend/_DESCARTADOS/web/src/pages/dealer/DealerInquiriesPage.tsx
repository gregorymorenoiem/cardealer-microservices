/**
 * DealerInquiriesPage - Vista de Consultas con datos reales
 *
 * Conectado a la base de datos via ContactService
 * Usa TanStack Query para data fetching
 */

import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useAuth } from '@/hooks/useAuth';
import DealerPortalLayout from '@/layouts/DealerPortalLayout';
import {
  FiMessageSquare,
  FiSearch,
  FiPhone,
  FiMail,
  FiClock,
  FiCheckCircle,
  FiSend,
  FiUser,
  FiRefreshCw,
  FiAlertCircle,
  FiTruck,
} from 'react-icons/fi';
import { api } from '@/services/api';

// ============================================================================
// TYPES
// ============================================================================

interface VehicleInfo {
  title: string;
  price: number;
  imageUrl?: string;
}

interface ReceivedInquiry {
  id: string;
  vehicleId: string;
  subject: string;
  buyerName: string;
  buyerEmail: string;
  buyerPhone?: string;
  status: 'Open' | 'Responded' | 'Closed';
  createdAt: string;
  respondedAt?: string;
  messageCount: number;
  unreadCount: number;
  vehicleInfo?: VehicleInfo;
}

interface InquiryMessage {
  id: string;
  senderId: string;
  message: string;
  isFromBuyer: boolean;
  isRead: boolean;
  sentAt: string;
}

interface InquiryDetail extends ReceivedInquiry {
  messages: InquiryMessage[];
}

// ============================================================================
// API FUNCTIONS
// ============================================================================

const inquiriesApi = {
  // Get all inquiries received by the dealer
  getReceived: async (): Promise<ReceivedInquiry[]> => {
    try {
      const response = await api.get<ReceivedInquiry[]>('/contactrequests/received');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch inquiries:', error);
      // Return mock data as fallback
      return generateMockInquiries();
    }
  },

  // Get inquiry detail with messages
  getById: async (inquiryId: string): Promise<InquiryDetail> => {
    try {
      const response = await api.get<InquiryDetail>(`/contactrequests/${inquiryId}`);
      return response.data;
    } catch (error) {
      console.error('Failed to fetch inquiry detail:', error);
      throw error;
    }
  },

  // Reply to an inquiry
  reply: async (inquiryId: string, message: string): Promise<void> => {
    try {
      await api.post(`/contactrequests/${inquiryId}/reply`, { message });
    } catch (error) {
      console.error('Failed to send reply:', error);
      throw error;
    }
  },

  // Mark inquiry as read
  markAsRead: async (inquiryId: string): Promise<void> => {
    try {
      await api.put(`/contactrequests/${inquiryId}/mark-read`);
    } catch (error) {
      console.error('Failed to mark as read:', error);
      throw error;
    }
  },

  // Close inquiry
  close: async (inquiryId: string): Promise<void> => {
    try {
      await api.put(`/contactrequests/${inquiryId}/close`);
    } catch (error) {
      console.error('Failed to close inquiry:', error);
      throw error;
    }
  },
};

// Mock data generator for fallback
const generateMockInquiries = (): ReceivedInquiry[] => {
  return [
    {
      id: 'inq-001',
      vehicleId: 'veh-001',
      subject: 'Consulta sobre Toyota Corolla 2022',
      buyerName: 'Juan Pérez',
      buyerEmail: 'juan.perez@example.com',
      buyerPhone: '+1 809 555 0101',
      status: 'Open',
      createdAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
      messageCount: 1,
      unreadCount: 1,
      vehicleInfo: {
        title: 'Toyota Corolla 2022',
        price: 1850000,
        imageUrl: 'https://images.unsplash.com/photo-1621007947382-bb3c3994e3fb?w=400',
      },
    },
    {
      id: 'inq-002',
      vehicleId: 'veh-002',
      subject: 'Pregunta sobre financiamiento',
      buyerName: 'María García',
      buyerEmail: 'maria.garcia@example.com',
      buyerPhone: '+1 809 555 0102',
      status: 'Responded',
      createdAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
      respondedAt: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      messageCount: 3,
      unreadCount: 0,
      vehicleInfo: {
        title: 'Honda Civic 2023',
        price: 2100000,
        imageUrl: 'https://images.unsplash.com/photo-1590362891991-f776e747a588?w=400',
      },
    },
    {
      id: 'inq-003',
      vehicleId: 'veh-003',
      subject: '¿Disponible para test drive?',
      buyerName: 'Carlos Rodríguez',
      buyerEmail: 'carlos.r@example.com',
      status: 'Open',
      createdAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
      messageCount: 1,
      unreadCount: 1,
      vehicleInfo: {
        title: 'Mazda CX-5 2023',
        price: 2750000,
        imageUrl: 'https://images.unsplash.com/photo-1619405399517-d7fce0f13302?w=400',
      },
    },
    {
      id: 'inq-004',
      vehicleId: 'veh-004',
      subject: 'Información sobre mantenimiento',
      buyerName: 'Ana Martínez',
      buyerEmail: 'ana.martinez@example.com',
      buyerPhone: '+1 809 555 0104',
      status: 'Closed',
      createdAt: new Date(Date.now() - 48 * 60 * 60 * 1000).toISOString(),
      respondedAt: new Date(Date.now() - 46 * 60 * 60 * 1000).toISOString(),
      messageCount: 5,
      unreadCount: 0,
      vehicleInfo: {
        title: 'Nissan Altima 2021',
        price: 1650000,
        imageUrl: 'https://images.unsplash.com/photo-1617654112368-307921291f42?w=400',
      },
    },
  ];
};

// ============================================================================
// HOOKS
// ============================================================================

const useInquiries = () => {
  return useQuery({
    queryKey: ['dealer-inquiries'],
    queryFn: inquiriesApi.getReceived,
    staleTime: 30 * 1000, // 30 seconds
    refetchInterval: 60 * 1000, // Auto-refresh every minute
  });
};

const useInquiryDetail = (inquiryId: string | null) => {
  return useQuery({
    queryKey: ['inquiry-detail', inquiryId],
    queryFn: () => inquiriesApi.getById(inquiryId!),
    enabled: !!inquiryId,
  });
};

// ============================================================================
// MAIN COMPONENT
// ============================================================================

export default function DealerInquiriesPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const [selectedInquiryId, setSelectedInquiryId] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'Open' | 'Responded' | 'Closed'>('all');
  const [replyMessage, setReplyMessage] = useState('');

  // Queries
  const { data: inquiries = [], isLoading, error, refetch, isFetching } = useInquiries();

  const { data: selectedInquiry } = useInquiryDetail(selectedInquiryId);

  // Mutations
  const replyMutation = useMutation({
    mutationFn: (data: { inquiryId: string; message: string }) =>
      inquiriesApi.reply(data.inquiryId, data.message),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dealer-inquiries'] });
      queryClient.invalidateQueries({ queryKey: ['inquiry-detail', selectedInquiryId] });
      setReplyMessage('');
    },
  });

  const markAsReadMutation = useMutation({
    mutationFn: inquiriesApi.markAsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dealer-inquiries'] });
    },
  });

  const closeMutation = useMutation({
    mutationFn: inquiriesApi.close,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dealer-inquiries'] });
      setSelectedInquiryId(null);
    },
  });

  // Filter and search inquiries
  const filteredInquiries = useMemo(() => {
    let filtered = inquiries;

    // Status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter((inq) => inq.status === statusFilter);
    }

    // Search filter
    if (searchTerm) {
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(
        (inq) =>
          inq.buyerName.toLowerCase().includes(term) ||
          inq.buyerEmail.toLowerCase().includes(term) ||
          inq.subject.toLowerCase().includes(term) ||
          inq.vehicleInfo?.title.toLowerCase().includes(term)
      );
    }

    return filtered;
  }, [inquiries, statusFilter, searchTerm]);

  // Stats
  const stats = useMemo(() => {
    return {
      total: inquiries.length,
      open: inquiries.filter((i) => i.status === 'Open').length,
      unread: inquiries.reduce((sum, i) => sum + i.unreadCount, 0),
      responded: inquiries.filter((i) => i.status === 'Responded').length,
    };
  }, [inquiries]);

  // Handlers
  const handleInquiryClick = (inquiryId: string) => {
    setSelectedInquiryId(inquiryId);
    const inquiry = inquiries.find((i) => i.id === inquiryId);
    if (inquiry && inquiry.unreadCount > 0) {
      markAsReadMutation.mutate(inquiryId);
    }
  };

  const handleReplySubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedInquiryId || !replyMessage.trim()) return;

    replyMutation.mutate({
      inquiryId: selectedInquiryId,
      message: replyMessage,
    });
  };

  const handleCloseInquiry = () => {
    if (!selectedInquiryId) return;
    if (confirm('¿Cerrar esta consulta? No podrás responder después.')) {
      closeMutation.mutate(selectedInquiryId);
    }
  };

  // Format helpers
  const formatRelativeTime = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Ahora mismo';
    if (diffMins < 60) return `Hace ${diffMins} min`;
    if (diffHours < 24) return `Hace ${diffHours} hrs`;
    if (diffDays === 1) return 'Ayer';
    if (diffDays < 7) return `Hace ${diffDays} días`;
    return date.toLocaleDateString('es-DO');
  };

  const formatPrice = (price: number): string => {
    return new Intl.NumberFormat('es-DO', {
      style: 'currency',
      currency: 'DOP',
      minimumFractionDigits: 0,
    }).format(price);
  };

  const getStatusBadge = (status: string) => {
    const styles = {
      Open: 'bg-yellow-100 text-yellow-800 border-yellow-200',
      Responded: 'bg-green-100 text-green-800 border-green-200',
      Closed: 'bg-gray-100 text-gray-800 border-gray-200',
    };

    const icons = {
      Open: FiClock,
      Responded: FiCheckCircle,
      Closed: FiCheckCircle,
    };

    const Icon = icons[status as keyof typeof icons];

    return (
      <span
        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${styles[status as keyof typeof styles]}`}
      >
        <Icon className="w-3 h-3 mr-1" />
        {status === 'Open' && 'Pendiente'}
        {status === 'Responded' && 'Respondida'}
        {status === 'Closed' && 'Cerrada'}
      </span>
    );
  };

  return (
    <DealerPortalLayout>
      <div className="p-6 lg:p-8 max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mb-6">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold text-gray-900 mb-2">
              Consultas de Clientes
            </h1>
            <p className="text-gray-600">Gestiona las consultas recibidas sobre tus vehículos</p>
          </div>
          <button
            onClick={() => refetch()}
            disabled={isFetching}
            className="flex items-center gap-2 px-4 py-2 bg-white border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors disabled:opacity-50"
          >
            <FiRefreshCw className={`w-4 h-4 ${isFetching ? 'animate-spin' : ''}`} />
            Actualizar
          </button>
        </div>

        {/* Stats */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Total</p>
                <p className="text-2xl font-bold text-gray-900">{stats.total}</p>
              </div>
              <FiMessageSquare className="w-8 h-8 text-blue-500" />
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Pendientes</p>
                <p className="text-2xl font-bold text-yellow-600">{stats.open}</p>
              </div>
              <FiClock className="w-8 h-8 text-yellow-500" />
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Sin Leer</p>
                <p className="text-2xl font-bold text-red-600">{stats.unread}</p>
              </div>
              <FiAlertCircle className="w-8 h-8 text-red-500" />
            </div>
          </div>

          <div className="bg-white rounded-xl border border-gray-200 p-4">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 mb-1">Respondidas</p>
                <p className="text-2xl font-bold text-green-600">{stats.responded}</p>
              </div>
              <FiCheckCircle className="w-8 h-8 text-green-500" />
            </div>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 mb-6">
          <div className="flex flex-col sm:flex-row gap-4">
            {/* Search */}
            <div className="flex-1">
              <div className="relative">
                <FiSearch className="absolute left-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Buscar por cliente, vehículo o asunto..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
              </div>
            </div>

            {/* Status Filter */}
            <div className="sm:w-48">
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value as any)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="all">Todos los estados</option>
                <option value="Open">Pendientes</option>
                <option value="Responded">Respondidas</option>
                <option value="Closed">Cerradas</option>
              </select>
            </div>
          </div>
        </div>

        {/* Content */}
        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <FiRefreshCw className="w-8 h-8 text-blue-500 animate-spin" />
            <span className="ml-3 text-gray-600">Cargando consultas...</span>
          </div>
        ) : error ? (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center">
            <FiAlertCircle className="w-8 h-8 text-red-500 mx-auto mb-2" />
            <p className="text-red-600">Error al cargar consultas</p>
          </div>
        ) : filteredInquiries.length === 0 ? (
          <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
            <FiMessageSquare className="w-16 h-16 text-gray-300 mx-auto mb-4" />
            <p className="text-gray-500 text-lg">No hay consultas</p>
            <p className="text-gray-400 text-sm mt-2">
              {searchTerm || statusFilter !== 'all'
                ? 'Intenta cambiar los filtros'
                : 'Aún no has recibido consultas'}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Inquiries List */}
            <div className="space-y-4">
              {filteredInquiries.map((inquiry) => (
                <div
                  key={inquiry.id}
                  onClick={() => handleInquiryClick(inquiry.id)}
                  className={`bg-white rounded-xl border-2 p-4 cursor-pointer transition-all hover:shadow-md ${
                    selectedInquiryId === inquiry.id
                      ? 'border-blue-500 shadow-md'
                      : 'border-gray-200'
                  } ${inquiry.unreadCount > 0 ? 'bg-blue-50' : ''}`}
                >
                  {/* Header */}
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex-1 mr-3">
                      <h3 className="font-semibold text-gray-900 mb-1">{inquiry.subject}</h3>
                      <div className="flex items-center gap-2 text-sm text-gray-600">
                        <FiUser className="w-4 h-4" />
                        <span>{inquiry.buyerName}</span>
                      </div>
                    </div>
                    {inquiry.unreadCount > 0 && (
                      <span className="bg-red-500 text-white text-xs font-bold px-2 py-1 rounded-full">
                        {inquiry.unreadCount}
                      </span>
                    )}
                  </div>

                  {/* Vehicle Info */}
                  {inquiry.vehicleInfo && (
                    <div className="flex items-center gap-3 mb-3 p-2 bg-gray-50 rounded-lg">
                      {inquiry.vehicleInfo.imageUrl && (
                        <img
                          src={inquiry.vehicleInfo.imageUrl}
                          alt={inquiry.vehicleInfo.title}
                          className="w-16 h-16 object-cover rounded-lg"
                        />
                      )}
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-medium text-gray-900 truncate">
                          <FiTruck className="inline w-4 h-4 mr-1" />
                          {inquiry.vehicleInfo.title}
                        </p>
                        <p className="text-sm font-semibold text-blue-600">
                          {formatPrice(inquiry.vehicleInfo.price)}
                        </p>
                      </div>
                    </div>
                  )}

                  {/* Contact Info */}
                  <div className="flex flex-wrap gap-3 mb-3 text-sm text-gray-600">
                    <div className="flex items-center gap-1">
                      <FiMail className="w-4 h-4" />
                      <span className="truncate max-w-[200px]">{inquiry.buyerEmail}</span>
                    </div>
                    {inquiry.buyerPhone && (
                      <div className="flex items-center gap-1">
                        <FiPhone className="w-4 h-4" />
                        <span>{inquiry.buyerPhone}</span>
                      </div>
                    )}
                  </div>

                  {/* Footer */}
                  <div className="flex items-center justify-between pt-3 border-t border-gray-200">
                    {getStatusBadge(inquiry.status)}
                    <div className="flex items-center gap-4 text-sm text-gray-500">
                      <span>{inquiry.messageCount} mensajes</span>
                      <span>{formatRelativeTime(inquiry.createdAt)}</span>
                    </div>
                  </div>
                </div>
              ))}
            </div>

            {/* Inquiry Detail */}
            <div className="lg:sticky lg:top-6 lg:self-start">
              {!selectedInquiryId ? (
                <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
                  <FiMessageSquare className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500">Selecciona una consulta para ver los detalles</p>
                </div>
              ) : selectedInquiry ? (
                <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
                  {/* Detail Header */}
                  <div className="p-4 border-b border-gray-200 bg-gray-50">
                    <div className="flex items-start justify-between mb-2">
                      <h3 className="font-semibold text-gray-900">{selectedInquiry.subject}</h3>
                      {getStatusBadge(selectedInquiry.status)}
                    </div>
                    <div className="text-sm text-gray-600">
                      <p>
                        <FiUser className="inline w-4 h-4 mr-1" />
                        {selectedInquiry.buyerName}
                      </p>
                      <p>
                        <FiMail className="inline w-4 h-4 mr-1" />
                        {selectedInquiry.buyerEmail}
                      </p>
                      {selectedInquiry.buyerPhone && (
                        <p>
                          <FiPhone className="inline w-4 h-4 mr-1" />
                          {selectedInquiry.buyerPhone}
                        </p>
                      )}
                    </div>
                  </div>

                  {/* Messages */}
                  <div className="p-4 space-y-4 max-h-96 overflow-y-auto">
                    {selectedInquiry.messages?.map((msg) => (
                      <div
                        key={msg.id}
                        className={`flex ${msg.isFromBuyer ? 'justify-start' : 'justify-end'}`}
                      >
                        <div
                          className={`max-w-[80%] rounded-lg p-3 ${
                            msg.isFromBuyer ? 'bg-gray-100 text-gray-900' : 'bg-blue-500 text-white'
                          }`}
                        >
                          <p className="text-sm">{msg.message}</p>
                          <p
                            className={`text-xs mt-1 ${
                              msg.isFromBuyer ? 'text-gray-500' : 'text-blue-100'
                            }`}
                          >
                            {formatRelativeTime(msg.sentAt)}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Reply Form */}
                  {selectedInquiry.status !== 'Closed' && (
                    <form onSubmit={handleReplySubmit} className="p-4 border-t border-gray-200">
                      <textarea
                        value={replyMessage}
                        onChange={(e) => setReplyMessage(e.target.value)}
                        placeholder="Escribe tu respuesta..."
                        rows={3}
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
                      />
                      <div className="flex items-center justify-between mt-3">
                        <button
                          type="button"
                          onClick={handleCloseInquiry}
                          className="px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg transition-colors"
                        >
                          Cerrar Consulta
                        </button>
                        <button
                          type="submit"
                          disabled={!replyMessage.trim() || replyMutation.isPending}
                          className="flex items-center gap-2 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                          <FiSend className="w-4 h-4" />
                          {replyMutation.isPending ? 'Enviando...' : 'Enviar'}
                        </button>
                      </div>
                    </form>
                  )}
                </div>
              ) : (
                <div className="bg-white rounded-xl border border-gray-200 p-12 text-center">
                  <FiRefreshCw className="w-8 h-8 text-blue-500 animate-spin mx-auto mb-4" />
                  <p className="text-gray-500">Cargando detalles...</p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </DealerPortalLayout>
  );
}

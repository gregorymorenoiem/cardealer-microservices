import React, { useState, useEffect } from 'react';
import { MessageSquare, Clock, CheckCircle, Car, User, Reply } from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';

interface Inquiry {
  id: string;
  vehicleId: string;
  subject: string;
  status: 'Open' | 'Responded' | 'Closed';
  createdAt: string;
  respondedAt?: string;
  messageCount: number;
  lastMessage?: string;
  vehicleInfo?: {
    title: string;
    price: number;
    imageUrl?: string;
  };
  sellerInfo?: {
    name: string;
    email: string;
  };
}

export const MyInquiriesPage: React.FC = () => {
  const [inquiries, setInquiries] = useState<Inquiry[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [selectedInquiry, setSelectedInquiry] = useState<string | null>(null);

  useEffect(() => {
    fetchMyInquiries();
  }, []);

  const fetchMyInquiries = async () => {
    try {
      setIsLoading(true);
      const token = localStorage.getItem('authToken');
      const response = await fetch('/api/contactrequests/my-inquiries', {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.ok) {
        const data = await response.json();
        setInquiries(data);
      }
    } catch (error) {
      console.error('Error fetching inquiries:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const getStatusBadge = (status: string) => {
    const styles = {
      Open: 'bg-yellow-100 text-yellow-800 border-yellow-200',
      Responded: 'bg-green-100 text-green-800 border-green-200',
      Closed: 'bg-gray-100 text-gray-800 border-gray-200',
    };

    const icons = {
      Open: Clock,
      Responded: CheckCircle,
      Closed: CheckCircle,
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

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffInHours = (now.getTime() - date.getTime()) / (1000 * 60 * 60);

    if (diffInHours < 24) {
      return date.toLocaleTimeString('es-DO', {
        hour: '2-digit',
        minute: '2-digit',
      });
    } else {
      return date.toLocaleDateString('es-DO', {
        day: '2-digit',
        month: 'short',
        year: diffInHours > 8760 ? 'numeric' : undefined,
      });
    }
  };

  if (isLoading) {
    return (
      <MainLayout>
        <div className="min-h-screen bg-gray-50">
          <div className="max-w-4xl mx-auto py-8 px-4">
            <div className="animate-pulse">
              <div className="h-8 bg-gray-200 rounded w-1/4 mb-6"></div>
              <div className="space-y-4">
                {[1, 2, 3].map((i) => (
                  <div key={i} className="bg-white rounded-lg p-6">
                    <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                    <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </MainLayout>
    );
  }

  return (
    <MainLayout>
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-4xl mx-auto py-8 px-4">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center space-x-3 mb-2">
              <MessageSquare className="h-8 w-8 text-blue-600" />
              <h1 className="text-3xl font-bold text-gray-900">Mis Consultas</h1>
            </div>
            <p className="text-gray-600">Historial de mensajes enviados a vendedores</p>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <div className="bg-white rounded-lg p-6 shadow-sm">
              <div className="flex items-center">
                <div className="p-2 bg-blue-100 rounded-lg">
                  <MessageSquare className="h-6 w-6 text-blue-600" />
                </div>
                <div className="ml-4">
                  <p className="text-2xl font-semibold text-gray-900">{inquiries.length}</p>
                  <p className="text-sm text-gray-600">Total consultas</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg p-6 shadow-sm">
              <div className="flex items-center">
                <div className="p-2 bg-yellow-100 rounded-lg">
                  <Clock className="h-6 w-6 text-yellow-600" />
                </div>
                <div className="ml-4">
                  <p className="text-2xl font-semibold text-gray-900">
                    {inquiries.filter((i) => i.status === 'Open').length}
                  </p>
                  <p className="text-sm text-gray-600">Pendientes</p>
                </div>
              </div>
            </div>

            <div className="bg-white rounded-lg p-6 shadow-sm">
              <div className="flex items-center">
                <div className="p-2 bg-green-100 rounded-lg">
                  <CheckCircle className="h-6 w-6 text-green-600" />
                </div>
                <div className="ml-4">
                  <p className="text-2xl font-semibold text-gray-900">
                    {inquiries.filter((i) => i.status === 'Responded').length}
                  </p>
                  <p className="text-sm text-gray-600">Respondidas</p>
                </div>
              </div>
            </div>
          </div>

          {/* Inquiries List */}
          {inquiries.length === 0 ? (
            <div className="bg-white rounded-lg p-12 text-center">
              <MessageSquare className="h-12 w-12 text-gray-300 mx-auto mb-4" />
              <h3 className="text-lg font-semibold text-gray-900 mb-2">No tienes consultas aún</h3>
              <p className="text-gray-600 mb-6">
                Cuando contactes vendedores sobre sus vehículos, aparecerán aquí.
              </p>
              <a
                href="/search"
                className="inline-flex items-center px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors"
              >
                <Car className="h-4 w-4 mr-2" />
                Buscar Vehículos
              </a>
            </div>
          ) : (
            <div className="space-y-4">
              {inquiries.map((inquiry) => (
                <div
                  key={inquiry.id}
                  className="bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow cursor-pointer"
                  onClick={() => setSelectedInquiry(inquiry.id)}
                >
                  <div className="p-6">
                    <div className="flex items-start justify-between mb-4">
                      <div className="flex-1">
                        <div className="flex items-center space-x-3 mb-2">
                          <h3 className="text-lg font-semibold text-gray-900">{inquiry.subject}</h3>
                          {getStatusBadge(inquiry.status)}
                        </div>

                        {inquiry.vehicleInfo && (
                          <div className="flex items-center space-x-3 text-sm text-gray-600 mb-2">
                            <Car className="h-4 w-4" />
                            <span>{inquiry.vehicleInfo.title}</span>
                            <span className="text-blue-600 font-semibold">
                              {inquiry.vehicleInfo.price.toLocaleString('es-DO', {
                                style: 'currency',
                                currency: 'DOP',
                              })}
                            </span>
                          </div>
                        )}

                        {inquiry.sellerInfo && (
                          <div className="flex items-center space-x-3 text-sm text-gray-600">
                            <User className="h-4 w-4" />
                            <span>{inquiry.sellerInfo.name}</span>
                          </div>
                        )}
                      </div>

                      <div className="text-right text-sm text-gray-500">
                        <p>{formatDate(inquiry.createdAt)}</p>
                        {inquiry.respondedAt && (
                          <p className="text-green-600">Resp: {formatDate(inquiry.respondedAt)}</p>
                        )}
                      </div>
                    </div>

                    {inquiry.lastMessage && (
                      <div className="bg-gray-50 rounded-md p-3 mb-3">
                        <p className="text-sm text-gray-700 line-clamp-2">{inquiry.lastMessage}</p>
                      </div>
                    )}

                    <div className="flex items-center justify-between text-sm">
                      <div className="flex items-center space-x-4 text-gray-600">
                        <span className="flex items-center space-x-1">
                          <Reply className="h-4 w-4" />
                          <span>{inquiry.messageCount} mensajes</span>
                        </span>
                      </div>

                      <button
                        className="text-blue-600 hover:text-blue-700 font-medium flex items-center space-x-1"
                        onClick={(e) => {
                          e.stopPropagation();
                          setSelectedInquiry(inquiry.id);
                        }}
                      >
                        <span>Ver conversación</span>
                        <Reply className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </MainLayout>
  );
};

export default MyInquiriesPage;

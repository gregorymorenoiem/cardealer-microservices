import React, { useState, useEffect } from 'react';
import {
  Mail,
  Phone,
  MessageSquare,
  Clock,
  CheckCircle,
  User,
  Car,
  Reply,
  Send,
} from 'lucide-react';
import MainLayout from '@/layouts/MainLayout';

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
  vehicleInfo?: {
    title: string;
    price: number;
    imageUrl?: string;
  };
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

export const ReceivedInquiriesPage: React.FC = () => {
  const [inquiries, setInquiries] = useState<ReceivedInquiry[]>([]);
  const [selectedInquiry, setSelectedInquiry] = useState<InquiryDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [replyMessage, setReplyMessage] = useState('');
  const [isSubmittingReply, setIsSubmittingReply] = useState(false);

  useEffect(() => {
    fetchReceivedInquiries();
  }, []);

  const fetchReceivedInquiries = async () => {
    try {
      setIsLoading(true);
      const token = localStorage.getItem('accessToken');
      const response = await fetch('/api/contactrequests/received', {
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

  const fetchInquiryDetail = async (inquiryId: string) => {
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`/api/contactrequests/${inquiryId}`, {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });

      if (response.ok) {
        const data = await response.json();
        setSelectedInquiry(data);
      }
    } catch (error) {
      console.error('Error fetching inquiry detail:', error);
    }
  };

  const handleReplySubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedInquiry || !replyMessage.trim()) return;

    setIsSubmittingReply(true);
    try {
      const token = localStorage.getItem('accessToken');
      const response = await fetch(`/api/contactrequests/${selectedInquiry.id}/reply`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ message: replyMessage }),
      });

      if (response.ok) {
        // Refresh the inquiry detail
        await fetchInquiryDetail(selectedInquiry.id);
        setReplyMessage('');
        // Also refresh the main list
        await fetchReceivedInquiries();
      }
    } catch (error) {
      console.error('Error sending reply:', error);
    } finally {
      setIsSubmittingReply(false);
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

    if (diffInHours < 1) {
      return 'Hace unos minutos';
    } else if (diffInHours < 24) {
      return `Hace ${Math.floor(diffInHours)} horas`;
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
          <div className="max-w-6xl mx-auto py-8 px-4">
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
        <div className="max-w-6xl mx-auto py-8 px-4">
          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center space-x-3 mb-2">
              <Mail className="h-8 w-8 text-blue-600" />
              <h1 className="text-3xl font-bold text-gray-900">Consultas Recibidas</h1>
            </div>
            <p className="text-gray-600">Mensajes de compradores interesados en tus vehículos</p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Inquiries List */}
            <div className="lg:col-span-1">
              <div className="bg-white rounded-lg shadow-sm">
                <div className="p-4 border-b">
                  <h2 className="text-lg font-semibold text-gray-900">
                    Mensajes ({inquiries.length})
                  </h2>
                </div>

                <div className="max-h-96 overflow-y-auto">
                  {inquiries.length === 0 ? (
                    <div className="p-6 text-center">
                      <Mail className="h-8 w-8 text-gray-300 mx-auto mb-3" />
                      <p className="text-gray-600">No hay consultas aún</p>
                    </div>
                  ) : (
                    inquiries.map((inquiry) => (
                      <div
                        key={inquiry.id}
                        className={`p-4 border-b cursor-pointer hover:bg-gray-50 transition-colors ${
                          selectedInquiry?.id === inquiry.id ? 'bg-blue-50 border-blue-200' : ''
                        }`}
                        onClick={() => fetchInquiryDetail(inquiry.id)}
                      >
                        <div className="flex items-start justify-between mb-2">
                          <div className="flex-1 min-w-0">
                            <p className="text-sm font-semibold text-gray-900 truncate">
                              {inquiry.buyerName}
                            </p>
                            <p className="text-xs text-gray-600 truncate">{inquiry.subject}</p>
                          </div>
                          {inquiry.unreadCount > 0 && (
                            <div className="bg-red-500 text-white text-xs rounded-full h-5 w-5 flex items-center justify-center ml-2">
                              {inquiry.unreadCount}
                            </div>
                          )}
                        </div>

                        <div className="flex items-center justify-between">
                          {getStatusBadge(inquiry.status)}
                          <span className="text-xs text-gray-500">
                            {formatDate(inquiry.createdAt)}
                          </span>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </div>

            {/* Conversation Detail */}
            <div className="lg:col-span-2">
              {selectedInquiry ? (
                <div className="bg-white rounded-lg shadow-sm h-96 flex flex-col">
                  {/* Header */}
                  <div className="p-6 border-b">
                    <div className="flex items-start justify-between mb-4">
                      <div>
                        <h3 className="text-lg font-semibold text-gray-900 mb-1">
                          {selectedInquiry.subject}
                        </h3>
                        <div className="flex items-center space-x-4 text-sm text-gray-600">
                          <span className="flex items-center space-x-1">
                            <User className="h-4 w-4" />
                            <span>{selectedInquiry.buyerName}</span>
                          </span>
                          <a
                            href={`mailto:${selectedInquiry.buyerEmail}`}
                            className="flex items-center space-x-1 text-blue-600 hover:text-blue-700"
                          >
                            <Mail className="h-4 w-4" />
                            <span>{selectedInquiry.buyerEmail}</span>
                          </a>
                          {selectedInquiry.buyerPhone && (
                            <a
                              href={`tel:${selectedInquiry.buyerPhone}`}
                              className="flex items-center space-x-1 text-green-600 hover:text-green-700"
                            >
                              <Phone className="h-4 w-4" />
                              <span>{selectedInquiry.buyerPhone}</span>
                            </a>
                          )}
                        </div>
                      </div>
                      {getStatusBadge(selectedInquiry.status)}
                    </div>

                    {/* Vehicle Info */}
                    {selectedInquiry.vehicleInfo && (
                      <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                        <Car className="h-5 w-5 text-gray-600" />
                        <div>
                          <p className="font-medium text-gray-900">
                            {selectedInquiry.vehicleInfo.title}
                          </p>
                          <p className="text-sm text-blue-600 font-semibold">
                            {selectedInquiry.vehicleInfo.price.toLocaleString('es-DO', {
                              style: 'currency',
                              currency: 'DOP',
                            })}
                          </p>
                        </div>
                      </div>
                    )}
                  </div>

                  {/* Messages */}
                  <div className="flex-1 overflow-y-auto p-6 space-y-4">
                    {selectedInquiry.messages.map((message) => (
                      <div
                        key={message.id}
                        className={`flex ${message.isFromBuyer ? 'justify-start' : 'justify-end'}`}
                      >
                        <div
                          className={`max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
                            message.isFromBuyer
                              ? 'bg-gray-100 text-gray-900'
                              : 'bg-blue-600 text-white'
                          }`}
                        >
                          <p className="text-sm whitespace-pre-wrap">{message.message}</p>
                          <p
                            className={`text-xs mt-1 ${
                              message.isFromBuyer ? 'text-gray-500' : 'text-blue-200'
                            }`}
                          >
                            {new Date(message.sentAt).toLocaleString('es-DO')}
                          </p>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Reply Form */}
                  <form onSubmit={handleReplySubmit} className="p-6 border-t">
                    <div className="flex space-x-3">
                      <textarea
                        value={replyMessage}
                        onChange={(e) => setReplyMessage(e.target.value)}
                        placeholder="Escribe tu respuesta..."
                        rows={3}
                        className="flex-1 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                      />
                      <button
                        type="submit"
                        disabled={!replyMessage.trim() || isSubmittingReply}
                        className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center space-x-2"
                      >
                        {isSubmittingReply ? (
                          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                        ) : (
                          <Send className="h-4 w-4" />
                        )}
                      </button>
                    </div>
                  </form>
                </div>
              ) : (
                <div className="bg-white rounded-lg shadow-sm h-96 flex items-center justify-center">
                  <div className="text-center">
                    <MessageSquare className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                    <p className="text-gray-600">
                      Selecciona una consulta para ver la conversación
                    </p>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </MainLayout>
  );
};

export default ReceivedInquiriesPage;

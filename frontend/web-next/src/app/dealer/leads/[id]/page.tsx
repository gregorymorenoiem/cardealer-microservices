/**
 * Dealer Lead Detail Page
 *
 * View and manage individual lead/inquiry details
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  ArrowLeft,
  User,
  Mail,
  Phone,
  Car,
  Calendar,
  Clock,
  MessageSquare,
  Send,
  CheckCircle,
  Star,
  Archive,
  MoreVertical,
} from 'lucide-react';
import Link from 'next/link';

const lead = {
  id: '1',
  customer: {
    name: 'Juan Martínez',
    email: 'juan.martinez@email.com',
    phone: '809-555-0123',
    avatar: null,
  },
  vehicle: {
    id: 'v1',
    title: 'Toyota Camry 2022',
    price: 1250000,
    image: '/vehicles/camry.jpg',
  },
  status: 'pending',
  priority: 'high',
  source: 'WhatsApp',
  createdAt: '2024-02-15T10:30:00Z',
  message:
    'Hola, estoy interesado en el Toyota Camry 2022. ¿Cuál es el precio final? ¿Tienen financiamiento disponible?',
  conversation: [
    {
      id: '1',
      sender: 'customer',
      message:
        'Hola, estoy interesado en el Toyota Camry 2022. ¿Cuál es el precio final? ¿Tienen financiamiento disponible?',
      timestamp: '2024-02-15T10:30:00Z',
    },
    {
      id: '2',
      sender: 'dealer',
      message:
        '¡Hola Juan! Gracias por contactarnos. El precio del Toyota Camry 2022 es RD$ 1,250,000. Sí tenemos opciones de financiamiento con varias entidades bancarias.',
      timestamp: '2024-02-15T11:15:00Z',
    },
    {
      id: '3',
      sender: 'customer',
      message: '¿Puedo visitarlos el sábado para verlo?',
      timestamp: '2024-02-15T11:45:00Z',
    },
  ],
};

export default function LeadDetailPage() {
  const [replyMessage, setReplyMessage] = useState('');
  const [isSending, setIsSending] = useState(false);

  const handleSendReply = () => {
    if (!replyMessage.trim()) return;
    setIsSending(true);
    // Simulate sending
    setTimeout(() => {
      setIsSending(false);
      setReplyMessage('');
    }, 1000);
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/dealer/leads">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Detalle del Lead</h1>
            <p className="text-gray-600">Consulta #{lead.id}</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Archive className="mr-2 h-4 w-4" />
            Archivar
          </Button>
          <Button variant="outline">
            <Star className="mr-2 h-4 w-4" />
            Destacar
          </Button>
          <Button variant="outline" size="icon">
            <MoreVertical className="h-4 w-4" />
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Conversation */}
        <div className="space-y-6 lg:col-span-2">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MessageSquare className="h-5 w-5" />
                Conversación
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="mb-4 max-h-96 space-y-4 overflow-y-auto">
                {lead.conversation.map(msg => (
                  <div
                    key={msg.id}
                    className={`flex gap-3 ${msg.sender === 'dealer' ? 'flex-row-reverse' : ''}`}
                  >
                    <Avatar className="h-8 w-8">
                      <AvatarImage
                        src={
                          msg.sender === 'customer' ? lead.customer.avatar || undefined : undefined
                        }
                      />
                      <AvatarFallback
                        className={msg.sender === 'dealer' ? 'bg-emerald-100 text-emerald-700' : ''}
                      >
                        {msg.sender === 'customer'
                          ? lead.customer.name
                              .split(' ')
                              .map(n => n[0])
                              .join('')
                          : 'D'}
                      </AvatarFallback>
                    </Avatar>
                    <div
                      className={`max-w-md flex-1 rounded-lg p-3 ${
                        msg.sender === 'dealer'
                          ? 'ml-auto bg-emerald-100 text-emerald-900'
                          : 'bg-gray-100'
                      }`}
                    >
                      <p className="text-sm">{msg.message}</p>
                      <p className="mt-1 text-xs text-gray-500">{formatDate(msg.timestamp)}</p>
                    </div>
                  </div>
                ))}
              </div>

              {/* Reply Input */}
              <div className="border-t pt-4">
                <Textarea
                  placeholder="Escribe tu respuesta..."
                  value={replyMessage}
                  onChange={e => setReplyMessage(e.target.value)}
                  rows={3}
                />
                <div className="mt-2 flex justify-end">
                  <Button
                    className="bg-emerald-600 hover:bg-emerald-700"
                    onClick={handleSendReply}
                    disabled={!replyMessage.trim() || isSending}
                  >
                    <Send className="mr-2 h-4 w-4" />
                    {isSending ? 'Enviando...' : 'Enviar Respuesta'}
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Customer Info */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base">
                <User className="h-5 w-5" />
                Cliente
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <Avatar className="h-12 w-12">
                  <AvatarImage src={lead.customer.avatar || undefined} />
                  <AvatarFallback>
                    {lead.customer.name
                      .split(' ')
                      .map(n => n[0])
                      .join('')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium">{lead.customer.name}</p>
                  <Badge variant="outline" className="text-xs">
                    Nuevo
                  </Badge>
                </div>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2 text-gray-600">
                  <Mail className="h-4 w-4" />
                  <a href={`mailto:${lead.customer.email}`} className="hover:text-emerald-600">
                    {lead.customer.email}
                  </a>
                </div>
                <div className="flex items-center gap-2 text-gray-600">
                  <Phone className="h-4 w-4" />
                  <a href={`tel:${lead.customer.phone}`} className="hover:text-emerald-600">
                    {lead.customer.phone}
                  </a>
                </div>
              </div>
              <div className="flex gap-2">
                <Button variant="outline" size="sm" className="flex-1">
                  <Phone className="mr-1 h-4 w-4" />
                  Llamar
                </Button>
                <Button variant="outline" size="sm" className="flex-1">
                  <Mail className="mr-1 h-4 w-4" />
                  Email
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Vehicle */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base">
                <Car className="h-5 w-5" />
                Vehículo de Interés
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="rounded-lg bg-gray-50 p-4">
                <div className="mb-3 flex aspect-video items-center justify-center rounded-lg bg-gray-200">
                  <Car className="h-12 w-12 text-gray-400" />
                </div>
                <p className="font-medium">{lead.vehicle.title}</p>
                <p className="font-bold text-emerald-600">
                  RD$ {lead.vehicle.price.toLocaleString()}
                </p>
                <Link href={`/dealer/inventario/${lead.vehicle.id}`}>
                  <Button variant="outline" size="sm" className="mt-3 w-full">
                    Ver Vehículo
                  </Button>
                </Link>
              </div>
            </CardContent>
          </Card>

          {/* Lead Info */}
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Información del Lead</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3 text-sm">
              <div className="flex items-center justify-between">
                <span className="text-gray-500">Estado</span>
                <Badge className="bg-yellow-100 text-yellow-700">Pendiente</Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-500">Prioridad</span>
                <Badge className="bg-red-100 text-red-700">Alta</Badge>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-500">Fuente</span>
                <span className="font-medium">{lead.source}</span>
              </div>
              <div className="flex items-center justify-between">
                <span className="text-gray-500">Recibido</span>
                <span className="font-medium">{formatDate(lead.createdAt)}</span>
              </div>
            </CardContent>
          </Card>

          {/* Actions */}
          <Card>
            <CardContent className="space-y-2 p-4">
              <Button className="w-full bg-emerald-600 hover:bg-emerald-700">
                <CheckCircle className="mr-2 h-4 w-4" />
                Marcar como Atendido
              </Button>
              <Button variant="outline" className="w-full">
                <Calendar className="mr-2 h-4 w-4" />
                Agendar Cita
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

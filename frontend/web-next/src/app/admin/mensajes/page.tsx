/**
 * Admin Messages Page
 *
 * View and manage platform messages/support tickets
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import {
  MessageSquare,
  Search,
  Mail,
  MailOpen,
  Clock,
  User,
  AlertCircle,
  CheckCircle,
  Send,
  Filter,
} from 'lucide-react';

const mockTickets = [
  {
    id: '1',
    subject: 'Problema con pago de suscripción',
    from: 'AutoMax Premium',
    email: 'admin@automax.com',
    type: 'dealer',
    status: 'open',
    priority: 'high',
    lastMessage: 'No puedo completar el pago de mi suscripción...',
    createdAt: '2024-02-15T10:30:00',
    messages: 3,
    unread: true,
  },
  {
    id: '2',
    subject: 'Vehículo rechazado sin razón',
    from: 'Juan Pérez',
    email: 'juan@email.com',
    type: 'user',
    status: 'open',
    priority: 'medium',
    lastMessage: 'Mi vehículo fue rechazado pero no entiendo por qué...',
    createdAt: '2024-02-15T09:15:00',
    messages: 2,
    unread: true,
  },
  {
    id: '3',
    subject: 'Solicitud de verificación de dealer',
    from: 'CarWorld RD',
    email: 'info@carworld.com.do',
    type: 'dealer',
    status: 'pending',
    priority: 'low',
    lastMessage: 'Buenos días, quisiera saber el estado de mi verificación...',
    createdAt: '2024-02-14T16:45:00',
    messages: 4,
    unread: false,
  },
  {
    id: '4',
    subject: 'Reporte de fraude',
    from: 'María García',
    email: 'maria@email.com',
    type: 'user',
    status: 'resolved',
    priority: 'high',
    lastMessage: 'Gracias por resolver mi caso.',
    createdAt: '2024-02-13T11:20:00',
    messages: 8,
    unread: false,
  },
];

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'open':
      return <Badge className="bg-blue-100 text-blue-700">Abierto</Badge>;
    case 'pending':
      return <Badge className="bg-amber-100 text-amber-700">Pendiente</Badge>;
    case 'resolved':
      return <Badge className="bg-emerald-100 text-emerald-700">Resuelto</Badge>;
    case 'closed':
      return <Badge variant="outline">Cerrado</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

const getPriorityBadge = (priority: string) => {
  switch (priority) {
    case 'high':
      return <Badge className="bg-red-100 text-red-700">Alta</Badge>;
    case 'medium':
      return <Badge className="bg-amber-100 text-amber-700">Media</Badge>;
    case 'low':
      return <Badge variant="outline">Baja</Badge>;
    default:
      return null;
  }
};

export default function AdminMessagesPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTicket, setSelectedTicket] = useState<(typeof mockTickets)[0] | null>(null);
  const [statusFilter, setStatusFilter] = useState<string>('all');

  const filteredTickets = mockTickets.filter(t => {
    if (statusFilter !== 'all' && t.status !== statusFilter) return false;
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      return (
        t.subject.toLowerCase().includes(query) ||
        t.from.toLowerCase().includes(query) ||
        t.email.toLowerCase().includes(query)
      );
    }
    return true;
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Mensajes</h1>
          <p className="text-gray-400">Soporte y comunicación con usuarios</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Mail className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {mockTickets.filter(t => t.status === 'open').length}
                </p>
                <p className="text-sm text-gray-500">Abiertos</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Clock className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {mockTickets.filter(t => t.status === 'pending').length}
                </p>
                <p className="text-sm text-gray-500">Pendientes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-100 p-2">
                <AlertCircle className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {mockTickets.filter(t => t.priority === 'high' && t.status !== 'resolved').length}
                </p>
                <p className="text-sm text-gray-500">Alta Prioridad</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-emerald-100 p-2">
                <CheckCircle className="h-5 w-5 text-emerald-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {mockTickets.filter(t => t.status === 'resolved').length}
                </p>
                <p className="text-sm text-gray-500">Resueltos</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <div className="flex gap-4">
        <div className="relative flex-1">
          <Search className="absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2 text-gray-400" />
          <Input
            placeholder="Buscar por asunto, usuario, email..."
            className="pl-10"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="flex gap-2">
          {['all', 'open', 'pending', 'resolved'].map(status => (
            <Button
              key={status}
              variant={statusFilter === status ? 'default' : 'outline'}
              size="sm"
              onClick={() => setStatusFilter(status)}
              className={statusFilter === status ? 'bg-emerald-600' : ''}
            >
              {status === 'all'
                ? 'Todos'
                : status === 'open'
                  ? 'Abiertos'
                  : status === 'pending'
                    ? 'Pendientes'
                    : 'Resueltos'}
            </Button>
          ))}
        </div>
      </div>

      {/* Tickets List */}
      <Card>
        <CardContent className="p-0">
          <div className="divide-y">
            {filteredTickets.map(ticket => (
              <div
                key={ticket.id}
                className={`cursor-pointer p-4 transition-colors hover:bg-gray-50 ${
                  ticket.unread ? 'bg-blue-50/50' : ''
                }`}
                onClick={() => setSelectedTicket(ticket)}
              >
                <div className="flex items-start gap-4">
                  <div className="flex-shrink-0 pt-1">
                    {ticket.unread ? (
                      <Mail className="h-5 w-5 text-blue-600" />
                    ) : (
                      <MailOpen className="h-5 w-5 text-gray-400" />
                    )}
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="mb-1 flex items-center gap-2">
                      <h4
                        className={`font-medium ${ticket.unread ? 'text-gray-900' : 'text-gray-600'}`}
                      >
                        {ticket.subject}
                      </h4>
                      {getPriorityBadge(ticket.priority)}
                    </div>
                    <p className="mb-2 text-sm text-gray-500">
                      <span className="font-medium">{ticket.from}</span>
                      <span className="mx-2">•</span>
                      {ticket.email}
                      <span className="mx-2">•</span>
                      <Badge variant="outline" className="text-xs">
                        {ticket.type === 'dealer' ? 'Dealer' : 'Usuario'}
                      </Badge>
                    </p>
                    <p className="truncate text-sm text-gray-600">{ticket.lastMessage}</p>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    {getStatusBadge(ticket.status)}
                    <span className="text-xs text-gray-400">
                      {new Date(ticket.createdAt).toLocaleDateString('es-DO')}
                    </span>
                    <span className="text-xs text-gray-400">{ticket.messages} mensajes</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Quick Reply Modal would go here */}
    </div>
  );
}

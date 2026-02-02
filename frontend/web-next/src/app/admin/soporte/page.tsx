/**
 * Admin Support Tickets Page
 *
 * View and manage support tickets
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Search,
  Ticket,
  Clock,
  CheckCircle,
  MessageSquare,
  User,
  AlertCircle,
  ChevronRight,
  Send,
} from 'lucide-react';

const mockTickets = [
  {
    id: '1',
    ticketNumber: 'TKT-2024-001',
    subject: 'No puedo subir fotos de mi vehículo',
    from: 'Juan Pérez',
    email: 'juan@email.com',
    category: 'technical',
    status: 'open',
    priority: 'high',
    createdAt: '2024-02-15T09:30:00',
    lastReply: '2024-02-15T10:00:00',
    messages: 3,
  },
  {
    id: '2',
    ticketNumber: 'TKT-2024-002',
    subject: 'Reembolso de publicación destacada',
    from: 'María García',
    email: 'maria@email.com',
    category: 'billing',
    status: 'open',
    priority: 'normal',
    createdAt: '2024-02-14T14:00:00',
    lastReply: '2024-02-14T16:30:00',
    messages: 5,
  },
  {
    id: '3',
    ticketNumber: 'TKT-2024-003',
    subject: 'Verificación de dealer demorada',
    from: 'AutoMax Premium',
    email: 'admin@automax.com',
    category: 'account',
    status: 'in_progress',
    priority: 'high',
    createdAt: '2024-02-13T11:00:00',
    lastReply: '2024-02-15T08:00:00',
    messages: 8,
  },
  {
    id: '4',
    ticketNumber: 'TKT-2024-004',
    subject: 'Consulta sobre planes de dealer',
    from: 'Pedro Santos',
    email: 'pedro@business.com',
    category: 'sales',
    status: 'resolved',
    priority: 'low',
    createdAt: '2024-02-12T16:00:00',
    lastReply: '2024-02-13T10:00:00',
    messages: 4,
  },
];

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'open':
      return <Badge className="bg-blue-100 text-blue-700">Abierto</Badge>;
    case 'in_progress':
      return <Badge className="bg-amber-100 text-amber-700">En Progreso</Badge>;
    case 'resolved':
      return <Badge className="bg-emerald-100 text-emerald-700">Resuelto</Badge>;
    case 'closed':
      return <Badge variant="outline">Cerrado</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

const getCategoryBadge = (category: string) => {
  switch (category) {
    case 'technical':
      return <Badge variant="outline">Técnico</Badge>;
    case 'billing':
      return <Badge variant="outline">Facturación</Badge>;
    case 'account':
      return <Badge variant="outline">Cuenta</Badge>;
    case 'sales':
      return <Badge variant="outline">Ventas</Badge>;
    default:
      return <Badge variant="outline">{category}</Badge>;
  }
};

export default function AdminSupportPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedTicket, setSelectedTicket] = useState<(typeof mockTickets)[0] | null>(null);

  const filteredTickets = mockTickets.filter(ticket => {
    if (statusFilter !== 'all' && ticket.status !== statusFilter) return false;
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      return (
        ticket.subject.toLowerCase().includes(query) ||
        ticket.from.toLowerCase().includes(query) ||
        ticket.ticketNumber.toLowerCase().includes(query)
      );
    }
    return true;
  });

  const openCount = mockTickets.filter(t => t.status === 'open').length;
  const inProgressCount = mockTickets.filter(t => t.status === 'in_progress').length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Soporte</h1>
          <p className="text-gray-400">Gestión de tickets de soporte</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Ticket className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{openCount}</p>
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
                <p className="text-2xl font-bold">{inProgressCount}</p>
                <p className="text-sm text-gray-500">En Progreso</p>
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
                <p className="text-sm text-gray-500">Resueltos Hoy</p>
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
            placeholder="Buscar tickets..."
            className="pl-10"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="flex gap-2">
          {['all', 'open', 'in_progress', 'resolved'].map(status => (
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
                  : status === 'in_progress'
                    ? 'En Progreso'
                    : 'Resueltos'}
            </Button>
          ))}
        </div>
      </div>

      {/* Tickets List */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* List */}
        <div className="lg:col-span-2">
          <Card>
            <CardContent className="p-0">
              <div className="divide-y">
                {filteredTickets.map(ticket => (
                  <div
                    key={ticket.id}
                    className={`cursor-pointer p-4 hover:bg-gray-50 ${selectedTicket?.id === ticket.id ? 'bg-gray-50' : ''}`}
                    onClick={() => setSelectedTicket(ticket)}
                  >
                    <div className="flex items-start justify-between">
                      <div className="flex-1">
                        <div className="mb-1 flex items-center gap-2">
                          <span className="text-xs text-gray-400">{ticket.ticketNumber}</span>
                          {ticket.priority === 'high' && (
                            <Badge className="bg-red-100 text-xs text-red-700">Urgente</Badge>
                          )}
                        </div>
                        <h4 className="mb-1 font-medium">{ticket.subject}</h4>
                        <p className="text-sm text-gray-500">
                          {ticket.from} • {ticket.email}
                        </p>
                        <div className="mt-2 flex items-center gap-3">
                          {getStatusBadge(ticket.status)}
                          {getCategoryBadge(ticket.category)}
                          <span className="text-xs text-gray-400">{ticket.messages} mensajes</span>
                        </div>
                      </div>
                      <ChevronRight className="h-5 w-5 text-gray-400" />
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Detail */}
        <div className="lg:col-span-1">
          {selectedTicket ? (
            <Card className="sticky top-4">
              <CardHeader className="pb-3">
                <CardTitle className="text-lg">{selectedTicket.ticketNumber}</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <p className="text-sm text-gray-500">Asunto</p>
                  <p className="font-medium">{selectedTicket.subject}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <p className="text-sm text-gray-500">De</p>
                    <p className="font-medium">{selectedTicket.from}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500">Estado</p>
                    {getStatusBadge(selectedTicket.status)}
                  </div>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Última respuesta</p>
                  <p className="text-sm">
                    {new Date(selectedTicket.lastReply).toLocaleString('es-DO')}
                  </p>
                </div>

                <div className="border-t pt-4">
                  <Textarea placeholder="Escribir respuesta..." className="mb-3" />
                  <Button className="w-full bg-emerald-600 hover:bg-emerald-700">
                    <Send className="mr-2 h-4 w-4" />
                    Enviar Respuesta
                  </Button>
                </div>

                <div className="flex gap-2">
                  {selectedTicket.status !== 'resolved' && (
                    <Button variant="outline" size="sm" className="flex-1">
                      <CheckCircle className="mr-1 h-4 w-4" />
                      Resolver
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent className="p-8 text-center text-gray-400">
                <MessageSquare className="mx-auto mb-4 h-12 w-12 opacity-50" />
                <p>Selecciona un ticket para ver detalles</p>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

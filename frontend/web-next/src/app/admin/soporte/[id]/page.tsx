/**
 * Admin Support Ticket Detail Page
 *
 * Individual support ticket management
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  ArrowLeft,
  User,
  Mail,
  Calendar,
  Clock,
  MessageSquare,
  Send,
  CheckCircle,
  AlertTriangle,
  Paperclip,
  Phone,
  Tag,
  ArrowUpRight,
} from 'lucide-react';
import Link from 'next/link';

const ticket = {
  id: 'TKT-2024-0156',
  subject: 'No puedo subir fotos de mi vehículo',
  status: 'open',
  priority: 'high',
  category: 'technical',
  createdAt: '2024-02-15T10:30:00Z',
  lastUpdated: '2024-02-15T14:45:00Z',
  user: {
    id: 'usr-123',
    name: 'Juan Carlos Martínez',
    email: 'juan.martinez@email.com',
    phone: '809-555-0123',
    avatar: null,
    type: 'seller',
  },
  assignedTo: null,
  conversation: [
    {
      id: '1',
      sender: 'user',
      message:
        'Hola, estoy intentando subir fotos de mi vehículo pero me da error. Ya intenté con diferentes formatos y tamaños pero sigue sin funcionar.',
      timestamp: '2024-02-15T10:30:00Z',
      attachments: [],
    },
    {
      id: '2',
      sender: 'system',
      message: 'Ticket asignado automáticamente a la cola de Soporte Técnico.',
      timestamp: '2024-02-15T10:31:00Z',
    },
    {
      id: '3',
      sender: 'agent',
      agentName: 'María Soporte',
      message:
        'Hola Juan, gracias por contactarnos. ¿Podrías indicarme qué mensaje de error exactamente te aparece? También, ¿desde qué dispositivo estás intentando subir las fotos?',
      timestamp: '2024-02-15T11:15:00Z',
      attachments: [],
    },
    {
      id: '4',
      sender: 'user',
      message:
        'Estoy usando mi celular Samsung. El error dice "Error al procesar imagen". Adjunto screenshot.',
      timestamp: '2024-02-15T14:45:00Z',
      attachments: [{ name: 'screenshot.jpg', size: '245 KB' }],
    },
  ],
  relatedTickets: [
    { id: 'TKT-2024-0098', subject: 'Problema con carga de imágenes', status: 'resolved' },
  ],
};

export default function SupportTicketDetailPage() {
  const [reply, setReply] = useState('');
  const [isSending, setIsSending] = useState(false);

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'open':
        return <Badge className="bg-blue-600">Abierto</Badge>;
      case 'in-progress':
        return <Badge className="bg-yellow-600">En Progreso</Badge>;
      case 'waiting':
        return <Badge className="bg-purple-600">Esperando Usuario</Badge>;
      case 'resolved':
        return <Badge className="bg-emerald-600">Resuelto</Badge>;
      case 'closed':
        return <Badge className="bg-gray-600">Cerrado</Badge>;
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const getPriorityBadge = (priority: string) => {
    switch (priority) {
      case 'high':
        return <Badge className="bg-red-600">Alta</Badge>;
      case 'medium':
        return <Badge className="bg-yellow-600">Media</Badge>;
      case 'low':
        return <Badge className="bg-gray-600">Baja</Badge>;
      default:
        return <Badge variant="outline">{priority}</Badge>;
    }
  };

  const handleSendReply = () => {
    if (!reply.trim()) return;
    setIsSending(true);
    setTimeout(() => {
      setIsSending(false);
      setReply('');
    }, 1000);
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Link href="/admin/soporte">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{ticket.id}</h1>
              {getStatusBadge(ticket.status)}
              {getPriorityBadge(ticket.priority)}
            </div>
            <p className="text-gray-400">{ticket.subject}</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <ArrowUpRight className="mr-2 h-4 w-4" />
            Escalar
          </Button>
          <Button className="bg-emerald-600 hover:bg-emerald-700">
            <CheckCircle className="mr-2 h-4 w-4" />
            Marcar Resuelto
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Conversation */}
        <div className="space-y-6 lg:col-span-2">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-white">
                <MessageSquare className="h-5 w-5" />
                Conversación
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="mb-4 max-h-[500px] space-y-4 overflow-y-auto">
                {ticket.conversation.map(msg => {
                  if (msg.sender === 'system') {
                    return (
                      <div key={msg.id} className="text-center">
                        <span className="rounded-full bg-slate-700 px-3 py-1 text-xs text-gray-500">
                          {msg.message} • {formatDate(msg.timestamp)}
                        </span>
                      </div>
                    );
                  }

                  const isAgent = msg.sender === 'agent';

                  return (
                    <div key={msg.id} className={`flex gap-3 ${isAgent ? 'flex-row-reverse' : ''}`}>
                      <Avatar className="h-8 w-8 flex-shrink-0">
                        <AvatarFallback className={isAgent ? 'bg-emerald-600' : 'bg-slate-600'}>
                          {isAgent
                            ? 'S'
                            : ticket.user.name
                                .split(' ')
                                .map(n => n[0])
                                .join('')}
                        </AvatarFallback>
                      </Avatar>
                      <div className={`max-w-md flex-1 ${isAgent ? 'text-right' : ''}`}>
                        <div className="mb-1 flex items-center gap-2">
                          {isAgent ? (
                            <>
                              <span className="text-xs text-gray-500">
                                {formatDate(msg.timestamp)}
                              </span>
                              <span className="text-sm font-medium text-emerald-400">
                                {msg.agentName}
                              </span>
                            </>
                          ) : (
                            <>
                              <span className="text-sm font-medium text-gray-300">
                                {ticket.user.name}
                              </span>
                              <span className="text-xs text-gray-500">
                                {formatDate(msg.timestamp)}
                              </span>
                            </>
                          )}
                        </div>
                        <div
                          className={`rounded-lg p-3 ${
                            isAgent
                              ? 'bg-emerald-900/30 text-gray-200'
                              : 'bg-slate-700 text-gray-200'
                          }`}
                        >
                          <p className="text-sm">{msg.message}</p>
                          {msg.attachments && msg.attachments.length > 0 && (
                            <div className="mt-2 border-t border-slate-600 pt-2">
                              {msg.attachments.map((att, i) => (
                                <div
                                  key={i}
                                  className="flex items-center gap-2 text-xs text-gray-400"
                                >
                                  <Paperclip className="h-3 w-3" />
                                  <span>{att.name}</span>
                                  <span>({att.size})</span>
                                </div>
                              ))}
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Reply Input */}
              <div className="border-t border-slate-700 pt-4">
                <Textarea
                  placeholder="Escribe tu respuesta..."
                  className="mb-2 border-slate-600 bg-slate-700"
                  value={reply}
                  onChange={e => setReply(e.target.value)}
                  rows={3}
                />
                <div className="flex items-center justify-between">
                  <Button variant="outline" size="sm">
                    <Paperclip className="mr-2 h-4 w-4" />
                    Adjuntar
                  </Button>
                  <div className="flex gap-2">
                    <Select defaultValue="public">
                      <SelectTrigger className="w-32 border-slate-600 bg-slate-700">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="public">Pública</SelectItem>
                        <SelectItem value="internal">Nota Interna</SelectItem>
                      </SelectContent>
                    </Select>
                    <Button
                      className="bg-emerald-600 hover:bg-emerald-700"
                      onClick={handleSendReply}
                      disabled={!reply.trim() || isSending}
                    >
                      <Send className="mr-2 h-4 w-4" />
                      {isSending ? 'Enviando...' : 'Enviar'}
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* User Info */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base text-white">
                <User className="h-5 w-5" />
                Usuario
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center gap-3">
                <Avatar className="h-12 w-12">
                  <AvatarFallback className="bg-slate-600">
                    {ticket.user.name
                      .split(' ')
                      .map(n => n[0])
                      .join('')}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <p className="font-medium text-white">{ticket.user.name}</p>
                  <Badge variant="outline" className="text-xs">
                    {ticket.user.type === 'seller' ? 'Vendedor' : 'Comprador'}
                  </Badge>
                </div>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2 text-gray-400">
                  <Mail className="h-4 w-4" />
                  <span>{ticket.user.email}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Phone className="h-4 w-4" />
                  <span>{ticket.user.phone}</span>
                </div>
              </div>
              <Link href={`/admin/usuarios/${ticket.user.id}`}>
                <Button variant="outline" size="sm" className="w-full">
                  Ver Perfil Completo
                </Button>
              </Link>
            </CardContent>
          </Card>

          {/* Ticket Details */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-base text-white">Detalles del Ticket</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between border-b border-slate-700 py-2">
                <span className="text-gray-400">Estado</span>
                <Select defaultValue={ticket.status}>
                  <SelectTrigger className="h-8 w-32 border-slate-600 bg-slate-700">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="open">Abierto</SelectItem>
                    <SelectItem value="in-progress">En Progreso</SelectItem>
                    <SelectItem value="waiting">Esperando</SelectItem>
                    <SelectItem value="resolved">Resuelto</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex justify-between border-b border-slate-700 py-2">
                <span className="text-gray-400">Prioridad</span>
                <Select defaultValue={ticket.priority}>
                  <SelectTrigger className="h-8 w-32 border-slate-600 bg-slate-700">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="low">Baja</SelectItem>
                    <SelectItem value="medium">Media</SelectItem>
                    <SelectItem value="high">Alta</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="flex justify-between border-b border-slate-700 py-2">
                <span className="text-gray-400">Categoría</span>
                <Badge variant="outline">Técnico</Badge>
              </div>
              <div className="flex justify-between border-b border-slate-700 py-2">
                <span className="text-gray-400">Creado</span>
                <span className="text-sm text-white">{formatDate(ticket.createdAt)}</span>
              </div>
              <div className="flex justify-between py-2">
                <span className="text-gray-400">Última actualización</span>
                <span className="text-sm text-white">{formatDate(ticket.lastUpdated)}</span>
              </div>
            </CardContent>
          </Card>

          {/* Quick Actions */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-base text-white">Acciones Rápidas</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              <Button variant="outline" className="w-full justify-start">
                <Tag className="mr-2 h-4 w-4" />
                Agregar Etiqueta
              </Button>
              <Button variant="outline" className="w-full justify-start">
                <User className="mr-2 h-4 w-4" />
                Asignar Agente
              </Button>
              <Button variant="outline" className="w-full justify-start text-yellow-400">
                <AlertTriangle className="mr-2 h-4 w-4" />
                Reportar Problema
              </Button>
            </CardContent>
          </Card>

          {/* Related Tickets */}
          {ticket.relatedTickets.length > 0 && (
            <Card className="border-slate-700 bg-slate-800">
              <CardHeader>
                <CardTitle className="text-base text-white">Tickets Relacionados</CardTitle>
              </CardHeader>
              <CardContent>
                {ticket.relatedTickets.map(related => (
                  <Link key={related.id} href={`/admin/soporte/${related.id}`}>
                    <div className="rounded-lg bg-slate-700/50 p-3 transition-colors hover:bg-slate-700">
                      <p className="text-sm font-medium text-white">{related.id}</p>
                      <p className="truncate text-xs text-gray-400">{related.subject}</p>
                      <Badge className="mt-1 bg-emerald-600 text-xs">Resuelto</Badge>
                    </div>
                  </Link>
                ))}
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}

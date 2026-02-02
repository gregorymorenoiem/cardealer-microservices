/**
 * Admin KYC Queue Page
 *
 * Review and process KYC verification requests
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import {
  Search,
  Clock,
  CheckCircle,
  XCircle,
  Eye,
  User,
  FileText,
  AlertTriangle,
  Building2,
  Calendar,
} from 'lucide-react';

const mockKycRequests = [
  {
    id: '1',
    type: 'dealer',
    name: 'AutoMax Premium',
    email: 'admin@automax.com',
    submittedAt: '2024-02-15T08:00:00',
    status: 'pending',
    priority: 'high',
    documents: ['RNC', 'Registro Mercantil', 'Cédula Representante'],
    notes: 'Documentos completos, listo para revisión',
  },
  {
    id: '2',
    type: 'dealer',
    name: 'CarWorld RD',
    email: 'info@carworld.do',
    submittedAt: '2024-02-14T14:30:00',
    status: 'pending',
    priority: 'normal',
    documents: ['RNC', 'Registro Mercantil'],
    notes: 'Falta cédula del representante legal',
  },
  {
    id: '3',
    type: 'user',
    name: 'Juan Pérez',
    email: 'juan@email.com',
    submittedAt: '2024-02-14T10:15:00',
    status: 'in_review',
    priority: 'normal',
    documents: ['Cédula', 'Comprobante Domicilio'],
    notes: 'En revisión por analista',
  },
  {
    id: '4',
    type: 'dealer',
    name: 'Motors Plus',
    email: 'contact@motorsplus.com',
    submittedAt: '2024-02-13T16:45:00',
    status: 'approved',
    priority: 'normal',
    documents: ['RNC', 'Registro Mercantil', 'Cédula Representante', 'Contrato Local'],
    notes: 'Verificación completa',
  },
  {
    id: '5',
    type: 'user',
    name: 'María García',
    email: 'maria@email.com',
    submittedAt: '2024-02-12T09:00:00',
    status: 'rejected',
    priority: 'normal',
    documents: ['Cédula'],
    notes: 'Documento ilegible, se solicitó reenvío',
  },
];

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'pending':
      return <Badge className="bg-amber-100 text-amber-700">Pendiente</Badge>;
    case 'in_review':
      return <Badge className="bg-blue-100 text-blue-700">En Revisión</Badge>;
    case 'approved':
      return <Badge className="bg-emerald-100 text-emerald-700">Aprobado</Badge>;
    case 'rejected':
      return <Badge className="bg-red-100 text-red-700">Rechazado</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

export default function AdminKycPage() {
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [selectedRequest, setSelectedRequest] = useState<(typeof mockKycRequests)[0] | null>(null);

  const filteredRequests = mockKycRequests.filter(req => {
    if (statusFilter !== 'all' && req.status !== statusFilter) return false;
    if (searchQuery) {
      const query = searchQuery.toLowerCase();
      return req.name.toLowerCase().includes(query) || req.email.toLowerCase().includes(query);
    }
    return true;
  });

  const pendingCount = mockKycRequests.filter(r => r.status === 'pending').length;
  const inReviewCount = mockKycRequests.filter(r => r.status === 'in_review').length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Verificación KYC</h1>
          <p className="text-gray-400">Cola de verificación de identidad</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Clock className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{pendingCount}</p>
                <p className="text-sm text-gray-500">Pendientes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Eye className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{inReviewCount}</p>
                <p className="text-sm text-gray-500">En Revisión</p>
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
                  {mockKycRequests.filter(r => r.status === 'approved').length}
                </p>
                <p className="text-sm text-gray-500">Aprobados Hoy</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-100 p-2">
                <XCircle className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">
                  {mockKycRequests.filter(r => r.status === 'rejected').length}
                </p>
                <p className="text-sm text-gray-500">Rechazados</p>
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
            placeholder="Buscar por nombre o email..."
            className="pl-10"
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />
        </div>
        <div className="flex gap-2">
          {['all', 'pending', 'in_review', 'approved', 'rejected'].map(status => (
            <Button
              key={status}
              variant={statusFilter === status ? 'default' : 'outline'}
              size="sm"
              onClick={() => setStatusFilter(status)}
              className={statusFilter === status ? 'bg-emerald-600' : ''}
            >
              {status === 'all'
                ? 'Todos'
                : status === 'pending'
                  ? 'Pendientes'
                  : status === 'in_review'
                    ? 'En Revisión'
                    : status === 'approved'
                      ? 'Aprobados'
                      : 'Rechazados'}
            </Button>
          ))}
        </div>
      </div>

      {/* KYC Queue */}
      <Card>
        <CardContent className="p-0">
          <div className="divide-y">
            {filteredRequests.map(request => (
              <div
                key={request.id}
                className="cursor-pointer p-4 hover:bg-gray-50"
                onClick={() => setSelectedRequest(request)}
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start gap-4">
                    <div
                      className={`rounded-lg p-2 ${request.type === 'dealer' ? 'bg-blue-100' : 'bg-gray-100'}`}
                    >
                      {request.type === 'dealer' ? (
                        <Building2 className="h-5 w-5 text-blue-600" />
                      ) : (
                        <User className="h-5 w-5 text-gray-600" />
                      )}
                    </div>
                    <div>
                      <div className="mb-1 flex items-center gap-2">
                        <h4 className="font-medium">{request.name}</h4>
                        {request.priority === 'high' && (
                          <Badge className="bg-red-100 text-xs text-red-700">Urgente</Badge>
                        )}
                      </div>
                      <p className="text-sm text-gray-500">{request.email}</p>
                      <div className="mt-2 flex items-center gap-4 text-xs text-gray-400">
                        <span className="flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {new Date(request.submittedAt).toLocaleDateString('es-DO')}
                        </span>
                        <span className="flex items-center gap-1">
                          <FileText className="h-3 w-3" />
                          {request.documents.length} documentos
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="flex flex-col items-end gap-2">
                    {getStatusBadge(request.status)}
                    <Badge variant="outline" className="text-xs">
                      {request.type === 'dealer' ? 'Dealer' : 'Usuario'}
                    </Badge>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Review Modal */}
      {selectedRequest && (
        <Dialog open={!!selectedRequest} onOpenChange={() => setSelectedRequest(null)}>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Revisar Solicitud KYC</DialogTitle>
            </DialogHeader>
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <p className="text-sm text-gray-500">Nombre</p>
                  <p className="font-medium">{selectedRequest.name}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Email</p>
                  <p className="font-medium">{selectedRequest.email}</p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Tipo</p>
                  <p className="font-medium">
                    {selectedRequest.type === 'dealer' ? 'Dealer' : 'Usuario'}
                  </p>
                </div>
                <div>
                  <p className="text-sm text-gray-500">Estado</p>
                  {getStatusBadge(selectedRequest.status)}
                </div>
              </div>

              <div>
                <p className="mb-2 text-sm text-gray-500">Documentos Subidos</p>
                <div className="space-y-2">
                  {selectedRequest.documents.map((doc, i) => (
                    <div
                      key={i}
                      className="flex items-center justify-between rounded-lg bg-gray-50 p-3"
                    >
                      <div className="flex items-center gap-2">
                        <FileText className="h-4 w-4 text-gray-400" />
                        <span>{doc}</span>
                      </div>
                      <Button variant="ghost" size="sm">
                        <Eye className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                </div>
              </div>

              <div>
                <p className="mb-2 text-sm text-gray-500">Notas</p>
                <p className="text-sm">{selectedRequest.notes}</p>
              </div>

              {selectedRequest.status === 'pending' && (
                <div>
                  <p className="mb-2 text-sm text-gray-500">Agregar Comentario</p>
                  <Textarea placeholder="Comentario interno..." />
                </div>
              )}

              <div className="flex gap-3">
                {selectedRequest.status === 'pending' && (
                  <>
                    <Button className="flex-1 bg-emerald-600 hover:bg-emerald-700">
                      <CheckCircle className="mr-2 h-4 w-4" />
                      Aprobar
                    </Button>
                    <Button variant="destructive" className="flex-1">
                      <XCircle className="mr-2 h-4 w-4" />
                      Rechazar
                    </Button>
                  </>
                )}
                <Button variant="outline" onClick={() => setSelectedRequest(null)}>
                  Cerrar
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </div>
  );
}

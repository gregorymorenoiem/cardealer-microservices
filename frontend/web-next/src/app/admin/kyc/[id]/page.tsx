/**
 * Admin KYC Detail Page
 *
 * Individual KYC verification review
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Textarea } from '@/components/ui/textarea';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  ArrowLeft,
  User,
  Mail,
  Phone,
  Calendar,
  MapPin,
  FileText,
  CheckCircle,
  XCircle,
  Clock,
  Eye,
  Download,
  ZoomIn,
  AlertTriangle,
  Shield,
} from 'lucide-react';
import Link from 'next/link';

const kycData = {
  id: 'kyc-123',
  user: {
    id: 'usr-456',
    name: 'Carlos Alberto Pérez',
    email: 'carlos.perez@email.com',
    phone: '809-555-0123',
    avatar: null,
    createdAt: '2024-01-15',
  },
  status: 'pending',
  submittedAt: '2024-02-15T10:30:00Z',
  documents: [
    {
      id: 'd1',
      type: 'Cédula de Identidad (Frente)',
      fileName: 'cedula_frente.jpg',
      uploadedAt: '2024-02-15T10:25:00Z',
      status: 'pending',
      preview: null,
    },
    {
      id: 'd2',
      type: 'Cédula de Identidad (Reverso)',
      fileName: 'cedula_reverso.jpg',
      uploadedAt: '2024-02-15T10:26:00Z',
      status: 'pending',
      preview: null,
    },
    {
      id: 'd3',
      type: 'Selfie con Documento',
      fileName: 'selfie.jpg',
      uploadedAt: '2024-02-15T10:28:00Z',
      status: 'pending',
      preview: null,
    },
    {
      id: 'd4',
      type: 'Comprobante de Domicilio',
      fileName: 'factura_edeeste.pdf',
      uploadedAt: '2024-02-15T10:30:00Z',
      status: 'pending',
      preview: null,
    },
  ],
  personalInfo: {
    fullName: 'Carlos Alberto Pérez Martínez',
    cedula: '001-1234567-8',
    dateOfBirth: '1985-06-15',
    nationality: 'Dominicana',
    address: 'Calle Principal #123, Sector Naco',
    city: 'Santo Domingo',
    province: 'Distrito Nacional',
  },
  verificationHistory: [
    {
      action: 'Solicitud creada',
      date: '2024-02-15T10:30:00Z',
      by: 'Sistema',
    },
    {
      action: 'Documentos subidos',
      date: '2024-02-15T10:30:00Z',
      by: 'Usuario',
    },
  ],
};

export default function KYCDetailPage() {
  const [selectedDoc, setSelectedDoc] = useState<(typeof kycData.documents)[0] | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('es-DO', {
      day: 'numeric',
      month: 'long',
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
          <Link href="/admin/kyc">
            <Button variant="ghost" size="icon">
              <ArrowLeft className="h-5 w-5" />
            </Button>
          </Link>
          <div>
            <h1 className="text-2xl font-bold">Verificación KYC</h1>
            <p className="text-gray-400">ID: {kycData.id}</p>
          </div>
        </div>
        <Badge className="bg-yellow-600 px-4 py-2">
          <Clock className="mr-2 h-4 w-4" />
          Pendiente de Revisión
        </Badge>
      </div>

      {/* User Info */}
      <Card className="border-slate-700 bg-slate-800">
        <CardContent className="py-6">
          <div className="flex items-start gap-6">
            <Avatar className="h-20 w-20">
              <AvatarImage src={kycData.user.avatar || undefined} />
              <AvatarFallback className="bg-slate-600 text-2xl">
                {kycData.user.name
                  .split(' ')
                  .map(n => n[0])
                  .join('')}
              </AvatarFallback>
            </Avatar>
            <div className="flex-1">
              <h2 className="mb-2 text-xl font-bold text-white">{kycData.user.name}</h2>
              <div className="grid grid-cols-2 gap-4 text-sm md:grid-cols-4">
                <div className="flex items-center gap-2 text-gray-400">
                  <Mail className="h-4 w-4" />
                  <span>{kycData.user.email}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Phone className="h-4 w-4" />
                  <span>{kycData.user.phone}</span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Calendar className="h-4 w-4" />
                  <span>
                    Usuario desde {new Date(kycData.user.createdAt).toLocaleDateString('es-DO')}
                  </span>
                </div>
                <div className="flex items-center gap-2 text-gray-400">
                  <Clock className="h-4 w-4" />
                  <span>Solicitud: {formatDate(kycData.submittedAt)}</span>
                </div>
              </div>
            </div>
            <Link href={`/admin/usuarios/${kycData.user.id}`}>
              <Button variant="outline">
                <User className="mr-2 h-4 w-4" />
                Ver Perfil
              </Button>
            </Link>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Documents */}
        <div className="space-y-6 lg:col-span-2">
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-white">
                <FileText className="h-5 w-5" />
                Documentos Subidos
              </CardTitle>
              <CardDescription>Revisa cada documento cuidadosamente</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                {kycData.documents.map(doc => (
                  <div
                    key={doc.id}
                    className="cursor-pointer rounded-lg border border-slate-600 bg-slate-700/50 p-4 transition-colors hover:border-emerald-500"
                    onClick={() => setSelectedDoc(doc)}
                  >
                    <div className="mb-3 flex aspect-video items-center justify-center rounded-lg bg-slate-600">
                      <FileText className="h-12 w-12 text-gray-400" />
                    </div>
                    <p className="text-sm font-medium text-white">{doc.type}</p>
                    <p className="text-xs text-gray-400">{doc.fileName}</p>
                    <div className="mt-2 flex items-center justify-between">
                      <Badge className="bg-yellow-600 text-xs">
                        <Clock className="mr-1 h-3 w-3" />
                        Pendiente
                      </Badge>
                      <div className="flex gap-1">
                        <Button variant="ghost" size="icon" className="h-6 w-6">
                          <ZoomIn className="h-3 w-3" />
                        </Button>
                        <Button variant="ghost" size="icon" className="h-6 w-6">
                          <Download className="h-3 w-3" />
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Personal Information */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Información Personal</CardTitle>
              <CardDescription>Datos proporcionados por el usuario</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 gap-4">
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="mb-1 text-xs text-gray-400">Nombre Completo</p>
                  <p className="font-medium text-white">{kycData.personalInfo.fullName}</p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="mb-1 text-xs text-gray-400">Cédula</p>
                  <p className="font-medium text-white">{kycData.personalInfo.cedula}</p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="mb-1 text-xs text-gray-400">Fecha de Nacimiento</p>
                  <p className="font-medium text-white">
                    {new Date(kycData.personalInfo.dateOfBirth).toLocaleDateString('es-DO')}
                  </p>
                </div>
                <div className="rounded-lg bg-slate-700/50 p-3">
                  <p className="mb-1 text-xs text-gray-400">Nacionalidad</p>
                  <p className="font-medium text-white">{kycData.personalInfo.nationality}</p>
                </div>
                <div className="col-span-2 rounded-lg bg-slate-700/50 p-3">
                  <p className="mb-1 text-xs text-gray-400">Dirección</p>
                  <p className="font-medium text-white">
                    {kycData.personalInfo.address}, {kycData.personalInfo.city},{' '}
                    {kycData.personalInfo.province}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Actions */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-white">Acciones</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <Button className="w-full bg-emerald-600 hover:bg-emerald-700">
                <CheckCircle className="mr-2 h-4 w-4" />
                Aprobar Verificación
              </Button>
              <div className="space-y-2">
                <Textarea
                  placeholder="Razón del rechazo (requerido para rechazar)..."
                  className="border-slate-600 bg-slate-700"
                  value={rejectReason}
                  onChange={e => setRejectReason(e.target.value)}
                />
                <Button
                  variant="outline"
                  className="w-full border-red-600 text-red-400"
                  disabled={!rejectReason.trim()}
                >
                  <XCircle className="mr-2 h-4 w-4" />
                  Rechazar
                </Button>
              </div>
              <Button variant="outline" className="w-full">
                <AlertTriangle className="mr-2 h-4 w-4" />
                Solicitar Más Documentos
              </Button>
            </CardContent>
          </Card>

          {/* Verification Checklist */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-base text-white">Checklist de Verificación</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {[
                  'Documento de identidad legible',
                  'Foto frontal y reverso presente',
                  'Selfie coincide con documento',
                  'Comprobante de domicilio válido',
                  'Información personal coincide',
                  'Sin señales de manipulación',
                ].map((item, i) => (
                  <label
                    key={i}
                    className="flex cursor-pointer items-center gap-3 rounded-lg bg-slate-700/50 p-2 hover:bg-slate-700"
                  >
                    <input type="checkbox" className="rounded border-slate-500" />
                    <span className="text-sm text-gray-300">{item}</span>
                  </label>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* History */}
          <Card className="border-slate-700 bg-slate-800">
            <CardHeader>
              <CardTitle className="text-base text-white">Historial</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {kycData.verificationHistory.map((event, i) => (
                  <div key={i} className="flex items-start gap-3">
                    <div className="rounded-full bg-slate-700 p-1.5">
                      <Clock className="h-3 w-3 text-gray-400" />
                    </div>
                    <div>
                      <p className="text-sm text-white">{event.action}</p>
                      <p className="text-xs text-gray-400">
                        {formatDate(event.date)} • {event.by}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

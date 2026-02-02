/**
 * Admin Compliance Page
 *
 * AML, DGII and regulatory compliance management
 */

'use client';

import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Shield,
  AlertTriangle,
  CheckCircle,
  FileText,
  Download,
  Calendar,
  Building2,
  Eye,
  Clock,
} from 'lucide-react';

const amlAlerts = [
  {
    id: '1',
    type: 'high_value_transaction',
    description: 'Transacción de alto valor detectada',
    amount: '$85,000',
    dealer: 'AutoMax Premium',
    status: 'pending_review',
    createdAt: '2024-02-15T10:00:00',
  },
  {
    id: '2',
    type: 'suspicious_pattern',
    description: 'Patrón de transacciones inusual',
    details: 'Múltiples compras en 24 horas',
    user: 'user@email.com',
    status: 'under_investigation',
    createdAt: '2024-02-14T15:30:00',
  },
  {
    id: '3',
    type: 'watchlist_match',
    description: 'Coincidencia en lista de vigilancia',
    matchScore: '85%',
    name: 'John Doe',
    status: 'resolved',
    createdAt: '2024-02-13T09:00:00',
  },
];

const dgiiReports = [
  {
    id: '1',
    type: '606',
    period: 'Enero 2024',
    status: 'pending',
    dueDate: '2024-02-15',
    records: 1245,
  },
  {
    id: '2',
    type: '607',
    period: 'Enero 2024',
    status: 'submitted',
    submittedAt: '2024-02-10',
    records: 856,
  },
  {
    id: '3',
    type: '608',
    period: 'Enero 2024',
    status: 'submitted',
    submittedAt: '2024-02-08',
    records: 234,
  },
];

const getStatusBadge = (status: string) => {
  switch (status) {
    case 'pending_review':
      return <Badge className="bg-amber-100 text-amber-700">Pendiente Revisión</Badge>;
    case 'under_investigation':
      return <Badge className="bg-blue-100 text-blue-700">En Investigación</Badge>;
    case 'resolved':
      return <Badge className="bg-emerald-100 text-emerald-700">Resuelto</Badge>;
    case 'pending':
      return <Badge className="bg-amber-100 text-amber-700">Pendiente</Badge>;
    case 'submitted':
      return <Badge className="bg-emerald-100 text-emerald-700">Enviado</Badge>;
    default:
      return <Badge variant="outline">{status}</Badge>;
  }
};

export default function AdminCompliancePage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Compliance</h1>
          <p className="text-gray-400">Cumplimiento regulatorio y AML</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <AlertTriangle className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">3</p>
                <p className="text-sm text-gray-500">Alertas AML</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <FileText className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">1</p>
                <p className="text-sm text-gray-500">Reportes Pendientes</p>
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
                <p className="text-2xl font-bold">98%</p>
                <p className="text-sm text-gray-500">Cumplimiento</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <Shield className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">45</p>
                <p className="text-sm text-gray-500">Verificaciones KYC</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="aml" className="space-y-6">
        <TabsList>
          <TabsTrigger value="aml">AML / Anti-Lavado</TabsTrigger>
          <TabsTrigger value="dgii">Reportes DGII</TabsTrigger>
          <TabsTrigger value="watchlist">Lista de Vigilancia</TabsTrigger>
        </TabsList>

        {/* AML Tab */}
        <TabsContent value="aml">
          <Card>
            <CardHeader>
              <CardTitle>Alertas AML</CardTitle>
              <CardDescription>Monitoreo de transacciones sospechosas</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {amlAlerts.map(alert => (
                  <div
                    key={alert.id}
                    className="flex items-start justify-between rounded-lg bg-gray-50 p-4"
                  >
                    <div className="flex items-start gap-4">
                      <div
                        className={`rounded-lg p-2 ${
                          alert.status === 'pending_review'
                            ? 'bg-amber-100'
                            : alert.status === 'under_investigation'
                              ? 'bg-blue-100'
                              : 'bg-emerald-100'
                        }`}
                      >
                        <AlertTriangle
                          className={`h-5 w-5 ${
                            alert.status === 'pending_review'
                              ? 'text-amber-600'
                              : alert.status === 'under_investigation'
                                ? 'text-blue-600'
                                : 'text-emerald-600'
                          }`}
                        />
                      </div>
                      <div>
                        <div className="mb-1 flex items-center gap-2">
                          <p className="font-medium">{alert.description}</p>
                          {getStatusBadge(alert.status)}
                        </div>
                        <p className="text-sm text-gray-500">
                          {alert.amount && `Monto: ${alert.amount}`}
                          {alert.dealer && ` • Dealer: ${alert.dealer}`}
                          {alert.user && ` • Usuario: ${alert.user}`}
                          {alert.matchScore && ` • Score: ${alert.matchScore}`}
                        </p>
                        <p className="mt-1 text-xs text-gray-400">
                          {new Date(alert.createdAt).toLocaleString('es-DO')}
                        </p>
                      </div>
                    </div>
                    <Button variant="outline" size="sm">
                      <Eye className="mr-1 h-4 w-4" />
                      Revisar
                    </Button>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* DGII Tab */}
        <TabsContent value="dgii">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Reportes DGII</CardTitle>
                  <CardDescription>
                    Formatos 606, 607, 608 y otros reportes fiscales
                  </CardDescription>
                </div>
                <Button className="bg-emerald-600 hover:bg-emerald-700">
                  <FileText className="mr-2 h-4 w-4" />
                  Generar Reporte
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {dgiiReports.map(report => (
                  <div
                    key={report.id}
                    className="flex items-center justify-between rounded-lg bg-gray-50 p-4"
                  >
                    <div className="flex items-center gap-4">
                      <div className="rounded-lg bg-blue-100 p-2">
                        <FileText className="h-5 w-5 text-blue-600" />
                      </div>
                      <div>
                        <div className="mb-1 flex items-center gap-2">
                          <p className="font-medium">Formato {report.type}</p>
                          {getStatusBadge(report.status)}
                        </div>
                        <p className="text-sm text-gray-500">
                          Período: {report.period} • {report.records} registros
                        </p>
                        {report.dueDate && (
                          <p className="mt-1 text-xs text-amber-600">
                            Vence: {new Date(report.dueDate).toLocaleDateString('es-DO')}
                          </p>
                        )}
                        {report.submittedAt && (
                          <p className="mt-1 text-xs text-emerald-600">
                            Enviado: {new Date(report.submittedAt).toLocaleDateString('es-DO')}
                          </p>
                        )}
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button variant="outline" size="sm">
                        <Download className="h-4 w-4" />
                      </Button>
                      {report.status === 'pending' && (
                        <Button size="sm" className="bg-emerald-600 hover:bg-emerald-700">
                          Enviar
                        </Button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        {/* Watchlist Tab */}
        <TabsContent value="watchlist">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Lista de Vigilancia</CardTitle>
                  <CardDescription>Usuarios y entidades bajo monitoreo</CardDescription>
                </div>
                <Button variant="outline">
                  <Download className="mr-2 h-4 w-4" />
                  Actualizar Lista
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="py-8 text-center text-gray-400">
                <Shield className="mx-auto mb-4 h-12 w-12 opacity-50" />
                <p>No hay entradas en la lista de vigilancia activa</p>
                <p className="mt-2 text-sm">
                  Última actualización: {new Date().toLocaleDateString('es-DO')}
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}

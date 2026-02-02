/**
 * Admin Billing Page
 *
 * Platform billing management, revenue tracking
 */

'use client';

import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  DollarSign,
  TrendingUp,
  CreditCard,
  Users,
  FileText,
  Download,
  Calendar,
  ArrowUpRight,
  ArrowDownRight,
  RefreshCw,
  AlertCircle,
} from 'lucide-react';

const formatPrice = (price: number) => {
  return new Intl.NumberFormat('es-DO', {
    style: 'currency',
    currency: 'DOP',
    maximumFractionDigits: 0,
  }).format(price);
};

const mockRevenueData = {
  mrr: 2450000,
  mrrChange: 12.5,
  arr: 29400000,
  activeSubscriptions: 156,
  churnRate: 2.1,
  avgRevenue: 15705,
};

const mockTransactions = [
  {
    id: '1',
    dealer: 'AutoMax Premium',
    plan: 'Enterprise',
    amount: 49999,
    date: '2024-02-15',
    status: 'completed',
  },
  {
    id: '2',
    dealer: 'CarWorld RD',
    plan: 'Pro',
    amount: 24999,
    date: '2024-02-15',
    status: 'completed',
  },
  {
    id: '3',
    dealer: 'Motors Plus',
    plan: 'Pro',
    amount: 24999,
    date: '2024-02-14',
    status: 'completed',
  },
  {
    id: '4',
    dealer: 'Elite Cars',
    plan: 'Starter',
    amount: 4999,
    date: '2024-02-14',
    status: 'failed',
  },
  {
    id: '5',
    dealer: 'Premium Auto',
    plan: 'Pro',
    amount: 24999,
    date: '2024-02-14',
    status: 'completed',
  },
];

const mockPendingPayments = [
  { id: '1', dealer: 'Santo Domingo Motors', amount: 24999, dueDate: '2024-02-20', daysPast: 0 },
  { id: '2', dealer: 'Auto Express', amount: 4999, dueDate: '2024-02-18', daysPast: 2 },
];

export default function AdminBillingPage() {
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-2xl font-bold">Facturación</h1>
          <p className="text-gray-400">Gestión de ingresos y pagos</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <Calendar className="mr-2 h-4 w-4" />
            Febrero 2024
          </Button>
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Exportar
          </Button>
        </div>
      </div>

      {/* Revenue Cards */}
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        <Card className="bg-gradient-to-br from-emerald-500 to-emerald-600 text-white">
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <DollarSign className="h-5 w-5 opacity-80" />
              <span className="flex items-center text-sm">
                <ArrowUpRight className="h-4 w-4" />
                {mockRevenueData.mrrChange}%
              </span>
            </div>
            <p className="text-2xl font-bold">{formatPrice(mockRevenueData.mrr)}</p>
            <p className="text-sm opacity-80">MRR (Ingreso Mensual)</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <TrendingUp className="h-5 w-5 text-blue-500" />
            </div>
            <p className="text-2xl font-bold">{formatPrice(mockRevenueData.arr)}</p>
            <p className="text-sm text-gray-500">ARR (Ingreso Anual)</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <Users className="h-5 w-5 text-purple-500" />
            </div>
            <p className="text-2xl font-bold">{mockRevenueData.activeSubscriptions}</p>
            <p className="text-sm text-gray-500">Suscripciones Activas</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="mb-2 flex items-center justify-between">
              <RefreshCw className="h-5 w-5 text-red-500" />
              <span className="flex items-center text-sm text-red-500">
                <ArrowDownRight className="h-4 w-4" />
                -0.3%
              </span>
            </div>
            <p className="text-2xl font-bold">{mockRevenueData.churnRate}%</p>
            <p className="text-sm text-gray-500">Tasa de Cancelación</p>
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Recent Transactions */}
        <div className="lg:col-span-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between">
              <CardTitle>Transacciones Recientes</CardTitle>
              <Button variant="ghost" size="sm">
                Ver todas
              </Button>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {mockTransactions.map(tx => (
                  <div
                    key={tx.id}
                    className="flex items-center justify-between rounded-lg border p-3"
                  >
                    <div className="flex items-center gap-3">
                      <div
                        className={`rounded-lg p-2 ${
                          tx.status === 'completed' ? 'bg-emerald-100' : 'bg-red-100'
                        }`}
                      >
                        <CreditCard
                          className={`h-4 w-4 ${
                            tx.status === 'completed' ? 'text-emerald-600' : 'text-red-600'
                          }`}
                        />
                      </div>
                      <div>
                        <p className="font-medium">{tx.dealer}</p>
                        <p className="text-sm text-gray-500">
                          Plan {tx.plan} • {new Date(tx.date).toLocaleDateString('es-DO')}
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <p className="font-bold">{formatPrice(tx.amount)}</p>
                      <Badge
                        className={
                          tx.status === 'completed'
                            ? 'bg-emerald-100 text-emerald-700'
                            : 'bg-red-100 text-red-700'
                        }
                      >
                        {tx.status === 'completed' ? 'Completado' : 'Fallido'}
                      </Badge>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Pending Payments */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <AlertCircle className="h-5 w-5 text-amber-500" />
                Pagos Pendientes
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {mockPendingPayments.map(payment => (
                <div
                  key={payment.id}
                  className="rounded-lg border border-amber-200 bg-amber-50 p-3"
                >
                  <div className="mb-1 flex justify-between">
                    <span className="font-medium">{payment.dealer}</span>
                    <span className="font-bold">{formatPrice(payment.amount)}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-500">
                      Vence: {new Date(payment.dueDate).toLocaleDateString('es-DO')}
                    </span>
                    {payment.daysPast > 0 && (
                      <span className="text-red-600">{payment.daysPast} días vencido</span>
                    )}
                  </div>
                </div>
              ))}
              <Button variant="outline" className="w-full">
                Enviar Recordatorios
              </Button>
            </CardContent>
          </Card>

          {/* Revenue by Plan */}
          <Card>
            <CardHeader>
              <CardTitle>Ingresos por Plan</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="h-3 w-3 rounded-full bg-purple-500" />
                  <span>Enterprise</span>
                </div>
                <span className="font-medium">{formatPrice(1200000)}</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="h-3 w-3 rounded-full bg-blue-500" />
                  <span>Pro</span>
                </div>
                <span className="font-medium">{formatPrice(950000)}</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="h-3 w-3 rounded-full bg-gray-400" />
                  <span>Starter</span>
                </div>
                <span className="font-medium">{formatPrice(300000)}</span>
              </div>
            </CardContent>
          </Card>

          {/* Quick Stats */}
          <Card>
            <CardHeader>
              <CardTitle>Promedio por Cliente</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">{formatPrice(mockRevenueData.avgRevenue)}</p>
              <p className="text-sm text-gray-500">Ingreso promedio por dealer</p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

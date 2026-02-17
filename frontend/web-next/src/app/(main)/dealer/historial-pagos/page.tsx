/**
 * Dealer Payment History Page
 *
 * Shows billing and payment transaction history for dealers
 */

'use client';

import * as React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  DollarSign,
  Download,
  Search,
  AlertCircle,
  Receipt,
  CreditCard,
  CheckCircle2,
  XCircle,
  Clock,
  FileText,
} from 'lucide-react';
import { useCurrentDealer } from '@/hooks/use-dealers';
import {
  getInvoices,
  formatCurrency,
  getInvoiceStatusLabel,
  type Invoice,
} from '@/services/dealer-billing';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';

const statusIcons: Record<string, React.ElementType> = {
  Paid: CheckCircle2,
  Pending: Clock,
  Overdue: XCircle,
  Cancelled: Receipt,
};

export default function HistorialPagosPage() {
  const { data: dealer } = useCurrentDealer();
  const [payments, setPayments] = React.useState<Invoice[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [search, setSearch] = React.useState('');
  const [statusFilter, setStatusFilter] = React.useState<string>('all');

  React.useEffect(() => {
    if (!dealer?.id) return;

    async function fetchPayments() {
      try {
        setLoading(true);
        const data = await getInvoices(dealer!.id);
        setPayments(data ?? []);
      } catch (err) {
        console.error('Error fetching payments:', err);
        setError('No se pudo cargar el historial de pagos.');
      } finally {
        setLoading(false);
      }
    }
    fetchPayments();
  }, [dealer]);

  const filteredPayments = payments.filter(p => {
    const matchesSearch =
      !search ||
      p.description.toLowerCase().includes(search.toLowerCase()) ||
      p.invoiceNumber?.toLowerCase().includes(search.toLowerCase());
    const matchesStatus = statusFilter === 'all' || p.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  const totals = React.useMemo(() => {
    const paid = payments.filter(p => p.status === 'Paid').reduce((sum, p) => sum + p.amount, 0);
    const pending = payments
      .filter(p => p.status === 'Pending')
      .reduce((sum, p) => sum + p.amount, 0);
    return { paid, pending, count: payments.length };
  }, [payments]);

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Historial de Pagos</h1>
        <div className="grid gap-4 sm:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-16 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
        <Card>
          <CardContent className="p-6">
            <Skeleton className="h-64 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <div className="text-center">
          <AlertCircle className="mx-auto h-12 w-12 text-red-500" />
          <p className="text-muted-foreground mt-4">{error}</p>
          <Button
            variant="outline"
            className="mt-4"
            onClick={() => {
              setError(null);
              setLoading(true);
            }}
          >
            Reintentar
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-foreground text-2xl font-bold">Historial de Pagos</h1>
          <p className="text-muted-foreground">Revisa todas tus transacciones y facturas</p>
        </div>
        <Button variant="outline" size="sm">
          <Download className="mr-2 h-4 w-4" />
          Exportar
        </Button>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-lg bg-green-50 p-3">
              <DollarSign className="h-6 w-6 text-green-600" />
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Total Pagado</p>
              <p className="text-2xl font-bold">{formatCurrency(totals.paid)}</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-lg bg-amber-50 p-3">
              <Clock className="h-6 w-6 text-amber-600" />
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Pendiente</p>
              <p className="text-2xl font-bold">{formatCurrency(totals.pending)}</p>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex items-center gap-4 p-6">
            <div className="rounded-lg bg-blue-50 p-3">
              <Receipt className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <p className="text-muted-foreground text-sm">Transacciones</p>
              <p className="text-2xl font-bold">{totals.count}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="flex flex-col gap-4 p-4 sm:flex-row">
          <div className="relative flex-1">
            <Search className="text-muted-foreground absolute top-1/2 left-3 h-4 w-4 -translate-y-1/2" />
            <Input
              placeholder="Buscar por descripción o referencia..."
              className="pl-9"
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
          </div>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger className="w-full sm:w-[180px]">
              <SelectValue placeholder="Estado" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Todos</SelectItem>
              <SelectItem value="Paid">Pagado</SelectItem>
              <SelectItem value="Pending">Pendiente</SelectItem>
              <SelectItem value="Overdue">Vencido</SelectItem>
              <SelectItem value="Cancelled">Cancelado</SelectItem>
            </SelectContent>
          </Select>
        </CardContent>
      </Card>

      {/* Payments List */}
      {filteredPayments.length === 0 ? (
        <div className="flex min-h-[200px] items-center justify-center">
          <div className="text-center">
            <Receipt className="text-muted-foreground mx-auto h-12 w-12" />
            <p className="text-muted-foreground mt-4">No se encontraron transacciones</p>
          </div>
        </div>
      ) : (
        <Card>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="bg-muted/50 border-b">
                    <th className="px-4 py-3 text-left font-medium">Fecha</th>
                    <th className="px-4 py-3 text-left font-medium">Descripción</th>
                    <th className="px-4 py-3 text-left font-medium">Método</th>
                    <th className="px-4 py-3 text-right font-medium">Monto</th>
                    <th className="px-4 py-3 text-center font-medium">Estado</th>
                    <th className="px-4 py-3 text-center font-medium">Factura</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredPayments.map(payment => {
                    const StatusIcon = statusIcons[payment.status] ?? Clock;
                    return (
                      <tr key={payment.id} className="hover:bg-muted/30 border-b last:border-0">
                        <td className="px-4 py-3 whitespace-nowrap">
                          {format(new Date(payment.createdAt), 'dd MMM yyyy', { locale: es })}
                        </td>
                        <td className="px-4 py-3">
                          <p className="font-medium">{payment.description}</p>
                          {payment.invoiceNumber && (
                            <p className="text-muted-foreground text-xs">
                              Ref: {payment.invoiceNumber}
                            </p>
                          )}
                        </td>
                        <td className="px-4 py-3">
                          <span className="flex items-center gap-1.5">
                            <CreditCard className="text-muted-foreground h-3.5 w-3.5" />
                            {payment.currency}
                          </span>
                        </td>
                        <td className="px-4 py-3 text-right font-medium">
                          {formatCurrency(payment.amount)}
                        </td>
                        <td className="px-4 py-3 text-center">
                          <Badge
                            variant={
                              payment.status === 'Paid'
                                ? 'default'
                                : payment.status === 'Pending'
                                  ? 'secondary'
                                  : 'danger'
                            }
                          >
                            <StatusIcon className="mr-1 h-3 w-3" />
                            {getInvoiceStatusLabel(payment.status)}
                          </Badge>
                        </td>
                        <td className="px-4 py-3 text-center">
                          {payment.pdfUrl ? (
                            <Button variant="ghost" size="sm" asChild>
                              <a href={payment.pdfUrl} target="_blank" rel="noopener noreferrer">
                                <FileText className="h-4 w-4" />
                              </a>
                            </Button>
                          ) : (
                            <span className="text-muted-foreground">—</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

/**
 * Seller Inquiries (Consultas) Page
 *
 * Shows contact inquiries received for the seller's vehicles
 */

'use client';

import * as React from 'react';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  MessageSquare,
  Clock,
  CheckCircle2,
  AlertCircle,
  Car,
  User,
  Mail,
  Phone,
} from 'lucide-react';
import { getReceivedInquiries, type ReceivedInquiry } from '@/services/contact';
import { formatDistanceToNow } from 'date-fns';
import { es } from 'date-fns/locale';

const statusConfig: Record<
  string,
  { label: string; variant: 'default' | 'secondary' | 'outline'; icon: React.ElementType }
> = {
  pending: { label: 'Pendiente', variant: 'default', icon: Clock },
  read: { label: 'Leída', variant: 'secondary', icon: CheckCircle2 },
  replied: { label: 'Respondida', variant: 'outline', icon: MessageSquare },
  archived: { label: 'Archivada', variant: 'outline', icon: CheckCircle2 },
};

export default function ConsultasPage() {
  const [inquiries, setInquiries] = React.useState<ReceivedInquiry[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [filter, setFilter] = React.useState<string>('all');

  React.useEffect(() => {
    async function fetchInquiries() {
      try {
        setLoading(true);
        const data = await getReceivedInquiries();
        setInquiries(data ?? []);
      } catch (err) {
        console.error('Error fetching inquiries:', err);
        setError('No se pudieron cargar las consultas.');
      } finally {
        setLoading(false);
      }
    }
    fetchInquiries();
  }, []);

  const filteredInquiries =
    filter === 'all' ? inquiries : inquiries.filter(i => i.status === filter);

  if (loading) {
    return (
      <div className="space-y-6">
        <h1 className="text-2xl font-bold">Consultas Recibidas</h1>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Card key={i}>
              <CardContent className="p-6">
                <Skeleton className="h-24 w-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex min-h-[300px] items-center justify-center">
        <div className="text-center">
          <AlertCircle className="mx-auto h-12 w-12 text-red-500" />
          <p className="text-muted-foreground mt-4">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-foreground text-2xl font-bold">Consultas Recibidas</h1>
        <p className="text-muted-foreground">
          Mensajes de compradores interesados en tus vehículos
        </p>
      </div>

      {/* Filter Tabs */}
      <div className="flex flex-wrap gap-2">
        {[
          { key: 'all', label: 'Todas' },
          { key: 'pending', label: 'Pendientes' },
          { key: 'read', label: 'Leídas' },
          { key: 'replied', label: 'Respondidas' },
        ].map(tab => (
          <Button
            key={tab.key}
            variant={filter === tab.key ? 'default' : 'outline'}
            size="sm"
            onClick={() => setFilter(tab.key)}
          >
            {tab.label}
          </Button>
        ))}
      </div>

      {/* Inquiries List */}
      {filteredInquiries.length === 0 ? (
        <div className="flex min-h-[200px] items-center justify-center">
          <div className="text-center">
            <MessageSquare className="text-muted-foreground mx-auto h-12 w-12" />
            <p className="text-muted-foreground mt-4">
              {filter === 'all'
                ? 'No has recibido consultas todavía'
                : 'No hay consultas con este filtro'}
            </p>
          </div>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredInquiries.map(inquiry => {
            const statusInfo = statusConfig[inquiry.status] ?? statusConfig.pending;
            const StatusIcon = statusInfo.icon;
            return (
              <Card key={inquiry.id} className="transition-shadow hover:shadow-md">
                <CardContent className="p-6">
                  <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
                    <div className="flex-1 space-y-2">
                      {/* Vehicle */}
                      <div className="flex items-center gap-2">
                        <Car className="text-muted-foreground h-4 w-4" />
                        <span className="text-sm font-medium">
                          {inquiry.vehicleTitle ?? inquiry.subject}
                        </span>
                      </div>

                      {/* Sender Info */}
                      <div className="text-muted-foreground flex flex-wrap items-center gap-4 text-sm">
                        <span className="flex items-center gap-1">
                          <User className="h-3.5 w-3.5" />
                          {inquiry.buyerName}
                        </span>
                        <span className="flex items-center gap-1">
                          <Mail className="h-3.5 w-3.5" />
                          {inquiry.buyerEmail}
                        </span>
                        {inquiry.buyerPhone && (
                          <span className="flex items-center gap-1">
                            <Phone className="h-3.5 w-3.5" />
                            {inquiry.buyerPhone}
                          </span>
                        )}
                      </div>

                      {/* Message */}
                      <p className="text-foreground line-clamp-2 text-sm">{inquiry.lastMessage}</p>

                      {/* Time */}
                      <p className="text-muted-foreground text-xs">
                        {formatDistanceToNow(new Date(inquiry.createdAt), {
                          addSuffix: true,
                          locale: es,
                        })}
                      </p>
                    </div>

                    <div className="flex items-center gap-2">
                      <Badge variant={statusInfo.variant}>
                        <StatusIcon className="mr-1 h-3 w-3" />
                        {statusInfo.label}
                      </Badge>
                    </div>
                  </div>
                </CardContent>
              </Card>
            );
          })}
        </div>
      )}
    </div>
  );
}

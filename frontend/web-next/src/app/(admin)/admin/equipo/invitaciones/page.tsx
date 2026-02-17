/**
 * Staff Invitations Management Page
 *
 * View and manage pending staff invitations.
 */

'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import {
  Mail,
  Search,
  UserPlus,
  ChevronLeft,
  ChevronRight,
  RefreshCw,
  Loader2,
  MoreHorizontal,
  Clock,
  CheckCircle,
  XCircle,
  Send,
  Trash2,
  ArrowLeft,
  Calendar,
} from 'lucide-react';
import { toast } from 'sonner';
import { formatDistanceToNow, format, isPast } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  useInvitations,
  useResendInvitation,
  useRevokeInvitation,
  type InvitationFilters,
  type StaffInvitation,
  type InvitationStatus,
  type StaffRole,
} from '@/hooks/use-staff';

// =============================================================================
// STATUS CONFIG
// =============================================================================

const STATUS_CONFIG: Record<
  InvitationStatus,
  { label: string; className: string; icon: typeof Clock }
> = {
  Pending: { label: 'Pendiente', className: 'bg-amber-100 text-amber-700', icon: Clock },
  Accepted: { label: 'Aceptada', className: 'bg-primary/10 text-primary', icon: CheckCircle },
  Expired: { label: 'Expirada', className: 'bg-slate-100 text-slate-700', icon: XCircle },
  Revoked: { label: 'Revocada', className: 'bg-red-100 text-red-700', icon: XCircle },
};

const ROLE_CONFIG: Record<StaffRole, { label: string; className: string }> = {
  SuperAdmin: { label: 'Super Admin', className: 'bg-red-100 text-red-700 border-red-200' },
  Admin: { label: 'Admin', className: 'bg-purple-100 text-purple-700 border-purple-200' },
  Moderator: { label: 'Moderador', className: 'bg-blue-100 text-blue-700 border-blue-200' },
  Support: { label: 'Soporte', className: 'bg-cyan-100 text-cyan-700 border-cyan-200' },
  Analyst: { label: 'Analista', className: 'bg-amber-100 text-amber-700 border-amber-200' },
  Compliance: {
    label: 'Compliance',
    className: 'bg-primary/10 text-primary border-primary',
  },
};

function StatusBadge({ status }: { status: InvitationStatus }) {
  const config = STATUS_CONFIG[status] || STATUS_CONFIG.Pending;
  const Icon = config.icon;
  return (
    <Badge className={`${config.className} gap-1`}>
      <Icon className="h-3 w-3" />
      {config.label}
    </Badge>
  );
}

function RoleBadge({ role }: { role: StaffRole }) {
  const config = ROLE_CONFIG[role] || ROLE_CONFIG.Support;
  return (
    <Badge variant="outline" className={config.className}>
      {config.label}
    </Badge>
  );
}

// =============================================================================
// SKELETON
// =============================================================================

function InvitationsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-40" />
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        {[1, 2, 3, 4].map(i => (
          <Skeleton key={i} className="h-20" />
        ))}
      </div>
      <Card>
        <CardContent className="p-0">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="border-border flex items-center gap-4 border-b p-4">
              <Skeleton className="h-10 w-10 rounded-full" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-40" />
                <Skeleton className="h-3 w-60" />
              </div>
              <Skeleton className="h-8 w-24" />
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
}

// =============================================================================
// INVITATION ROW
// =============================================================================

interface InvitationRowProps {
  invitation: StaffInvitation;
  onResend: (id: string) => void;
  onCancel: (id: string) => void;
  isResending: boolean;
  isCancelling: boolean;
}

function InvitationRow({
  invitation,
  onResend,
  onCancel,
  isResending,
  isCancelling,
}: InvitationRowProps) {
  const isExpired = isPast(new Date(invitation.expiresAt));
  const canResend = invitation.status === 'Pending' && !isExpired;
  const canCancel = invitation.status === 'Pending';

  return (
    <div className="border-border flex items-center gap-4 border-b p-4 transition-colors hover:bg-slate-50 dark:hover:bg-slate-800/50">
      {/* Icon */}
      <div className="bg-primary/10 flex h-10 w-10 items-center justify-center rounded-full">
        <Mail className="text-primary h-5 w-5" />
      </div>

      {/* Info */}
      <div className="min-w-0 flex-1">
        <div className="flex flex-wrap items-center gap-2">
          <span className="truncate font-medium">{invitation.email}</span>
          <RoleBadge role={invitation.assignedRole} />
          <StatusBadge status={invitation.status} />
        </div>
        <div className="text-muted-foreground mt-1 flex items-center gap-4 text-sm">
          <span className="flex items-center gap-1">
            <Calendar className="h-3.5 w-3.5" />
            {invitation.status === 'Accepted' && invitation.acceptedAt
              ? `Aceptada ${formatDistanceToNow(new Date(invitation.acceptedAt), { addSuffix: true, locale: es })}`
              : invitation.status === 'Pending'
                ? invitation.isExpired
                  ? 'Expirada'
                  : `Expira ${formatDistanceToNow(new Date(invitation.expiresAt), { addSuffix: true, locale: es })}`
                : `Creada ${format(new Date(invitation.createdAt), 'dd/MM/yyyy')}`}
          </span>
          {invitation.departmentName && (
            <span>
              {invitation.departmentName}
              {invitation.positionTitle && ` · ${invitation.positionTitle}`}
            </span>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm" disabled={isResending || isCancelling}>
              {isResending || isCancelling ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <MoreHorizontal className="h-4 w-4" />
              )}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            {canResend && (
              <DropdownMenuItem onClick={() => onResend(invitation.id)}>
                <Send className="mr-2 h-4 w-4 text-blue-600" />
                Reenviar invitación
              </DropdownMenuItem>
            )}
            {canCancel && (
              <DropdownMenuItem onClick={() => onCancel(invitation.id)} className="text-red-600">
                <Trash2 className="mr-2 h-4 w-4" />
                Cancelar invitación
              </DropdownMenuItem>
            )}
            {!canResend && !canCancel && (
              <DropdownMenuItem disabled>Sin acciones disponibles</DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function InvitationsPage() {
  const [filters, setFilters] = useState<InvitationFilters>({
    page: 1,
    pageSize: 20,
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false);
  const [pendingCancelId, setPendingCancelId] = useState<string | null>(null);

  const { data: invitationsData, isLoading, refetch } = useInvitations(filters);
  const resendMutation = useResendInvitation();
  const cancelMutation = useRevokeInvitation();

  const handleSearch = () => {
    setFilters(prev => ({ ...prev, search: searchTerm, page: 1 }));
  };

  const handleFilterChange = (key: string, value: string) => {
    setFilters(prev => ({
      ...prev,
      [key]: value === 'all' ? undefined : value,
      page: 1,
    }));
  };

  const handleResend = async (id: string) => {
    try {
      await resendMutation.mutateAsync(id);
      toast.success('Invitación reenviada');
    } catch {
      toast.error('Error al reenviar invitación');
    }
  };

  const handleCancelRequest = (id: string) => {
    setPendingCancelId(id);
    setCancelDialogOpen(true);
  };

  const handleCancelConfirm = async () => {
    if (!pendingCancelId) return;

    try {
      await cancelMutation.mutateAsync(pendingCancelId);
      toast.success('Invitación cancelada');
    } catch {
      toast.error('Error al cancelar invitación');
    } finally {
      setCancelDialogOpen(false);
      setPendingCancelId(null);
    }
  };

  if (isLoading) {
    return <InvitationsSkeleton />;
  }

  const invitations = invitationsData?.items || [];
  const totalPages = invitationsData?.totalPages || 1;

  // Count by status
  const pendingCount = invitations.filter(i => i.status === 'Pending').length;
  const acceptedCount = invitations.filter(i => i.status === 'Accepted').length;
  const expiredCount = invitations.filter(i => i.status === 'Expired').length;
  const cancelledCount = invitations.filter(i => i.status === 'Revoked').length;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/admin/equipo">
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <div className="flex-1">
          <h1 className="text-foreground text-3xl font-bold">Invitaciones</h1>
          <p className="text-muted-foreground">Gestiona las invitaciones de personal</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Actualizar
          </Button>
          <Button asChild>
            <Link href="/admin/equipo/invitar">
              <UserPlus className="mr-2 h-4 w-4" />
              Nueva Invitación
            </Link>
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card
          className="cursor-pointer transition-colors hover:border-amber-300"
          onClick={() => handleFilterChange('status', 'Pending')}
        >
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Clock className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{pendingCount}</p>
                <p className="text-muted-foreground text-sm">Pendientes</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card
          className="cursor-pointer transition-colors hover:border-primary"
          onClick={() => handleFilterChange('status', 'Accepted')}
        >
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-primary/10 p-2">
                <CheckCircle className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{acceptedCount}</p>
                <p className="text-muted-foreground text-sm">Aceptadas</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card
          className="cursor-pointer transition-colors hover:border-slate-300"
          onClick={() => handleFilterChange('status', 'Expired')}
        >
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-slate-100 p-2">
                <XCircle className="h-5 w-5 text-slate-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{expiredCount}</p>
                <p className="text-muted-foreground text-sm">Expiradas</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card
          className="cursor-pointer transition-colors hover:border-red-300"
          onClick={() => handleFilterChange('status', 'Revoked')}
        >
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-red-100 p-2">
                <Trash2 className="h-5 w-5 text-red-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{cancelledCount}</p>
                <p className="text-muted-foreground text-sm">Revocadas</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="p-4">
          <div className="flex flex-col gap-4 md:flex-row md:items-end">
            <div className="flex-1">
              <label className="mb-1.5 block text-sm font-medium">Buscar</label>
              <div className="flex gap-2">
                <Input
                  placeholder="Buscar por nombre o email..."
                  value={searchTerm}
                  onChange={e => setSearchTerm(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && handleSearch()}
                />
                <Button onClick={handleSearch}>
                  <Search className="h-4 w-4" />
                </Button>
              </div>
            </div>
            <div className="w-full md:w-40">
              <label className="mb-1.5 block text-sm font-medium">Estado</label>
              <Select
                value={filters.status || 'all'}
                onValueChange={v => handleFilterChange('status', v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="Pending">Pendiente</SelectItem>
                  <SelectItem value="Accepted">Aceptada</SelectItem>
                  <SelectItem value="Expired">Expirada</SelectItem>
                  <SelectItem value="Revoked">Revocada</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div className="w-full md:w-40">
              <label className="mb-1.5 block text-sm font-medium">Rol</label>
              <Select
                value={filters.role || 'all'}
                onValueChange={v => handleFilterChange('role', v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  <SelectItem value="SuperAdmin">Super Admin</SelectItem>
                  <SelectItem value="Admin">Admin</SelectItem>
                  <SelectItem value="Moderator">Moderador</SelectItem>
                  <SelectItem value="Support">Soporte</SelectItem>
                  <SelectItem value="Analyst">Analista</SelectItem>
                  <SelectItem value="Compliance">Compliance</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Invitations List */}
      <Card>
        <CardHeader className="border-b px-4 py-3">
          <CardTitle className="text-lg font-medium">
            Invitaciones ({invitationsData?.totalCount || 0})
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {invitations.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Mail className="text-muted-foreground/50 mb-4 h-12 w-12" />
              <h3 className="text-lg font-medium">No hay invitaciones</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                {filters.search || filters.status || filters.role
                  ? 'No se encontraron resultados con los filtros aplicados'
                  : 'Envía la primera invitación'}
              </p>
              {!filters.search && !filters.status && !filters.role && (
                <Button className="mt-4" asChild>
                  <Link href="/admin/equipo/invitar">
                    <UserPlus className="mr-2 h-4 w-4" />
                    Nueva invitación
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            invitations.map(invitation => (
              <InvitationRow
                key={invitation.id}
                invitation={invitation}
                onResend={handleResend}
                onCancel={handleCancelRequest}
                isResending={resendMutation.isPending}
                isCancelling={cancelMutation.isPending}
              />
            ))
          )}
        </CardContent>
      </Card>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-muted-foreground text-sm">
            Página {filters.page} de {totalPages}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) - 1 }))}
              disabled={filters.page === 1}
            >
              <ChevronLeft className="h-4 w-4" />
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setFilters(prev => ({ ...prev, page: (prev.page || 1) + 1 }))}
              disabled={filters.page === totalPages}
            >
              Siguiente
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      {/* Cancel Dialog */}
      <AlertDialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cancelar invitación</AlertDialogTitle>
            <AlertDialogDescription>
              ¿Estás seguro de cancelar esta invitación? El enlace de invitación dejará de
              funcionar.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>No, mantener</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleCancelConfirm}
              className="bg-red-600 hover:bg-red-700"
            >
              {cancelMutation.isPending ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : null}
              Sí, cancelar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

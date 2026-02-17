/**
 * Staff Management Page
 *
 * Main page for managing internal staff members.
 * Only accessible by SuperAdmin and Admin roles.
 */

'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
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
  Users,
  Search,
  UserPlus,
  ChevronLeft,
  ChevronRight,
  Shield,
  ShieldCheck,
  ShieldAlert,
  Eye,
  RefreshCw,
  Loader2,
  MoreHorizontal,
  Mail,
  Phone,
  Building2,
  Briefcase,
  Ban,
  UserX,
  UserCheck,
} from 'lucide-react';
import { toast } from 'sonner';
import {
  useStaffMembers,
  useStaffStats,
  useChangeStaffStatus,
  useActiveDepartments,
  usePendingInvitationsCount,
  type StaffFilters,
  type StaffMember,
  type StaffRole,
  type StaffStatus,
} from '@/hooks/use-staff';

// =============================================================================
// ROLE BADGES
// =============================================================================

const ROLE_CONFIG: Record<StaffRole, { label: string; className: string; icon: typeof Shield }> = {
  SuperAdmin: {
    label: 'Super Admin',
    className: 'bg-red-100 text-red-700 border-red-200',
    icon: ShieldAlert,
  },
  Admin: {
    label: 'Admin',
    className: 'bg-purple-100 text-purple-700 border-purple-200',
    icon: ShieldCheck,
  },
  Moderator: {
    label: 'Moderador',
    className: 'bg-blue-100 text-blue-700 border-blue-200',
    icon: Shield,
  },
  Support: {
    label: 'Soporte',
    className: 'bg-cyan-100 text-cyan-700 border-cyan-200',
    icon: Shield,
  },
  Analyst: {
    label: 'Analista',
    className: 'bg-amber-100 text-amber-700 border-amber-200',
    icon: Shield,
  },
  Compliance: {
    label: 'Compliance',
    className: 'bg-primary/10 text-primary border-primary',
    icon: Shield,
  },
};

const STATUS_CONFIG: Record<StaffStatus, { label: string; className: string }> = {
  Pending: { label: 'Pendiente', className: 'bg-blue-100 text-blue-700' },
  Active: { label: 'Activo', className: 'bg-primary/10 text-primary' },
  Suspended: { label: 'Suspendido', className: 'bg-amber-100 text-amber-700' },
  OnLeave: { label: 'Licencia', className: 'bg-indigo-100 text-indigo-700' },
  Terminated: { label: 'Terminado', className: 'bg-red-100 text-red-700' },
};

function RoleBadge({ role }: { role: StaffRole }) {
  const config = ROLE_CONFIG[role] || ROLE_CONFIG.Support;
  return (
    <Badge variant="outline" className={config.className}>
      {config.label}
    </Badge>
  );
}

function StatusBadge({ status }: { status: StaffStatus }) {
  const config = STATUS_CONFIG[status] || STATUS_CONFIG.Active;
  return <Badge className={config.className}>{config.label}</Badge>;
}

// =============================================================================
// SKELETON
// =============================================================================

function StaffSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-10 w-40" />
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        {[1, 2, 3, 4].map(i => (
          <Skeleton key={i} className="h-24" />
        ))}
      </div>
      <Card>
        <CardContent className="p-0">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="border-border flex items-center gap-4 border-b p-4">
              <Skeleton className="h-12 w-12 rounded-full" />
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
// STAFF ROW
// =============================================================================

interface StaffRowProps {
  staff: StaffMember;
  onStatusChange: (id: string, status: StaffStatus) => void;
  isUpdating: boolean;
}

function StaffRow({ staff, onStatusChange, isUpdating }: StaffRowProps) {
  const nameParts = staff.fullName?.split(' ') || [];
  const initials = (nameParts[0]?.[0] || '') + (nameParts[1]?.[0] || '') || 'S';

  return (
    <div className="border-border flex items-center gap-4 border-b p-4 transition-colors hover:bg-slate-50 dark:hover:bg-slate-800/50">
      {/* Avatar */}
      <Avatar className="h-12 w-12">
        <AvatarImage src={staff.avatarUrl} alt={staff.fullName} />
        <AvatarFallback className="bg-primary/10 text-primary font-medium">
          {initials}
        </AvatarFallback>
      </Avatar>

      {/* Info */}
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <Link
            href={`/admin/equipo/${staff.id}`}
            className="hover:text-primary truncate font-medium transition-colors"
          >
            {staff.fullName}
          </Link>
          <RoleBadge role={staff.role} />
          <StatusBadge status={staff.status} />
        </div>
        <div className="text-muted-foreground mt-1 flex items-center gap-4 text-sm">
          <span className="flex items-center gap-1 truncate">
            <Mail className="h-3.5 w-3.5" />
            {staff.email}
          </span>
          {staff.phoneNumber && (
            <span className="flex items-center gap-1">
              <Phone className="h-3.5 w-3.5" />
              {staff.phoneNumber}
            </span>
          )}
        </div>
        <div className="text-muted-foreground mt-1 flex items-center gap-4 text-sm">
          {staff.departmentName && (
            <span className="flex items-center gap-1">
              <Building2 className="h-3.5 w-3.5" />
              {staff.departmentName}
            </span>
          )}
          {staff.positionTitle && (
            <span className="flex items-center gap-1">
              <Briefcase className="h-3.5 w-3.5" />
              {staff.positionTitle}
            </span>
          )}
        </div>
      </div>

      {/* Actions */}
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/admin/equipo/${staff.id}`}>
            <Eye className="h-4 w-4" />
          </Link>
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm" disabled={isUpdating}>
              {isUpdating ? (
                <Loader2 className="h-4 w-4 animate-spin" />
              ) : (
                <MoreHorizontal className="h-4 w-4" />
              )}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem asChild>
              <Link href={`/admin/equipo/${staff.id}`}>
                <Eye className="mr-2 h-4 w-4" />
                Ver detalle
              </Link>
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            {staff.status !== 'Active' && (
              <DropdownMenuItem onClick={() => onStatusChange(staff.id, 'Active')}>
                <UserCheck className="mr-2 h-4 w-4 text-primary" />
                Activar
              </DropdownMenuItem>
            )}
            {staff.status === 'Active' && (
              <DropdownMenuItem onClick={() => onStatusChange(staff.id, 'Suspended')}>
                <Ban className="mr-2 h-4 w-4 text-amber-600" />
                Suspender
              </DropdownMenuItem>
            )}
            {staff.status !== 'Terminated' && (
              <DropdownMenuItem
                onClick={() => onStatusChange(staff.id, 'Terminated')}
                className="text-red-600"
              >
                <UserX className="mr-2 h-4 w-4" />
                Terminar
              </DropdownMenuItem>
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

export default function StaffPage() {
  const [filters, setFilters] = useState<StaffFilters>({
    page: 1,
    pageSize: 20,
  });
  const [searchTerm, setSearchTerm] = useState('');
  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [pendingStatusChange, setPendingStatusChange] = useState<{
    id: string;
    status: StaffStatus;
  } | null>(null);

  const { data: staffData, isLoading, refetch } = useStaffMembers(filters);
  const { data: stats } = useStaffStats();
  const { data: departments } = useActiveDepartments();
  const { data: pendingInvitationsCount } = usePendingInvitationsCount();
  const changeStatusMutation = useChangeStaffStatus();

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

  const handleStatusChangeRequest = (id: string, status: StaffStatus) => {
    setPendingStatusChange({ id, status });
    setStatusDialogOpen(true);
  };

  const handleStatusChangeConfirm = async () => {
    if (!pendingStatusChange) return;

    try {
      await changeStatusMutation.mutateAsync({
        id: pendingStatusChange.id,
        data: {
          status: pendingStatusChange.status,
          reason: 'Cambio de estado desde panel admin',
        },
      });
      toast.success(`Estado actualizado a ${STATUS_CONFIG[pendingStatusChange.status].label}`);
    } catch {
      toast.error('Error al actualizar estado');
    } finally {
      setStatusDialogOpen(false);
      setPendingStatusChange(null);
    }
  };

  if (isLoading) {
    return <StaffSkeleton />;
  }

  const staffList = staffData?.items || [];
  const totalPages = staffData?.totalPages || 1;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col justify-between gap-4 sm:flex-row">
        <div>
          <h1 className="text-foreground text-3xl font-bold">Equipo</h1>
          <p className="text-muted-foreground">Gestiona el personal interno de la plataforma</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => refetch()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            Actualizar
          </Button>
          <Button asChild>
            <Link href="/admin/equipo/invitar">
              <UserPlus className="mr-2 h-4 w-4" />
              Invitar
            </Link>
          </Button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-blue-100 p-2">
                <Users className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{stats?.total || 0}</p>
                <p className="text-muted-foreground text-sm">Total</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-primary/10 p-2">
                <UserCheck className="h-5 w-5 text-primary" />
              </div>
              <div>
                <p className="text-2xl font-bold">{stats?.active || 0}</p>
                <p className="text-muted-foreground text-sm">Activos</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-slate-100 p-2">
                <UserX className="h-5 w-5 text-slate-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{stats?.terminated || 0}</p>
                <p className="text-muted-foreground text-sm">Terminados</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-amber-100 p-2">
                <Ban className="h-5 w-5 text-amber-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{stats?.suspended || 0}</p>
                <p className="text-muted-foreground text-sm">Suspendidos</p>
              </div>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="p-4">
            <div className="flex items-center gap-3">
              <div className="rounded-lg bg-purple-100 p-2">
                <Mail className="h-5 w-5 text-purple-600" />
              </div>
              <div>
                <p className="text-2xl font-bold">{pendingInvitationsCount || 0}</p>
                <p className="text-muted-foreground text-sm">Invitaciones</p>
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
            <div className="w-full md:w-48">
              <label className="mb-1.5 block text-sm font-medium">Departamento</label>
              <Select
                value={filters.departmentId || 'all'}
                onValueChange={v => handleFilterChange('departmentId', v)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Todos" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Todos</SelectItem>
                  {departments?.map(dept => (
                    <SelectItem key={dept.id} value={dept.id}>
                      {dept.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
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
                  <SelectItem value="Active">Activo</SelectItem>
                  <SelectItem value="Pending">Pendiente</SelectItem>
                  <SelectItem value="Suspended">Suspendido</SelectItem>
                  <SelectItem value="OnLeave">Licencia</SelectItem>
                  <SelectItem value="Terminated">Terminado</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Staff List */}
      <Card>
        <CardHeader className="border-b px-4 py-3">
          <CardTitle className="text-lg font-medium">
            Personal ({staffData?.totalCount || 0})
          </CardTitle>
        </CardHeader>
        <CardContent className="p-0">
          {staffList.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <Users className="text-muted-foreground/50 mb-4 h-12 w-12" />
              <h3 className="text-lg font-medium">No hay personal</h3>
              <p className="text-muted-foreground mt-1 text-sm">
                {filters.search || filters.role || filters.departmentId || filters.status
                  ? 'No se encontraron resultados con los filtros aplicados'
                  : 'Invita a tu primer miembro del equipo'}
              </p>
              {!filters.search && !filters.role && !filters.departmentId && !filters.status && (
                <Button className="mt-4" asChild>
                  <Link href="/admin/equipo/invitar">
                    <UserPlus className="mr-2 h-4 w-4" />
                    Invitar personal
                  </Link>
                </Button>
              )}
            </div>
          ) : (
            staffList.map(staff => (
              <StaffRow
                key={staff.id}
                staff={staff}
                onStatusChange={handleStatusChangeRequest}
                isUpdating={changeStatusMutation.isPending}
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

      {/* Status Change Dialog */}
      <AlertDialog open={statusDialogOpen} onOpenChange={setStatusDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Confirmar cambio de estado</AlertDialogTitle>
            <AlertDialogDescription>
              {pendingStatusChange && (
                <>
                  ¿Estás seguro de cambiar el estado a{' '}
                  <strong>{STATUS_CONFIG[pendingStatusChange.status].label}</strong>?
                  {pendingStatusChange.status === 'Terminated' && (
                    <span className="mt-2 block text-red-600">
                      Esta acción desactivará permanentemente el acceso del empleado.
                    </span>
                  )}
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleStatusChangeConfirm}
              className={
                pendingStatusChange?.status === 'Terminated' ? 'bg-red-600 hover:bg-red-700' : ''
              }
            >
              {changeStatusMutation.isPending ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : null}
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

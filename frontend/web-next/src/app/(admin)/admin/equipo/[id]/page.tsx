/**
 * Staff Member Detail Page
 *
 * View and edit staff member details.
 */

'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
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
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  ArrowLeft,
  Loader2,
  Mail,
  Phone,
  Shield,
  ShieldCheck,
  ShieldAlert,
  Building2,
  Briefcase,
  User,
  Calendar,
  Edit,
  Save,
  X,
  Ban,
  UserX,
  UserCheck,
  Key,
} from 'lucide-react';
import { toast } from 'sonner';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import {
  useStaffMember,
  useUpdateStaff,
  useChangeStaffStatus,
  useActiveDepartments,
  useActivePositions,
  useStaffMembers,
  type StaffRole,
  type StaffStatus,
} from '@/hooks/use-staff';

// =============================================================================
// ROLE & STATUS CONFIG
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

const ROLES: StaffRole[] = ['SuperAdmin', 'Admin', 'Moderator', 'Support', 'Analyst', 'Compliance'];

// =============================================================================
// SKELETON
// =============================================================================

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-10 w-10" />
        <div className="space-y-2">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-64" />
        </div>
      </div>
      <div className="grid gap-6 lg:grid-cols-3">
        <div className="space-y-6 lg:col-span-2">
          <Skeleton className="h-64" />
          <Skeleton className="h-48" />
        </div>
        <div className="space-y-6">
          <Skeleton className="h-48" />
          <Skeleton className="h-32" />
        </div>
      </div>
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function StaffDetailPage() {
  const params = useParams();
  const staffId = params.id as string;

  const { data: staff, isLoading } = useStaffMember(staffId);
  const { data: departments } = useActiveDepartments();
  const [selectedDepartmentId, setSelectedDepartmentId] = useState<string>('');
  const { data: positions } = useActivePositions(
    selectedDepartmentId || staff?.departmentId || undefined
  );
  const { data: staffData } = useStaffMembers({ status: 'Active', pageSize: 100 });

  const updateMutation = useUpdateStaff();
  const changeStatusMutation = useChangeStaffStatus();

  const [isEditing, setIsEditing] = useState(false);
  const [editData, setEditData] = useState({
    firstName: '',
    lastName: '',
    phoneNumber: '',
    departmentId: '',
    positionId: '',
    supervisorId: '',
  });

  const [roleDialogOpen, setRoleDialogOpen] = useState(false);
  const [pendingRole, setPendingRole] = useState<StaffRole | null>(null);
  const [roleReason, setRoleReason] = useState('');

  const [statusDialogOpen, setStatusDialogOpen] = useState(false);
  const [pendingStatus, setPendingStatus] = useState<StaffStatus | null>(null);
  const [statusReason, setStatusReason] = useState('');

  // Start editing
  const handleStartEdit = () => {
    if (staff) {
      const nameParts = staff.fullName?.split(' ') || [];
      setEditData({
        firstName: staff.firstName || nameParts[0] || '',
        lastName: staff.lastName || nameParts.slice(1).join(' ') || '',
        phoneNumber: staff.phoneNumber || '',
        departmentId: staff.departmentId || '',
        positionId: staff.positionId || '',
        supervisorId: staff.supervisorId || '',
      });
      setSelectedDepartmentId(staff.departmentId || '');
      setIsEditing(true);
    }
  };

  // Save changes
  const handleSave = async () => {
    try {
      await updateMutation.mutateAsync({
        id: staffId,
        data: {
          firstName: editData.firstName,
          lastName: editData.lastName,
          phoneNumber: editData.phoneNumber || undefined,
          departmentId: editData.departmentId || undefined,
          positionId: editData.positionId || undefined,
          supervisorId: editData.supervisorId || undefined,
        },
      });
      toast.success('Cambios guardados');
      setIsEditing(false);
    } catch {
      toast.error('Error al guardar cambios');
    }
  };

  // Change role
  const handleRoleChangeRequest = (role: StaffRole) => {
    setPendingRole(role);
    setRoleReason('');
    setRoleDialogOpen(true);
  };

  const handleRoleChangeConfirm = async () => {
    if (!pendingRole) return;

    try {
      await updateMutation.mutateAsync({
        id: staffId,
        data: { role: pendingRole },
      });
      toast.success(`Rol cambiado a ${ROLE_CONFIG[pendingRole].label}`);
      setRoleDialogOpen(false);
    } catch {
      toast.error('Error al cambiar rol');
    }
  };

  // Change status
  const handleStatusChangeRequest = (status: StaffStatus) => {
    setPendingStatus(status);
    setStatusReason('');
    setStatusDialogOpen(true);
  };

  const handleStatusChangeConfirm = async () => {
    if (!pendingStatus) return;

    try {
      await changeStatusMutation.mutateAsync({
        id: staffId,
        data: { status: pendingStatus, reason: statusReason || 'Cambio de estado' },
      });
      toast.success(`Estado cambiado a ${STATUS_CONFIG[pendingStatus].label}`);
      setStatusDialogOpen(false);
    } catch {
      toast.error('Error al cambiar estado');
    }
  };

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (!staff) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <User className="text-muted-foreground/50 mb-4 h-12 w-12" />
        <h2 className="text-xl font-semibold">Personal no encontrado</h2>
        <p className="text-muted-foreground mt-1">
          El miembro del equipo no existe o fue eliminado.
        </p>
        <Button className="mt-4" asChild>
          <Link href="/admin/equipo">Volver al listado</Link>
        </Button>
      </div>
    );
  }

  const nameParts = staff.fullName?.split(' ') || [];
  const initials = ((nameParts[0]?.[0] || '') + (nameParts[1]?.[0] || '')).toUpperCase() || 'S';
  const RoleIcon = ROLE_CONFIG[staff.role]?.icon || Shield;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/admin/equipo">
            <ArrowLeft className="h-5 w-5" />
          </Link>
        </Button>
        <Avatar className="h-16 w-16">
          <AvatarImage src={staff.avatarUrl} alt={staff.fullName} />
          <AvatarFallback className="bg-primary/10 text-primary text-xl font-medium">
            {initials}
          </AvatarFallback>
        </Avatar>
        <div className="flex-1">
          <div className="flex flex-wrap items-center gap-3">
            <h1 className="text-3xl font-bold">{staff.fullName}</h1>
            <Badge variant="outline" className={ROLE_CONFIG[staff.role]?.className}>
              <RoleIcon className="mr-1 h-3.5 w-3.5" />
              {ROLE_CONFIG[staff.role]?.label}
            </Badge>
            <Badge className={STATUS_CONFIG[staff.status]?.className}>
              {STATUS_CONFIG[staff.status]?.label}
            </Badge>
          </div>
          <p className="text-muted-foreground mt-1">{staff.email}</p>
        </div>
        {!isEditing && (
          <Button variant="outline" onClick={handleStartEdit}>
            <Edit className="mr-2 h-4 w-4" />
            Editar
          </Button>
        )}
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Main Content */}
        <div className="space-y-6 lg:col-span-2">
          <Tabs defaultValue="info">
            <TabsList>
              <TabsTrigger value="info">Información</TabsTrigger>
              <TabsTrigger value="permissions">Permisos</TabsTrigger>
              <TabsTrigger value="activity">Actividad</TabsTrigger>
            </TabsList>

            <TabsContent value="info" className="mt-6 space-y-6">
              {/* Personal Info */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <User className="h-5 w-5" />
                    Información Personal
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {isEditing ? (
                    <>
                      <div className="grid gap-4 sm:grid-cols-2">
                        <div className="space-y-2">
                          <Label htmlFor="firstName">Nombre</Label>
                          <Input
                            id="firstName"
                            value={editData.firstName}
                            onChange={e =>
                              setEditData(prev => ({ ...prev, firstName: e.target.value }))
                            }
                          />
                        </div>
                        <div className="space-y-2">
                          <Label htmlFor="lastName">Apellido</Label>
                          <Input
                            id="lastName"
                            value={editData.lastName}
                            onChange={e =>
                              setEditData(prev => ({ ...prev, lastName: e.target.value }))
                            }
                          />
                        </div>
                      </div>
                      <div className="space-y-2">
                        <Label htmlFor="phone">Teléfono</Label>
                        <Input
                          id="phone"
                          value={editData.phoneNumber}
                          onChange={e =>
                            setEditData(prev => ({ ...prev, phoneNumber: e.target.value }))
                          }
                          placeholder="809-555-0000"
                        />
                      </div>
                    </>
                  ) : (
                    <div className="grid gap-4 sm:grid-cols-2">
                      <div className="flex items-center gap-3">
                        <Mail className="text-muted-foreground h-4 w-4" />
                        <div>
                          <p className="text-muted-foreground text-sm">Email</p>
                          <p className="font-medium">{staff.email}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Phone className="text-muted-foreground h-4 w-4" />
                        <div>
                          <p className="text-muted-foreground text-sm">Teléfono</p>
                          <p className="font-medium">{staff.phoneNumber || 'No registrado'}</p>
                        </div>
                      </div>
                    </div>
                  )}
                </CardContent>
              </Card>

              {/* Organization */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Building2 className="h-5 w-5" />
                    Organización
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {isEditing ? (
                    <>
                      <div className="grid gap-4 sm:grid-cols-2">
                        <div className="space-y-2">
                          <Label>Departamento</Label>
                          <Select
                            value={editData.departmentId}
                            onValueChange={value => {
                              setSelectedDepartmentId(value);
                              setEditData(prev => ({
                                ...prev,
                                departmentId: value,
                                positionId: '',
                              }));
                            }}
                          >
                            <SelectTrigger>
                              <SelectValue placeholder="Seleccionar" />
                            </SelectTrigger>
                            <SelectContent>
                              {departments?.map(dept => (
                                <SelectItem key={dept.id} value={dept.id}>
                                  {dept.name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </div>
                        <div className="space-y-2">
                          <Label>Posición</Label>
                          <Select
                            value={editData.positionId}
                            onValueChange={value =>
                              setEditData(prev => ({ ...prev, positionId: value }))
                            }
                            disabled={!editData.departmentId}
                          >
                            <SelectTrigger>
                              <SelectValue placeholder="Seleccionar" />
                            </SelectTrigger>
                            <SelectContent>
                              {positions?.map(pos => (
                                <SelectItem key={pos.id} value={pos.id}>
                                  {pos.title}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </div>
                      </div>
                      <div className="space-y-2">
                        <Label>Supervisor</Label>
                        <Select
                          value={editData.supervisorId}
                          onValueChange={value =>
                            setEditData(prev => ({ ...prev, supervisorId: value }))
                          }
                        >
                          <SelectTrigger>
                            <SelectValue placeholder="Seleccionar supervisor" />
                          </SelectTrigger>
                          <SelectContent>
                            {staffData?.items
                              ?.filter(s => s.id !== staffId)
                              .map(s => (
                                <SelectItem key={s.id} value={s.id}>
                                  {s.fullName} - {s.role}
                                </SelectItem>
                              ))}
                          </SelectContent>
                        </Select>
                      </div>
                    </>
                  ) : (
                    <div className="grid gap-4 sm:grid-cols-2">
                      <div className="flex items-center gap-3">
                        <Building2 className="text-muted-foreground h-4 w-4" />
                        <div>
                          <p className="text-muted-foreground text-sm">Departamento</p>
                          <p className="font-medium">{staff.departmentName || 'Sin asignar'}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <Briefcase className="text-muted-foreground h-4 w-4" />
                        <div>
                          <p className="text-muted-foreground text-sm">Posición</p>
                          <p className="font-medium">{staff.positionTitle || 'Sin asignar'}</p>
                        </div>
                      </div>
                      <div className="flex items-center gap-3">
                        <User className="text-muted-foreground h-4 w-4" />
                        <div>
                          <p className="text-muted-foreground text-sm">Supervisor</p>
                          <p className="font-medium">{staff.supervisorName || 'Sin supervisor'}</p>
                        </div>
                      </div>
                    </div>
                  )}

                  {isEditing && (
                    <div className="flex gap-2 pt-4">
                      <Button onClick={handleSave} disabled={updateMutation.isPending}>
                        {updateMutation.isPending ? (
                          <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        ) : (
                          <Save className="mr-2 h-4 w-4" />
                        )}
                        Guardar
                      </Button>
                      <Button variant="outline" onClick={() => setIsEditing(false)}>
                        <X className="mr-2 h-4 w-4" />
                        Cancelar
                      </Button>
                    </div>
                  )}
                </CardContent>
              </Card>

              {/* Dates */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Calendar className="h-5 w-5" />
                    Fechas
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid gap-4 sm:grid-cols-3">
                    <div>
                      <p className="text-muted-foreground text-sm">Fecha de contratación</p>
                      <p className="font-medium">
                        {staff.hireDate
                          ? format(new Date(staff.hireDate), "d 'de' MMMM, yyyy", { locale: es })
                          : 'No disponible'}
                      </p>
                    </div>
                    <div>
                      <p className="text-muted-foreground text-sm">Última actividad</p>
                      <p className="font-medium">
                        {staff.lastLoginAt
                          ? format(new Date(staff.lastLoginAt), "d 'de' MMMM, yyyy", {
                              locale: es,
                            })
                          : 'Sin actividad'}
                      </p>
                    </div>
                    <div>
                      <p className="text-muted-foreground text-sm">Creado</p>
                      <p className="font-medium">
                        {staff.createdAt
                          ? format(new Date(staff.createdAt), "d 'de' MMMM, yyyy", { locale: es })
                          : 'N/A'}
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="permissions" className="mt-6">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Key className="h-5 w-5" />
                    Permisos Asignados
                  </CardTitle>
                  <CardDescription>Permisos específicos del usuario</CardDescription>
                </CardHeader>
                <CardContent>
                  <p className="text-muted-foreground text-sm">
                    Los permisos se heredan del rol asignado (
                    {ROLE_CONFIG[staff.role]?.label || staff.role}). La gestión de permisos
                    individuales estará disponible próximamente.
                  </p>
                </CardContent>
              </Card>
            </TabsContent>

            <TabsContent value="activity" className="mt-6">
              <Card>
                <CardHeader>
                  <CardTitle>Historial de Actividad</CardTitle>
                </CardHeader>
                <CardContent>
                  <p className="text-muted-foreground text-sm">
                    El historial de actividad estará disponible próximamente.
                  </p>
                </CardContent>
              </Card>
            </TabsContent>
          </Tabs>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Role Management */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Shield className="h-5 w-5" />
                Cambiar Rol
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <Select
                value={staff.role}
                onValueChange={value => handleRoleChangeRequest(value as StaffRole)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {ROLES.map(role => (
                    <SelectItem key={role} value={role}>
                      {ROLE_CONFIG[role].label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </CardContent>
          </Card>

          {/* Status Management */}
          <Card>
            <CardHeader>
              <CardTitle>Acciones</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {staff.status !== 'Active' && (
                <Button
                  variant="outline"
                  className="w-full justify-start"
                  onClick={() => handleStatusChangeRequest('Active')}
                >
                  <UserCheck className="mr-2 h-4 w-4 text-primary" />
                  Activar
                </Button>
              )}
              {staff.status === 'Active' && (
                <Button
                  variant="outline"
                  className="w-full justify-start"
                  onClick={() => handleStatusChangeRequest('Suspended')}
                >
                  <Ban className="mr-2 h-4 w-4 text-amber-600" />
                  Suspender
                </Button>
              )}
              {staff.status !== 'Terminated' && (
                <Button
                  variant="outline"
                  className="w-full justify-start text-red-600 hover:text-red-700"
                  onClick={() => handleStatusChangeRequest('Terminated')}
                >
                  <UserX className="mr-2 h-4 w-4" />
                  Terminar contrato
                </Button>
              )}
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Role Change Dialog */}
      <AlertDialog open={roleDialogOpen} onOpenChange={setRoleDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cambiar rol</AlertDialogTitle>
            <AlertDialogDescription>
              {pendingRole && (
                <>
                  ¿Cambiar el rol a <strong>{ROLE_CONFIG[pendingRole].label}</strong>?
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="py-4">
            <Label htmlFor="roleReason">Razón del cambio (opcional)</Label>
            <Input
              id="roleReason"
              value={roleReason}
              onChange={e => setRoleReason(e.target.value)}
              placeholder="Ej: Promoción, reestructuración..."
              className="mt-2"
            />
          </div>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction onClick={handleRoleChangeConfirm}>
              {updateMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Status Change Dialog */}
      <AlertDialog open={statusDialogOpen} onOpenChange={setStatusDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Cambiar estado</AlertDialogTitle>
            <AlertDialogDescription>
              {pendingStatus && (
                <>
                  ¿Cambiar el estado a <strong>{STATUS_CONFIG[pendingStatus].label}</strong>?
                  {pendingStatus === 'Terminated' && (
                    <span className="mt-2 block text-red-600">
                      Esta acción desactivará permanentemente el acceso del empleado.
                    </span>
                  )}
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <div className="py-4">
            <Label htmlFor="statusReason">Razón del cambio</Label>
            <Input
              id="statusReason"
              value={statusReason}
              onChange={e => setStatusReason(e.target.value)}
              placeholder="Ej: Renuncia voluntaria, incumplimiento..."
              className="mt-2"
            />
          </div>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleStatusChangeConfirm}
              className={pendingStatus === 'Terminated' ? 'bg-red-600 hover:bg-red-700' : ''}
            >
              {changeStatusMutation.isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Confirmar
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
